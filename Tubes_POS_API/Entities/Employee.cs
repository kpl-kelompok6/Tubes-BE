using System.ComponentModel.DataAnnotations;
using Tubes_POS_API.Entities.Enums;

namespace Tubes_POS_API.Entities;

public sealed class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; } = EmployeeRole.Kasir;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Transaction> Transactions { get; set; } = [];
}
