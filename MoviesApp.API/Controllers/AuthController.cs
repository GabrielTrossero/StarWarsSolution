using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Application.DTOs;
using MoviesApp.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace MoviesApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public AuthController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a new user account with the provided registration data.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User successfully registered", typeof(UserDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data or user already exists")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var user = await _userService.CreateAsync(dto);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "User login",
            Description = "Authenticates the user and returns a JWT token if credentials are valid.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Authentication successful", typeof(AuthResponseDto))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid username or password")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var auth = await _authService.AuthenticateAsync(dto);
                return Ok(auth);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
        }
    }
}
