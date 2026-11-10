using Domain.Model.Dto;
using Domain.Model.Dto.Admin;
using Domain.Model.Dto.Venta;
using FluentValidation;
using Infrastructure.Kafka;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Security;
using Infrastructure.Services.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class VentaService : IVentaService
    {
        private readonly VentaRepository _ventaRepository;
        private readonly IValidator<Ventas> _validator;
        private readonly IMasterData _masterDataValidator;
        private readonly IAdminVentaApiClient _adminClient;
        private readonly IInventoryClient _inventoryClient;
        private readonly IAuthApiClient _authClient;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<VentaService> _logger;

        public VentaService(VentaRepository ventaRepository, IValidator<Ventas> validator, IMasterData masterDataValidator, IInventoryClient inventoryClient, IAdminVentaApiClient adminClient, IAuthApiClient authClient, IKafkaProducer kafkaProducer, ICurrentUserService currentUser, ILogger<VentaService> logger)
        {
            _currentUser = currentUser;
            _logger = logger;
            _ventaRepository = ventaRepository;
            _validator = validator;
            _masterDataValidator = masterDataValidator;
            _inventoryClient = inventoryClient;
            _adminClient = adminClient;
            _authClient = authClient;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<ApiResponse<string>> ObtenerNumeroDocumentoAsync()
        {
            var numero = await _ventaRepository.ObtenerNumeroDocumentoAsync();
            return new ApiResponse<string> { IsSuccess = true, Message = "Número de documento generado correctamente.", Data = numero };
        }

        public async Task<ApiResponse<VentaRespuesta>> ObtenerVentaAsync(string numeroDocumento)
        {
            var ventaBase = await _ventaRepository.ObtenerVentaAsync(numeroDocumento);
            if (ventaBase == null)
                return new ApiResponse<VentaRespuesta> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY };

            var sucursalTask = ventaBase.Id_Sucursal > 0 ? _adminClient.ObtenerSucursalAsync(ventaBase.Id_Sucursal) : Task.FromResult<SucursalAdmin?>(null);

            var usuarioTask = ventaBase.Id_Usuario > 0 ? _authClient.ObtenerUsuarioAsync(ventaBase.Id_Usuario) : Task.FromResult<UsuarioAdmin?>(null);

            var clienteTask = ventaBase.Id_Cliente > 0 ? _adminClient.ObtenerClienteAsync(ventaBase.Id_Cliente) : Task.FromResult<ClienteAdmin?>(null);

            await Task.WhenAll(sucursalTask, usuarioTask, clienteTask);

            var sucursal = await sucursalTask;
            var cliente = await clienteTask;
            var usuario = await usuarioTask;

            var response = new VentaRespuesta
            {
                Id_Venta = ventaBase.Id_Venta,
                Id_Usuario = ventaBase.Id_Usuario,
                Id_Sucursal = ventaBase.Id_Sucursal,
                Id_Cliente = ventaBase.Id_Cliente,

                Tipo_Documento = ventaBase.Tipo_Documento,
                Numero_Documento = ventaBase.Numero_Documento,
                Monto_Total = ventaBase.Monto_Total,
                Monto_Pago = ventaBase.Monto_Pago,
                Monto_Cambio = ventaBase.Monto_Cambio,
                Fecha_Venta = ventaBase.Fecha_Venta,

                Codigo_Usuario = usuario?.Codigo,
                Nombre_Completo = usuario?.Nombre_Completo,

                Codigo_Sucursal = sucursal?.Codigo,
                Nombre_Sucursal = sucursal?.Nombre_Sucursal,
                Direccion_Sucursal = sucursal?.Direccion_Sucursal,

                Codigo_Cliente = cliente?.Codigo,
                Nombres_Cliente = cliente?.Nombres,
                Apellidos_Cliente = cliente?.Apellidos,
                Cedula_Cliente = cliente?.Cedula
            };

            if (ventaBase == null)
                return new ApiResponse<VentaRespuesta> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY };

            return new ApiResponse<VentaRespuesta> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = response };
        }

        public async Task<ApiResponse<List<DetalleVentasRepuesta>>> ObtenerDetallesVentaAsync(int idVenta)
        {
            var detallesSp = await _ventaRepository.ObtenerDetallesVentaAsync(idVenta);

            if (detallesSp == null || detallesSp.Count == 0)
            {
                return new ApiResponse<List<DetalleVentasRepuesta>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY };
            }

            var detalles = detallesSp.Select(d => new DetalleVentasRepuesta
            {
                Id_Producto = d.Id_Producto,
                Precio_Venta = d.Precio_Venta,
                Cantidad = d.Cantidad,
                SubTotal = d.SubTotal,
                Descuento = d.Descuento,
                Productos = null
            }).ToList();

            foreach (var detalle in detalles)
            {
                if (detalle.Id_Producto > 0)
                {
                    try
                    {
                        var producto = await _adminClient.ObtenerProductoAsync(detalle.Id_Producto);
                        detalle.Productos = producto?.Nombre_Producto ?? "Producto no encontrado";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "No se pudo obtener de Admin el producto {IdProducto} (detalle de venta {IdVenta})", detalle.Id_Producto, idVenta);
                        detalle.Productos = "Error al obtener producto";
                    }
                }
            }

            return new ApiResponse<List<DetalleVentasRepuesta>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = detalles };
        }

        public async Task<ApiResponse<object>> RegistrarVentaAsync(Ventas ventaDto)
        {
            var idUsuario = _currentUser.IdUsuario;
            if (idUsuario is null)
                return ErrorNegocio("No se pudo identificar al usuario del token.", ventaDto);
            ventaDto.Id_Usuario = idUsuario.Value;

            var validationResult = await _validator.ValidateAsync(ventaDto);
            if (!validationResult.IsValid)
                return ErrorNegocio(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), ventaDto);

            if (!await _masterDataValidator.SucursalExisteAsync(ventaDto.Id_Sucursal))
                return ErrorNegocio("La Sucursal no existe", ventaDto);

            if (!await _masterDataValidator.ClienteExisteAsync(ventaDto.Id_Cliente))
                return ErrorNegocio("El Cliente no existe", ventaDto);

            foreach (var detalle in ventaDto.Detalles!)
            {
                if (!await _masterDataValidator.ProductoExisteAsync(detalle.Id_Producto))
                    return ErrorNegocio($"Producto {detalle.Id_Producto} no existe", ventaDto);
            }

            var errorStock = await ValidarStockAsync(ventaDto);
            if (errorStock is not null)
                return errorStock;

            bool registrado;
            try
            {
                registrado = await _ventaRepository.RegistrarVentaAsync(ventaDto);
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                return ErrorNegocio($"Ya existe una venta con el número de documento {ventaDto.Numero_Documento}.", ventaDto);
            }

            if (!registrado)
            {
                _logger.LogError("El SP PA_REGISTRAR_VENTA no registró la venta {NumeroDocumento}", ventaDto.Numero_Documento);
                return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_REGISTER_FAILLED };
            }

            _logger.LogInformation("Venta {NumeroDocumento} registrada por el usuario {IdUsuario} con {CantidadDetalles} detalles",
                ventaDto.Numero_Documento, ventaDto.Id_Usuario, ventaDto.Detalles!.Count);

            var stockPendiente = !await PublicarMovimientosAsync(ventaDto);

            return new ApiResponse<object>
            {
                IsSuccess = true,
                Message = stockPendiente
                    ? $"{Mensajes.MESSAGE_REGISTER} Atención: no se pudo notificar a Inventario; el stock no se actualizó."
                    : Mensajes.MESSAGE_REGISTER
            };
        }

        private async Task<ApiResponse<object>?> ValidarStockAsync(Ventas ventaDto)
        {
            var cantidadesPorProducto = ventaDto.Detalles!
                .GroupBy(d => d.Id_Producto)
                .Select(g => new { IdProducto = g.Key, Cantidad = g.Sum(d => d.Cantidad) });

            foreach (var item in cantidadesPorProducto)
            {
                int stock;
                try
                {
                    stock = await _inventoryClient.ObtenerStockAsync(item.IdProducto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No se pudo consultar el stock del producto {IdProducto} en Inventory (venta {NumeroDocumento})",
                        item.IdProducto, ventaDto.Numero_Documento);
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "No se pudo verificar el stock en este momento. Intente nuevamente."
                    };
                }

                if (item.Cantidad > stock)
                    return ErrorNegocio($"Stock insuficiente para el producto {item.IdProducto}. Disponible: {stock}", ventaDto);
            }

            return null;
        }

        private async Task<bool> PublicarMovimientosAsync(Ventas ventaDto)
        {
            var todoOk = true;
            foreach (var detalle in ventaDto.Detalles!)
            {
                var movimiento = new ProductoMovimientoStock
                {
                    IdProducto = detalle.Id_Producto,
                    Cantidad = detalle.Cantidad,
                    Tipo = "SALIDA",
                    Referencia = "VENTA",
                    PrecioVenta = detalle.Precio_Venta,
                    IdUsuario = ventaDto.Id_Usuario
                };

                try
                {
                    await _kafkaProducer.PublishAsync(KafkaTopics.MovimientosStock, movimiento, detalle.Id_Producto.ToString());
                }
                catch (Exception ex)
                {
                    todoOk = false;
                    _logger.LogError(ex, "No se pudo publicar en Kafka el movimiento SALIDA del producto {IdProducto} (venta {NumeroDocumento})",
                        detalle.Id_Producto, ventaDto.Numero_Documento);
                }
            }
            return todoOk;
        }

        private ApiResponse<object> ErrorNegocio(string mensaje, Ventas ventaDto)
        {
            _logger.LogWarning("Venta {NumeroDocumento} rechazada: {Motivo}", ventaDto.Numero_Documento, mensaje);
            return new ApiResponse<object> { IsSuccess = false, Message = mensaje };
        }
    }
}
