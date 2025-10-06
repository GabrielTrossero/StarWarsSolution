using MoviesApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.MovieExternal
{
    public class SwapiFilmResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<SwapiFilmResult> Result { get; set; } = new();
    }
}
