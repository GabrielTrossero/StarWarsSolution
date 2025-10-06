using MoviesApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.MovieExternal
{
    public class SwapiFilmResult
    {
        public string Uid { get; set; } = string.Empty;
        public SwapiFilmProperties Properties { get; set; } = new();
    }
}
