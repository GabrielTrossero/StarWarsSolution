using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs
{
    public record RegisterDto(string Username, string Email, string Password, string? Role);
    public record LoginDto(string UsernameOrEmail, string Password);
    public record UserDto(int Id, string Username, string Email, Role Role, DateTime CreatedAt);
    public record AuthResponseDto(string Token, DateTime ExpiresAt, UserDto User);
}
