using Domain.Model.Dto.Venta;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface IVentaRepository
    {
        Task<string> ObtenerNumeroDocumentoAsync();
        Task<VentaSpDto?> ObtenerVentaAsync(string numeroDocumento);
        Task<List<DetalleVentaSpDto>> ObtenerDetallesVentaAsync(int idCompra);
        Task<bool> RegistrarVentaAsync(Ventas ventaDto);
    }
}
