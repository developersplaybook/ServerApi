using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerAPI.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumsController : ControllerBase
    {
        private readonly IAlbums _albums;
        private readonly ILogger<AlbumsController> _logger;

        public AlbumsController(IAlbums albums, ILogger<AlbumsController> logger)
        {
            _albums = albums;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all albums", Description = "Get all albums")]
        public IActionResult Index()
        {
            try
            {
                var albums = _albums.GetAlbumsWithPhotoCount();
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
        public IActionResult Add([FromBody] string caption)
        {
            var albumViewModel = _albums.AddAlbum(caption);
            if (albumViewModel != null)
            {
                return Ok(albumViewModel);
            }
            else
            {
                return BadRequest(new { success = false, message = "Failed to create album" });
            }
        }

        [HttpPut("update/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Update album", Description = "Update album")]
        public IActionResult Update(int id, [FromBody] string caption)
        {
            var response = _albums.UpdateAlbum(caption, id);
            if (response > 0)
            {
                return Ok(new { success = true, data = caption });
            }
            else
            {
                return BadRequest(new { success = false, message = "Failed to create album" });
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete album", Description = "Delete album")]
        public IActionResult Delete(int id)
        {
            int response = _albums.DeleteAlbum(id);
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
