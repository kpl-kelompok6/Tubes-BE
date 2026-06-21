namespace Tubes_POS_API.Models.DTOs;

public sealed class TransactionHistoryResponse
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public int TransactionId { get; set; }

    public string TransactionCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? CustomerName { get; set; }

    public string? TableNumber { get; set; }

    public string? CashierName { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal Change { get; set; }
}
