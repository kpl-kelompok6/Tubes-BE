using System.ComponentModel.DataAnnotations;
using Tubes_POS_API.Entities.Enums;

namespace Tubes_POS_API.Models.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    [MinLength(3, ErrorMessage = "Username minimal harus 3 karakter.")]
    [MaxLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username hanya boleh terdiri dari huruf dan angka.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    [MinLength(6, ErrorMessage = "Password minimal harus 6 karakter.")]
    [MaxLength(100, ErrorMessage = "Password maksimal 100 karakter.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display Name wajib diisi.")]
    [MaxLength(100, ErrorMessage = "Display Name maksimal 100 karakter.")]
    public string DisplayName { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; } = EmployeeRole.Kasir;
}
