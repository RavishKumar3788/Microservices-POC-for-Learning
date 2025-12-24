using Orders.DTOs;

namespace Orders.ExternalServices
{
    public interface IUserServiceClient
    {
        Task<List<UserDto>> GetAllUsersAsync();
    }

    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching users from Users service");
                var response = await _httpClient.GetAsync("/api/users");
                response.EnsureSuccessStatusCode();

                var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                _logger.LogInformation("Successfully fetched {Count} users", users?.Count ?? 0);
                return users ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users from Users service");
                return new List<UserDto>();
            }
        }
    }
}
