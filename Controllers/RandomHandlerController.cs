using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Interfaces;
using ServerAPI.Models;
using System.IO;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    public class RandomHandlerController : Controller
    {
        private const string SessionRandomPhotoID = "RandomPhotoID";

        private readonly IRandomHandlerService _randomHandler;

        public RandomHandlerController(IRandomHandlerService randomHandler)
        {
            _randomHandler = randomHandler;
        }

        // GET: /Images/
        public async Task<IActionResult> Index(string arg1, string arg2)
        {
            PhotoSize size = arg2.Replace("Size=", "") switch
            {
                "S" => PhotoSize.Small,
                "M" => PhotoSize.Medium,
                "L" => PhotoSize.Large,
                _ => PhotoSize.Original,
            };

            using var stream = new MemoryStream();

            if (arg1.StartsWith("PhotoID"))
            {
                await ProcessPhotoIdRequest(arg1, size, stream);
            }
            else if (arg1.StartsWith("AlbumID"))
            {
                await ProcessAlbumIdRequest(arg1, size, stream);
            }

            return File(stream.ToArray(), "image/png");
        }

        private async Task ProcessPhotoIdRequest(string arg1, PhotoSize size, MemoryStream stream)
        {
            int photoId;

            if (arg1 == "PhotoID=0")
            {
                var randomAlbumId = await _randomHandler.GetRandomAlbumIdAsync().ConfigureAwait(false);
                photoId = await _randomHandler.GetRandomPhotoIdAsync(randomAlbumId);
                HttpContext.Session.SetValue(HttpContext, SessionRandomPhotoID, photoId.ToString());
            }
            else
            {
                photoId = int.Parse(arg1.Replace("PhotoID=", ""));
            }

            var result = await _randomHandler.GetPhotoAsync(photoId, size);
            result.CopyTo(stream);
        }

        private async Task ProcessAlbumIdRequest(string arg1, PhotoSize size, MemoryStream stream)
        {
            var albumId = int.Parse(arg1.Replace("AlbumID=", ""));
            var result = await _randomHandler.GetFirstPhotoAsync(albumId, size);
            result.CopyTo(stream);
        }


        public IActionResult Download(string arg1, string arg2)
        {
            string photoId;
            if (arg1 == "0")
            {
                photoId = HttpContext.Session.GetValue<string>(HttpContext, SessionRandomPhotoID);
            }
            else
            {
                photoId = arg1;
            }

            ViewData["PhotoID"] = photoId;
            ViewData["Size"] = "L";

            return View();
        }
    }
}
