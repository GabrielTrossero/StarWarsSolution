using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Application.DTOs.Auth;
using MoviesApp.Application.DTOs.User;
using MoviesApp.Application.Interfaces;
using MoviesApp.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace MoviesApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AuthController(IUserService userService, IAuthService authService, IMapper mapper)
        {
            _userService = userService;
            _authService = authService;
            _mapper = mapper;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// 

        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a new user account. Role can be 'Regular' or 'Admin'.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status200OK, "User successfully registered", typeof(UserDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data or user already exists")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var user = _mapper.Map<User>(dto);
                var response = await _userService.CreateAsync(user, dto.Password);
                return Ok(response);
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
