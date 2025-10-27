using Microsoft.AspNetCore.Mvc;
using Users.Models;
using Users.Services;
using Users.ViewModels;

namespace Users.Controllers
{
    /// <summary>
    /// Controller for managing user.
    /// Provides endpoints to retrieve, add, and bulk insert user.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A list of all users.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Request received for GetAllUsers");
                var users = await _userService.GetAllUsersAsync();
                _logger.LogInformation("Successfully retrieved {Count} users", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all users");
                return StatusCode(500, "Internal server error");
            }
        }
        /// <summary>
        /// Adds users in bulk from a file data.json.
        /// </summary>
        /// <remarks>
        /// This endpoint reads users from a JSON file and adds them to the database in bulk and return the list of inserted users.
        /// </remarks>
        /// <returns>A list of added users.</returns>
        [HttpGet("addUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<User>>> AddUsers()
        {
            try
            {
                _logger.LogInformation("Request received for AddUsers from file");
                var addedUsers = await _userService.AddUsersFromFileAsync();
                _logger.LogInformation("Successfully added {Count} users", addedUsers.Count);
                return Ok(addedUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding users from file");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Adds a new user.
        /// </summary>
        /// <param name="userViewModel">The user view model containing user details.</param>
        /// <returns>The created user.</returns>
        [HttpPost("addUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> AddUser([FromBody] UserViewModel userViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid user model received");
                    return BadRequest(ModelState);
                }

                var user = new User
                {
                    Name = userViewModel.Name,
                    Email = userViewModel.Email,
                    Country = userViewModel.Country,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Request received to add new user: {UserName}", user.Name);
                var addedUser = await _userService.AddUserAsync(user);
                _logger.LogInformation("Successfully added user with ID: {UserId}", addedUser.Id);

                return CreatedAtAction(nameof(GetAllUsers), new { id = addedUser.Id }, addedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding user: {UserName}", userViewModel.Name);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Health check endpoint.
        /// </summary>
        /// <returns>Health status of the API.</returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok("User API is running");
        }

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        [HttpGet("getUserById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Endpoint to create a test error for error handling verification.
        /// </summary>
        /// <returns>Throws an exception.</returns>
        [HttpGet("CreateError")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateError([FromQuery] List<string> ids)
        {
            throw new Exception("This is a test exception for error handling.");
        }
    }
}