using Domain.Model.Dto;

namespace Infrastructure.Services.Interface
{
    public interface IInventoryClient
    {
        Task<bool> RegistrarMovimientoAsync(ProductoMovimientoStock request);
    }
}
