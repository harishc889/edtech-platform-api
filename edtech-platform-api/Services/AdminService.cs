using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using edtech_platform_api.Data;
using edtech_platform_api.Models;
using edtech_platform_api.Models.Dtos;

namespace edtech_platform_api.Services
{
    public class AdminService
    {
        private readonly AppDbContext _db;

        public AdminService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<AdminDashboardStats> GetDashboardStatsAsync()
        {
            var stats = new AdminDashboardStats
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalCourses = await _db.Courses.CountAsync(),
                TotalBatches = await _db.Batches.CountAsync(),
                TotalEnrollments = await _db.Enrollments.CountAsync(),
                ActiveSessions = await _db.LiveSessions.CountAsync(s => s.IsActive && s.StartTime > DateTime.UtcNow),
                TotalRevenue = await _db.Payments
                    .Where(p => p.Status == PaymentStatus.Success)
                    .SumAsync(p => p.Amount),
                PendingPayments = await _db.Payments.CountAsync(p => p.Status == PaymentStatus.Pending),
                SuccessfulPayments = await _db.Payments.CountAsync(p => p.Status == PaymentStatus.Success)
            };

            return stats;
        }

        public async Task<List<User>> GetAllUsersAsync(string? role = null)
        {
            var query = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            return await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> UpdateUserRoleAsync(int userId, string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole))
            {
                throw new ArgumentException("Role cannot be empty", nameof(newRole));
            }

            // Validate role
            var validRoles = new[] { "User", "Admin" };
            if (!validRoles.Contains(newRole))
            {
                throw new ArgumentException($"Invalid role. Must be one of: {string.Join(", ", validRoles)}");
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            user.Role = newRole;
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<List<Payment>> GetAllPaymentsAsync(PaymentStatus? status = null, int? userId = null)
        {
            var query = _db.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(p => p.UserId == userId.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetEnrollmentsByBatchAsync()
        {
            return await _db.Enrollments
                .GroupBy(e => e.BatchId)
                .Select(g => new { BatchId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BatchId.ToString(), x => x.Count);
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int months = 6)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);

            return await _db.Payments
                .Where(p => p.Status == PaymentStatus.Success && p.PaidAt >= startDate)
                .GroupBy(p => new { p.PaidAt!.Value.Year, p.PaidAt!.Value.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(p => p.Amount)
                })
                .ToDictionaryAsync(x => x.Month, x => x.Revenue);
        }
    }
}
