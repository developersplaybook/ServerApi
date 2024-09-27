using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Interfaces;
using ServerAPI.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoDetailsController : ControllerBase
    {
	    private const string SessionRandomPhotoID = "RandomPhotoID";
        private readonly IPhotoDetailsService _photoDetailsService;
        public PhotoDetailsController(IPhotoDetailsService photoDetails)
        {
            _photoDetailsService = photoDetails;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get all photos by album id", Description = "Get all photos in an album")]
        public async Task<IActionResult> GetPhotosByAlbumId(int id)  //id=albumId
        {
            if (id == 0)
            {
                var photoList = new List<PhotoSlim>();
                var randomPhotoID = HttpContext.Session.GetValue<string>(SessionRandomPhotoID);
                if (randomPhotoID != null && int.TryParse(randomPhotoID, out int randomPhotoId))
                {
                    var tmpPhoto = await _photoDetailsService.GetPhotoSlimAsync(randomPhotoId);
                    var allPhotos = await _photoDetailsService.GetPhotoSlimByAlbumIdAsync(tmpPhoto.AlbumID);
                    return Ok(allPhotos);
                }
                else
                {
                    var randomAlbumId = await _photoDetailsService.GetRandomAlbumIdAsync().ConfigureAwait(false);
                    var tmpPhotoID = await _photoDetailsService.GetRandomPhotoIdAsync(randomAlbumId);
                    photoList.Add(await _photoDetailsService.GetPhotoSlimAsync(tmpPhotoID));
                }

                return Ok(photoList);
            }

            var photos = await _photoDetailsService.GetPhotoSlimByAlbumIdAsync(id);
            return Ok(photos);
        }

        [HttpGet("savedphotoid")]
        [SwaggerOperation(Summary = "Get the saved photo id", Description = "Get the saved photo random photo")]
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