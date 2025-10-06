using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Application.DTOs;
using MoviesApp.Application.DTOs.MovieSync;
using MoviesApp.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace MoviesApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieExternalController : ControllerBase
    {
        private readonly IMovieExternalService _movieExternalService;

        public MovieExternalController(IMovieExternalService movieExternalService)
        {
            _movieExternalService = movieExternalService;
        }


        /*
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
        }*/

        /// <summary>
        /// Retrieves all external movies with their current status compared to the local database.
        /// </summary>
        [HttpGet("get-status")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Get movies sync status",
            Description = "Returns the status of all external movies relative to the local database.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSyncStatus()
        {
            var statusList = await _movieExternalService.GetSyncStatusAsync();
            return Ok(statusList);
        }

        /// <summary>
        /// Synchronizes movies based on selected sync statuses.
        /// </summary>
        [HttpPost("sync-by-status")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Sync movies by status",
            Description = "Sync movies by status. If a status is set to true, movies with that status will be updated based on information from the external API.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SyncByStatus([FromBody] MovieSyncRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var statusesToSync = new List<MovieSyncStatus>();
            if (request.NotAdded) statusesToSync.Add(MovieSyncStatus.NotAdded);
            if (request.Added) statusesToSync.Add(MovieSyncStatus.Added);
            if (request.Deleted) statusesToSync.Add(MovieSyncStatus.Deleted);
            if (request.UpdatedLocal) statusesToSync.Add(MovieSyncStatus.UpdatedLocal);
            if (request.UpdatedExternal) statusesToSync.Add(MovieSyncStatus.UpdatedExternal);

            if (!statusesToSync.Any())
                return BadRequest("At least one status must be true.");

            var movieList = await _movieExternalService.SyncMoviesByStatusAsync(statusesToSync);

            return Ok(movieList);
        }


        /// <summary>
        /// Forces the update of a specific movie from the external API.
        /// </summary>
        [HttpPost("force-update/{externalId}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Force update movie",
            Description = "Force updates a specific movie from the external API.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForceUpdate(string externalId)
        {
            if (string.IsNullOrEmpty(externalId))
                return BadRequest("ExternalId must be provided.");

            await _movieExternalService.ForceUpdateMovieAsync(externalId);
            return Ok($"Movie {externalId} was force-updated.");
        }
    }
}
