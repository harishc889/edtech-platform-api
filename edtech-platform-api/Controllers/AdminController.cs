using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using edtech_platform_api.Models;
using edtech_platform_api.Models.Dtos;
using edtech_platform_api.Services;

namespace edtech_platform_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role = null)
        {
            var users = await _adminService.GetAllUsersAsync(role);

            var result = users.Select(u => new
            {
                id = u.Id,
                name = u.Name,
                email = u.Email,
                role = u.Role,
                createdAt = u.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var result = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                role = user.Role,
                createdAt = user.CreatedAt
            };

            return Ok(result);
        }

        [HttpPatch("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            var user = await _adminService.UpdateUserRoleAsync(id, dto.Role);

            var result = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                role = user.Role,
                message = $"User role updated to {user.Role}"
            };

            return Ok(result);
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetAllPayments(
            [FromQuery] string? status = null,
            [FromQuery] int? userId = null)
        {
            PaymentStatus? paymentStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsed))
            {
                paymentStatus = parsed;
            }

            var payments = await _adminService.GetAllPaymentsAsync(paymentStatus, userId);

            var result = payments.Select(p => new
            {
                id = p.Id,
                orderId = p.RazorpayOrderId,
                amount = p.Amount,
                currency = p.Currency,
                status = p.Status.ToString(),
                user = new
                {
                    id = p.User.Id,
                    name = p.User.Name,
                    email = p.User.Email
                },
                course = new
                {
                    id = p.Course.Id,
                    title = p.Course.Title
                },
                createdAt = p.CreatedAt,
                paidAt = p.PaidAt
            });

            return Ok(result);
        }

        [HttpGet("analytics/enrollments")]
        public async Task<IActionResult> GetEnrollmentAnalytics()
        {
            var data = await _adminService.GetEnrollmentsByBatchAsync();
            return Ok(data);
        }

        [HttpGet("analytics/revenue")]
        public async Task<IActionResult> GetRevenueAnalytics([FromQuery] int months = 6)
        {
            var data = await _adminService.GetRevenueByMonthAsync(months);
            return Ok(data);
        }
    }
}
