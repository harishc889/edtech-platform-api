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
    public class LiveSessionController : ControllerBase
    {
        private readonly LiveSessionService _liveSessionService;

        public LiveSessionController(LiveSessionService liveSessionService)
        {
            _liveSessionService = liveSessionService ?? throw new ArgumentNullException(nameof(liveSessionService));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSession([FromBody] CreateLiveSessionDto dto)
        {
            var session = await _liveSessionService.CreateSessionAsync(dto);

            var result = new
            {
                id = session.Id,
                batchId = session.BatchId,
                title = session.Title,
                meetingUrl = session.MeetingUrl,
                meetingId = session.MeetingId,
                hostUrl = session.HostUrl,
                provider = session.Provider,
                startTime = session.StartTime,
                endTime = session.EndTime,
                durationMinutes = session.DurationMinutes,
                password = session.Password,
                batch = new
                {
                    id = session.Batch.Id,
                    courseId = session.Batch.CourseId,
                    startDate = session.Batch.StartDate,
                    mentorName = session.Batch.MentorName
                },
                createdAt = session.CreatedAt
            };

            return Created($"/api/live-session/{session.Id}", result);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMySessions()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing userId claim" });
            }

            var sessions = await _liveSessionService.GetUserEnrolledSessionsAsync(userId);

            var result = sessions.Select(s => new
            {
                id = s.Id,
                title = s.Title,
                meetingUrl = s.MeetingUrl,
                provider = s.Provider,
                startTime = s.StartTime,
                endTime = s.EndTime,
                durationMinutes = s.DurationMinutes,
                password = s.Password,
                batch = new
                {
                    id = s.Batch.Id,
                    startDate = s.Batch.StartDate,
                    mentorName = s.Batch.MentorName
                },
                course = new
                {
                    id = s.Batch.Course.Id,
                    title = s.Batch.Course.Title,
                    thumbnailUrl = s.Batch.Course.ThumbnailUrl
                }
            });

            return Ok(result);
        }

        [HttpGet("batch/{batchId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBatchSessions(int batchId)
        {
            var sessions = await _liveSessionService.GetBatchSessionsAsync(batchId);

            var result = sessions.Select(s => new
            {
                id = s.Id,
                title = s.Title,
                startTime = s.StartTime,
                endTime = s.EndTime,
                durationMinutes = s.DurationMinutes,
                provider = s.Provider
            });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            await _liveSessionService.DeleteSessionAsync(id);
            return NoContent();
        }
    }
}
