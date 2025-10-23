using Products.Models;

namespace Products.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> AddProductAsync(Product product);
    }
}