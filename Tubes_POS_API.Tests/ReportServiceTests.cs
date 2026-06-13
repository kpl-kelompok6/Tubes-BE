using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class ReportServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        var historyService = new HistoryService(_db);
        _service = new ReportService(historyService);
    }

    [Fact]
    public async Task GetReportAsync_EmptyRange_ShouldReturnZeroTotals()
    {
        var result = await _service.GetReportAsync(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-9));

        Assert.Equal(0, result.TotalTransaksi);
        Assert.Equal(0m, result.TotalPendapatan);
        Assert.Equal(0m, result.RataRata);
        Assert.All(result.Breakdown, kvp => Assert.Equal(0m, kvp.Value));
    }

    [Fact]
    public async Task GetReportAsync_SingleTransaction_ShouldMatch()
    {
        _db.TransactionHistories.Add(new TransactionHistory
        {
            Id = 1,
            TransactionId = 1,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            TotalAmount = 50_000m
        });
        _db.SaveChanges();

        var result = await _service.GetReportAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        Assert.Equal(1, result.TotalTransaksi);
        Assert.Equal(50_000m, result.TotalPendapatan);
        Assert.Equal(50_000m, result.RataRata);
        Assert.Equal(50_000m, result.Breakdown["cash"]);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
