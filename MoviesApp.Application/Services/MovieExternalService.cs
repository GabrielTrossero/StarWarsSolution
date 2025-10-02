using Microsoft.Extensions.Logging;
using MoviesApp.Application.DTOs;
using MoviesApp.Application.Interfaces;
using MoviesApp.Domain.Entities;
using MoviesApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Application.Services
{
    public class MovieExternalService : IMovieExternalService
    {
        private readonly ILogger<MovieExternalService> _logger;
        private readonly IMovieRepository _movieRepository;
        private readonly HttpClient _httpClient;
        private static bool _isSyncing = false;

        public MovieExternalService(
            ILogger<MovieExternalService> logger,
            IMovieRepository movieRepository,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _movieRepository = movieRepository;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task SyncMoviesAsync()
        {
            if (_isSyncing) return;

            try
            {
                _isSyncing = true;
                _logger.LogInformation("Starting movie synchronization...");

                var films = await FetchExternalFilmsAsync();
                if (films == null || !films.Any())
                {
                    _logger.LogWarning("No movies retrieved from the external API.");
                    return;
                }

                var existingMovies = await GetExistingMoviesAsync(films.Select(f => f.Uid).ToList());
                var newMovies = MapNewMovies(films, existingMovies);

                await PersistNewMoviesAsync(newMovies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during movie synchronization");
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private async Task<List<SwapiFilmResult>?> FetchExternalFilmsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<SwapiFilmResponse>("https://www.swapi.tech/api/films");
            return response?.Result;
        }

        private async Task<Dictionary<string, Movie>> GetExistingMoviesAsync(List<string> externalIds)
        {
            var existing = await _movieRepository.GetByExternalIdsAsync(externalIds);
            return existing.ToDictionary(m => m.ExternalId);
        }

        private List<Movie> MapNewMovies(List<SwapiFilmResult> films, Dictionary<string, Movie> existingMovies)
        {
            var newMovies = new List<Movie>();

            foreach (var film in films)
            {
                if (!existingMovies.ContainsKey(film.Uid))
                {
                    var movie = new Movie
                    {
                        ExternalId = film.Uid,
                        Title = film.Properties.Title,
                        Director = film.Properties.Director,
                        Producer = film.Properties.Producer,
                        Created = DateTime.Parse(film.Properties.Created),
                        Edited = DateTime.Parse(film.Properties.Edited),
                        ReleaseDate = DateTime.Parse(film.Properties.Release_date),
                        EpisodeId = film.Properties.Episode_id,
                        Url = film.Properties.Url
                    };
                    newMovies.Add(movie);
                    _logger.LogInformation("New movie detected: {title} (ExternalId: {externalId})", movie.Title, movie.ExternalId);
                }
                else
                {
                    _logger.LogInformation("Existing movie found: {title} (ExternalId: {externalId})", existingMovies[film.Uid].Title, film.Uid);
                }
            }

            return newMovies;
        }

        private async Task PersistNewMoviesAsync(List<Movie> newMovies)
        {
            if (!newMovies.Any())
            {
                _logger.LogInformation("No new movies to add.");
                return;
            }

            await _movieRepository.AddRangeAsync(newMovies);
            await _movieRepository.SaveChangesAsync();
            _logger.LogInformation("{count} new movies added to the database.", newMovies.Count);
            _logger.LogInformation("Movie synchronization completed successfully.");
        }
    }
}
