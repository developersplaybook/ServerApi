using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Interfaces;
using ServerAPI.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoDetailsController : ControllerBase
    {
        private const string SessionRandomPhotoID = "RandomPhotoID";
        private readonly IPhotoDetails _photoDetails;
        public PhotoDetailsController(IPhotoDetails photoDetails)
        {
            _photoDetails = photoDetails;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get all photos by album id", Description = "Get all photos in an album")]
        public IActionResult GetPhotosByAlbumId(int id)  //id=albumId
        {
            if (id == 0)
            {
                var photoList = new List<Photo>();
                var randomPhotoID = HttpContext.Session.GetValue<string>(SessionRandomPhotoID);
                if (randomPhotoID != null && int.TryParse(randomPhotoID, out int randomPhotoId))
                {
                    var tmpAlbumId = _photoDetails.GetPhoto(randomPhotoId).AlbumID;
                    return Ok(_photoDetails.GetPhotosByAlbumId(tmpAlbumId).Select(o => new { o.PhotoID, o.AlbumID, o.Caption }));
                }
                else
                {
                    var tmpPhotoID = _photoDetails.GetRandomPhotoId(_photoDetails.GetRandomAlbumId());
                    photoList.Add(_photoDetails.GetPhoto(tmpPhotoID));
                }
                return Ok(photoList.Select(o => new { o.PhotoID, o.AlbumID, o.Caption }));
            }

            return Ok(_photoDetails.GetPhotosByAlbumId(id).Select(o => new { o.PhotoID, o.AlbumID, o.Caption }));
        }


        [HttpGet("savedphotoid")]
        [SwaggerOperation(Summary = "Get the saved photo id", Description = "Get a saved random photo")]
        public IActionResult GetSavedRandomPhotoID()
        {
            var randomPhotoID = HttpContext.Session.GetValue<string>(SessionRandomPhotoID);
            if (randomPhotoID != null && int.TryParse(randomPhotoID, out int idd))
            {
                return Ok(idd);
            }
            else
            {
                return Ok(0);
            }
        }
    }
}