using System.Collections.Generic;
using System.IO;
using ServerAPI.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Interfaces
{
    public interface IPhotoDetails
    {
        Photo GetPhoto(int photoId);
        List<Photo> GetPhotosByAlbumId(int albumId);
        int GetRandomAlbumId();
        int GetRandomPhotoId(int albumId);
    }
}