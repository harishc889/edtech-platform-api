using System;
using Microsoft.Extensions.Configuration;

namespace edtech_platform_api.Services.PaymentGateways
{
    public class PaymentGatewayFactory
    {
        private readonly IConfiguration _config;

        public PaymentGatewayFactory(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IPaymentGateway CreateGateway(string? gatewayName = null)
        {
            // Use configured gateway or default to Razorpay
            var gateway = gatewayName ?? _config["Payment:DefaultGateway"] ?? "Razorpay";

            return gateway.ToLower() switch
            {
                "razorpay" => new RazorpayGateway(_config),
                "stripe" => new StripeGateway(_config),
                // Add more gateways here
                // "payu" => new PayUGateway(_config),
                // "paypal" => new PayPalGateway(_config),
                _ => throw new NotSupportedException($"Payment gateway '{gateway}' is not supported")
            };
        }
    }
}
