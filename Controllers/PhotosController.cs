using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.IO;
using System.Linq;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotos _photos;
        public PhotosController(IPhotos photos)
        {
            _photos = photos;
        }

        [HttpGet("album/{id}")]
        [SwaggerOperation(Summary = "Get all photos in album", Description = "Get all photos in album")]
        public ActionResult GetPhotos(int id) // id = albumId
        {
            var photos = _photos.GetPhotosByAlbumId(id).Select(o => new { o.PhotoID, o.AlbumID, o.Caption });
            return Ok(photos);
        }


        [HttpPost("add")]
        [Authorize]
        [SwaggerOperation(Summary = "Add photo", Description = "Add photo")]
        public ActionResult Add([FromForm] FormData formData)
        {
            using (var ms = new MemoryStream())
            {
                formData.Image.CopyTo(ms);
                var fileBytes = ms.ToArray();
                _photos.AddPhoto(formData.AlbumId, formData.Caption, fileBytes);
            }

            return Ok(new { message = "Photo added successfully." });

        }

        [HttpPut("update/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Update photo", Description = "Update photo")]
        public ActionResult Update(int id, [FromBody] string caption)
        {
            _photos.UpdatePhoto(caption, id);
            return Ok(new { message = "Photo updated successfully." });
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete photo", Description = "Delete photo")]
        public IActionResult Delete(int id)
        {
            _photos.DeletePhoto(id);
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