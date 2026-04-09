using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class VerifyPaymentDto
    {
        [Required]
        public string RazorpayOrderId { get; set; } = null!;

        [Required]
        public string RazorpayPaymentId { get; set; } = null!;

        [Required]
        public string RazorpaySignature { get; set; } = null!;

        [Required]
        public int BatchId { get; set; }
    }
}
