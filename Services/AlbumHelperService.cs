using ServerAPI.Interfaces;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class AlbumHelperService: IAlbumHelperService
    {
        private readonly IAlbumsRepository _albumsRepository;
        private readonly IPhotosRepository _photosRepository;

        public AlbumHelperService(IAlbumsRepository albumsRepository, IPhotosRepository photosRepository)
        {
            _albumsRepository = albumsRepository;
            _photosRepository = photosRepository;
        }

        public async Task<List<AlbumViewModel>> GetAlbumsWithPhotoCountAsync()
        {
            // Fetch albums asynchronously
            var albums = await _albumsRepository.GetAllAlbumsAsync().ConfigureAwait(false);

            // Fetch photo counts per album asynchronously
            var photoCounts = await _photosRepository.GetPhotoCountsPerAlbumAsync().ConfigureAwait(false);

            // Construct the AlbumViewModel list with photo counts
            var albumViewModels = albums.Select(album => new AlbumViewModel
            {
                AlbumID = album.AlbumID,
                Caption = album.Caption,
                IsPublic = album.IsPublic,
                PhotoCount = photoCounts.ContainsKey(album.AlbumID) ? photoCounts[album.AlbumID] : 0
            }).ToList();

            return albumViewModels;
        }
    }
}
