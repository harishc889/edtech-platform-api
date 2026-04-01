using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class RegisterDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = null!;
    }
}
