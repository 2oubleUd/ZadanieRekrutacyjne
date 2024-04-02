using Microsoft.EntityFrameworkCore;

namespace ZadanieRekrutacyjne.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<StackOverflowTag> Tags { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
