using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Application.Interfaces;
using MoviesApp.Domain.Entities;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _movieService.GetAllMovies();
            return Ok(movies);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Regular")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _movieService.GetMovieById(id);
            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Movie movie)
        {
            var created = await _movieService.CreateMovie(movie);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Movie movie)
        {
            if (id != movie.Id)
                return BadRequest("The ID does not match.");

            var updated = await _movieService.UpdateMovie(movie);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _movieService.DeleteMovie(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPost("sync")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Sync()
        {
            await _movieExternalService.SyncMoviesAsync();
            return Ok("Synchronization executed.");
        }
    }
}
