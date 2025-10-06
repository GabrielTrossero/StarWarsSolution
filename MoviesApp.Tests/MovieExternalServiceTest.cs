using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using MoviesApp.Application.DTOs;
using MoviesApp.Application.Services;
using MoviesApp.Domain.Entities;
using MoviesApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MoviesApp.Application.DTOs.MovieExternal;
using AutoMapper;
using MoviesApp.Application.DTOs.MovieSync;

namespace MoviesApp.Tests
{
    public class MovieExternalServiceTests
    {
        private readonly Mock<IMovieRepository> _movieRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<MovieExternalService>> _loggerMock = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

        private HttpClient CreateMockHttpClient(object response)
        {
            var messageHandler = new Mock<HttpMessageHandler>();
            messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(response)
                });

            return new HttpClient(messageHandler.Object);
        }

        private MovieExternalService CreateService(object? apiResponse = null)
        {
            var httpClient = apiResponse != null ? CreateMockHttpClient(apiResponse) : new HttpClient();
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return new MovieExternalService(
                _loggerMock.Object,
                _movieRepoMock.Object,
                _httpClientFactoryMock.Object,
                _mapperMock.Object
            );
        }

        private static SwapiFilmResponse CreateSampleSwapiResponse() => new()
        {
            Message = "ok",
            Result = new List<SwapiFilmResult>
        {
            new()
            {
                Uid = "1",
                Properties = new SwapiFilmProperties
                {
                    Title = "A New Hope",
                    Director = "George Lucas",
                    Created = "2024-01-01T00:00:00Z",
                    Edited = "2024-01-02T00:00:00Z",
                    Episode_id = 4
                }
            }
        }
        };

        [Fact(DisplayName = "GetSyncStatusAsync - Returns empty list if external API does not respond")]
        public async Task GetSyncStatusAsync_ReturnsEmpty_WhenApiFails()
        {
            // Arrange
            var service = CreateService(new SwapiFilmResponse { Result = new List<SwapiFilmResult>() });

            // Act
            var result = await service.GetSyncStatusAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact(DisplayName = "SyncMoviesByStatusAsync - Does not sync if already in process")]
        public async Task SyncMoviesByStatusAsync_ReturnsEmpty_IfAlreadySyncing()
        {
            // Arrange
            var service = CreateService(CreateSampleSwapiResponse());
            typeof(MovieExternalService)
                .GetField("_isSyncing", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(null, true);

            // Act
            var result = await service.SyncMoviesByStatusAsync(new[] { MovieSyncStatus.NotAdded });

            // Assert
            Assert.Empty(result);
            typeof(MovieExternalService)
                .GetField("_isSyncing", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(null, false);
        }

        [Fact(DisplayName = "ForceUpdateMovieAsync - Add new movie if it doesn't exist")]
        public async Task ForceUpdateMovieAsync_AddsNewMovie_IfNotExists()
        {
            // Arrange
            var response = CreateSampleSwapiResponse();
            var film = response.Result.First();

            _movieRepoMock.Setup(r => r.GetByExternalIdAsync(film.Uid))
                          .ReturnsAsync((Movie?)null);

            _mapperMock.Setup(m => m.Map<Movie>(It.IsAny<SwapiFilmResult>()))
                       .Returns(new Movie { ExternalId = film.Uid, Title = film.Properties.Title });

            var service = CreateService(response);

            // Act
            await service.ForceUpdateMovieAsync(film.Uid);

            // Assert
            _movieRepoMock.Verify(r => r.AddAsync(It.Is<Movie>(m => m.ExternalId == film.Uid)), Times.Once);
        }

        [Fact(DisplayName = "ForceUpdateMovieAsync - Update existing movie")]
        public async Task ForceUpdateMovieAsync_UpdatesMovie_IfExists()
        {
            // Arrange
            var response = CreateSampleSwapiResponse();
            var film = response.Result.First();

            var existing = new Movie { ExternalId = film.Uid };

            _movieRepoMock.Setup(r => r.GetByExternalIdAsync(film.Uid))
                          .ReturnsAsync(existing);

            _mapperMock.Setup(m => m.Map(film, existing));

            var service = CreateService(response);

            // Act
            await service.ForceUpdateMovieAsync(film.Uid);

            // Assert
            _movieRepoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Theory(DisplayName = "CalculateSyncResultsAsync - Returns correct status according to differences")]
        [InlineData(1, "2024-01-03T00:00:00Z", "2024-01-02T00:00:00Z", null, MovieSyncStatus.UpdatedLocal)]
        [InlineData(1, "2024-01-01T00:00:00Z", "2024-01-02T00:00:00Z", null, MovieSyncStatus.UpdatedExternal)]
        [InlineData(1, "2024-01-02T00:00:00Z", "2024-01-02T00:00:00Z", null, MovieSyncStatus.Added)]
        [InlineData(0, "2024-01-02T00:00:00Z", "2024-01-02T00:00:00Z", null, MovieSyncStatus.NotAdded)]
        [InlineData(1, "2024-01-02T00:00:00Z", "2024-01-02T00:00:00Z", "2024-01-02T00:00:00Z", MovieSyncStatus.Deleted)]
        public async Task CalculateSyncResultsAsync_ReturnsExpectedStatus(int idLocal, string localEdited, string externalEdited, string dateTo, MovieSyncStatus expected)
        {
            // Arrange
            var response = CreateSampleSwapiResponse();
            var film = response.Result.First();

            var localMovie = new Movie
            {
                Id = idLocal,
                ExternalId = film.Uid,
                Edited = DateTime.Parse(localEdited),
                DateTo = string.IsNullOrEmpty(dateTo) ? (DateTime?)null : DateTime.Parse(dateTo)
            };

            var localMovies = idLocal == 0
                ? new List<Movie>()  // No hay películas locales
                : new List<Movie> { localMovie };

            _movieRepoMock.Setup(r => r.GetByExternalIdsAsync(It.IsAny<IEnumerable<string>>()))
                          .ReturnsAsync(localMovies);

            _mapperMock.Setup(m => m.Map<Movie>(It.IsAny<SwapiFilmResult>()))
                       .Returns(new Movie
                       {
                           ExternalId = film.Uid,
                           Edited = DateTime.Parse(externalEdited)
                       });

            var service = CreateService(response);

            // Act
            var result = await service.GetSyncStatusAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(expected, result.First().Status);
        }

        [Fact(DisplayName = "ForceUpdateMovieAsync - Log warning if no external movie found")]
        public async Task ForceUpdateMovieAsync_LogsWarning_WhenFilmNotFound()
        {
            // Arrange
            var response = new SwapiFilmResponse { Result = new List<SwapiFilmResult>() };

            var service = CreateService(response);

            // Act
            await service.ForceUpdateMovieAsync("999");

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

}
