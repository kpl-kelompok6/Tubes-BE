using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Repositories;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class PerformanceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly string _dbName;

    public PerformanceTests()
    {
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        _db = new AppDbContext(options);
    }

    [Fact]
    public async Task ConcurrentTransactionCreation_50Tasks_ShouldCompleteUnder5Seconds()
    {
        SeedMenusForConcurrent();

        var stopwatch = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(_dbName)
                .Options;
            using var db = new AppDbContext(opts);
            var service = new TransactionService(db);
            var tx = await service.CreateTransactionAsync(new CreateTransactionRequest
            {
                CustomerName = $"Concurrent {i}"
            });
            await service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = (i % 50) + 1, Quantity = 1 });
        });

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        var checkOpts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        using var checkDb = new AppDbContext(checkOpts);
        var all = await checkDb.Transactions.CountAsync();

        Assert.Equal(50, all);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5),
            $"50 transaksi concurrent memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 5000ms)");
    }

    [Fact]
    public async Task HistoryFiltering_1000Records_ShouldCompleteUnder2Seconds()
    {
        var now = DateTime.UtcNow;
        for (int i = 0; i < 1000; i++)
        {
            _db.TransactionHistories.Add(new TransactionHistory
            {
                TransactionDate = now.AddDays(-i),
                PaymentMethod = i % 3 == 0 ? "cash" : i % 3 == 1 ? "qris" : "debit",
                TotalAmount = 10_000m + (i * 100m)
            });
        }
        _db.SaveChanges();

        var historyService = new HistoryService(_db);
        var stopwatch = Stopwatch.StartNew();

        var filtered = await historyService.GetByDateRangeAsync(now.AddDays(-100), now.AddDays(1));

        stopwatch.Stop();

        Assert.Equal(101, filtered.Count);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2),
            $"Filter 1000 record memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 2000ms)");
    }

    [Fact]
    public async Task BulkMenuCreation_100Menus_ShouldCompleteUnder2Seconds()
    {
        var menuService = new MenuService(new MenuRepository(_db));
        var stopwatch = Stopwatch.StartNew();

        for (int i = 1; i <= 100; i++)
        {
            menuService.Add(new Menu
            {
                Name = $"Bulk Menu {i}",
                Price = 5_000m + (i * 100m),
                Category = i % 2 == 0 ? "Makanan" : "Minuman",
                IsAvailable = true
            });
        }

        stopwatch.Stop();

        var all = menuService.GetAll();
        Assert.Equal(100, all.Count);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2),
            $"Buat 100 menu memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 2000ms)");
    }

    private void SeedMenusForConcurrent()
    {
        for (int i = 1; i <= 50; i++)
        {
            _db.Menus.Add(new Menu
            {
                Id = i,
                Name = $"Menu {i}",
                Price = 10_000m,
                Category = "Makanan",
                IsAvailable = true
            });
        }
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
