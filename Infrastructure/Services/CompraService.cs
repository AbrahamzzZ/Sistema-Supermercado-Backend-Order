using Domain.Model.Dto;
using Domain.Model.Dto.Admin;
using Domain.Model.Dto.Compra;
using FluentValidation;
using Infrastructure.Kafka;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Security;
using Infrastructure.Services.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Utilities.Shared;


namespace Infrastructure.Services
{
    public class CompraService : ICompraService
    {
        private readonly CompraRepository _compraRepository;
        private readonly IValidator<Compras> _validator;
        private readonly IMasterData _masterDataValidator;
        private readonly IAdminCompraApiClient _adminClient;
        private readonly IInventoryClient _inventoryClient;
        private readonly IAuthApiClient _authClient;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CompraService> _logger;

        public CompraService(CompraRepository compraRepository, IValidator<Compras> validator, IMasterData masterDataValidator, IInventoryClient inventoryClient, IAdminCompraApiClient adminClient, IAuthApiClient authClient, IKafkaProducer kafkaProducer, ICurrentUserService currentUser, ILogger<CompraService> logger)
        {
            _currentUser = currentUser;
            _logger = logger;
            _compraRepository = compraRepository;
            _validator = validator;
            _masterDataValidator = masterDataValidator;
            _inventoryClient = inventoryClient;
            _adminClient = adminClient;
            _authClient = authClient;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<ApiResponse<string>> ObtenerNumeroDocumentoAsync()
        {
            var numero = await _compraRepository.ObtenerNumeroDocumentoAsync();
            return new ApiResponse<string> { IsSuccess = true, Message = "Número de documento generado correctamente.", Data = numero };
        }

        public async Task<ApiResponse<CompraRespuesta>> ObtenerCompraAsync(string numeroDocumento)
        {
            var compraBase = await _compraRepository.ObtenerCompraAsync(numeroDocumento);
            if (compraBase == null)
                return new ApiResponse<CompraRespuesta> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY };

            var proveedorTask = compraBase.Id_Proveedor > 0 ? _adminClient.ObtenerProveedorAsync(compraBase.Id_Proveedor)  : Task.FromResult<ProveedorAdmin?>(null);

            var sucursalTask = compraBase.Id_Sucursal > 0 ? _adminClient.ObtenerSucursalAsync(compraBase.Id_Sucursal) : Task.FromResult<SucursalAdmin?>(null);

            var transportistaTask = compraBase.Id_Transportista > 0  ? _adminClient.ObtenerTransportistaAsync(compraBase.Id_Transportista) : Task.FromResult<TransportistaAdmin?>(null);

            var usuarioTask = compraBase.Id_Usuario > 0 ? _authClient.ObtenerUsuarioAsync(compraBase.Id_Usuario) : Task.FromResult<UsuarioAdmin?>(null);

            await Task.WhenAll(proveedorTask, sucursalTask, usuarioTask, transportistaTask);

            var proveedor = await proveedorTask;
            var sucursal = await sucursalTask;
            var transportista = await transportistaTask;
            var usuario = await usuarioTask;

            var response = new CompraRespuesta
            {
                Id_Compra = compraBase.Id_Compra,
                Id_Usuario = compraBase.Id_Usuario,
                Id_Sucursal = compraBase.Id_Sucursal,
                Id_Proveedor = compraBase.Id_Proveedor,
                Id_Transportista = compraBase.Id_Transportista,

                Tipo_Documento = compraBase.Tipo_Documento,
                Numero_Documento = compraBase.Numero_Documento,
                Monto_Total = compraBase.Monto_Total,
                Fecha_Compra = compraBase.Fecha_Compra,

                Codigo_Usuario = usuario?.Codigo,
                Nombre_Completo = usuario?.Nombre_Completo,

                Codigo_Proveedor = proveedor?.Codigo,
                Nombres_Proveedor = proveedor?.Nombres,
                Apellidos_Proveedor = proveedor?.Apellidos,
                Cedula_Proveedor = proveedor?.Cedula,

                Codigo_Sucursal = sucursal?.Codigo,
                Nombre_Sucursal = sucursal?.Nombre_Sucursal,
                Direccion_Sucursal = sucursal?.Direccion_Sucursal,

                Codigo_Transportista = transportista?.Codigo,
                Nombres_Transportista = transportista?.Nombres,
                Apellidos_Transportista = transportista?.Apellidos,
                Cedula_Transportista = transportista?.Cedula
            };

            if (compraBase == null)
                return new ApiResponse<CompraRespuesta> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY };

            return new ApiResponse<CompraRespuesta> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = response };
        }

        public async Task<ApiResponse<List<DetalleComprasRepuesta>>> ObtenerDetallesCompraAsync(int idCompra)
        {
            var detallesSp = await _compraRepository.ObtenerDetallesCompraAsync(idCompra);

            if (detallesSp == null || detallesSp.Count == 0)
            {
                return new ApiResponse<List<DetalleComprasRepuesta>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY };
            }

            var detalles = detallesSp.Select(d => new DetalleComprasRepuesta
            {
                Id_Producto = d.Id_Producto,
                Precio_Compra = d.Precio_Compra,
                Precio_Venta = d.Precio_Venta,
                Cantidad = d.Cantidad,
                SubTotal = d.SubTotal,
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
                        _logger.LogError(ex, "No se pudo obtener de Admin el producto {IdProducto} (detalle de compra {IdCompra})", detalle.Id_Producto, idCompra);
                        detalle.Productos = "Error al obtener producto";
                    }
                }
            }

            return new ApiResponse<List<DetalleComprasRepuesta>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = detalles };
        }

        public async Task<ApiResponse<object>> RegistrarCompraAsync(Compras compraDto)
        {
            var idUsuario = _currentUser.IdUsuario;
            if (idUsuario is null)
                return ErrorNegocio("No se pudo identificar al usuario del token.", compraDto);
            compraDto.Id_Usuario = idUsuario.Value;

            var validationResult = await _validator.ValidateAsync(compraDto);
            if (!validationResult.IsValid)
                return ErrorNegocio(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), compraDto);

            if (!await _masterDataValidator.SucursalExisteAsync(compraDto.Id_Sucursal))
                return ErrorNegocio("La Sucursal no existe", compraDto);

            if (!await _masterDataValidator.ProveedorExisteAsync(compraDto.Id_Proveedor))
                return ErrorNegocio("El proveedor no existe", compraDto);

            if (!await _masterDataValidator.TransportistaExisteAsync(compraDto.Id_Transportista))
                return ErrorNegocio("El transportista no existe", compraDto);

            foreach (var detalle in compraDto.Detalles!)
            {
                if (!await _masterDataValidator.ProductoExisteAsync(detalle.Id_Producto))
                    return ErrorNegocio($"Producto {detalle.Id_Producto} no existe", compraDto);
            }

            bool registrado;
            try
            {
                registrado = await _compraRepository.RegistrarCompraAsync(compraDto);
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                return ErrorNegocio($"Ya existe una compra con el número de documento {compraDto.Numero_Documento}.", compraDto);
            }

            if (!registrado)
            {
                _logger.LogError("El SP PA_REGISTRAR_COMPRA no registró la compra {NumeroDocumento}", compraDto.Numero_Documento);
                return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_REGISTER_FAILLED };
            }

            _logger.LogInformation("Compra {NumeroDocumento} registrada por el usuario {IdUsuario} con {CantidadDetalles} detalles",
                compraDto.Numero_Documento, compraDto.Id_Usuario, compraDto.Detalles!.Count);

            var stockPendiente = !await PublicarMovimientosAsync(compraDto);

            return new ApiResponse<object>
            {
                IsSuccess = true,
                Message = stockPendiente
                    ? $"{Mensajes.MESSAGE_REGISTER} Atención: no se pudo notificar a Inventario; el stock no se actualizó."
                    : Mensajes.MESSAGE_REGISTER
            };
        }

        private async Task<bool> PublicarMovimientosAsync(Compras compraDto)
        {
            var todoOk = true;
            foreach (var detalle in compraDto.Detalles!)
            {
                var movimiento = new ProductoMovimientoStock
                {
                    IdProducto = detalle.Id_Producto,
                    Cantidad = detalle.Cantidad,
                    Tipo = "ENTRADA",
                    Referencia = "COMPRA",
                    PrecioCompra = detalle.Precio_Compra,
                    PrecioVenta = detalle.Precio_Venta,
                    IdUsuario = compraDto.Id_Usuario
                };

                try
                {
                    await _kafkaProducer.PublishAsync(KafkaTopics.MovimientosStock, movimiento, detalle.Id_Producto.ToString());
                }
                catch (Exception ex)
                {
                    todoOk = false;
                    _logger.LogError(ex, "No se pudo publicar en Kafka el movimiento ENTRADA del producto {IdProducto} (compra {NumeroDocumento})",
                        detalle.Id_Producto, compraDto.Numero_Documento);
                }
            }
            return todoOk;
        }

        private ApiResponse<object> ErrorNegocio(string mensaje, Compras compraDto)
        {
            _logger.LogWarning("Compra {NumeroDocumento} rechazada: {Motivo}", compraDto.Numero_Documento, mensaje);
            return new ApiResponse<object> { IsSuccess = false, Message = mensaje };
        }
    }
}
