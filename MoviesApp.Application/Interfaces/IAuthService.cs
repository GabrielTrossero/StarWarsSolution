using MoviesApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> AuthenticateAsync(LoginDto dto);
    }
}
