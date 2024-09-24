using ServerAPI.Models;
using System.IO;

namespace ServerAPI.Interfaces
{
    public interface IRandomHandler
    {
        int GetRandomAlbumId();
        int GetRandomPhotoId(int albumId);
        public Stream GetFirstPhoto(int albumId, PhotoSize size);
        public Stream GetPhoto(int photoId, PhotoSize size);
    }
}
