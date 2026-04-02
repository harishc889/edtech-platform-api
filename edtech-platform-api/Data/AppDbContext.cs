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
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Batch> Batches { get; set; } = null!;

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

                entity.Property(u => u.Role)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("User");

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

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(c => c.Description)
                      .HasMaxLength(2000);

                entity.Property(c => c.Price)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(c => c.ThumbnailUrl)
                      .HasMaxLength(500);

                entity.Property(c => c.IsPublished)
                      .IsRequired()
                      .HasDefaultValue(false);

                entity.Property(c => c.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                // Configure one-to-many relationship: Course -> Batches
                entity.HasMany(c => c.Batches)
                      .WithOne(b => b.Course)
                      .HasForeignKey(b => b.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Batch>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.StartDate)
                      .IsRequired();

                entity.Property(b => b.MentorName)
                      .HasMaxLength(100);

                entity.Property(b => b.Capacity)
                      .IsRequired();

                entity.Property(b => b.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                entity.Property(b => b.CourseId)
                      .IsRequired();
            });
        }
    }
}
