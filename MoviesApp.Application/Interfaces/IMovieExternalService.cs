using MoviesApp.Application.DTOs;
using MoviesApp.Application.DTOs.MovieSync;
using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.Interfaces
{
    public interface IMovieExternalService
    {
        Task<List<MovieSyncResult>> GetSyncStatusAsync();
        Task<List<Movie>> SyncMoviesByStatusAsync(IEnumerable<MovieSyncStatus> statusesToSync);
        Task<Movie> ForceUpdateMovieAsync(string externalId);
    }
}
