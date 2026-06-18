using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs.Auth;

public sealed class LoginRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username hanya boleh terdiri dari huruf dan angka.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
