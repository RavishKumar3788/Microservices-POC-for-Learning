using Users.Models;

namespace Users.Repositories
{
    public interface IUsersRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        Task<List<User>> AddUsersFromFileAsync(List<User> users);
        Task<User> GetUserByIdAsync(string id);
    }
}
