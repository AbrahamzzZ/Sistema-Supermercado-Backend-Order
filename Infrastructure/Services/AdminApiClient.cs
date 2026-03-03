using Domain.Model.Dto.Admin;
using Infrastructure.Services.Interface;
using System.Net.Http.Json;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class AdminApiClient : IAdminApiClient
    {
        private readonly HttpClient _client;

        public AdminApiClient(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("AdminApi");
        }

        public async Task<ProveedorAdmin?> ObtenerProveedorAsync(int id) => await GetAsync<ProveedorAdmin>($"/admin/Proveedor/{id}");

        public async Task<SucursalAdmin?> ObtenerSucursalAsync(int id) => await GetAsync<SucursalAdmin>($"/admin/Sucursal/{id}");

        public async Task<UsuarioAdmin?> ObtenerUsuarioAsync(int id) => await GetAsync<UsuarioAdmin>($"/auth/Usuario/{id}");
        public async Task<TransportistaAdmin?> ObtenerTransportistaAsync(int id) => await GetAsync<TransportistaAdmin>($"/admin/Transportista/{id}");

        private async Task<T?> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return default;

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<T>>();

            if (apiResponse is null || !apiResponse.IsSuccess)
                return default;

            return apiResponse.Data;
        }
    }
}
