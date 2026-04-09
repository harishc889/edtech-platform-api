using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace edtech_platform_api.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        [MaxLength(100)]
        public string RazorpayOrderId { get; set; } = null!;

        [MaxLength(100)]
        public string? RazorpayPaymentId { get; set; }

        [MaxLength(500)]
        public string? RazorpaySignature { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;
    }
}
