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

        public async Task<PhotoSlim> GetPhotoSlimAsync(int photoId)
        {
            return await _photosRepository.GetPhotoSlimByIdAsync(photoId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<PhotoSlim>> GetPhotoSlimByAlbumIdAsync(int albumId)
        {
            return await _photosRepository.GetPhotoSlimByAlbumIdAsync(albumId).ConfigureAwait(false);
        }

        public async Task<int> GetRandomAlbumIdAsync()
        {
            var all = await GetAlbumsWithPhotoCountAsync().ConfigureAwait(false);
            var albumsList = all.Where(a => a.PhotoCount > 0).ToList();
            if (albumsList.Count == 0) return 0;
            return albumsList[new Random().Next(albumsList.Count)].AlbumID;

        }

        public async Task<int> GetRandomPhotoIdAsync(int albumId)
        {
            var all = await GetPhotoSlimByAlbumIdAsync(albumId).ConfigureAwait(false);
            var photoList = all.ToList();
            if (photoList.Count == 0) return 0;
            return photoList[new Random().Next(photoList.Count)].PhotoID;
        }

        private async Task<List<AlbumViewModel>> GetAlbumsWithPhotoCountAsync()
        {
            return await _albumHelperService.GetAlbumsWithPhotoCountAsync().ConfigureAwait(false);  
        }
    }
}
