using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using edtech_platform_api.Data;

namespace edtech_platform_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UserController(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing userId claim" });
            }

            var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var result = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                createdAt = user.CreatedAt
            };

            return Ok(result);
        }
    }
}
