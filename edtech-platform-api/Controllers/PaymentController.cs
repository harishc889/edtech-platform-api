using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using edtech_platform_api.Models.Dtos;
using edtech_platform_api.Services;

namespace edtech_platform_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("payment")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing userId claim" });
            }

            // Optional: Allow gateway selection from request
            string? gateway = Request.Headers["X-Payment-Gateway"].FirstOrDefault();

            var payment = await _paymentService.CreateOrderAsync(userId, dto.CourseId, dto.BatchId, gateway);

            var result = new
            {
                orderId = payment.RazorpayOrderId,
                amount = payment.Amount,
                currency = payment.Currency,
                keyId = _paymentService.GetGatewayPublicKey(gateway),
                gateway = _paymentService.GetGatewayName(gateway),
                courseTitle = payment.Course.Title,
                paymentId = payment.Id
            };

            return Ok(result);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentDto dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing userId claim" });
            }

            // Optional: Allow gateway selection from request
            string? gateway = Request.Headers["X-Payment-Gateway"].FirstOrDefault();

            var payment = await _paymentService.VerifyAndCompletePaymentAsync(
                userId,
                dto.RazorpayOrderId,
                dto.RazorpayPaymentId,
                dto.RazorpaySignature,
                dto.BatchId,
                gateway
            );

            var result = new
            {
                paymentId = payment.Id,
                status = payment.Status.ToString(),
                amount = payment.Amount,
                courseId = payment.CourseId,
                paidAt = payment.PaidAt,
                message = "Payment verified successfully. You are now enrolled in the course!"
            };

            return Ok(result);
        }

        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing userId claim" });
            }

            var payments = await _paymentService.GetUserPaymentsAsync(userId);

            var result = payments.Select(p => new
            {
                paymentId = p.Id,
                orderId = p.RazorpayOrderId,
                amount = p.Amount,
                currency = p.Currency,
                status = p.Status.ToString(),
                course = new
                {
                    id = p.Course.Id,
                    title = p.Course.Title,
                    thumbnailUrl = p.Course.ThumbnailUrl
                },
                createdAt = p.CreatedAt,
                paidAt = p.PaidAt
            });

            return Ok(result);
        }
    }
}
