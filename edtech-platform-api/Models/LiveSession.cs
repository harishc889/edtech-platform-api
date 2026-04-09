using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace edtech_platform_api.Models
{
    public class LiveSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BatchId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string MeetingUrl { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string MeetingId { get; set; } = null!;

        [MaxLength(500)]
        public string? HostUrl { get; set; }

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = "Zoom";

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [MaxLength(500)]
        public string? Password { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(BatchId))]
        public Batch Batch { get; set; } = null!;
    }
}
