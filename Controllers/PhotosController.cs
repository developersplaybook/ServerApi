using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotosService _photosService;
        public PhotosController(IPhotosService photos)
        {
            _photosService = photos;
        }

        [HttpGet("album/{id}")]
        [SwaggerOperation(Summary = "Get all photos in album", Description = "Get all photos in album")]
        public async Task<ActionResult> GetPhotos(int id) // id = albumId
        {
            var allPhotosSlim = await _photosService.GetPhotoSlimByAlbumIdAsync(id);
            return Ok(allPhotosSlim);
        }


        [HttpPost("add")]
        [Authorize]
        [SwaggerOperation(Summary = "Add photo", Description = "Add photo")]
        public async Task<IActionResult> Add([FromForm] FormData formData)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    formData.Image.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    await _photosService.AddPhotoAsync(formData.AlbumId, formData.Caption, fileBytes);
                }

                return Ok(new { message = "Photo added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request.", details = ex.InnerException?.Message ?? ex.Message });
            }
        }


        [HttpPut("update/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Update photo", Description = "Update photo")]
        public async Task<ActionResult> Update(int id, [FromBody] string caption)
        {
            await _photosService.UpdatePhotoAsync(caption, id);
            return Ok(new { message = "Photo updated successfully." });
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete photo", Description = "Delete photo")]
        public async Task<IActionResult> Delete(int id)
        {
            await _photosService.DeletePhotoAsync(id);
            return Ok(new { message = "Photo deleted successfully." });
        }
    }

    public class FormData
    {
        public int AlbumId { get; set; }
        public string Caption { get; set; }
        public IFormFile Image { get; set; }
    }
}