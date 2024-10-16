using ServerAPI.Interfaces;
using ServerAPI.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class AlbumsService : IAlbumsService
    {
        private readonly IAlbumsRepository _albumsRepository;
        private readonly IPhotosRepository _photosRepository;
        private readonly IAlbumValidator _albumValidator;
        private readonly IAlbumHelperService _albumHelperService;


        public AlbumsService(IAlbumsRepository albumsRepository, IPhotosRepository photosRepository, IAlbumValidator albumValidator, IAlbumHelperService albumHelperService)
        {
            _albumsRepository = albumsRepository;
            _photosRepository = photosRepository;
            _albumValidator = albumValidator;
            _albumHelperService = albumHelperService;
        }

        public async Task<int> DeleteAlbumAsync(int albumId)
        {
            await _albumsRepository.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var album = await _albumsRepository.GetAlbumByIdAsync(albumId).ConfigureAwait(false);
                if (album == null)
                {
                    throw new KeyNotFoundException("Album not found.");
                }

                _albumsRepository.DeleteAlbum(album);
                int result = await _albumsRepository.SaveChangesAsync().ConfigureAwait(false);
                await _albumsRepository.CommitTransactionAsync().ConfigureAwait(false);
                return result;
            }
            catch
            {
                await _albumsRepository.RollbackTransactionAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<AlbumValidationResult> AddAlbumAsync(string caption)
        {
            var validationResult = await _albumValidator.ValidateAlbumCaptionAsync(caption);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            await _albumsRepository.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var album = new Album
                {
                    Caption = caption,
                    IsPublic = true,
                };

                await _albumsRepository.AddAlbumAsync(album);
                var result = await _albumsRepository.SaveChangesAsync().ConfigureAwait(false);

                if (result != 1)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add("Could not add album");
                    await _albumsRepository.RollbackTransactionAsync().ConfigureAwait(false);
                    return validationResult;
                }

                await _albumsRepository.CommitTransactionAsync().ConfigureAwait(false);

                var albumViewModel = new AlbumViewModel
                {
                    Caption = caption,
                    IsPublic = true,
                    AlbumID = album.AlbumID,
                    PhotoCount = 0,
                };

                validationResult.Album = albumViewModel;

                return validationResult;
            }
            catch
            {
                await _albumsRepository.RollbackTransactionAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<AlbumValidationResult> UpdateAlbumAsync(string caption, int albumId)
        {
            var validationResult = await _albumValidator.ValidateAlbumCaptionAsync(caption, albumId);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            await _albumsRepository.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var album = await _albumsRepository.GetAlbumByIdAsync(albumId).ConfigureAwait(false);
                if (album == null)
                {
                    throw new KeyNotFoundException("Album not found.");
                }

                album.Caption = caption;
                _albumsRepository.UpdateAlbum(album);
                var result = await _albumsRepository.SaveChangesAsync().ConfigureAwait(false);

                if (result != 1)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add("Could not update album");
                    await _albumsRepository.RollbackTransactionAsync().ConfigureAwait(false);
                    return validationResult;
                }

                await _albumsRepository.CommitTransactionAsync().ConfigureAwait(false);
                return validationResult;
            }
            catch
            {
                await _albumsRepository.RollbackTransactionAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<List<AlbumViewModel>> GetAlbumsWithPhotoCountAsync()
        {
            return await _albumHelperService.GetAlbumsWithPhotoCountAsync().ConfigureAwait(false);
        }
    }
}
