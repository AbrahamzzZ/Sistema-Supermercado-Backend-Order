using Domain.Model.Dto;
using Domain.Model.Dto.Inventory;
using Infrastructure.Services.Interface;
using System.Net;
using System.Net.Http.Json;
using Utilities.Shared;

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

        public async Task<int> ObtenerStockAsync(int idProducto)
        {
            var response = await _client.GetAsync($"/inventory/ProductoStock/{idProducto}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return 0;

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Inventory respondió {(int)response.StatusCode} al consultar el stock del producto {idProducto}.",
                    null, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProductoStockDto>>();
            return apiResponse?.Data?.Stock ?? 0;
        }
    }
}
