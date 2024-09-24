using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Interfaces;
using ServerAPI.Models;
using System.IO;

namespace ServerAPI.Controllers
{
    public class RandomHandlerController : Controller
    {
        private const string SessionRandomPhotoID = "RandomPhotoID";
        private const string SessionPhotoID = "PhotoID";
        private readonly IRandomHandler _randomHandler;

        public RandomHandlerController(IRandomHandler randomHandler)
        {
            _randomHandler = randomHandler;
        }


        // GET: /Images/
        public IActionResult Index(string arg1, string arg2)
        {
            PhotoSize size = arg2.Replace("Size=", "") switch
            {
                "S" => PhotoSize.Small,
                "M" => PhotoSize.Medium,
                "L" => PhotoSize.Large,
                _ => PhotoSize.Original,
            };

            string photoId = arg1.Replace("PhotoID=", "");

            if (photoId == "0")
            {
                photoId = _randomHandler.GetRandomPhotoId(_randomHandler.GetRandomAlbumId()).ToString();
                HttpContext.Session.SetString(SessionRandomPhotoID, photoId);
            }

            HttpContext.Session.SetString(SessionPhotoID, photoId);

            using var stream = new MemoryStream();
            if (arg1.StartsWith("PhotoID"))
            {
                var parsedPhotoId = int.Parse(photoId);
                _randomHandler.GetPhoto(parsedPhotoId, size).CopyTo(stream);
            }
            else if (arg1.StartsWith("AlbumID"))
            {
                var albumId = int.Parse(arg1.Replace("AlbumID=", ""));
                _randomHandler.GetFirstPhoto(albumId, size).CopyTo(stream);
            }

            return File(stream.ToArray(), "image/png");
        }

        public IActionResult Download(string arg1, string arg2)
        {
            var photoId = HttpContext.Session.GetString(SessionPhotoID);
            if (string.IsNullOrEmpty(photoId))
            {
                photoId = _randomHandler.GetRandomPhotoId(_randomHandler.GetRandomAlbumId()).ToString();
                HttpContext.Session.SetString(SessionPhotoID, photoId);
            }

            ViewData["PhotoID"] = photoId;
            ViewData["Size"] = "L";

            return View();
        }
    }
}
