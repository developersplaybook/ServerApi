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
        [SwaggerOperation(Summary = "Get photo by id", Description = "Get photo")]
        public async Task<IActionResult> GetPhotoById(int id)
        {
            var photo = await _photoDetailsService.GetPhotoViewModelByIdAsync(id);
            return Ok(photo);
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