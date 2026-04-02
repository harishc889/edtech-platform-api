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
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;

        public CourseController(CourseService courseService)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            try
            {
                var course = await _courseService.CreateCourseAsync(dto);

                var result = new
                {
                    id = course.Id,
                    title = course.Title,
                    description = course.Description,
                    price = course.Price,
                    thumbnailUrl = course.ThumbnailUrl,
                    isPublished = course.IsPublished,
                    createdAt = course.CreatedAt
                };

                return Created($"/api/course/{course.Id}", result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while creating the course." });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedCourses()
        {
            try
            {
                var courses = await _courseService.GetPublishedCoursesAsync();

                var result = courses.Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    price = c.Price,
                    thumbnailUrl = c.ThumbnailUrl,
                    isPublished = c.IsPublished,
                    createdAt = c.CreatedAt
                });

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while fetching courses." });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);

                if (course == null)
                {
                    return NotFound(new { error = "Course not found" });
                }

                var result = new
                {
                    id = course.Id,
                    title = course.Title,
                    description = course.Description,
                    price = course.Price,
                    thumbnailUrl = course.ThumbnailUrl,
                    isPublished = course.IsPublished,
                    createdAt = course.CreatedAt,
                    batches = course.Batches.Select(b => new
                    {
                        id = b.Id,
                        startDate = b.StartDate,
                        endDate = b.EndDate,
                        mentorName = b.MentorName,
                        capacity = b.Capacity,
                        createdAt = b.CreatedAt
                    })
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while fetching the course." });
            }
        }
    }
}
