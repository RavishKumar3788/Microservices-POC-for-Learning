using System.Text.Json;
using Products.Models;
using Products.Repositories;
using Products.ViewModels;

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

        public async Task<List<Product>> AddProductsFromFileAsync()
        {
            // this method will read products from ViewModels/data.json file and add them to the database in batches
            _logger.LogInformation("Adding products from file");

            // Read the JSON file
            var jsonData = File.ReadAllText("ViewModels/data.json");

            // Deserialize to list
            var products = JsonSerializer.Deserialize<List<ProductViewModel>>(jsonData, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (products == null)
            {
                _logger.LogWarning("No products found in the file");
                return [];
            }

            const int batchSize = 50;
            var allAddedProducts = new List<Product>();
            var totalProducts = products.Count;
            var processedCount = 0;

            while (processedCount < totalProducts)
            {
                var batch = products.Skip(processedCount).Take(batchSize)
                    .Select(product => new Product
                    {
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                _logger.LogInformation($"Processing batch of {batch.Count} products. Progress: {processedCount + batch.Count}/{totalProducts}");
                var addedProducts = await _productRepository.AddProductsFromFileAsync(batch);
                allAddedProducts.AddRange(addedProducts);
                processedCount += batch.Count;
            }

            _logger.LogInformation($"Completed adding all {processedCount} products");
            return allAddedProducts;
        }
    }
}