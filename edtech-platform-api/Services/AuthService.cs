using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using edtech_platform_api.Configuration;
using edtech_platform_api.Data;
using edtech_platform_api.Models;

namespace edtech_platform_api.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly TokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IEmailSender _emailSender;
        private readonly PasswordResetSettings _passwordResetSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext db,
            TokenService tokenService,
            IEmailSender emailSender,
            IOptions<PasswordResetSettings> passwordResetSettings,
            ILogger<AuthService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _passwordResetSettings = passwordResetSettings.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User> RegisterAsync(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required", nameof(password));

            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (exists)
            {
                throw new InvalidOperationException("Email already in use");
            }

            var user = new User
            {
                Name = name,
                Email = email
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<string> LoginAsync(string email, string password, string? deviceName = null, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required", nameof(password));

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var activeSessions = await _db.UserSessions
                .Where(s => s.UserId == user.Id && s.IsActive)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            // Ensure we allow at most 2 active sessions. If creating a new session would exceed the limit,
            // deactivate oldest sessions until there is room for the new session.
            while (activeSessions.Count >= 2)
            {
                var oldest = activeSessions.First();
                oldest.IsActive = false;
                _db.UserSessions.Update(oldest);
                activeSessions.RemoveAt(0);
            }

            var newSession = new UserSession
            {
                UserId = user.Id,
                SessionId = Guid.NewGuid().ToString(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                DeviceName = string.IsNullOrWhiteSpace(deviceName) ? "Unknown" : deviceName,
                IPAddress = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress
            };

            _db.UserSessions.Add(newSession);
            await _db.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user.Id.ToString(), newSession.SessionId, user.Role);
            return token;
        }

        public async Task LogoutAsync(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("SessionId is required", nameof(sessionId));

            var session = await _db.UserSessions.SingleOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null)
            {
                // nothing to do if session not found
                return;
            }

            if (session.IsActive)
            {
                session.IsActive = false;
                _db.UserSessions.Update(session);
                await _db.SaveChangesAsync();
            }
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user == null)
            {
                return;
            }

            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLower(CultureInfo.InvariantCulture);
            var tokenHash = HashToken(rawToken);
            var now = DateTime.UtcNow;

            var existingTokens = await _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > now)
                .ToListAsync();

            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                token.TokenUsedAt = now;
            }

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = now.AddMinutes(30),
                IsUsed = false,
                CreatedAt = now
            };

            _db.PasswordResetTokens.Add(resetToken);
            await _db.SaveChangesAsync();

            var frontendBase = _passwordResetSettings.FrontendBaseUrl.TrimEnd('/');
            var resetUrl = $"{frontendBase}/login/reset-password?token={Uri.EscapeDataString(rawToken)}";
            var subject = "Reset your password";
            var body = $"<p>We received a request to reset your password.</p><p><a href=\"{resetUrl}\">Reset password</a></p><p>This link expires in 30 minutes.</p>";

            try
            {
                await _emailSender.SendAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending forgot-password email for userId {UserId}.", user.Id);
                throw;
            }
        }

        public async Task ResetPasswordAsync(string rawToken, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                throw new ArgumentException("Reset token is invalid.");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters.");
            }

            var tokenHash = HashToken(rawToken.Trim());
            var now = DateTime.UtcNow;

            var resetToken = await _db.PasswordResetTokens
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt <= now)
            {
                throw new ArgumentException("Reset link is invalid or expired.");
            }

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == resetToken.UserId);
            if (user == null)
            {
                throw new ArgumentException("Reset link is invalid or expired.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

            resetToken.IsUsed = true;
            resetToken.TokenUsedAt = now;

            var activeTokens = await _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > now)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsUsed = true;
                token.TokenUsedAt = now;
            }

            await _db.SaveChangesAsync();
        }

        private static string NormalizeEmail(string email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string HashToken(string rawToken)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
