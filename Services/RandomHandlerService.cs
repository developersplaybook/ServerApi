using ServerAPI.Interfaces;
using ServerAPI.Models;
using ServerAPI.ViewModels;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class RandomHandlerService : IRandomHandlerService
    {
        private readonly IPhotosRepository _photosRepository;
        private readonly IAlbumsRepository _albumsRepository;
        public RandomHandlerService(IPhotosRepository photosRepository, IAlbumsRepository albumsRepository)
        {
            _photosRepository = photosRepository;
            _albumsRepository = albumsRepository;
        }

        public async Task<int> GetRandomAlbumIdAsync()
        {
            return await _albumsRepository.GetRandomAlbumIdWithPhotosAsync();
        }

        public async Task<int> GetRandomPhotoIdAsync(int albumId)
        {
            return await _photosRepository.GetRandomPhotoIdAsync(albumId);
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


        public async Task<Stream> GetPhotoAsync(int photoId, PhotoSize size)
        {
            Photo photo;

            // Fetch the photo asynchronously based on the given photoId
            if (photoId == 0)
            {
                photo = GetDefaultPhotoAllSizes();
            }
            else
            {
                photo = await _photosRepository.GetPhotoByIdAsync(photoId).ConfigureAwait(false);

                if (photo == null)
                {
                    // Handle the case where the photo is not found
                    throw new KeyNotFoundException($"Photo with ID {photoId} not found.");
                }
            }

            // Select the appropriate byte array based on the size parameter
            byte[] result = size switch
            {
                PhotoSize.Large => photo.BytesFull,
                PhotoSize.Medium => photo.BytesPoster,
                PhotoSize.Original => photo.BytesOriginal,
                PhotoSize.Small => photo.BytesThumb,
                _ => null
            };

            // Return the byte array as a MemoryStream, or null if no photo is found
            return result != null ? new MemoryStream(result) : null;
        }


        public async Task<Stream> GetFirstPhotoAsync(int albumId, PhotoSize size)
        {
            byte[] result = null;
            var all = await GetPhotoSlimByAlbumIdAsync(albumId).ConfigureAwait(false);
            var photos = all.ToList();

            // Check if the album has photos
            if (photos == null || photos.Count == 0)
            {
                return GetDefaultPhotoStream(PhotoSize.Small);
            }
            var completePhoto = await _photosRepository.GetPhotoByIdAsync(photos[0].PhotoID);

            // Get the appropriate photo based on the requested size
            switch (size)
            {
                case PhotoSize.Large:
                    result = completePhoto.BytesFull;
                    break;
                case PhotoSize.Medium:
                    result = completePhoto.BytesPoster;
                    break;
                case PhotoSize.Original:
                    result = completePhoto.BytesOriginal;
                    break;
                case PhotoSize.Small:
                    result = completePhoto.BytesThumb;
                    break;
                default:
                    // Handle unexpected size values if necessary
                    return null;
            }

            if (result == null)
            {
                return null;
            }

            return new MemoryStream(result);
        }

        private async Task<IEnumerable<PhotoSlim>> GetPhotoSlimByAlbumIdAsync(int albumId)
        {
            return await _photosRepository.GetPhotoSlimByAlbumIdAsync(albumId).ConfigureAwait(false);
        }

        private Stream GetDefaultPhotoStream(PhotoSize size)
        {
            return new MemoryStream(GetDefaultPhoto(size));
        }

        private byte[] GetDefaultPhoto(PhotoSize size)
        {
            // Path to the default image, adjust this to your actual default image path
            string defaultImagePath = size switch
            {
                PhotoSize.Large => "wwwroot/images/default-image-large.png",
                PhotoSize.Medium => "wwwroot/images/default-image-medium.png",
                PhotoSize.Original => "wwwroot/images/default-image.png",
                PhotoSize.Small => "wwwroot/images/default-image-small.png",
                _ => "wwwroot/images/default-image-small.png" // Fallback to a generic default image
            };

            // Load the default image as a byte array
            byte[] defaultImageBytes = File.ReadAllBytes(defaultImagePath);

            // Return the image as a stream
            return defaultImageBytes;
        }

        private Photo GetDefaultPhotoAllSizes()
        {
            var photo = new Photo
            {
                AlbumID = 0,
                Caption = "",
                PhotoID = 0,
                BytesFull = File.ReadAllBytes("wwwroot/images/default-image-large.png"),
                BytesOriginal = File.ReadAllBytes("wwwroot/images/default-image-medium.png"),
                BytesPoster = File.ReadAllBytes("wwwroot/images/default-image.png"),
                BytesThumb = File.ReadAllBytes("wwwroot/images/default-image-small.png")
            };

            return photo;
        }
    }
}
