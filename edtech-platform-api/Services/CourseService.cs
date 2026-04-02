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
    public class CourseService
    {
        private readonly AppDbContext _db;

        public CourseService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Course> CreateCourseAsync(CreateCourseDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                ThumbnailUrl = dto.ThumbnailUrl,
                IsPublished = dto.IsPublished,
                CreatedAt = DateTime.UtcNow
            };

            _db.Courses.Add(course);
            await _db.SaveChangesAsync();

            return course;
        }

        public async Task<List<Course>> GetPublishedCoursesAsync()
        {
            return await _db.Courses
                .AsNoTracking()
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _db.Courses
                .AsNoTracking()
                .Include(c => c.Batches)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
