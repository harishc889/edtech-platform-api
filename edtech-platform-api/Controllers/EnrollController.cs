using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using edtech_platform_api.Models.Dtos;
using edtech_platform_api.Services;

namespace edtech_platform_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollController : ControllerBase
    {
        private readonly EnrollmentService _enrollmentService;

        public EnrollController(EnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
        }

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollDto dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing userId claim" });
            }

            var enrollment = await _enrollmentService.EnrollUserAsync(userId, dto.BatchId);

            var result = new
            {
                id = enrollment.Id,
                userId = enrollment.UserId,
                batchId = enrollment.BatchId,
                enrolledAt = enrollment.EnrolledAt,
                batch = new
                {
                    id = enrollment.Batch.Id,
                    courseId = enrollment.Batch.CourseId,
                    startDate = enrollment.Batch.StartDate,
                    endDate = enrollment.Batch.EndDate,
                    mentorName = enrollment.Batch.MentorName,
                    capacity = enrollment.Batch.Capacity
                },
                course = new
                {
                    id = enrollment.Batch.Course.Id,
                    title = enrollment.Batch.Course.Title,
                    description = enrollment.Batch.Course.Description,
                    thumbnailUrl = enrollment.Batch.Course.ThumbnailUrl
                }
            };

            return Created($"/api/enroll/{enrollment.Id}", result);
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing userId claim" });
            }

            var enrollments = await _enrollmentService.GetUserEnrollmentsAsync(userId);

            var result = enrollments.Select(e => new
            {
                enrollmentId = e.Id,
                enrolledAt = e.EnrolledAt,
                batch = new
                {
                    id = e.Batch.Id,
                    startDate = e.Batch.StartDate,
                    endDate = e.Batch.EndDate,
                    mentorName = e.Batch.MentorName,
                    capacity = e.Batch.Capacity
                },
                course = new
                {
                    id = e.Batch.Course.Id,
                    title = e.Batch.Course.Title,
                    description = e.Batch.Course.Description,
                    price = e.Batch.Course.Price,
                    thumbnailUrl = e.Batch.Course.ThumbnailUrl
                }
            });

            return Ok(result);
        }
    }
}
