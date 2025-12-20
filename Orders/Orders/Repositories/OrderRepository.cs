using MongoDB.Driver;
using Orders.Models;
using Orders.Settings;

namespace Orders.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(MongoDbSettings settings, ILogger<OrderRepository> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _orders = database.GetCollection<Order>(settings.CollectionName);
            _logger = logger;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all orders from database");
                return await _orders.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving orders");
                throw;
            }
        }

        public async Task<Order?> GetByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Retrieving order with ID: {OrderId}", id);
                return await _orders.Find(order => order.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order with ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<Order> CreateAsync(Order order)
        {
            try
            {
                _logger.LogInformation("Creating new order for user: {UserId}", order.UserId);
                await _orders.InsertOneAsync(order);
                _logger.LogInformation("Successfully created order with ID: {OrderId}", order.Id);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order for user: {UserId}", order.UserId);
                throw;
            }
        }

        public async Task<Order?> UpdateAsync(string id, Order order)
        {
            try
            {
                _logger.LogInformation("Updating order with ID: {OrderId}", id);
                order.UpdatedAt = DateTime.UtcNow;

                var result = await _orders.ReplaceOneAsync(
                    o => o.Id == id,
                    order,
                    new ReplaceOptions { IsUpsert = false }
                );

                if (result.ModifiedCount == 0)
                {
                    _logger.LogWarning("Order with ID: {OrderId} not found for update", id);
                    return null;
                }

                _logger.LogInformation("Successfully updated order with ID: {OrderId}", id);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order with ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting order with ID: {OrderId}", id);
                var result = await _orders.DeleteOneAsync(order => order.Id == id);

                if (result.DeletedCount == 0)
                {
                    _logger.LogWarning("Order with ID: {OrderId} not found for deletion", id);
                    return false;
                }

                _logger.LogInformation("Successfully deleted order with ID: {OrderId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order with ID: {OrderId}", id);
                throw;
            }
        }
    }
}
