using System.ComponentModel.DataAnnotations;
using Tubes_POS_API.Entities.Enums;

namespace Tubes_POS_API.Models.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username hanya boleh terdiri dari huruf dan angka.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; } = EmployeeRole.Kasir;
}
