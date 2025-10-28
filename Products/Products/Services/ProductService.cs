using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Products.Models;
using Products.Repositories;
using Products.ViewModels;

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

        /// <summary>
        /// Retrieves all products, using cache if available.
        /// </summary>
        public async Task<List<Product>> GetAllProductsAsync()
        {
            _logger.LogInformation("Getting all products");
            try
            {
                var productList = await _cache.GetOrSetAsync<List<Product>>(cacheKey, async () =>
                {
                    _logger.LogInformation($"Cache miss for key: {cacheKey}. Retrieving from database.");
                    return await _productRepository.GetAllAsync();
                });
                return productList ?? new List<Product>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all products.");
                throw;
            }
        }

        /// <summary>
        /// Adds a new product and invalidates cache.
        /// </summary>
        public async Task<Product> AddProductAsync(Product product)
        {
            _logger.LogInformation("Adding new product: {@Product}", product);
            try
            {
                var insertedProduct = await _productRepository.AddAsync(product);
                InvalidateProductsCache();
                return insertedProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new product.");
                throw;
            }
        }

        /// <summary>
        /// Invalidates the products cache.
        /// </summary>
        private void InvalidateProductsCache()
        {
            try
            {
                _logger.LogInformation($"Invalidating cache for key: {cacheKey}.");
                _cache.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while invalidating cache for key: {cacheKey}.");
            }
        }

        /// <summary>
        /// Reads products from a file and adds them to the database in batches.
        /// </summary>
        public async Task<List<Product>> AddProductsFromFileAsync()
        {
            _logger.LogInformation("Adding products from file: ViewModels/data.json");
            try
            {
                var jsonData = await File.ReadAllTextAsync("ViewModels/data.json");
                var products = JsonSerializer.Deserialize<List<ProductViewModel>>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (products == null || products.Count == 0)
                {
                    _logger.LogWarning("No products found in the file.");
                    return new List<Product>();
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

                    _logger.LogInformation("Processing batch of {BatchCount} products. Progress: {Progress}/{Total}", batch.Count, processedCount + batch.Count, totalProducts);
                    var addedProducts = await _productRepository.AddProductsFromFileAsync(batch);
                    allAddedProducts.AddRange(addedProducts);
                    processedCount += batch.Count;
                }

                _logger.LogInformation("Completed adding all {ProcessedCount} products.", processedCount);
                InvalidateProductsCache();
                return allAddedProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding products from file.");
                throw;
            }
        }

        /// <summary>
        /// Gets a product by its ID, using cache if available.
        /// </summary>
        public async Task<Product> GetProductByIdAsync(string id)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);
            try
            {
                var product = await _cache.GetOrSetAsync<Product>($"{cacheKey}_{id}", () =>
                {
                    _logger.LogInformation("Cache miss for product ID: {ProductId}. Retrieving from database.", id);
                    return _productRepository.GetProductByIdAsync(id);
                });
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found.", id);
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
                }
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product by ID: {ProductId}", id);
                throw;
            }
        }
    }
}