using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace edtech_platform_api.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailSender(IConfiguration config)
        {
            _apiKey = config["SendGrid:ApiKey"] ?? throw new Exception("SendGrid:ApiKey not configured");
            _fromEmail = config["SendGrid:FromEmail"] ?? throw new Exception("SendGrid:FromEmail not configured");
            _fromName = config["SendGrid:FromName"] ?? "Ed-Tech Platform";
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, htmlBody, htmlBody);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.OK && 
                response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid failed: {response.StatusCode} - {body}");
            }
        }
    }
}
