using ServerAPI.Models;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IAlbumValidator
    {
        Task<AlbumValidationResult> ValidateAlbumCaptionAsync(string caption, int albumId = 0);
    }
}
