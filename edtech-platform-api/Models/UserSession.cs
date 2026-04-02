using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace edtech_platform_api.Models
{
    [Index(nameof(SessionId), IsUnique = true)]
    public class UserSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(36)]
        public string SessionId { get; set; } = null!;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Optional: device name from which the user logged in (e.g. "Chrome on Windows")
        [MaxLength(500)]
        public string? DeviceName { get; set; }

        // Optional: IP address of the client (IPv4 or IPv6)
        [MaxLength(45)]
        public string? IPAddress { get; set; }
    }
}