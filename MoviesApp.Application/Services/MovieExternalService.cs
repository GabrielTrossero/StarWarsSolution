using AutoMapper;
using Microsoft.Extensions.Logging;
using MoviesApp.Application.DTOs;
using MoviesApp.Application.DTOs.MovieExternal;
using MoviesApp.Application.DTOs.MovieSync;
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
        private readonly IMapper _mapper;

        public MovieExternalService(
            ILogger<MovieExternalService> logger,
            IMovieRepository movieRepository,
            IHttpClientFactory httpClientFactory,
            IMapper mapper)
        {
            _logger = logger;
            _movieRepository = movieRepository;
            _httpClient = httpClientFactory.CreateClient();
            _mapper = mapper;
        }

        public async Task<List<MovieSyncResult>> GetSyncStatusAsync()
        {
            return await CalculateSyncResultsAsync();
        }

        public async Task<List<Movie>> SyncMoviesByStatusAsync(IEnumerable<MovieSyncStatus> statusesToSync)
        {
            var syncedMovies = new List<Movie>();

            if (_isSyncing) return syncedMovies;

            try
            {
                _isSyncing = true;
                _logger.LogInformation("Starting configurable movie synchronization...");

                var results = await CalculateSyncResultsAsync();

                var newMovies = new List<Movie>();
                var restoredMovies = new List<Movie>();
                var updatedExternal = new List<Movie>();
                var updatedLocal = new List<Movie>();

                foreach (var result in results.Where(r => statusesToSync.Contains(r.Status)))
                {
                    switch (result.Status)
                    {
                        case MovieSyncStatus.NotAdded:
                            newMovies.Add(result.ExternalMovie);
                            break;

                        case MovieSyncStatus.Deleted:
                            result.LocalMovie.DateTo = null;
                            restoredMovies.Add(result.LocalMovie);
                            break;

                        case MovieSyncStatus.UpdatedExternal:
                            _mapper.Map(result.ExternalMovie, result.LocalMovie);
                            result.LocalMovie.Created = result.ExternalMovie.Created;
                            result.LocalMovie.Edited = result.ExternalMovie.Edited;
                            result.LocalMovie.DateTo = null;
                            updatedExternal.Add(result.LocalMovie);
                            break;

                        case MovieSyncStatus.UpdatedLocal:
                            // Forzar actualización desde la API externa
                            _mapper.Map(result.ExternalMovie, result.LocalMovie);
                            result.LocalMovie.Created = result.ExternalMovie.Created;
                            result.LocalMovie.Edited = result.ExternalMovie.Edited;
                            result.LocalMovie.DateTo = null;
                            updatedLocal.Add(result.LocalMovie);
                            _logger.LogInformation("Movie {id} forced update from external source.", result.LocalMovie.Id);
                            break;
                    }
                }

                await PersistNewMoviesAsync(newMovies);
                await PersistRestoredMoviesAsync(restoredMovies);
                await PersistUpdatedExternalMoviesAsync(updatedExternal);
                await PersistUpdatedLocalMoviesAsync(updatedLocal);

                syncedMovies.AddRange(newMovies);
                syncedMovies.AddRange(restoredMovies);
                syncedMovies.AddRange(updatedExternal);
                syncedMovies.AddRange(updatedLocal);

                _logger.LogInformation("Synchronization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during movie synchronization");
            }
            finally
            {
                _isSyncing = false;
            }

            return syncedMovies;
        }

        public async Task ForceUpdateMovieAsync(string externalId)
        {
            var films = await FetchExternalFilmsAsync();
            var film = films?.FirstOrDefault(f => f.Uid == externalId);

            if (film == null)
            {
                _logger.LogWarning("External movie {externalId} not found.", externalId);
                return;
            }

            var existing = await _movieRepository.GetByExternalIdAsync(externalId);
            if (existing == null)
            {
                var movie = _mapper.Map<Movie>(film);
                await _movieRepository.AddAsync(movie);
            }
            else
            {
                _mapper.Map(film, existing);
                await _movieRepository.UpdateAsync(existing);
            }

            _logger.LogInformation("Movie {externalId} was force-updated.", externalId);
        }

        private async Task<List<MovieSyncResult>> CalculateSyncResultsAsync()
        {
            var results = new List<MovieSyncResult>();

            var films = await FetchExternalFilmsAsync();
            if (films == null || !films.Any())
                return results;

            var externalIds = films.Select(f => f.Uid).ToList();
            var existingMovies = await _movieRepository.GetByExternalIdsAsync(externalIds);
            var existingDict = existingMovies.ToDictionary(m => m.ExternalId);

            foreach (var film in films)
            {
                // Mapea SwapiFilmResult a Movie **una sola vez**
                var externalMovie = _mapper.Map<Movie>(film);

                existingDict.TryGetValue(film.Uid, out var localMovie);

                if (localMovie == null)
                {
                    results.Add(new MovieSyncResult
                    {
                        Status = MovieSyncStatus.NotAdded,
                        ExternalMovie = externalMovie
                    });
                }
                else if (localMovie.DateTo != null)
                {
                    results.Add(new MovieSyncResult
                    {
                        Status = MovieSyncStatus.Deleted,
                        LocalMovie = localMovie,
                        ExternalMovie = externalMovie
                    });
                }
                else if (localMovie.Edited > externalMovie.Edited)
                {
                    results.Add(new MovieSyncResult
                    {
                        Status = MovieSyncStatus.UpdatedLocal,
                        LocalMovie = localMovie,
                        ExternalMovie = externalMovie
                    });
                }
                else if (localMovie.Edited < externalMovie.Edited)
                {
                    results.Add(new MovieSyncResult
                    {
                        Status = MovieSyncStatus.UpdatedExternal,
                        LocalMovie = localMovie,
                        ExternalMovie = externalMovie
                    });
                }
                else if (localMovie.Edited == externalMovie.Edited)
                {
                    results.Add(new MovieSyncResult
                    {
                        Status = MovieSyncStatus.Added,
                        LocalMovie = localMovie,
                        ExternalMovie = externalMovie
                    });
                }
            }

            return results;
        }

        private async Task<List<SwapiFilmResult>?> FetchExternalFilmsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<SwapiFilmResponse>("https://www.swapi.tech/api/films");
            return response?.Result;
        }

        private async Task PersistNewMoviesAsync(List<Movie> newMovies) =>
            await PersistMoviesAsync(newMovies, _movieRepository.AddRangeAsync, "{count} new movies added.");

        private async Task PersistRestoredMoviesAsync(List<Movie> restoredMovies) =>
            await PersistMoviesAsync(restoredMovies, m => { _movieRepository.UpdateRange(m); return Task.CompletedTask; }, "{count} deleted movies restored.");

        private async Task PersistUpdatedExternalMoviesAsync(List<Movie> updatedMovies) =>
            await PersistMoviesAsync(updatedMovies, m => { _movieRepository.UpdateRange(m); return Task.CompletedTask; }, "{count} movies updated from external changes.");

        private async Task PersistUpdatedLocalMoviesAsync(List<Movie> updatedMovies) =>
            await PersistMoviesAsync(updatedMovies, m => { _movieRepository.UpdateRange(m); return Task.CompletedTask; }, "{count} movies updated from local override by external data.");


        private async Task PersistMoviesAsync(
            IEnumerable<Movie> movies,
            Func<IEnumerable<Movie>, Task> persistAction,
            string logMessage)
        {
            if (movies == null || !movies.Any())
                return;

            await persistAction(movies);
            await _movieRepository.SaveChangesAsync();
            _logger.LogInformation(logMessage, movies.Count());
        }
    }
}
