using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace edtech_platform_api.Services
{
    public class TokenService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(IConfiguration configuration)
        {
            // Expect configuration key: Jwt:Secret
            _secret = configuration["Jwt:Secret"] ?? throw new ArgumentException("JWT secret not configured (Jwt:Secret)");
            _issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentException("JWT issuer not configured (Jwt:Issuer)");
            _audience = configuration["Jwt:Audience"] ?? throw new ArgumentException("JWT audience not configured (Jwt:Audience)");
        }

        public string GenerateToken(string userId, string sessionId, string role = "User")
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", userId),
                new Claim("sessionId", sessionId),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
