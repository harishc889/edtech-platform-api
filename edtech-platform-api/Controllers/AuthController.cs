using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using edtech_platform_api.Models.Dtos;
using edtech_platform_api.Services;

namespace edtech_platform_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var sessionId = User.FindFirst("sessionId")?.Value;
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return Unauthorized(new { error = "Missing sessionId claim" });
                }

                await _authService.LogoutAsync(sessionId);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        // Example of a protected endpoint (requires valid JWT)
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            // return the claims for demonstration
            return Ok(new
            {
                user = User.Identity?.Name,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // [ApiController] will automatically validate ModelState and return 400 if invalid.
            try
            {
                var user = await _authService.RegisterAsync(dto.Name, dto.Email, dto.Password);

                // Return minimal user info (no password hash)
                var result = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    createdAt = user.CreatedAt
                };

                return Created(string.Empty, result);
            }
            catch (InvalidOperationException ex)
            {
                // e.g., email already in use
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                // Try to capture device info and client IP
                var device = Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : null;
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                var token = await _authService.LoginAsync(dto.Email, dto.Password, device, ip);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    }
}
