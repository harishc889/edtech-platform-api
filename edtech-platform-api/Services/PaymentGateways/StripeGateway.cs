using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace edtech_platform_api.Services.PaymentGateways
{
    public class StripeGateway : IPaymentGateway
    {
        private readonly string _apiKey;

        public StripeGateway(IConfiguration config)
        {
            _apiKey = config["Stripe:SecretKey"] ?? throw new Exception("Stripe:SecretKey not configured");
        }

        public async Task<PaymentOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt)
        {
            // TODO: Implement Stripe PaymentIntent API
            // var service = new PaymentIntentService();
            // var intent = await service.CreateAsync(options);

            throw new NotImplementedException("Stripe gateway not yet implemented");
        }

        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
        {
            // TODO: Implement Stripe webhook signature verification
            // Stripe.EventUtility.ConstructEvent(json, signature, webhookSecret);

            throw new NotImplementedException("Stripe gateway not yet implemented");
        }

        public string GetPublicKey()
        {
            // TODO: Return Stripe publishable key
            throw new NotImplementedException("Stripe gateway not yet implemented");
        }

        public string GetGatewayName()
        {
            return "Stripe";
        }
    }
}
