using System.ComponentModel.DataAnnotations;
using Tubes_POS_API.Entities.Enums;

namespace Tubes_POS_API.Models.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required]
    [MaxLength(50)]
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
