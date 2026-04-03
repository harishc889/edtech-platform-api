using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace edtech_platform_api.Models
{
    [Index(nameof(UserId), nameof(BatchId), IsUnique = true)]
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BatchId { get; set; }

        [Required]
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(BatchId))]
        public Batch Batch { get; set; } = null!;
    }
}
