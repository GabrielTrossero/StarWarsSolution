using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.User
{
    public record UserDto(int Id, string Username, string Email, Role Role, DateTime CreatedAt);
}