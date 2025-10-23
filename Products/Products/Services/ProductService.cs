using Products.Models;
using Products.Repositories;

namespace Products.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            _logger.LogInformation("Getting all products");
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            _logger.LogInformation("Adding new product");
            return await _productRepository.AddAsync(product);
        }
    }
}