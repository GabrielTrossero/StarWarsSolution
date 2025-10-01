using MoviesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.Interfaces
{
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> GetAllMovies();
        Task<Movie?> GetMovieById(int id);
        Task<Movie> CreateMovie(Movie movie);
        Task<Movie?> UpdateMovie(Movie movie);
        Task<bool> DeleteMovie(int id);
    }
}
