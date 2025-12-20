using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Users.Models;
using Users.Repositories;
using Users.ViewModels;

namespace Users.Services
{
    public class UserService : IUserService
    {
        private const string cacheKey = "users";
        private readonly IUsersRepository _UserRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IDistributedCache _cache;

        public UserService(IUsersRepository UserRepository, ILogger<UserService> logger, IDistributedCache cache)
        {
            _UserRepository = UserRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Getting all Users");
            try
            {
                var userList = await _cache.GetOrSetAsync<List<User>>(cacheKey, async () =>
                {
                    _logger.LogInformation($"Cache miss for key: {cacheKey}. Retrieving from database.");
                    return await _UserRepository.GetAllAsync();
                });
                return userList ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all users.");
                throw;
            }
        }

        public async Task<User> AddUserAsync(User User)
        {
            _logger.LogInformation("Adding new User");
            try
            {
                var insertedUser = await _UserRepository.AddAsync(User);
                InvalidateUsersCache();
                return insertedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new user.");
                throw;
            }
        }

        private void InvalidateUsersCache()
        {
            try
            {
                _logger.LogInformation($"Invalidating cache for key: {cacheKey}.");
                _cache.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while invalidating cache for key: {cacheKey}.");
            }
        }

        public async Task<List<User>> AddUsersFromFileAsync()
        {
            // this method will read Users from ViewModels/data.json file and add them to the database in batches
            _logger.LogInformation("Adding Users from file");

            // Read the JSON file
            var jsonData = File.ReadAllText("ViewModels/data.json");

            // Deserialize to list
            var Users = JsonSerializer.Deserialize<List<UserViewModel>>(jsonData, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (Users == null)
            {
                _logger.LogWarning("No Users found in the file");
                return [];
            }

            const int batchSize = 50;
            var allAddedUsers = new List<User>();
            var totalUsers = Users.Count;
            var processedCount = 0;

            while (processedCount < totalUsers)
            {
                var batch = Users.Skip(processedCount).Take(batchSize)
                    .Select(User => new User
                    {
                        Name = User.Name,
                        Email = User.Email,
                        Country = User.Country,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                _logger.LogInformation($"Processing batch of {batch.Count} Users. Progress: {processedCount + batch.Count}/{totalUsers}");
                var addedUsers = await _UserRepository.AddUsersFromFileAsync(batch);
                allAddedUsers.AddRange(addedUsers);
                processedCount += batch.Count;
            }

            _logger.LogInformation($"Completed adding all {processedCount} Users");
            InvalidateUsersCache();
            return allAddedUsers;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            _logger.LogInformation($"Getting User with User Id {id}");
            try
            {
                var user = await _cache.GetOrSetAsync<User>($"{cacheKey}_{id}", () =>
                {
                    _logger.LogInformation("Cache miss for user ID: {UserId}. Retrieving from database.", id);
                    return _UserRepository.GetUserByIdAsync(id);
                });
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", id);
                    throw new KeyNotFoundException($"User with ID {id} not found.");
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by ID: {UserId}", id);
                throw;
            }
        }
    }
}
