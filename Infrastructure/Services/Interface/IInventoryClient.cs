using Domain.Model.Dto;

namespace Infrastructure.Services.Interface
{
    public interface IInventoryClient
    {
        Task<bool> RegistrarMovimientoAsync(ProductoMovimientoStock request);
        Task<int> ObtenerStockAsync(int idProducto);
    }
}
