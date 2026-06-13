using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs.Auth;

public sealed class LoginRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
