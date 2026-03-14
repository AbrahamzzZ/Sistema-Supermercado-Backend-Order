using Infrastructure.Services.Interface;

namespace Infrastructure.Services
{
    public class MasterDataValidationService : IMasterData
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MasterDataValidationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ProveedorExisteAsync(int idProveedor)
        {
            var client = _httpClientFactory.CreateClient("AdminApi");
            var response = await client.GetAsync($"/admin/Proveedor/{idProveedor}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TransportistaExisteAsync(int idTransportista)
        {
            var client = _httpClientFactory.CreateClient("AdminApi");
            var response = await client.GetAsync($"/admin/Transportista/{idTransportista}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ProductoExisteAsync(int idProducto)
        {
            var client = _httpClientFactory.CreateClient("AdminApi");
            var response = await client.GetAsync($"/admin/Producto/{idProducto}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UsuarioExisteAsync(int idUsuario)
        {
            var client = _httpClientFactory.CreateClient("AuthApi");
            var response = await client.GetAsync($"/auth/Usuario/{idUsuario}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SucursalExisteAsync(int idSucursal)
        {
            var client = _httpClientFactory.CreateClient("AdminApi");
            var response = await client.GetAsync($"/admin/Sucursal/{idSucursal}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ClienteExisteAsync(int idCliente)
        {
            var client = _httpClientFactory.CreateClient("AdminApi");
            var response = await client.GetAsync($"/admin/Cliente/{idCliente}");
            return response.IsSuccessStatusCode;
        }
    }
}
