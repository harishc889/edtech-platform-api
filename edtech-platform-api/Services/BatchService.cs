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
    public class BatchService
    {
        private readonly AppDbContext _db;

        public BatchService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Batch> CreateBatchAsync(CreateBatchDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Verify course exists
            var courseExists = await _db.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
            {
                throw new KeyNotFoundException($"Course with ID {dto.CourseId} not found");
            }

            // Validate dates
            if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
            {
                throw new ArgumentException("End date cannot be before start date");
            }

            var batch = new Batch
            {
                CourseId = dto.CourseId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MentorName = dto.MentorName,
                Capacity = dto.Capacity,
                CreatedAt = DateTime.UtcNow
            };

            _db.Batches.Add(batch);
            await _db.SaveChangesAsync();

            return batch;
        }

        public async Task<List<Batch>> GetBatchesByCourseIdAsync(int courseId)
        {
            // Verify course exists
            var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found");
            }

            return await _db.Batches
                .AsNoTracking()
                .Where(b => b.CourseId == courseId)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }
    }
}
