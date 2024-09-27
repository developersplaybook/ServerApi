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
    public class AlbumsRepository : IAlbumsRepository
    {
        private readonly PersonalContext _context;
        private IDbContextTransaction _transaction; // Holds the current transaction

        public AlbumsRepository(PersonalContext context)
        {
            _context = context;
        }

        public async Task<int> GetRandomAlbumIdWithPhotosAsync()
        {
            // Select only AlbumIDs where PhotoCount is greater than 0
            var albumIds = await _context.Albums
                .Where(a => _context.Photos.Where(p => p.AlbumID == a.AlbumID).Count() > 0)
                .Select(a => a.AlbumID)
                .ToListAsync();

            if (albumIds.Count == 0)
                return 0; // Return 0 if no album has photos

            var random = new Random();
            return albumIds[random.Next(albumIds.Count)];
        }


        public async Task<IEnumerable<Album>> GetAllAlbumsAsync()
        {
            return await _context.Albums.ToListAsync().ConfigureAwait(false);
        }

        public async Task<Album> GetAlbumByIdAsync(int id)
        {
            return await _context.Albums.SingleOrDefaultAsync(a => a.AlbumID == id);
        }

        public async Task AddAlbumAsync(Album album)
        {
            await _context.Albums.AddAsync(album);
        }


        public void UpdateAlbum(Album album)
        {
            _context.Albums.Update(album);
        }


        public void DeleteAlbum(Album album)
        {
            _context.Albums.Remove(album);
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
