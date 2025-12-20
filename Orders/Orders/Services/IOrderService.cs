using Orders.Models;

namespace Orders.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(string id);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> UpdateOrderAsync(string id, Order order);
        Task<bool> DeleteOrderAsync(string id);
    }
}
