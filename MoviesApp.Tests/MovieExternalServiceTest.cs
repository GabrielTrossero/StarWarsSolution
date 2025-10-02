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

namespace MoviesApp.Tests
{
    public class MovieExternalServiceParamTests
    {
        private readonly Mock<IMovieRepository> _movieRepoMock;
        private readonly Mock<ILogger<MovieExternalService>> _loggerMock;

        public MovieExternalServiceParamTests()
        {
            _movieRepoMock = new Mock<IMovieRepository>();
            _loggerMock = new Mock<ILogger<MovieExternalService>>();
        }

        public static IEnumerable<object[]> GetSyncScenarios()
        {
            yield return new object[]
            {
                "NewMovies",
                new List<SwapiFilmResult>
                {
                    new SwapiFilmResult
                    {
                        Uid = "1",
                        Properties = new SwapiFilmProperties
                        {
                            Title = "A New Hope",
                            Director = "Lucas",
                            Producer="Lucasfilm",
                            Created="2020-01-01",
                            Edited="2020-01-02",
                            Release_date="1977-05-25",
                            Episode_id=4,
                            Url="url1"
                        }
                    }
                },
                new List<Movie>(),
                1,
                0
            };

            yield return new object[]
            {
                "AllExist",
                new List<SwapiFilmResult>
                {
                    new SwapiFilmResult
                    {
                        Uid = "1",
                        Properties = new SwapiFilmProperties
                        {
                            Title = "Existing Movie",
                            Director = "Dir",
                            Producer="Prod",
                            Created="2020-01-01",
                            Edited="2020-01-02",
                            Release_date="2000-01-01",
                            Episode_id=1,
                            Url="url1"
                        }
                    }
                },
                new List<Movie> { new Movie { ExternalId = "1", Title = "Existing Movie" } },
                0,
                1 
            };

            yield return new object[]
            {
                "EmptyApi",
                new List<SwapiFilmResult>(),
                new List<Movie>(),
                0,
                0
            };
        }

        private HttpClient CreateFakeHttpClient(SwapiFilmResponse response)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(response)
                });

            return new HttpClient(handlerMock.Object);
        }

        private MovieExternalService CreateService(HttpClient httpClient)
        {
            var httpFactoryMock = new Mock<IHttpClientFactory>();
            httpFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return new MovieExternalService(_loggerMock.Object, _movieRepoMock.Object, httpFactoryMock.Object);
        }

        [Theory]
        [MemberData(nameof(GetSyncScenarios))]
        public async Task SyncMoviesAsync_ParametrizedTests(
            string scenarioName,
            List<SwapiFilmResult> apiFilms,
            List<Movie> existingMovies, // database
            int expectedNew, // new movies
            int expectedExisting) //existing movies
        {
            // Arrange
            var response = new SwapiFilmResponse { Result = apiFilms };
            var httpClient = CreateFakeHttpClient(response);

            _movieRepoMock.Setup(r => r.GetByExternalIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(existingMovies);

            var service = CreateService(httpClient);

            // Act
            await service.SyncMoviesAsync();

            // Assert
            if (expectedNew > 0)
            {
                _movieRepoMock.Verify(r => r.AddRangeAsync(It.Is<List<Movie>>(l => l.Count == expectedNew)), Times.Once);
                _movieRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            }
            else
            {
                _movieRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<List<Movie>>()), Times.Never);
                _movieRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            }
        }
    }
}
