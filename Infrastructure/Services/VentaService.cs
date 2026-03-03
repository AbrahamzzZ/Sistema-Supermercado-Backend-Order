using Domain.Model.Dto;
using Domain.Model.Dto.Venta;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Services.Interface;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class VentaService : IVentaService
    {
        private readonly VentaRepository _ventaRepository;
        private readonly IValidator<Ventas> _validator;
        private readonly IMasterData _masterDataValidator;
        private readonly IInventoryClient _inventoryClient;

        public VentaService(VentaRepository ventaRepository, IValidator<Ventas> validator, IMasterData masterDataValidator, IInventoryClient inventoryClient)
        {
            _ventaRepository = ventaRepository;
            _validator = validator;
            _masterDataValidator = masterDataValidator;
            _inventoryClient = inventoryClient;
        }

        //Para pruebas unitarias, descomenta este constructor y comenta el constructor anterior.

        /*readonly IVentaRepository _ventaRepository;
        private readonly IValidator<Ventas> _validator;
        public VentaService(IVentaRepository ventaRepository, IValidator<Ventas> validator)
        {
            _ventaRepository = ventaRepository;
            _validator = validator;
        }*/

        public async Task<ApiResponse<string>> ObtenerNumeroDocumentoAsync()
        {
            var numero = await _ventaRepository.ObtenerNumeroDocumentoAsync();
            return new ApiResponse<string> { IsSuccess = true, Message = "Número de documento generado correctamente.", Data = numero };
        }

        public async Task<ApiResponse<VentaRespuesta>> ObtenerVentaAsync(string numeroDocumento)
        {
            var numero = await _ventaRepository.ObtenerVentaAsync(numeroDocumento);
            if(numero == null) 
                return new ApiResponse<VentaRespuesta> {IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY };

            return new ApiResponse<VentaRespuesta> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = numero };
        }

        public async Task<ApiResponse<List<DetalleVentasRepuesta>>> ObtenerDetallesVentaAsync(int idVenta)
        {
            var detalleVenta = await _ventaRepository.ObtenerDetallesVentaAsync(idVenta);

            if(detalleVenta == null || detalleVenta.Count == 0)
                return new ApiResponse<List<DetalleVentasRepuesta>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = detalleVenta };

            return new ApiResponse<List<DetalleVentasRepuesta>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = detalleVenta };
        }

        public async Task<ApiResponse<object>> RegistrarVentaAsync(Ventas ventaDto)
        {
            var validationResult = await _validator.ValidateAsync(ventaDto);

            if (!validationResult.IsValid)
                return new ApiResponse<object> { IsSuccess = false, Message = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)) };

            if (!await _masterDataValidator.SucursalExisteAsync(ventaDto.Id_Sucursal))
                return new ApiResponse<object> { IsSuccess = false, Message = "La Sucursal no existe" };

            if (!await _masterDataValidator.ClienteExisteAsync(ventaDto.Id_Cliente))
                return new ApiResponse<object> { IsSuccess = false, Message = "El Cliente no existe" };

            foreach (var detalle in ventaDto.Detalles)
            {
                if (!await _masterDataValidator.ProductoExisteAsync(detalle.Id_Producto))
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Producto {detalle.Id_Producto} no existe"
                    };
            }

            var registrado = await _ventaRepository.RegistrarVentaAsync(ventaDto);
            if (registrado)
            {
                foreach (var detalle in ventaDto.Detalles)
                {
                    var movimiento = new ProductoMovimientoStock
                    {
                        IdProducto = detalle.Id_Producto,
                        Cantidad = detalle.Cantidad,
                        Tipo = "SALIDA",
                        Referencia = "VENTA",
                        PrecioVenta = detalle.Precio_Venta
                    };

                    var ok = await _inventoryClient.RegistrarMovimientoAsync(movimiento);

                }
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };
            }

            return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_REGISTER_FAILLED };
        }
    }
}
