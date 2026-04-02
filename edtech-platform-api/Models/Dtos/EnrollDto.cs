using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class EnrollDto
    {
        [Required]
        public int BatchId { get; set; }
    }
}
