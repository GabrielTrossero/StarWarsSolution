using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.Auth
{
    public record LoginDto(string UsernameOrEmail, string Password);
}
