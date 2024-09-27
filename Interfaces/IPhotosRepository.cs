using ServerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IPhotosRepository
    {
        Task<int> GetRandomPhotoIdAsync(int albumId);
        Task<Dictionary<int, int>> GetPhotoCountsPerAlbumAsync();
        Task<Photo> GetPhotoByIdAsync(int photoId);
        Task<PhotoSlim> GetPhotoSlimByIdAsync(int photoId);
        Task<IEnumerable<PhotoSlim>> GetPhotoSlimByAlbumIdAsync(int albumId);
        Task AddPhotoAsync(Photo photo);
        void DeletePhoto(int photoId);
        void UpdatePhoto(string caption, int photoId);
        Task<int> SaveChangesAsync();

        // Transaction methods
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
