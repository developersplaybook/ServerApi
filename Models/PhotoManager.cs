using ServerAPI.DAL;
using ServerAPI.Interfaces;
using ServerAPI.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;


namespace ServerAPI.Models
{
    public class PhotoManager : IRandomHandler, IAlbums, IPhotos, IPhotoDetails
    {
        private readonly PersonalContext _context;

        #region RadomHandler
        public int GetRandomAlbumId()
        {
            var albumsList = GetAlbumsWithPhotoCount().Where(a => a.PhotoCount > 0).ToList();
            if (albumsList.Count == 0) return 0;
            return albumsList[new Random().Next(albumsList.Count)].AlbumID;

        }

        public int GetRandomPhotoId(int albumId)
        {
            var photoList = GetPhotosByAlbumId(albumId);
            if (photoList.Count == 0) return 0;
            return photoList[new Random().Next(photoList.Count)].PhotoID;
        }

        public PhotoManager(PersonalContext context)
        {
            _context = context;
        }


        public Stream GetPhoto(int photoId, PhotoSize size)
        {
            Photo photo = photoId == 0 ? GetDefaultPhotoAllSizes() : _context.Photos.Single(o => o.PhotoID == photoId);


            byte[] result = null;
            switch (size)
            {
                case PhotoSize.Large:
                    result = photo.BytesFull;
                    break;
                case PhotoSize.Medium:
                    result = photo.BytesPoster;
                    break;
                case PhotoSize.Original:
                    result = photo.BytesOriginal;
                    break;
                case PhotoSize.Small:
                    result = photo.BytesThumb;
                    break;
            }

            return result != null ? new MemoryStream(result) : null;
        }

        public Stream GetFirstPhoto(int albumId, PhotoSize size)
        {
            byte[] result = null;
            var photos = GetPhotosByAlbumId(albumId);

            // Check if the album has photos
            if (photos == null || photos.Count == 0)
            {
                return GetDefaultPhotoStream(PhotoSize.Small);
            }

            // Get the appropriate photo based on the requested size
            switch (size)
            {
                case PhotoSize.Large:
                    result = photos[0].BytesFull;
                    break;
                case PhotoSize.Medium:
                    result = photos[0].BytesPoster;
                    break;
                case PhotoSize.Original:
                    result = photos[0].BytesOriginal;
                    break;
                case PhotoSize.Small:
                    result = photos[0].BytesThumb;
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

        #endregion

        #region Albums
        public Album GetAlbum(int albumId)
        {
            return _context.Albums.Single(o => o.AlbumID == albumId);
        }

        public int DeleteAlbum(int albumId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var album = _context.Albums.Single(o => o.AlbumID == albumId);
                    _context.Albums.Remove(album);
                    var response = _context.SaveChanges();
                    transaction.Commit();
                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public AlbumViewModel AddAlbum(string caption)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var album = new Album()
                    {
                        Caption = caption,
                        IsPublic = true,
                    };

                    _context.Albums.Add(album);
                    _context.SaveChanges();
                    var response = GetAlbumsWithPhotoCount().Single(a => a.AlbumID == album.AlbumID);
                    transaction.Commit();
                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public int UpdateAlbum(string caption, int albumId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var album = _context.Albums.Single(o => o.AlbumID == albumId);
                    album.Caption = caption;
                    _context.Albums.Update(album);
                    var response = _context.SaveChanges();
                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public List<Photo> GetPhotosByAlbumId(int albumId)
        {
            return _context.Photos.Where(o => o.AlbumID == albumId).ToList();
        }

        public List<AlbumViewModel> GetAlbumsWithPhotoCount()
        {
            var albums = _context.Albums
                .Select(album => new AlbumViewModel
                {
                    AlbumID = album.AlbumID,
                    Caption = album.Caption,
                    IsPublic = album.IsPublic,
                    PhotoCount = _context.Photos.Where(photo => photo.AlbumID == album.AlbumID).Count(),
                })
                .ToList();

            return albums;
        }
        #endregion

        #region Photos

        public Photo GetPhoto(int photoId)
        {
            if (photoId == 0) return GetDefaultPhotoAllSizes();
            return _context.Photos.Single(o => o.PhotoID == photoId);
        }

        public void AddPhoto(int albumId, string caption, byte[] bytesOriginal)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
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

                    _context.Photos.Add(photo);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public string GetAlbumCaptionByPhotoId(int photoId)
        {
            if (photoId == 0) return "";
            int albumId = GetPhoto(photoId).AlbumID;
            return GetAlbum(albumId).Caption;
        }

        public void DeletePhoto(int photoId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var photo = _context.Photos.Single(p => p.PhotoID == photoId);
                    _context.Photos.Remove(photo);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdatePhoto(string caption, int photoId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var photo = _context.Photos.Single(p => p.PhotoID == photoId);
                    photo.Caption = caption;
                    _context.Photos.Update(photo);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

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

        private Stream GetDefaultPhotoStream(PhotoSize size)
        {
            return new MemoryStream(GetDefaultPhoto(size));
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