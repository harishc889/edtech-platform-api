using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using edtech_platform_api.Configuration;
using edtech_platform_api.Infrastructure;
using edtech_platform_api.Models.Dtos;
using edtech_platform_api.Services;

namespace edtech_platform_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly CookieAuthSettings _cookieAuth;
        private readonly SecuritySettings _security;

        public AuthController(
            AuthService authService,
            IOptions<CookieAuthSettings> cookieAuth,
            IOptions<SecuritySettings> security)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _cookieAuth = cookieAuth.Value;
            _security = security.Value;
        }

        /// <summary>
        /// Returns a CSRF request token and sets the antiforgery cookie. Required before payment POSTs when Security:RequirePaymentCsrf is true.
        /// </summary>
        [HttpGet("csrf")]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public IActionResult GetCsrfToken([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { csrfToken = tokens.RequestToken });
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

            var deleteOpts = CookieAuthHelper.CreateAuthCookieOptions(_cookieAuth);
            Response.Cookies.Delete("auth_token", deleteOpts);

            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
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
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var device = Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : null;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            var token = await _authService.LoginAsync(dto.Email, dto.Password, device, ip);

            var cookieOptions = CookieAuthHelper.CreateAuthCookieOptions(
                _cookieAuth,
                DateTimeOffset.UtcNow.AddDays(7));

            Response.Cookies.Append("auth_token", token, cookieOptions);

            if (_security.ExposeTokenInLoginResponse)
            {
                return Ok(new
                {
                    token,
                    message = "Login successful. Token set in cookie."
                });
            }

            return Ok(new { message = "Login successful. Token set in cookie." });
        }
    }
}
