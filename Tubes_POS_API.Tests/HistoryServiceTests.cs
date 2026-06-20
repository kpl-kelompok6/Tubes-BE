using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class HistoryServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HistoryService _service;

    public HistoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _service = new HistoryService(_db);

        _db.TransactionHistories.AddRange(
            new TransactionHistory { Id = 1, TransactionId = 1, TransactionDate = DateTime.UtcNow.AddDays(-2), PaymentMethod = "cash", TotalAmount = 50_000m },
            new TransactionHistory { Id = 2, TransactionId = 2, TransactionDate = DateTime.UtcNow.AddDays(-1), PaymentMethod = "qris", TotalAmount = 75_000m },
            new TransactionHistory { Id = 3, TransactionId = 3, TransactionDate = DateTime.UtcNow, PaymentMethod = "cash", TotalAmount = 25_000m }
        );
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnRecord()
    {
        var result = await _service.GetByIdAsync(2);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
        Assert.Equal(75_000m, result.TotalAmount);
        Assert.Equal("qris", result.PaymentMethod);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFilteredAsync_WithDateRange_ShouldFilter()
    {
        var results = await _service.GetFilteredAsync(DateTime.UtcNow.AddDays(-1).AddMinutes(-1), DateTime.UtcNow, null, 1, 20);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetFilteredAsync_WithPaymentMethod_ShouldFilter()
    {
        var results = await _service.GetFilteredAsync(null, null, "qris", 1, 20);

        Assert.Single(results);
    }

    [Fact]
    public async Task GetFilteredAsync_WithPagination_ShouldRespectLimit()
    {
        var results = await _service.GetFilteredAsync(null, null, null, 1, 2);

        Assert.Equal(2, results.Count);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
