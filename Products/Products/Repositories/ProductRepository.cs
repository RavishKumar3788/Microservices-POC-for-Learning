using MongoDB.Driver;
using Products.Models;
using Products.Settings;

namespace Products.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _products;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(MongoDbSettings settings, ILogger<ProductRepository> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _products = database.GetCollection<Product>(settings.CollectionName);
            _logger = logger;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all products from database");
                return await _products.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving products");
                throw;
            }
        }

        public async Task<Product> AddAsync(Product product)
        {
            try
            {
                _logger.LogInformation("Adding new product: {ProductName}", product.Name);
                await _products.InsertOneAsync(product);
                _logger.LogInformation("Successfully added product with ID: {ProductId}", product.Id);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding product: {ProductName}", product.Name);
                throw;
            }
        }
    }
}