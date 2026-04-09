using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using edtech_platform_api.Data;
using edtech_platform_api.Models;
using edtech_platform_api.Services.PaymentGateways;

namespace edtech_platform_api.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly EnrollmentService _enrollmentService;
        private readonly PaymentGatewayFactory _gatewayFactory;

        public PaymentService(AppDbContext db, IConfiguration config, EnrollmentService enrollmentService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
            _gatewayFactory = new PaymentGatewayFactory(config);
        }

        public async Task<Payment> CreateOrderAsync(int userId, int courseId, int batchId, string? gatewayName = null)
        {
            // Verify course exists and get price
            var course = await _db.Courses.FindAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found");
            }

            // Verify batch exists and belongs to the course
            var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.CourseId == courseId);
            if (batch == null)
            {
                throw new KeyNotFoundException($"Batch with ID {batchId} not found for this course");
            }

            // Check if user already enrolled in this batch
            var existingEnrollment = await _db.Enrollments
                .AnyAsync(e => e.UserId == userId && e.BatchId == batchId);

            if (existingEnrollment)
            {
                throw new InvalidOperationException("You are already enrolled in this batch");
            }

            // Check if user has a pending/successful payment for this course+batch
            var existingPayment = await _db.Payments
                .FirstOrDefaultAsync(p => p.UserId == userId 
                                       && p.CourseId == courseId 
                                       && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Success));

            if (existingPayment != null)
            {
                if (existingPayment.Status == PaymentStatus.Success)
                {
                    throw new InvalidOperationException("You have already paid for this course");
                }
                // Return existing pending order
                await _db.Entry(existingPayment).Reference(p => p.Course).LoadAsync();
                return existingPayment;
            }

            // Get payment gateway
            var gateway = _gatewayFactory.CreateGateway(gatewayName);

            // Create order in payment gateway
            var orderResult = await gateway.CreateOrderAsync(
                amount: course.Price,
                currency: "INR",
                receipt: $"course_{courseId}_user_{userId}"
            );

            // Create payment record
            var payment = new Payment
            {
                UserId = userId,
                CourseId = courseId,
                Amount = course.Price,
                Status = PaymentStatus.Pending,
                RazorpayOrderId = orderResult.OrderId,
                Currency = orderResult.Currency,
                CreatedAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Load navigation properties
            await _db.Entry(payment).Reference(p => p.Course).LoadAsync();

            return payment;
        }

        public async Task<Payment> VerifyAndCompletePaymentAsync(int userId, string razorpayOrderId, string razorpayPaymentId, string razorpaySignature, int batchId, string? gatewayName = null)
        {
            // Find the payment
            var payment = await _db.Payments
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.RazorpayOrderId == razorpayOrderId && p.UserId == userId);

            if (payment == null)
            {
                throw new KeyNotFoundException("Payment order not found");
            }

            if (payment.Status == PaymentStatus.Success)
            {
                throw new InvalidOperationException("Payment already completed");
            }

            // Get payment gateway
            var gateway = _gatewayFactory.CreateGateway(gatewayName);

            // Verify payment signature
            bool isValid = gateway.VerifyPaymentSignature(razorpayOrderId, razorpayPaymentId, razorpaySignature);

            if (!isValid)
            {
                // Mark payment as failed
                payment.Status = PaymentStatus.Failed;
                payment.RazorpayPaymentId = razorpayPaymentId;
                payment.RazorpaySignature = razorpaySignature;
                await _db.SaveChangesAsync();

                throw new UnauthorizedAccessException("Invalid payment signature. Payment verification failed.");
            }

            // Update payment status
            payment.Status = PaymentStatus.Success;
            payment.RazorpayPaymentId = razorpayPaymentId;
            payment.RazorpaySignature = razorpaySignature;
            payment.PaidAt = DateTime.UtcNow;

            // Create enrollment
            await _enrollmentService.EnrollUserAsync(userId, batchId);

            // Save payment update
            await _db.SaveChangesAsync();

            return payment;
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(string razorpayOrderId)
        {
            return await _db.Payments
                .Include(p => p.Course)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.RazorpayOrderId == razorpayOrderId);
        }

        public async Task<List<Payment>> GetUserPaymentsAsync(int userId)
        {
            return await _db.Payments
                .Include(p => p.Course)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public string GetGatewayPublicKey(string? gatewayName = null)
        {
            var gateway = _gatewayFactory.CreateGateway(gatewayName);
            return gateway.GetPublicKey();
        }

        public string GetGatewayName(string? gatewayName = null)
        {
            var gateway = _gatewayFactory.CreateGateway(gatewayName);
            return gateway.GetGatewayName();
        }
    }
}
