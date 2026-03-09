using Domain.Model.Dto.Admin;

namespace Infrastructure.Services.Interface
{
    public interface IAuthApiClient
    {
        Task<UsuarioAdmin?> ObtenerUsuarioAsync(int id);
    }
}
