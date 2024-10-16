using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerAPI.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumsController : ControllerBase
    {
        private readonly IAlbumsService _albumsService;
        private readonly ILogger<AlbumsController> _logger;

        public AlbumsController(IAlbumsService albums, ILogger<AlbumsController> logger)
        {
            _albumsService = albums;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get album by ID", Description = "Get a specific album by its ID")]
        public async Task<IActionResult> GetAlbumById(int id)
        {
            try
            {
                var albums = await _albumsService.GetAlbumsWithPhotoCountAsync();
                var album = albums.SingleOrDefault(a => a.AlbumID == id);

                return album != null ? Ok(album) : NotFound($"Album with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving album with ID {id}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all albums", Description = "Get all albums")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var albums = await _albumsService.GetAlbumsWithPhotoCountAsync().ConfigureAwait(false);
                return Ok(albums); // HTTP 200 OK with JSON payload
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving albums.");
                return StatusCode(500, "An error occurred while processing your request."); // HTTP 500 Internal Server Error
            }
        }

        [HttpPost("add")]
        [Authorize]
        [SwaggerOperation(Summary = "Add album", Description = "Add album")]
        public async Task<IActionResult> Add([FromBody] string caption)
        {
            var response = await _albumsService.AddAlbumAsync(caption);
            if (response.IsValid)
            {
                return Ok(response.Album);
            }
            else
            {
                return BadRequest(new { success = false, message = "Failed to create album" });
            }
        }

        [HttpPut("update/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Update album", Description = "Update album")]
        public async Task<IActionResult> Update(int id, [FromBody] string caption)
        {
            var response = await _albumsService.UpdateAlbumAsync(caption, id);
            if (response.IsValid)
            {
                return Ok(new { success = true, data = caption });
            }
            else
            {
                return BadRequest(new { success = false, message = "Failed to update album" });
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete album", Description = "Delete album")]
        public async Task<IActionResult> Delete(int id)
        {
            int response = await _albumsService.DeleteAlbumAsync(id);
            if (response > 0)
            {
                return Ok(new { success = true, message = "Album deleted successfully" });
            }
            else
            {
                return NotFound(new { success = false, message = "Album not found" });
            }
        }
    }
}
