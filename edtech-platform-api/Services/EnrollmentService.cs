using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using edtech_platform_api.Data;
using edtech_platform_api.Models;

namespace edtech_platform_api.Services
{
    public class EnrollmentService
    {
        private readonly AppDbContext _db;

        public EnrollmentService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Enrollment> EnrollUserAsync(int userId, int batchId)
        {
            // Verify batch exists
            var batch = await _db.Batches
                .Include(b => b.Course)
                .FirstOrDefaultAsync(b => b.Id == batchId);

            if (batch == null)
            {
                throw new KeyNotFoundException($"Batch with ID {batchId} not found");
            }

            // Check if user is already enrolled in this batch
            var existingEnrollment = await _db.Enrollments
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == userId && e.BatchId == batchId);

            if (existingEnrollment != null)
            {
                throw new InvalidOperationException("You are already enrolled in this batch");
            }

            // Check batch capacity
            var currentEnrollmentCount = await _db.Enrollments
                .CountAsync(e => e.BatchId == batchId);

            if (currentEnrollmentCount >= batch.Capacity)
            {
                throw new InvalidOperationException("This batch has reached its maximum capacity");
            }

            // TODO: In future, verify payment before enrollment
            // For now, we'll allow direct enrollment

            var enrollment = new Enrollment
            {
                UserId = userId,
                BatchId = batchId,
                EnrolledAt = DateTime.UtcNow
            };

            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();

            // Load navigation properties for return
            await _db.Entry(enrollment)
                .Reference(e => e.Batch)
                .LoadAsync();
            await _db.Entry(enrollment.Batch)
                .Reference(b => b.Course)
                .LoadAsync();

            return enrollment;
        }

        public async Task<List<Enrollment>> GetUserEnrollmentsAsync(int userId)
        {
            return await _db.Enrollments
                .AsNoTracking()
                .Include(e => e.Batch)
                    .ThenInclude(b => b.Course)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }
    }
}
