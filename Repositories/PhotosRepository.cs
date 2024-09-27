using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ServerAPI.DAL;
using ServerAPI.Interfaces;
using ServerAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Repositories
{
    public class PhotosRepository : IPhotosRepository
    {
        private readonly PersonalContext _context;
        private IDbContextTransaction _transaction; // Holds the current transaction
        public PhotosRepository(PersonalContext context)
        {
            _context = context;
        }

        public async Task<int> GetRandomPhotoIdAsync(int albumId)
        {
            // Fetch only the PhotoIDs for the given album
            var photoIds = await GetPhotoIdsByAlbumIdAsync(albumId).ConfigureAwait(false);

            // Return 0 if no photos exist in the album
            if (!photoIds.Any()) return 0;

            // Use a single instance of Random for generating random numbers
            var random = new Random();

            // Return a random PhotoID from the list
            return photoIds[random.Next(photoIds.Count)];
        }


        public async Task<Dictionary<int, int>> GetPhotoCountsPerAlbumAsync()
        {
            return await _context.Photos
                .GroupBy(photo => photo.AlbumID)
                .Select(group => new { AlbumID = group.Key, Count = group.Count() })
                .ToDictionaryAsync(x => x.AlbumID, x => x.Count);
        }

        public async Task<Photo> GetPhotoByIdAsync(int photoId)
        {
            return await _context.Photos.FindAsync(photoId).ConfigureAwait(false);
        }

        public async Task<PhotoSlim> GetPhotoSlimByIdAsync(int photoId)
        {
            return await _context.Photos
                .Where(o => o.PhotoID == photoId)
                .Select(photo => new PhotoSlim
                {
                    PhotoID = photo.PhotoID,
                    AlbumID = photo.AlbumID,
                    Caption = photo.Caption
                })
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<PhotoSlim>> GetPhotoSlimByAlbumIdAsync(int albumId)
        {
            return await _context.Photos
                .Where(o => o.AlbumID == albumId) // Apply the filter first
                .Select(photo => new PhotoSlim    // Project to PhotoSlim objects
                {
                    PhotoID = photo.PhotoID,
                    AlbumID = photo.AlbumID,
                    Caption = photo.Caption
                })
                .ToListAsync()
                .ConfigureAwait(false); // Perform the query asynchronously
        }


        public async Task AddPhotoAsync(Photo photo)
        {
            await _context.Photos.AddAsync(photo);
        }

        public void DeletePhoto(int photoId)
        {
            var photo = _context.Photos.Single(p => p.PhotoID == photoId);
            _context.Photos.Remove(photo);
        }

        public void UpdatePhoto(string caption, int photoId)
        {
            var photo = _context.Photos.Single(p => p.PhotoID == photoId);
            photo.Caption = caption;
            _context.Photos.Update(photo);
        }

        private async Task<List<int>> GetPhotoIdsByAlbumIdAsync(int albumId)
        {
            return await _context.Photos
                .Where(photo => photo.AlbumID == albumId)
                .Select(photo => photo.PhotoID)
                .ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        // Transaction Management Methods
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync().ConfigureAwait(false);
                await DisposeTransactionAsync().ConfigureAwait(false);
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync().ConfigureAwait(false);
                await DisposeTransactionAsync().ConfigureAwait(false);
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
                _transaction = null;
            }
        }
    }
}