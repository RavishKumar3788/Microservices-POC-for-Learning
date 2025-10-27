using Products.Models;

namespace Products.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product> AddAsync(Product product);
        Task<List<Product>> AddProductsFromFileAsync(List<Product> products);
        Task<Product> GetProductByIdAsync(string id);
    }
}