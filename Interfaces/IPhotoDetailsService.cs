using ServerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IPhotoDetailsService
    {
        Task<PhotoSlim> GetPhotoSlimAsync(int photoId);
        Task<IEnumerable<PhotoSlim>> GetPhotoSlimByAlbumIdAsync(int albumId);
        Task<int> GetRandomAlbumIdAsync();
        Task<int> GetRandomPhotoIdAsync(int albumId);
    }
}