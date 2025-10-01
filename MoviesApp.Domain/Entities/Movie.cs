using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Domain.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int EpisodeId { get; set; }
        public string OpeningCrawl { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Producer { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public DateTime? DateTo { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
