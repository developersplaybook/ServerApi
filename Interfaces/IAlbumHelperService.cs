using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IAlbumHelperService
    {
        Task<List<AlbumViewModel>> GetAlbumsWithPhotoCountAsync();
    }
}
