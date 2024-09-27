using ServerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IPhotosService
    {
        Task AddPhotoAsync(int albumId, string caption, byte[] bytesOriginal);
        Task DeletePhotoAsync(int photoId);
        Task UpdatePhotoAsync(string caption, int photoId);
        Task<IEnumerable<PhotoSlim>> GetPhotoSlimByAlbumIdAsync(int albumId);
    }
}