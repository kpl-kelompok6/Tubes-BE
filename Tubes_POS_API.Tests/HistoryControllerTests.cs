using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Controllers;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class HistoryControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HistoryController _controller;

    public HistoryControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        var historyService = new HistoryService(_db);
        var reportService = new ReportService(historyService);
        _controller = new HistoryController(historyService, reportService);

        _db.TransactionHistories.Add(new TransactionHistory
        {
            Id = 1,
            TransactionId = 1,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            TotalAmount = 50_000m
        });
        _db.SaveChanges();
    }

    // Tests that history list response is wrapped.
    [Fact]
    public async Task GetAll_ShouldReturnWrappedResponse()
    {
        var result = await _controller.GetAll(null, null, null, 1, 20);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<TransactionHistoryResponse>>>(ok.Value);

        Assert.True(response.Success);
        Assert.Single(response.Data!);
    }

    // Tests that report response is wrapped.
    [Fact]
    public async Task GetReport_ShouldReturnWrappedResponse()
    {
        var result = await _controller.GetReport(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ReportResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetAll_WithDateFilter_ShouldReturnFiltered()
    {
        var result = await _controller.GetAll(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, null, 1, 20);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<TransactionHistoryResponse>>>(ok.Value);

        Assert.True(response.Success);
        Assert.Single(response.Data!);
    }

    [Fact]
    public async Task GetAll_WithPaymentMethodFilter_ShouldReturnFiltered()
    {
        _db.TransactionHistories.Add(new TransactionHistory
        {
            Id = 2,
            TransactionId = 2,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = "qris",
            TotalAmount = 75_000m
        });
        _db.SaveChanges();

        var result = await _controller.GetAll(null, null, "cash", 1, 20);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<TransactionHistoryResponse>>>(ok.Value);

        Assert.Single(response.Data!);
        Assert.Equal("cash", response.Data![0].PaymentMethod);
    }

    [Fact]
    public async Task GetAll_WithPagination_ShouldRespectLimit()
    {
        _db.TransactionHistories.Add(new TransactionHistory
        {
            Id = 2,
            TransactionId = 2,
            TransactionDate = DateTime.UtcNow.AddHours(-1),
            PaymentMethod = "qris",
            TotalAmount = 75_000m
        });
        _db.SaveChanges();

        var result = await _controller.GetAll(null, null, null, 1, 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<TransactionHistoryResponse>>>(ok.Value);

        Assert.Single(response.Data!);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
