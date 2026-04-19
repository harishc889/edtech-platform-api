using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace edtech_platform_api.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IConfiguration config, ILogger<WebhookController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("razorpay")]
        public async Task<IActionResult> RazorpayWebhook()
        {
            try
            {
                // Read webhook body
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var webhookBody = await reader.ReadToEndAsync();

                // Get signature from header
                var signature = Request.Headers["X-Razorpay-Signature"].ToString();
                var secret = _config["Razorpay:WebhookSecret"];
                if (string.IsNullOrWhiteSpace(secret))
                {
                    _logger.LogWarning("Razorpay:WebhookSecret is not configured; rejecting webhook.");
                    return Unauthorized(new { error = "Webhook not configured" });
                }

                if (!VerifySignature(webhookBody, signature, secret))
                {
                    return Unauthorized(new { error = "Invalid signature" });
                }

                // Parse webhook data
                var data = JsonDocument.Parse(webhookBody);
                var eventType = data.RootElement.GetProperty("event").GetString();

                _logger.LogInformation($"Webhook received: {eventType}");

                // Handle different events
                if (eventType == "payment.captured")
                {
                    var paymentId = data.RootElement
                        .GetProperty("payload")
                        .GetProperty("payment")
                        .GetProperty("entity")
                        .GetProperty("id").GetString();

                    _logger.LogInformation($"Payment captured: {paymentId}");
                    
                    // TODO: Update payment status in database
                    // await _paymentService.UpdatePaymentStatusFromWebhook(paymentId);
                }

                return Ok(new { status = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook error");
                return Ok(new { status = "error" }); // Return 200 to prevent retries
            }
        }

        private bool VerifySignature(string body, string signature, string secret)
        {
            if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(secret))
                return false;

            try
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
                var computed = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return computed == signature.ToLower();
            }
            catch
            {
                return false;
            }
        }
    }
}