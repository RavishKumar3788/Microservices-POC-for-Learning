using Users.Models;

namespace Users.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> AddUserAsync(User User);
        Task<List<User>> AddUsersFromFileAsync();
        Task<User> GetUserByIdAsync(string id);
    }
}
