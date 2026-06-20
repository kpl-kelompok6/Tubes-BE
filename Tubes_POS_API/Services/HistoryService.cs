using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tubes_POS_API.Services;

public class HistoryService
{
    private readonly AppDbContext _context;

    public HistoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransactionHistory>> GetAllAsync()
    {
        return await _context.TransactionHistories
            .OrderByDescending(h => h.TransactionDate)
            .ToListAsync();
    }

    public async Task<TransactionHistory?> GetByIdAsync(int id)
    {
        return await _context.TransactionHistories
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<List<TransactionHistory>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.TransactionHistories
            .Where(h => h.TransactionDate >= start && h.TransactionDate <= end)
            .OrderByDescending(h => h.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<TransactionHistory>> GetByPaymentMethodAsync(string method)
    {
        return await _context.TransactionHistories
            .Where(h => h.PaymentMethod.ToLower() == method.ToLower())
            .ToListAsync();
    }

    public async Task<List<TransactionHistory>> GetFilteredAsync(DateTime? startDate, DateTime? endDate, string? paymentMethod, int page, int limit)
    {
        var query = _context.TransactionHistories.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(h => h.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(h => h.TransactionDate <= endDate.Value);

        if (!string.IsNullOrWhiteSpace(paymentMethod))
            query = query.Where(h => h.PaymentMethod.ToLower() == paymentMethod.ToLower());

        return await query
            .OrderByDescending(h => h.TransactionDate)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }
}
