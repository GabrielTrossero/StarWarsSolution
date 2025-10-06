using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MoviesApp.Application.DTOs.Auth;
using MoviesApp.Application.DTOs.User;
using MoviesApp.Application.Interfaces;
using MoviesApp.Domain.Entities;
using MoviesApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepo, IPasswordHasher<User> passwordHasher, IMapper mapper)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<UserDto> CreateAsync(User user, string password)
        {
            var existingUser = await _userRepo.GetByUsernameAsync(user.Username);
            if (existingUser != null) throw new InvalidOperationException("Username already exists.");

            var existingEmail = await _userRepo.GetByEmailAsync(user.Email);
            if (existingEmail != null) throw new InvalidOperationException("Email already registered.");

            user.CreatedAt = DateTime.Now;
            user.IsActive = true;

            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();
            
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }
    }
}
