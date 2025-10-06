using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.DTOs.Movie
{
    public record MovieRequestDto(
        string Title,
        int EpisodeId,
        string OpeningCrawl,
        string Director,
        string Producer,
        DateTime ReleaseDate,
        string Url
    );
}
