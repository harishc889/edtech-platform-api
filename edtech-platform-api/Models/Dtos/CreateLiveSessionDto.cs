using System;
using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class CreateLiveSessionDto
    {
        [Required]
        public int BatchId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes")]
        public int DurationMinutes { get; set; }

        [MaxLength(50)]
        public string? Password { get; set; }

        [MaxLength(50)]
        public string? VideoProvider { get; set; }
    }
}
