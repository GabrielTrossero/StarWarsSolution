using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Application.DTOs;
using MoviesApp.Application.DTOs.Movie;
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
        private readonly IMapper _mapper;

        public MovieController(IMovieService movieService, IMapper mapper)
        {
            _movieService = movieService;
            _mapper = mapper;
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
        [Authorize(Roles = "Regular,Admin")]
        [SwaggerOperation(Summary = "Get movie by ID", Description = "Accessible to users with Regular or Admin roles.")]
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
        public async Task<IActionResult> Create(MovieRequestDto movieDto)
        {
            var movie = _mapper.Map<Movie>(movieDto);
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
        public async Task<IActionResult> Update(int id, MovieRequestDto movieDto)
        {
            var movie = _mapper.Map<Movie>(movieDto);
            movie.Id = id;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _movieService.DeleteMovie(id);
            if (!deleted)
                return NotFound();

            return Ok("Movie successfully deleted.");
        }
    }
}
