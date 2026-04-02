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
    public class BatchController : ControllerBase
    {
        private readonly BatchService _batchService;

        public BatchController(BatchService batchService)
        {
            _batchService = batchService ?? throw new ArgumentNullException(nameof(batchService));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBatch([FromBody] CreateBatchDto dto)
        {
            var batch = await _batchService.CreateBatchAsync(dto);

            var result = new
            {
                id = batch.Id,
                courseId = batch.CourseId,
                startDate = batch.StartDate,
                endDate = batch.EndDate,
                mentorName = batch.MentorName,
                capacity = batch.Capacity,
                createdAt = batch.CreatedAt
            };

            return Created($"/api/batch/{batch.Id}", result);
        }

        [HttpGet("course/{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBatchesByCourse(int courseId)
        {
            var batches = await _batchService.GetBatchesByCourseIdAsync(courseId);

            var result = batches.Select(b => new
            {
                id = b.Id,
                courseId = b.CourseId,
                startDate = b.StartDate,
                endDate = b.EndDate,
                mentorName = b.MentorName,
                capacity = b.Capacity,
                createdAt = b.CreatedAt
            });

            return Ok(result);
        }
    }
}
