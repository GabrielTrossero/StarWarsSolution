using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.Auth
{
    public record RegisterDto(string Username, string Email, string Password, Role Role = Role.Regular);
}
