using ServerAPI.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IPhotoDetailsService
    {
        Task<PhotoViewModel> GetPhotoViewModelByIdAsync(int photoId);
    }
}