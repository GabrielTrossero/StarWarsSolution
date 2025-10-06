using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoviesApp.Application.DTOs.User;

namespace MoviesApp.Application.DTOs.Auth
{
    public record AuthResponseDto(string Token, DateTime ExpiresAt, UserDto User);
}
