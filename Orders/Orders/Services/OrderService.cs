using Orders.Models;
using Orders.Repositories;

namespace Orders.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Getting all orders");
            try
            {
                return await _orderRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all orders");
                throw;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(string id)
        {
            _logger.LogInformation("Getting order with ID: {OrderId}", id);
            try
            {
                return await _orderRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order with ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _logger.LogInformation("Creating new order for user: {UserId}", order.UserId);
            try
            {
                // Calculate total price
                order.TotalPrice = order.ProductPrice * order.Quantity;
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                return await _orderRepository.CreateAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order");
                throw;
            }
        }

        public async Task<Order?> UpdateOrderAsync(string id, Order order)
        {
            _logger.LogInformation("Updating order with ID: {OrderId}", id);
            try
            {
                // Recalculate total price
                order.TotalPrice = order.ProductPrice * order.Quantity;
                order.UpdatedAt = DateTime.UtcNow;

                return await _orderRepository.UpdateAsync(id, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order with ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(string id)
        {
            _logger.LogInformation("Deleting order with ID: {OrderId}", id);
            try
            {
                return await _orderRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order with ID: {OrderId}", id);
                throw;
            }
        }
    }
}
