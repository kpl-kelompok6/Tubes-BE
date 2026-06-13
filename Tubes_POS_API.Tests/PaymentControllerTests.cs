using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Controllers;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class PaymentControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        var stateMachine = new PaymentStateMachine();
        var service = new PaymentService(_db, stateMachine);
        _controller = new PaymentController(service);
    }

    [Fact]
    public async Task Process_WithValidRequest_ShouldReturnOkResponse()
    {
        SeedTransaction();

        var result = await _controller.Process(new PaymentRequest
        {
            TransactionId = 1,
            PaidAmount = 60_000m,
            PaymentMethod = "cash"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PaymentResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(50_000m, response.Data.TotalAmount);
        Assert.Equal(10_000m, response.Data.ChangeAmount);
        Assert.Equal("Completed", response.Data.Status);
    }

    [Fact]
    public async Task Process_WhenDuplicatePayment_ShouldThrow()
    {
        SeedTransaction();
        _db.Payments.Add(new Payment
        {
            TransactionId = 1,
            AmountPaid = 60_000m,
            ChangeAmount = 10_000m,
            PaymentMethod = "cash",
            Status = PaymentStatus.Completed
        });
        _db.SaveChanges();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Process(new PaymentRequest
        {
            TransactionId = 1,
            PaidAmount = 60_000m,
            PaymentMethod = "cash"
        }));

        Assert.Contains("sudah memiliki pembayaran", ex.Message);
    }

    [Fact]
    public async Task Process_WithInsufficientAmount_ShouldThrow()
    {
        SeedTransaction();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _controller.Process(new PaymentRequest
        {
            TransactionId = 1,
            PaidAmount = 30_000m,
            PaymentMethod = "cash"
        }));

        Assert.Contains("Uang tidak cukup", ex.Message);
    }

    private void SeedTransaction()
    {
        _db.Menus.Add(new Menu
        {
            Id = 1,
            Name = "Nasi Goreng",
            Price = 25_000m,
            Category = "Makanan",
            IsAvailable = true
        });

        _db.Transactions.Add(new Transaction
        {
            Id = 1,
            TransactionCode = "TRX-TEST",
            CustomerName = "Budi",
            TotalAmount = 50_000m,
            Status = TransactionStatus.Created,
            CreatedAt = DateTime.UtcNow,
            Items =
            [
                new TransactionItem
                {
                    Id = 1,
                    TransactionId = 1,
                    MenuId = 1,
                    Quantity = 2,
                    UnitPrice = 25_000m
                }
            ]
        });

        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
