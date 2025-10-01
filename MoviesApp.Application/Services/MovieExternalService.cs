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
            if (_isSyncing)
            {
                return;
            }

            try
            {
                _isSyncing = true;
                _logger.LogInformation("Iniciando sincronización de películas...");

                // Obtener listado desde la API externa
                var response = await _httpClient.GetFromJsonAsync<SwapiFilmResponse>("https://www.swapi.tech/api/films");

                if (response?.Result == null || !response.Result.Any())
                {
                    _logger.LogWarning("No se obtuvieron películas desde la API externa.");
                    return;
                }

                // Extraer los UIDs de todas las películas externas
                var externalIds = response.Result.Select(f => f.Uid).ToList();

                // Traer de una sola vez todas las películas existentes en BD
                var existingMoviesList = await _movieRepository.GetByExternalIdsAsync(externalIds);
                var existingMovies = existingMoviesList.ToDictionary(m => m.ExternalId);

                var newMovies = new List<Movie>();

                foreach (var film in response.Result)
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
                            Url = film.Properties.Url,
                        };
                        newMovies.Add(movie);
                        _logger.LogInformation("Nueva película detectada: {title}", movie.Title);
                    }
                    else
                    {
                        _logger.LogInformation("Película existente encontrada: {title}", existingMovies[film.Uid].Title);
                    }
                }

                if (newMovies.Any())
                {
                    await _movieRepository.AddRangeAsync(newMovies);
                    await _movieRepository.SaveChangesAsync();
                    _logger.LogInformation("{count} nuevas películas agregadas a la BD.", newMovies.Count);
                }
                else
                {
                    _logger.LogInformation("No hay nuevas películas para agregar.");
                }

                _logger.LogInformation("Sincronización completada con éxito.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la sincronización de películas");
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
