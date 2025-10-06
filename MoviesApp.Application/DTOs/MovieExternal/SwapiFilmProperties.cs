using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.MovieExternal
{
    public class SwapiFilmProperties
    {
        public string Title { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Producer { get; set; } = string.Empty;
        public string Release_date { get; set; } = string.Empty;
        public string Created { get; set; } = string.Empty;
        public string Edited { get; set; } = string.Empty;
        public int Episode_id { get; set; }
        public string Opening_crawl { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
