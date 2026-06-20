using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    [MinLength(3, ErrorMessage = "Username minimal harus 3 karakter.")]
    [MaxLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username hanya boleh terdiri dari huruf dan angka.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    public string Password { get; set; } = string.Empty;
}
