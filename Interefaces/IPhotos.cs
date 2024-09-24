using System.Collections.Generic;
using System.IO;
using ServerAPI.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Interfaces
{
    public interface IPhotos
    {
        void AddPhoto(int albumId, string caption, byte[] bytesOriginal);
        void DeletePhoto(int photoId);
        void UpdatePhoto(string caption, int photoId);
        string GetAlbumCaptionByPhotoId(int photoId);
        public List<Photo> GetPhotosByAlbumId(int albumId);
    }
}