using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tubes_POS_API.Entities
{
    public class TransactionHistory
    {
        [Key]
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        public int TransactionId { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(30)]
        public string TransactionCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? CustomerName { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? TableNumber { get; set; }

    public string? CashierName { get; set; }

    public string PaymentMethod { get; set; } = "";

    public decimal TotalAmount { get; set; }

    [Precision(18, 2)]
    public decimal PaidAmount { get; set; }

    [Precision(18, 2)]
    public decimal Change { get; set; }
    }
}