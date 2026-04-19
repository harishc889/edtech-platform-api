using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace edtech_platform_api.Services.PaymentGateways
{
    public class RazorpayGateway : IPaymentGateway
    {
        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly RazorpayClient _client;

        public RazorpayGateway(IConfiguration config)
        {
            _keyId = config["Razorpay:KeyId"] ?? throw new Exception("Razorpay:KeyId not configured");
            _keySecret = config["Razorpay:KeySecret"] ?? throw new Exception("Razorpay:KeySecret not configured");

            // Initialize Razorpay client
            _client = new RazorpayClient(_keyId, _keySecret);
        }

        public async Task<PaymentOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt)
        {
            try {

                //prepare order options
                var options = new Dictionary<string, object>
                {
                    { "amount", (int)(amount * 100) }, // Razorpay expects amount in paise
                    { "currency", currency },
                    { "receipt", receipt },
                    { "payment_capture", 1 } // Auto-capture payment
                };

                // Create order using Razorpay client
                var order = _client.Order.Create(options);

                return new PaymentOrderResult
                {
                    OrderId = order["id"].ToString(),
                    Amount = amount,
                    Currency = currency,
                    GatewayName = GetGatewayName()
                };

            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception("Failed to create Razorpay order: " + ex.Message);
            }

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
            catch (Exception ex)
            {
                throw new Exception($"Failed to verify Razorpay signature: {ex.Message}", ex);
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

    }
}
