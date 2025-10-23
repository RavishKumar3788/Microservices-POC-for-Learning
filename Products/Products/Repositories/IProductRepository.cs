using Products.Models;

namespace Products.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product> AddAsync(Product product);
    }
}