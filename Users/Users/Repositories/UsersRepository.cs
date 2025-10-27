using MongoDB.Driver;
using Users.Models;
using Users.Settings;

namespace Users.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly ILogger<UsersRepository> _logger;

        public UsersRepository(MongoDbSettings settings, ILogger<UsersRepository> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.CollectionName);
            _logger = logger;
        }

        public async Task<List<User>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all users from database");
                return await _users.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users");
                throw;
            }
        }

        public async Task<User> AddAsync(User user)
        {
            try
            {
                _logger.LogInformation("Adding new user: {userName}", user.Name);
                await _users.InsertOneAsync(user);
                _logger.LogInformation("Successfully added user with ID: {userId}", user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding user: {userName}", user.Name);
                throw;
            }
        }

        public Task<List<User>> AddUsersFromFileAsync(List<User> users)
        {
            try
            {
                _logger.LogInformation("Adding {Count} users to the database", users.Count);
                return _users.InsertManyAsync(users)
                    .ContinueWith(_ =>
                    {
                        _logger.LogInformation("Successfully added {Count} users", users.Count);
                        return users;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding users from file");
                throw;
            }
        }

        public Task<User> GetUserByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Retrieving user with ID: {userId}", id);
                return _users.Find(user => user.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with ID: {userId}", id);
                throw;
            }
        }
    }
}
