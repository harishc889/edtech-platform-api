namespace edtech_platform_api.Models.Dtos;

public class ResetPasswordRequestDto
{
    public string? Token { get; set; }
    public string? NewPassword { get; set; }
}
