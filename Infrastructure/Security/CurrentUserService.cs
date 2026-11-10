using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Security
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? IdUsuario
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var valor = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? user?.FindFirst("sub")?.Value;

                return int.TryParse(valor, out var id) ? id : null;
            }
        }
    }
}
