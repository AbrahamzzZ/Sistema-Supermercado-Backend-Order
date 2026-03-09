using Domain.Model.Dto.Admin;
using Infrastructure.Services.Interface;
using System.Net.Http.Json;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _client;

        public AuthApiClient(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("AuthApi");
        }

        public async Task<UsuarioAdmin?> ObtenerUsuarioAsync(int id) => await GetAsync<UsuarioAdmin>($"/auth/Usuario/{id}");
    
        private async Task<T?> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);
            Console.WriteLine(response.StatusCode);

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
