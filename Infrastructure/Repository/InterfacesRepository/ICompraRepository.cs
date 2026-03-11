using Domain.Model.Dto.Compra;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface ICompraRepository
    {
        Task<string> ObtenerNumeroDocumentoAsync();
        Task<CompraSpDto?> ObtenerCompraAsync(string numeroDocumento);
        Task<List<DetalleCompraSpDto>> ObtenerDetallesCompraAsync(int idCompra);
        Task<bool> RegistrarCompraAsync(Compras compraDto);
    }
}
