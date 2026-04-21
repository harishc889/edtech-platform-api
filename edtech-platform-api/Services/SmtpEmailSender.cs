using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using edtech_platform_api.Configuration;

namespace edtech_platform_api.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _smtp;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> smtpOptions, ILogger<SmtpEmailSender> logger)
    {
        _smtp = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_smtp.Host)
            || string.IsNullOrWhiteSpace(_smtp.FromEmail)
            || string.IsNullOrWhiteSpace(_smtp.Username)
            || string.IsNullOrWhiteSpace(_smtp.Password))
        {
            throw new InvalidOperationException("SMTP settings are incomplete. Configure Smtp:Host/Username/Password/FromEmail.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_smtp.FromEmail, _smtp.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            EnableSsl = _smtp.EnableSsl,
            Credentials = new NetworkCredential(_smtp.Username, _smtp.Password)
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Forgot-password email dispatched to domain {Domain}", toEmail.Split('@').LastOrDefault() ?? "unknown");
    }
}
