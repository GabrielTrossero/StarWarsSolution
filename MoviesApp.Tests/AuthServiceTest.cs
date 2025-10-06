using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using MoviesApp.Application.DTOs.Auth;
using MoviesApp.Application.DTOs.User;
using MoviesApp.Application.Services;
using MoviesApp.Domain.Entities;
using MoviesApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthService _authService;
        private readonly JwtSettings _jwtSettings;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            _mapperMock = new Mock<IMapper>();

            _jwtSettings = new JwtSettings
            {
                Secret = "N8v#4kL!7xR$2pQ@5tZ6yM&hW1fU9bG*D0sE^cR!jK%lV?yH",
                ExpMinutes = 60,
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            };

            var options = Options.Create(_jwtSettings);

            _authService = new AuthService(_userRepoMock.Object, _passwordHasherMock.Object, options, _mapperMock.Object);
        }

        #region AuthenticateAsync - Casos felices

        [Theory]
        [InlineData("user1", "pass123", "user1@email.com", "Regular")]
        [InlineData("admin", "adminpass", "admin@email.com", "Admin")]
        public async Task AuthenticateAsync_ReturnsToken_WhenCredentialsAreValid(
            string username, string password, string email, string role)
        {
            var user = new User
            {
                Id = 1,
                Username = username,
                Email = email,
                Role = Enum.Parse<Role>(role),
                PasswordHash = "hashedpass"
            };

            var dto = new LoginDto(username, password);

            _userRepoMock.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.GetByEmailAsync(username)).ReturnsAsync((User?)null);
            _passwordHasherMock.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, password))
                               .Returns(PasswordVerificationResult.Success);
            _mapperMock.Setup(m => m.Map<UserDto>(user))
                       .Returns(new UserDto(user.Id, user.Username, user.Email, user.Role, DateTime.UtcNow));

            var result = await _authService.AuthenticateAsync(dto);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal(user.Username, result.User.Username);
            Assert.Equal(user.Email, result.User.Email);
            Assert.Equal(user.Role, result.User.Role);
        }

        #endregion

        #region AuthenticateAsync - Casos de error

        [Theory]
        [InlineData("unknownuser", "pass123")]
        [InlineData("nouser@email.com", "pass123")]
        public async Task AuthenticateAsync_ThrowsUnauthorized_WhenUserNotFound(string usernameOrEmail, string password)
        {
            _userRepoMock.Setup(r => r.GetByUsernameAsync(usernameOrEmail)).ReturnsAsync((User?)null);
            _userRepoMock.Setup(r => r.GetByEmailAsync(usernameOrEmail)).ReturnsAsync((User?)null);

            var dto = new LoginDto(usernameOrEmail, password);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.AuthenticateAsync(dto));
        }

        [Theory]
        [InlineData("user1", "wrongpass")]
        public async Task AuthenticateAsync_ThrowsUnauthorized_WhenPasswordIncorrect(string username, string password)
        {
            var user = new User
            {
                Id = 1,
                Username = username,
                Email = $"{username}@email.com",
                PasswordHash = "hashedpass"
            };

            _userRepoMock.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.GetByEmailAsync(username)).ReturnsAsync((User?)null);
            _passwordHasherMock.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, password))
                               .Returns(PasswordVerificationResult.Failed);

            var dto = new LoginDto(username, password);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.AuthenticateAsync(dto));
        }

        #endregion
    }
}
