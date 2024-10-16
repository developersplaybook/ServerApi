using Microsoft.EntityFrameworkCore;
using ServerAPI.Models;

namespace ServerAPI.DAL
{
    public class PersonalContext : DbContext
    {
        public PersonalContext(DbContextOptions options)
            : base(options)
        {
        }

	    public DbSet<Photo> Photos { get; set; }

	    public DbSet<Album> Albums { get; set; }
	}
}