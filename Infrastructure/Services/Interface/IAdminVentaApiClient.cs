using Domain.Model.Dto.Admin;

namespace Infrastructure.Services.Interface
{
    public interface IAdminVentaApiClient
    {
        Task<SucursalAdmin?> ObtenerSucursalAsync(int id);
        Task<ClienteAdmin?> ObtenerClienteAsync(int id);
        Task<ProductoAdmin?> ObtenerProductoAsync(int id);
    }
}
