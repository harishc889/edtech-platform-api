using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using edtech_platform_api.Data;
using edtech_platform_api.Models;
using edtech_platform_api.Models.Dtos;
using edtech_platform_api.Services.VideoProviders;

namespace edtech_platform_api.Services
{
    public class LiveSessionService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly VideoProviderFactory _providerFactory;

        public LiveSessionService(AppDbContext db, IConfiguration config)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _providerFactory = new VideoProviderFactory(config);
        }

        public async Task<LiveSession> CreateSessionAsync(CreateLiveSessionDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Verify batch exists
            var batch = await _db.Batches
                .Include(b => b.Course)
                .FirstOrDefaultAsync(b => b.Id == dto.BatchId);

            if (batch == null)
            {
                throw new KeyNotFoundException($"Batch with ID {dto.BatchId} not found");
            }

            // Validate start time (must be in future)
            if (dto.StartTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("Session start time must be in the future");
            }

            // Get video provider
            var provider = _providerFactory.CreateProvider(dto.VideoProvider);

            // Create meeting in video platform
            var meeting = await provider.CreateMeetingAsync(
                topic: dto.Title,
                startTime: dto.StartTime,
                durationMinutes: dto.DurationMinutes,
                password: dto.Password
            );

            // Calculate end time
            var endTime = dto.StartTime.AddMinutes(dto.DurationMinutes);

            // Create live session record
            var session = new LiveSession
            {
                BatchId = dto.BatchId,
                Title = dto.Title,
                MeetingUrl = meeting.JoinUrl,
                MeetingId = meeting.MeetingId,
                HostUrl = meeting.HostUrl,
                Provider = meeting.Provider,
                StartTime = dto.StartTime,
                EndTime = endTime,
                DurationMinutes = dto.DurationMinutes,
                Password = meeting.Password,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.LiveSessions.Add(session);
            await _db.SaveChangesAsync();

            // Load navigation properties
            await _db.Entry(session).Reference(s => s.Batch).LoadAsync();

            return session;
        }

        public async Task<List<LiveSession>> GetUserEnrolledSessionsAsync(int userId)
        {
            // Get all batches user is enrolled in
            var enrolledBatchIds = await _db.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.BatchId)
                .ToListAsync();

            if (!enrolledBatchIds.Any())
            {
                return new List<LiveSession>();
            }

            // Get all active sessions for those batches
            return await _db.LiveSessions
                .Include(s => s.Batch)
                    .ThenInclude(b => b.Course)
                .Where(s => enrolledBatchIds.Contains(s.BatchId) && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<LiveSession>> GetBatchSessionsAsync(int batchId)
        {
            var batchExists = await _db.Batches.AnyAsync(b => b.Id == batchId);
            if (!batchExists)
            {
                throw new KeyNotFoundException($"Batch with ID {batchId} not found");
            }

            return await _db.LiveSessions
                .Where(s => s.BatchId == batchId && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<bool> DeleteSessionAsync(int sessionId)
        {
            var session = await _db.LiveSessions.FindAsync(sessionId);
            if (session == null)
            {
                throw new KeyNotFoundException($"Session with ID {sessionId} not found");
            }

            // Try to delete from video provider
            try
            {
                var provider = _providerFactory.CreateProvider(session.Provider);
                await provider.DeleteMeetingAsync(session.MeetingId);
            }
            catch
            {
                // Continue even if provider deletion fails
            }

            // Mark as inactive instead of deleting (soft delete)
            session.IsActive = false;
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
