using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class LoginDto
    {
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
