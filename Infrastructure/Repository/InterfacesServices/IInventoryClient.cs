using Domain.Model.Dto;

namespace Infrastructure.Repository.InterfacesServices
{
    public interface IInventoryClient
    {
        Task<bool> RegistrarMovimientoAsync(ProductoMovimientoStock request);
    }
}
