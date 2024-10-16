using ServerAPI.Interfaces;
using ServerAPI.Models;
using ServerAPI.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class PhotosService : IPhotosService
    {
        private readonly IPhotosRepository _photosRepository;
        private readonly IAlbumsRepository _albumsRepository;
        public PhotosService(IPhotosRepository photosRepository, IAlbumsRepository albumsRepository)
        {
            _photosRepository = photosRepository;
            _albumsRepository = albumsRepository;
        }

        public async Task AddPhotoAsync(int albumId, string caption, byte[] bytesOriginal)
        {
            await _photosRepository.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var photo = new Photo
                {
                    AlbumID = albumId,
                    Caption = caption,
                    BytesOriginal = bytesOriginal,
                    BytesFull = ResizeImageFile(bytesOriginal, 600),
                    BytesPoster = ResizeImageFile(bytesOriginal, 198),
                    BytesThumb = ResizeImageFile(bytesOriginal, 100),
                    PhotoID = 0
                };

                await _photosRepository.AddPhotoAsync(photo);
                await _photosRepository.SaveChangesAsync();
                await _photosRepository.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch
            {
                await _photosRepository.RollbackTransactionAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task DeletePhotoAsync(int photoId)
        {
            await _photosRepository.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                _photosRepository.DeletePhoto(photoId);
                await _photosRepository.SaveChangesAsync();
                await _photosRepository.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch
            {
                await _photosRepository.RollbackTransactionAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<IEnumerable<PhotoViewModel>> GetPhotosViewModelByAlbumIdAsync(int albumId)
        {
            var album = await _albumsRepository.GetAlbumByIdAsync(albumId);
            var photos = await _photosRepository.GetPhotoSlimByAlbumIdAsync(albumId).ConfigureAwait(false);

            return photos.Select(photo => new PhotoViewModel
            {
                AlbumCaption = album.Caption,
                AlbumID = photo.AlbumID,
                Caption = photo.Caption,
                PhotoID = photo.PhotoID
            });
        }

        public async Task UpdatePhotoAsync(string caption, int photoId)
        {
            await _photosRepository.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                _photosRepository.UpdatePhoto(caption, photoId);
                await _photosRepository.SaveChangesAsync();
                await _photosRepository.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch
            {
                await _photosRepository.RollbackTransactionAsync().ConfigureAwait(false);
                throw;
            }
        }


        private static byte[] ResizeImageFile(byte[] imageFile, int targetSize)
        {
            using (var image = Image.Load(imageFile))
            {
                var newSize = CalculateDimensions(image.Size, targetSize);
                image.Mutate(x => x
                    .Resize(newSize.Width, newSize.Height));

                using var m = new MemoryStream();
                image.Save(m, new JpegEncoder()); // Use the JpegEncoder here
                return m.ToArray();
            }
        }

        private static Size CalculateDimensions(Size originalSize, int targetSize)
        {
            // Adjust this method to properly calculate new dimensions
            var aspectRatio = (double)originalSize.Width / originalSize.Height;
            if (aspectRatio > 1)
            {
                return new Size(targetSize, (int)(targetSize / aspectRatio));
            }
            else
            {
                return new Size((int)(targetSize * aspectRatio), targetSize);

            }
        }
    }
}
