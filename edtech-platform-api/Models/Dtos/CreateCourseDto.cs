using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class CreateCourseDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        public bool IsPublished { get; set; } = false;
    }
}
