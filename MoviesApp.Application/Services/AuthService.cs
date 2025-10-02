using Microsoft.Extensions.Options;
using MoviesApp.Application.Interfaces;
using MoviesApp.Domain.Entities;
using MoviesApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using MoviesApp.Application.DTOs;
using AutoMapper;

namespace MoviesApp.Application.Services
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public int ExpMinutes { get; set; } = 60;
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;

        public AuthService(IUserRepository userRepo, IPasswordHasher<User> passwordHasher, IOptions<JwtSettings> jwtOpt, IMapper mapper)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _jwtSettings = jwtOpt.Value;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> AuthenticateAsync(LoginDto dto)
        {
            var user = await GetUserByUsernameOrEmailAsync(dto.UsernameOrEmail);
            ValidatePassword(user, dto.Password);

            var token = GenerateJwtToken(user);

            var userDto = _mapper.Map<UserDto>(user);
            return new AuthResponseDto(token.TokenString, token.Expires, userDto);
        }

        private async Task<User> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var user = await _userRepo.GetByUsernameAsync(usernameOrEmail)
                       ?? await _userRepo.GetByEmailAsync(usernameOrEmail);

            if (user == null) throw new UnauthorizedAccessException("Credenciales inválidas.");
            return user;
        }

        private void ValidatePassword(User user, string password)
        {
            var res = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (res == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        private (string TokenString, DateTime Expires) GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expires);
        }
    }
}
