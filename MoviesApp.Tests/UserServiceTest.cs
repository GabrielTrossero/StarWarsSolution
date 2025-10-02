using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using MoviesApp.Application.DTOs;
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
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(_userRepoMock.Object, _passwordHasherMock.Object, _mapperMock.Object);
        }

        #region CreateAsync

        [Theory]
        [InlineData("user1", "user1@email.com", "Regular")]
        [InlineData("admin", "admin@email.com", "Admin")]
        [InlineData("userNoRole", "nrole@email.com", null)]
        [InlineData("userEmptyRole", "emptyrole@email.com", "")]
        public async Task CreateAsync_CreatesUser_WhenValid(string username, string email, string? role)
        {
            var dto = new RegisterDto(username, email, "password123", role);

            _userRepoMock.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync((User?)null);
            _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);
            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<User>(), dto.Password)).Returns("hashedpassword");
            _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
                .Returns((User u) => new UserDto(u.Id, u.Username, u.Email, u.Role, u.CreatedAt));

            var result = await _userService.CreateAsync(dto);

            Assert.Equal(username, result.Username);
            Assert.Equal(email, result.Email);
            if (string.IsNullOrWhiteSpace(role))
                Assert.Equal(Role.Regular, result.Role);
            else
                Assert.Equal(Enum.Parse<Role>(role, true), result.Role);

            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
            _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _passwordHasherMock.Verify(p => p.HashPassword(It.IsAny<User>(), dto.Password), Times.Once);
        }

        [Theory]
        [InlineData("existingUser", "new@email.com")]
        [InlineData("user2", "existing@email.com")]
        public async Task CreateAsync_ThrowsException_WhenUsernameOrEmailExists(string username, string email)
        {
            var dto = new RegisterDto(username, email, "pass123", "Regular");

            _userRepoMock.Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync(username == "existingUser" ? new User() : null);
            _userRepoMock.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync(email == "existing@email.com" ? new User() : null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateAsync(dto));
        }

        #endregion

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsMappedUsers()
        {
            var users = new List<User>
        {
            new User { Id = 1, Username = "user1", Email = "u1@email.com", Role = Role.Regular, CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Username = "user2", Email = "u2@email.com", Role = Role.Admin, CreatedAt = DateTime.UtcNow }
        };

            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(users))
                .Returns(users.Select(u => new UserDto(u.Id, u.Username, u.Email, u.Role, u.CreatedAt)));

            var result = await _userService.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.Username == "user1");
            Assert.Contains(result, u => u.Role == Role.Admin);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoUsers()
        {
            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            _mapperMock.Setup(m => m.Map<IEnumerable<UserDto>>(It.IsAny<List<User>>()))
                .Returns(new List<UserDto>());

            var result = await _userService.GetAllAsync();

            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync

        [Theory]
        [InlineData(1, "user1")]
        [InlineData(2, "user2")]
        public async Task GetByIdAsync_ReturnsUser_WhenExists(int id, string username)
        {
            var user = new User { Id = id, Username = username, Email = $"{username}@email.com", Role = Role.Regular, CreatedAt = DateTime.UtcNow };
            _userRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<UserDto>(user))
                .Returns(new UserDto(user.Id, user.Username, user.Email, user.Role, user.CreatedAt));

            var result = await _userService.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(10)]
        public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist(int id)
        {
            _userRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

            var result = await _userService.GetByIdAsync(id);

            Assert.Null(result);
        }

        #endregion
    }
}
