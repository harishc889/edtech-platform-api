using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using edtech_platform_api.Configuration;
using edtech_platform_api.Controllers;
using edtech_platform_api.Data;
using edtech_platform_api.Models;
using edtech_platform_api.Models.Dtos;
using edtech_platform_api.Services;
using Xunit;

namespace EdtechPlatformApi.Tests;

public class AuthControllerForgotPasswordTests
{
    [Fact]
    public async Task ForgotPassword_WithExistingEmail_ReturnsGenericHttp200Message()
    {
        await using var db = CreateDbContext();
        db.Users.Add(new User
        {
            Name = "Demo User",
            Email = "user@example.com",
            PasswordHash = "hash"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        var result = await controller.ForgotPassword(new ForgotPasswordRequestDto { Email = "user@example.com" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<MessageResponseDto>(ok.Value);
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Equal("If an account exists, reset instructions have been sent.", payload.Message);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistingEmail_ReturnsSameGenericHttp200Message()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db);

        var result = await controller.ForgotPassword(new ForgotPasswordRequestDto { Email = "missing@example.com" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<MessageResponseDto>(ok.Value);
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Equal("If an account exists, reset instructions have been sent.", payload.Message);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static AuthController CreateController(AppDbContext db)
    {
        var authService = new AuthService(
            db,
            new TokenService(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = "test-secret-key-with-minimum-32-characters",
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience"
                })
                .Build()),
            new NoOpEmailSender(),
            Options.Create(new PasswordResetSettings { FrontendBaseUrl = "http://localhost:3000" }),
            NullLogger<AuthService>.Instance);

        return new AuthController(
            authService,
            Options.Create(new CookieAuthSettings()),
            Options.Create(new SecuritySettings()));
    }

    private sealed class NoOpEmailSender : IEmailSender
    {
        public Task SendAsync(string toEmail, string subject, string htmlBody) => Task.CompletedTask;
    }
}
