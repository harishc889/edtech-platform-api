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
            var sessionId = User.FindFirst("sessionId")?.Value;
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Unauthorized(new { error = "Missing sessionId claim" });
            }

            await _authService.LogoutAsync(sessionId);

            // Clear the auth cookie
            Response.Cookies.Delete("auth_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return NoContent();
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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var device = Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : null;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            var token = await _authService.LoginAsync(dto.Email, dto.Password, device, ip);

            // Set JWT in HTTP-only cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  // Prevents JavaScript access (XSS protection)
                Secure = true,    // HTTPS only
                SameSite = SameSiteMode.Strict,  // CSRF protection
                Expires = DateTimeOffset.UtcNow.AddDays(7),  // Same as JWT expiry
                Path = "/"
            };

            Response.Cookies.Append("auth_token", token, cookieOptions);

            // Return token in body for mobile apps or other clients
            return Ok(new 
            { 
                token,
                message = "Login successful. Token set in cookie."
            });
        }
    }
}
