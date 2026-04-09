using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class CreateOrderDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public int BatchId { get; set; }
    }
}
