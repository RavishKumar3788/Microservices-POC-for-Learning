using Orders.DTOs;

namespace Orders.ExternalServices
{
    public interface IProductServiceClient
    {
        Task<List<ProductDto>> GetAllProductsAsync();
    }

    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching products from Products service");
                var response = await _httpClient.GetAsync("/api/products");
                response.EnsureSuccessStatusCode();

                var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
                _logger.LogInformation("Successfully fetched {Count} products", products?.Count ?? 0);
                return products ?? new List<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from Products service");
                return new List<ProductDto>();
            }
        }
    }
}
