using Domain.Model.Dto;
using Infrastructure.Repository.InterfacesServices;
using System.Net.Http.Json;

namespace Infrastructure.Repository
{
    public class InventoryClient : IInventoryClient
    {
        private readonly HttpClient _client;

        public InventoryClient(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("InventoryApi");
        }

        public async Task<bool> RegistrarMovimientoAsync(ProductoMovimientoStock request)
        {
            var response = await _client.PostAsJsonAsync("/api/MovimientoStock",request);
            return response.IsSuccessStatusCode;
        }
    }
}
