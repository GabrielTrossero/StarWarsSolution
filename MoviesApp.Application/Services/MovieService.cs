using AutoMapper;
using MoviesApp.Application.Interfaces;
using MoviesApp.Domain.Entities;
using MoviesApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IMapper _mapper;

        public MovieService(IMovieRepository movieRepository, IMapper mapper)
        {
            _movieRepository = movieRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Movie>> GetAllMovies()
        {
            return await _movieRepository.GetAllAsync();
        }

        public async Task<Movie?> GetMovieById(int id)
        {
            return await _movieRepository.GetByIdAsync(id);
        }

        public async Task<Movie> CreateMovie(Movie movie)
        {
            movie.Created = DateTime.UtcNow;
            movie.Edited = DateTime.UtcNow;

            await _movieRepository.AddAsync(movie);
            return movie;
        }

        public async Task<Movie?> UpdateMovie(Movie movie)
        {
            var existing = await _movieRepository.GetByIdAsync(movie.Id);
            if (existing == null)
                return null;

            _mapper.Map(movie, existing);

            existing.Edited = DateTime.UtcNow;

            await _movieRepository.UpdateAsync(existing);
            return existing;
        }

        /*
        public async Task<Movie?> UpdateMovie(Movie movie)
        {
            var existing = await _movieRepository.GetByIdAsync(movie.Id);
            if (existing == null)
                return null;

            existing.Title = movie.Title;
            existing.ExternalId = movie.ExternalId;
            existing.EpisodeId = movie.EpisodeId;
            existing.Director = movie.Director;
            existing.Producer = movie.Producer;
            existing.ReleaseDate = movie.ReleaseDate;
            existing.OpeningCrawl = movie.OpeningCrawl;
            existing.Url = movie.Url;
            existing.Edited = DateTime.UtcNow;

            await _movieRepository.UpdateAsync(existing);
            return existing;
        }*/

        public async Task<bool> DeleteMovie(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
                return false;

            movie.DateTo = DateTime.UtcNow;
            await _movieRepository.UpdateAsync(movie);
            return true;
        }
    }
}
