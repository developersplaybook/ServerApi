using Microsoft.EntityFrameworkCore;
using ServerAPI.DAL;

namespace ServerAPI.Repositories
{
    public class BaseRepository : DbContext
    {
        private readonly PersonalContext _context;
        public BaseRepository(PersonalContext context)
        {
            _context = context;
        }

        public override int SaveChanges()
        {
            return _context.SaveChanges();
        }


        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _context.Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            _context.Database.RollbackTransaction();
        }
    }
}