using Moq;
using MoviesApp.Application.Services;
using MoviesApp.Domain.Entities;
using MoviesApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Tests
{
    public class MovieServiceTest
    {
        private readonly Mock<IMovieRepository> _movieRepoMock;
        private readonly MovieService _movieService;

        public MovieServiceTest()
        {
            _movieRepoMock = new Mock<IMovieRepository>();
            _movieService = new MovieService(_movieRepoMock.Object);
        }

        [Fact]
        public async Task GetAllMovies_ReturnsAllMovies()
        {
            // Arrange
            var movies = new List<Movie>
            {
                new Movie { Id = 1, Title = "A New Hope" },
                new Movie { Id = 2, Title = "The Empire Strikes Back" }
            };
            _movieRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(movies);

            // Act
            var result = await _movieService.GetAllMovies();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, m => m.Title == "A New Hope");
            Assert.Contains(result, m => m.Title == "The Empire Strikes Back");
        }

        [Fact]
        public async Task GetAllMovies_ReturnsEmptyList_WhenNoMovies()
        {
            _movieRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Movie>());

            var result = await _movieService.GetAllMovies();

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(1, "A New Hope")]
        [InlineData(2, "The Empire Strikes Back")]
        public async Task GetMovieById_ReturnsMovie_WhenExists(int id, string expectedTitle)
        {
            var movie = new Movie { Id = id, Title = expectedTitle };
            _movieRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(movie);

            var result = await _movieService.GetMovieById(id);

            Assert.NotNull(result);
            Assert.Equal(expectedTitle, result.Title);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        public async Task GetMovieById_ReturnsNull_WhenNotExists(int id)
        {
            _movieRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Movie?)null);

            var result = await _movieService.GetMovieById(id);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("New Movie", "Director 1", 1)]
        [InlineData("Another Movie", "Director 2", 2)]
        public async Task CreateMovie_SetsCreatedAndEdited(string title, string director, int id)
        {
            var movie = new Movie { Id = id, Title = title, Director = director };
            _movieRepoMock.Setup(r => r.AddAsync(movie)).Returns(Task.CompletedTask);

            var result = await _movieService.CreateMovie(movie);

            Assert.Equal(title, result.Title);
            Assert.Equal(director, result.Director);
            Assert.True(result.Created <= DateTime.UtcNow);
            Assert.True(result.Edited <= DateTime.UtcNow);
            _movieRepoMock.Verify(r => r.AddAsync(movie), Times.Once);
        }

        [Theory]
        [InlineData(1, "Updated Movie")]
        [InlineData(2, "Another Update")]
        public async Task UpdateMovie_UpdatesFields_WhenMovieExists(int id, string newTitle)
        {
            var existing = new Movie
            {
                Id = id,
                Title = "Old Title",
                Director = "Old Director",
                EpisodeId = 1,
                Producer = "Old Producer",
                ReleaseDate = DateTime.UtcNow.AddYears(-1),
                ExternalId = "ext123",
                OpeningCrawl = "Old crawl",
                Url = "old.url"
            };

            var updatedMovie = new Movie
            {
                Id = id,
                Title = newTitle,
                Director = "New Director",
                EpisodeId = 2,
                Producer = "New Producer",
                ReleaseDate = DateTime.UtcNow,
                ExternalId = "ext456",
                OpeningCrawl = "New crawl",
                Url = "new.url"
            };

            _movieRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _movieRepoMock.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);

            var result = await _movieService.UpdateMovie(updatedMovie);

            Assert.NotNull(result);
            Assert.Equal(newTitle, result.Title);
            Assert.Equal("New Director", result.Director);
            Assert.Equal(2, result.EpisodeId);
            Assert.Equal("New Producer", result.Producer);
            Assert.Equal("ext456", result.ExternalId);
            Assert.Equal("New crawl", result.OpeningCrawl);
            Assert.Equal("new.url", result.Url);

            _movieRepoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task UpdateMovie_ReturnsNull_WhenMovieDoesNotExist(int id)
        {
            var movie = new Movie { Id = id, Title = "Test" };
            _movieRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Movie?)null);

            var result = await _movieService.UpdateMovie(movie);

            Assert.Null(result);
            _movieRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task DeleteMovie_SetsDateTo_WhenMovieExists(int id)
        {
            var movie = new Movie { Id = id };
            _movieRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(movie);
            _movieRepoMock.Setup(r => r.UpdateAsync(movie)).Returns(Task.CompletedTask);

            var result = await _movieService.DeleteMovie(id);

            Assert.True(result);
            Assert.NotNull(movie.DateTo);
            _movieRepoMock.Verify(r => r.UpdateAsync(movie), Times.Once);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        public async Task DeleteMovie_ReturnsFalse_WhenMovieDoesNotExist(int id)
        {
            _movieRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Movie?)null);

            var result = await _movieService.DeleteMovie(id);

            Assert.False(result);
            _movieRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
        }
    }
}
