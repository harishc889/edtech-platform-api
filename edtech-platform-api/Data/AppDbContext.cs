using Microsoft.EntityFrameworkCore;
using edtech_platform_api.Models;

namespace edtech_platform_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(u => u.PasswordHash)
                      .IsRequired()
                      .HasMaxLength(512);

                // Configure unique index on Email (fluent API)
                entity.HasIndex(u => u.Email).IsUnique();

                // Default for CreatedAt (Postgres: now())
                entity.Property(u => u.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.SessionId)
                      .IsRequired()
                      .HasMaxLength(36);

                entity.Property(s => s.IsActive)
                      .IsRequired();

                entity.Property(s => s.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                entity.HasIndex(s => s.SessionId).IsUnique();

                // Configure one-to-many relationship: User -> Sessions
                // UserSession has navigation property 'User'; User does not need a collection nav.
                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
