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
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<LiveSession> LiveSessions { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

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

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                      .IsRequired();

                entity.Property(e => e.BatchId)
                      .IsRequired();

                entity.Property(e => e.EnrolledAt)
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                // Configure unique index on UserId + BatchId to prevent duplicate enrollments
                entity.HasIndex(e => new { e.UserId, e.BatchId })
                      .IsUnique();

                // Configure relationships
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Batch)
                      .WithMany()
                      .HasForeignKey(e => e.BatchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.UserId)
                      .IsRequired();

                entity.Property(p => p.CourseId)
                      .IsRequired();

                entity.Property(p => p.Amount)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Status)
                      .IsRequired()
                      .HasConversion<string>()  // Store enum as string in DB
                      .HasMaxLength(20);

                entity.Property(p => p.RazorpayOrderId)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.RazorpayPaymentId)
                      .HasMaxLength(100);

                entity.Property(p => p.RazorpaySignature)
                      .HasMaxLength(500);

                entity.Property(p => p.Currency)
                      .IsRequired()
                      .HasMaxLength(10)
                      .HasDefaultValue("INR");

                entity.Property(p => p.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                // Create index on RazorpayOrderId for faster lookups
                entity.HasIndex(p => p.RazorpayOrderId);

                // Create index on RazorpayPaymentId for webhook verification
                entity.HasIndex(p => p.RazorpayPaymentId);

                // Create composite index for user payment history queries
                entity.HasIndex(p => new { p.UserId, p.Status });

                // Configure relationships
                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);  // Don't delete user if they have payments

                entity.HasOne(p => p.Course)
                      .WithMany()
                      .HasForeignKey(p => p.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);  // Don't delete course if it has payments
            });

            modelBuilder.Entity<LiveSession>(entity =>
            {
                entity.HasKey(ls => ls.Id);

                entity.Property(ls => ls.BatchId)
                      .IsRequired();

                entity.Property(ls => ls.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(ls => ls.MeetingUrl)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(ls => ls.MeetingId)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(ls => ls.HostUrl)
                      .HasMaxLength(500);

                entity.Property(ls => ls.Provider)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasDefaultValue("Zoom");

                entity.Property(ls => ls.StartTime)
                      .IsRequired();

                entity.Property(ls => ls.DurationMinutes)
                      .IsRequired();

                entity.Property(ls => ls.Password)
                      .HasMaxLength(500);

                entity.Property(ls => ls.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true);

                entity.Property(ls => ls.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                // Create index on MeetingId for quick lookups
                entity.HasIndex(ls => ls.MeetingId);

                // Create composite index for batch session queries
                entity.HasIndex(ls => new { ls.BatchId, ls.StartTime });

                // Create index on provider for filtering
                entity.HasIndex(ls => ls.Provider);

                // Configure relationship
                entity.HasOne(ls => ls.Batch)
                      .WithMany()
                      .HasForeignKey(ls => ls.BatchId)
                      .OnDelete(DeleteBehavior.Cascade);  // Delete sessions if batch is deleted
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.TokenHash)
                      .IsRequired()
                      .HasMaxLength(128);

                entity.Property(t => t.ExpiresAt)
                      .IsRequired();

                entity.Property(t => t.IsUsed)
                      .IsRequired()
                      .HasDefaultValue(false);

                entity.Property(t => t.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                entity.HasIndex(t => t.TokenHash)
                      .IsUnique();

                entity.HasIndex(t => new { t.UserId, t.IsUsed, t.ExpiresAt });

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
