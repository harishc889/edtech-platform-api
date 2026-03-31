using Microsoft.EntityFrameworkCore;

namespace edtech_platform_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Add DbSets here, e.g.
        // public DbSet<User> Users { get; set; }
    }
}
