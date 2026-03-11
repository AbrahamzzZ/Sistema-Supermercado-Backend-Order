using Domain.Model.Dto.Admin;

namespace Infrastructure.Services.Interface
{
    public interface IAdminCompraApiClient
    {
        Task<ProveedorAdmin?> ObtenerProveedorAsync(int id);
        Task<SucursalAdmin?> ObtenerSucursalAsync(int id);
        Task<TransportistaAdmin?> ObtenerTransportistaAsync(int id);
        Task<ProductoAdmin?> ObtenerProductoAsync(int id);
    }
}
