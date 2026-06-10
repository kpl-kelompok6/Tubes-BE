using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

public class ReportService
{
    private readonly HistoryService _historyService;

    public ReportService(HistoryService historyService)
    {
        _historyService = historyService;
    }

    public async Task<ReportResponse> GetReportAsync(DateTime start, DateTime end)
    {
        var data = await _historyService.GetByDateRangeAsync(start, end);

        int totalTransaksi = data.Count;
        decimal totalPendapatan = data.Sum(h => h.TotalAmount);
        decimal rataRata = totalTransaksi > 0 ? totalPendapatan / totalTransaksi : 0;

        string[] metodePembayaran = { "cash", "debit", "qris", "transfer" };

        var breakdown = new Dictionary<string, decimal>();
        foreach (var metode in metodePembayaran)
        {
            decimal total = data
                .Where(h => h.PaymentMethod.ToLower() == metode)
                .Sum(h => h.TotalAmount);

            breakdown[metode] = total;
        }

        return new ReportResponse
        {
            StartDate = start,
            EndDate = end,
            TotalTransaksi = totalTransaksi,
            TotalPendapatan = totalPendapatan,
            RataRata = rataRata,
            Breakdown = breakdown
        };
    }
}
