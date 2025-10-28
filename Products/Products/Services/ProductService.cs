using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Caching.Distributed;
using Products.Models;
using Products.Repositories;
using Products.ViewModels;
using System.Text.Json;

namespace Products.Services
{
    public class ProductService : IProductService
    {
        private const string cacheKey = "products";
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;
        private readonly IDistributedCache _cache;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger, IDistributedCache cache)
        {
            _productRepository = productRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            _logger.LogInformation("Getting all products");

            var productList = await _cache.GetOrSetAsync<List<Product>>(cacheKey, async () =>
            {
                _logger.LogInformation("Cache miss for key: {CacheKey}. Retrieving from database.", cacheKey);
                return await _productRepository.GetAllAsync();
            });

            return productList ?? new List<Product>();
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            _logger.LogInformation("Adding new product");
            var insertedProduct = await _productRepository.AddAsync(product);
            InvalidateProductsCache();

            return insertedProduct;
        }

        private void InvalidateProductsCache()
        {
            // invalidate cache for products, as new product is added
            _logger.LogInformation("invalidating cache for key: {CacheKey} from cache.", cacheKey);
            _cache.Remove(cacheKey);
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
            InvalidateProductsCache();
            return allAddedProducts;
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);

            return await _cache.GetOrSetAsync<Product>($"{cacheKey}_{id}", () =>
            {
                _logger.LogInformation("Cache miss for product ID: {ProductId}. Retrieving from database.", id);
                return _productRepository.GetProductByIdAsync(id);
            }) ?? throw new KeyNotFoundException($"Product with ID {id} not found.");
        }
    }
}