using MoviesApp.Application.DTOs;
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
        Task<UserDto> CreateAsync(RegisterDto dto);
    }
}
