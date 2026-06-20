using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Helpers;
using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly PaymentStateMachine _stateMachine;

    public PaymentService(AppDbContext db, PaymentStateMachine stateMachine)
    {
        _db = db;
        _stateMachine = stateMachine;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        var transaction = await _db.Transactions
            .Include(t => t.Items)
            .Include(t => t.Payment)
            .Include(t => t.Cashier)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId)
            ?? throw new KeyNotFoundException($"Transaksi dengan ID {request.TransactionId} tidak ditemukan.");

        if (transaction.Payment is not null)
        {
            throw new InvalidOperationException("Transaksi ini sudah memiliki pembayaran.");
        }

        if (transaction.Status != TransactionStatus.Created)
        {
            throw new InvalidOperationException("Transaksi sudah diproses dan tidak bisa dibayar ulang.");
        }

        if (transaction.Items.Count == 0)
        {
            throw new InvalidOperationException("Transaksi belum memiliki item.");
        }

        var totalAmount = transaction.TotalAmount;

        if (request.PaidAmount < totalAmount)
        {
            _stateMachine.Fail();
            transaction.Status = TransactionStatus.Cancelled;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            throw new ArgumentException("Uang tidak cukup.");
        }

        _stateMachine.MarkPaid();
        _stateMachine.Complete();

        var change = request.PaidAmount - totalAmount;
        var paymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "cash" : request.PaymentMethod;

        var payment = new Payment
        {
            Code = CodeHelper.GenerateCode("PAY"),
            TransactionId = transaction.Id,
            AmountPaid = request.PaidAmount,
            ChangeAmount = change,
            PaymentMethod = paymentMethod,
            Status = _stateMachine.CurrentState,
            CreatedAt = DateTime.UtcNow
        };

        transaction.PaidAmount = request.PaidAmount;
        transaction.Change = change;
        transaction.PaymentMethod = paymentMethod;
        transaction.Status = TransactionStatus.Completed;
        transaction.UpdatedAt = DateTime.UtcNow;

        _db.TransactionHistories.Add(new TransactionHistory
        {
            Code = CodeHelper.GenerateCode("HIST"),
            TransactionId = transaction.Id,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = paymentMethod,
            TotalAmount = totalAmount,
            CashierName = transaction.Cashier?.DisplayName
        });

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return new PaymentResponse
        {
            PaymentId = payment.Id,
            Code = payment.Code,
            TransactionId = transaction.Id,
            TransactionCode = transaction.TransactionCode,
            TotalAmount = totalAmount,
            PaidAmount = payment.AmountPaid,
            ChangeAmount = payment.ChangeAmount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<List<PaymentResponse>> GetAllPaymentsAsync(DateTime? startDate, DateTime? endDate, string? paymentMethod, int page, int limit)
    {
        var query = _db.Payments
            .Include(p => p.Transaction)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(p => p.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.CreatedAt <= endDate.Value);

        if (!string.IsNullOrWhiteSpace(paymentMethod))
            query = query.Where(p => p.PaymentMethod.ToLower() == paymentMethod.ToLower());

        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return payments.Select(p => new PaymentResponse
        {
            PaymentId = p.Id,
            Code = p.Code,
            TransactionId = p.TransactionId,
            TransactionCode = p.Transaction?.TransactionCode ?? string.Empty,
            TotalAmount = p.Transaction?.TotalAmount ?? 0m,
            PaidAmount = p.AmountPaid,
            ChangeAmount = p.ChangeAmount,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    public async Task<PaymentResponse?> GetPaymentByIdAsync(int id)
    {
        var payment = await _db.Payments
            .Include(p => p.Transaction)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment is null) return null;

        return new PaymentResponse
        {
            PaymentId = payment.Id,
            Code = payment.Code,
            TransactionId = payment.TransactionId,
            TransactionCode = payment.Transaction?.TransactionCode ?? string.Empty,
            TotalAmount = payment.Transaction?.TotalAmount ?? 0m,
            PaidAmount = payment.AmountPaid,
            ChangeAmount = payment.ChangeAmount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt
        };
    }
}
