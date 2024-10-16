using ServerAPI.Interfaces;
using ServerAPI.Models;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class PhotoDetailsService : IPhotoDetailsService
    {
        private readonly IPhotosRepository _photosRepository;
        private readonly IAlbumsRepository _albumsRepository;
        private readonly IAlbumHelperService _albumHelperService;
        public PhotoDetailsService(IPhotosRepository photosRepository, IAlbumsRepository albumsRepository, IAlbumHelperService albumHelperService)
        {
            _photosRepository = photosRepository;
            _albumsRepository = albumsRepository;
            _albumHelperService = albumHelperService;
        }

        public async Task<PhotoViewModel> GetPhotoViewModelByIdAsync(int photoId)
        {
            var photo = await _photosRepository.GetPhotoSlimByIdAsync(photoId).ConfigureAwait(false);
            if (photo == null)
                return null;

            var album = await _albumsRepository.GetAlbumByIdAsync(photo.AlbumID).ConfigureAwait(false);

            return new PhotoViewModel
            {
                AlbumCaption = album?.Caption ?? string.Empty,
                AlbumID = photo.AlbumID,
                Caption = photo.Caption,
                PhotoID = photo.PhotoID
            };
        }
    }
}
