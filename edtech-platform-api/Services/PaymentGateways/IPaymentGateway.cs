using System.Threading.Tasks;

namespace edtech_platform_api.Services.PaymentGateways
{
    public interface IPaymentGateway
    {
        /// <summary>
        /// Creates a payment order in the gateway
        /// </summary>
        Task<PaymentOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt);

        /// <summary>
        /// Verifies payment signature/webhook from the gateway
        /// </summary>
        bool VerifyPaymentSignature(string orderId, string paymentId, string signature);

        /// <summary>
        /// Gets the gateway's public key/identifier for frontend
        /// </summary>
        string GetPublicKey();

        /// <summary>
        /// Gets the gateway name (Razorpay, Stripe, etc.)
        /// </summary>
        string GetGatewayName();
    }

    public class PaymentOrderResult
    {
        public string OrderId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string GatewayName { get; set; } = null!;
    }
}
