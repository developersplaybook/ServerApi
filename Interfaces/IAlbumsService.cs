using ServerAPI.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IAlbumsService
    {
        Task<AlbumValidationResult> AddAlbumAsync(string caption);
        Task<int> DeleteAlbumAsync(int albumId);
        Task<AlbumValidationResult> UpdateAlbumAsync(string caption, int albumId);
        Task<List<AlbumViewModel>> GetAlbumsWithPhotoCountAsync();
    }
}