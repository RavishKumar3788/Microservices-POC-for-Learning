using System.Text.Json;
using Users.Models;
using Users.Repositories;
using Users.ViewModels;

namespace Users.Services
{
    public class UserService : IUserService
    {
        private readonly IUsersRepository _UserRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUsersRepository UserRepository, ILogger<UserService> logger)
        {
            _UserRepository = UserRepository;
            _logger = logger;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Getting all Users");
            return await _UserRepository.GetAllAsync();
        }

        public async Task<User> AddUserAsync(User User)
        {
            _logger.LogInformation("Adding new User");
            return await _UserRepository.AddAsync(User);
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
            return allAddedUsers;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            _logger.LogInformation($"Getting User with User Id {id}");
            return await _UserRepository.GetUserByIdAsync(id);
        }
    }
}
