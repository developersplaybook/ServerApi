using System.Collections.Generic;
using System.IO;
using ServerAPI.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Interfaces
{
    public interface IAlbums
    {
        Album GetAlbum(int albumId);
        AlbumViewModel AddAlbum(string caption);
        int DeleteAlbum(int albumId);
        int UpdateAlbum(string caption, int albumId);
        List<AlbumViewModel> GetAlbumsWithPhotoCount();
        Photo GetPhoto(int photoId);
        List<Photo> GetPhotosByAlbumId(int albumId);
    }
}