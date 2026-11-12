using Domain.Context;
using Domain.Model.Dto.Estadistica;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class EstadisticaService
    {
        private readonly OrderSistemaSupermercadoContext _context;
        private readonly ILogger<EstadisticaService> _logger;

        public EstadisticaService(OrderSistemaSupermercadoContext context, ILogger<EstadisticaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<ApiResponse<List<EstadisticaTotal>>> ProductosCompradosAsync() => TotalesAsync("PA_ESTADISTICA_PRODUCTOS_COMPRADOS");
        public Task<ApiResponse<List<EstadisticaTotal>>> ProductosVendidosAsync() => TotalesAsync("PA_ESTADISTICA_PRODUCTOS_VENDIDOS");
        public Task<ApiResponse<List<EstadisticaTotal>>> TopClientesAsync() => TotalesAsync("PA_ESTADISTICA_TOP_CLIENTES");
        public Task<ApiResponse<List<EstadisticaTotal>>> TopProveedoresAsync() => TotalesAsync("PA_ESTADISTICA_TOP_PROVEEDORES");
        public Task<ApiResponse<List<EstadisticaTotal>>> ViajesTransportistaAsync() => TotalesAsync("PA_ESTADISTICA_VIAJES_TRANSPORTISTA");
        public Task<ApiResponse<List<EstadisticaTotal>>> VentasEmpleadoAsync() => TotalesAsync("PA_ESTADISTICA_VENTAS_EMPLEADO");

        public Task<ApiResponse<List<EstadisticaTendencia>>> TendenciaVendidosAsync() => TendenciaAsync("PA_ESTADISTICA_TENDENCIA_VENDIDOS");
        public Task<ApiResponse<List<EstadisticaTendencia>>> TendenciaCompradosAsync() => TendenciaAsync("PA_ESTADISTICA_TENDENCIA_COMPRADOS");

        private async Task<ApiResponse<List<EstadisticaTotal>>> TotalesAsync(string procedimiento)
        {
            var lista = await _context.Database.SqlQueryRaw<EstadisticaTotal>($"EXEC {procedimiento}").ToListAsync();
            return Respuesta(lista, procedimiento);
        }

        private async Task<ApiResponse<List<EstadisticaTendencia>>> TendenciaAsync(string procedimiento)
        {
            var lista = await _context.Database.SqlQueryRaw<EstadisticaTendencia>($"EXEC {procedimiento}").ToListAsync();
            return Respuesta(lista, procedimiento);
        }

        private ApiResponse<List<T>> Respuesta<T>(List<T> lista, string procedimiento)
        {
            _logger.LogInformation("Estadística {Procedimiento}: {Cantidad} registros", procedimiento, lista.Count);

            return new ApiResponse<List<T>>
            {
                IsSuccess = true,
                Message = lista.Count == 0 ? Mensajes.MESSAGE_QUERY_EMPTY : Mensajes.MESSAGE_QUERY,
                Data = lista
            };
        }
    }
}
