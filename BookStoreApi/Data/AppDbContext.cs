using BookStoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        
        public virtual DbSet<Book> Books { get; set; }
    }
}
