using Domain.Model.Dto.Estadistica;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Shared;

namespace Order.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EstadisticaController : ControllerBase
    {
        private readonly EstadisticaService _estadisticaService;

        public EstadisticaController(EstadisticaService estadisticaService)
        {
            _estadisticaService = estadisticaService;
        }

        [HttpGet("productos-comprados")]
        public async Task<ActionResult<ApiResponse<List<EstadisticaTotal>>>> ProductosComprados()
            => Ok(await _estadisticaService.ProductosCompradosAsync());

        [HttpGet("productos-vendidos")]
        public async Task<ActionResult<ApiResponse<List<EstadisticaTotal>>>> ProductosVendidos()
            => Ok(await _estadisticaService.ProductosVendidosAsync());

        [HttpGet("top-clientes")]
        public async Task<ActionResult<ApiResponse<List<EstadisticaTotal>>>> TopClientes()
            => Ok(await _estadisticaService.TopClientesAsync());

        [HttpGet("top-proveedores")]
        public async Task<ActionResult<ApiResponse<List<EstadisticaTotal>>>> TopProveedores()
            => Ok(await _estadisticaService.TopProveedoresAsync());

        [HttpGet("viajes-transportista")]
        public async Task<ActionResult<ApiResponse<List<EstadisticaTotal>>>> ViajesTransportista()
            => Ok(await _estadisticaService.ViajesTransportistaAsync());

        [HttpGet("ventas-empleado")]
        public async Task<ActionResult<ApiResponse<List<EstadisticaTotal>>>> VentasEmpleado()
            => Ok(await _estadisticaService.VentasEmpleadoAsync());

        [HttpGet("tendencia-vendidos")]
        public async Task<ActionResult<ApiResponse<List<EstadisticaTendencia>>>> TendenciaVendidos()
            => Ok(await _estadisticaService.TendenciaVendidosAsync());

        [HttpGet("tendencia-comprados")]
        public async Task<ActionResult<ApiResponse<List<EstadisticaTendencia>>>> TendenciaComprados()
            => Ok(await _estadisticaService.TendenciaCompradosAsync());
    }
}
