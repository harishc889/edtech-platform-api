using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class UpdateUserRoleDto
    {
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = null!;
    }

    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalBatches { get; set; }
        public int TotalEnrollments { get; set; }
        public int ActiveSessions { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingPayments { get; set; }
        public int SuccessfulPayments { get; set; }
    }
}
