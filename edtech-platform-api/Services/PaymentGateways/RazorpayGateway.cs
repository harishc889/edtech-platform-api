using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace edtech_platform_api.Services.PaymentGateways
{
    public class RazorpayGateway : IPaymentGateway
    {
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayGateway(IConfiguration config)
        {
            _keyId = config["Razorpay:KeyId"] ?? throw new Exception("Razorpay:KeyId not configured");
            _keySecret = config["Razorpay:KeySecret"] ?? throw new Exception("Razorpay:KeySecret not configured");
        }

        public async Task<PaymentOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt)
        {
            // TODO: In production, call actual Razorpay API
            // var client = new RazorpayClient(_keyId, _keySecret);
            // var order = client.Order.Create(options);

            // Mock implementation for now
            var orderId = GenerateRazorpayOrderId();

            return await Task.FromResult(new PaymentOrderResult
            {
                OrderId = orderId,
                Amount = amount,
                Currency = currency,
                GatewayName = GetGatewayName()
            });
        }

        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
        {
            try
            {
                // Razorpay signature format: HMAC SHA256 of "orderId|paymentId"
                var text = $"{orderId}|{paymentId}";
                var keyBytes = Encoding.UTF8.GetBytes(_keySecret);
                var textBytes = Encoding.UTF8.GetBytes(text);

                using (var hmac = new HMACSHA256(keyBytes))
                {
                    var hashBytes = hmac.ComputeHash(textBytes);
                    var generatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                    return generatedSignature == signature.ToLower();
                }
            }
            catch
            {
                return false;
            }
        }

        public string GetPublicKey()
        {
            return _keyId;
        }

        public string GetGatewayName()
        {
            return "Razorpay";
        }

        private string GenerateRazorpayOrderId()
        {
            return $"order_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 14)}";
        }
    }
}
