using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Application.Interfaces;
using MoviesApp.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace MoviesApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IMovieExternalService _movieExternalService;

        public MovieController(IMovieService movieService, IMovieExternalService movieExternalService)
        {
            _movieService = movieService;
            _movieExternalService = movieExternalService;
        }

        /// <summary>
        /// Retrieves all movies.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "List all movies", Description = "Returns the complete list of movies.")]
        [ProducesResponseType(typeof(IEnumerable<Movie>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _movieService.GetAllMovies();
            return Ok(movies);
        }

        /// <summary>
        /// Retrieves a movie by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Regular")]
        [SwaggerOperation(Summary = "Get movie by ID", Description = "Accessible only to users with the Regular role.")]
        [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _movieService.GetMovieById(id);
            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        /// <summary>
        /// Creates a new movie.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Create a movie", Description = "Accessible only to users with the Admin role.")]
        [ProducesResponseType(typeof(Movie), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(Movie movie)
        {
            var created = await _movieService.CreateMovie(movie);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing movie.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Update a movie", Description = "Accessible only to users with the Admin role.")]
        [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, Movie movie)
        {
            if (id != movie.Id)
                return BadRequest("The ID does not match.");

            var updated = await _movieService.UpdateMovie(movie);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        /// <summary>
        /// Deletes a movie.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Delete a movie", Description = "Accessible only to users with the Admin role.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _movieService.DeleteMovie(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Synchronizes movies with an external service.
        /// </summary>
        [HttpPost("sync")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Synchronize movies", Description = "Calls the external service to synchronize movies. Accessible only to Admin.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Sync()
        {
            await _movieExternalService.SyncMoviesAsync();
            return Ok("Synchronization executed.");
        }
    }
}
