using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using edtech_platform_api.Data;
using edtech_platform_api.Models;

namespace edtech_platform_api.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly TokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(AppDbContext db, TokenService tokenService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
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

        public async Task<string> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required", nameof(password));

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
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
                CreatedAt = DateTime.UtcNow
            };

            _db.UserSessions.Add(newSession);
            await _db.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user.Id.ToString(), newSession.SessionId);
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
    }
}
