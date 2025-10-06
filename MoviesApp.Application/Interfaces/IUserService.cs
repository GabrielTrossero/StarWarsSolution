using MoviesApp.Application.DTOs.Auth;
using MoviesApp.Application.DTOs.User;
using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateAsync(User dto, string password);
    }
}
