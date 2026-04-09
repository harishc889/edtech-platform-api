using System.Threading.Tasks;

namespace edtech_platform_api.Services.VideoProviders
{
    public interface IVideoProvider
    {
        /// <summary>
        /// Creates a meeting/session
        /// </summary>
        Task<VideoMeetingResult> CreateMeetingAsync(string topic, System.DateTime startTime, int durationMinutes, string? password = null);

        /// <summary>
        /// Deletes/cancels a meeting
        /// </summary>
        Task<bool> DeleteMeetingAsync(string meetingId);

        /// <summary>
        /// Gets meeting details
        /// </summary>
        Task<VideoMeetingResult?> GetMeetingAsync(string meetingId);

        /// <summary>
        /// Gets the provider name
        /// </summary>
        string GetProviderName();
    }

    public class VideoMeetingResult
    {
        public string MeetingId { get; set; } = null!;
        public string JoinUrl { get; set; } = null!;
        public string? HostUrl { get; set; }
        public string? Password { get; set; }
        public string Provider { get; set; } = null!;
        public System.DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
    }
}
