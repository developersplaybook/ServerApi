using ServerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IAlbumsRepository
    {
        Task<int> GetRandomAlbumIdWithPhotosAsync();
        Task<IEnumerable<Album>> GetAllAlbumsAsync();
        Task<Album> GetAlbumByIdAsync(int albumId);
        Task AddAlbumAsync(Album album);
        void UpdateAlbum(Album album);
        void DeleteAlbum(Album album);
        Task<int> SaveChangesAsync();

        // Transaction methods
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
