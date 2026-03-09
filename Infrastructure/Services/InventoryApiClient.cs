using Domain.Model.Dto;
using Infrastructure.Services.Interface;
using System.Net.Http.Json;

namespace Infrastructure.Services
{
    public class InventoryApiClient : IInventoryClient
    {
        private readonly HttpClient _client;

        public InventoryApiClient(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("InventoryApi");
        }

        public async Task<bool> RegistrarMovimientoAsync(ProductoMovimientoStock request)
        {
            var response = await _client.PostAsJsonAsync("/inventory/MovimientoStock", request);
            return response.IsSuccessStatusCode;
        }
    }
}
