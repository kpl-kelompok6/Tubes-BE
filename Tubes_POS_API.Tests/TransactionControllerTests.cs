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

public class TransactionControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly TransactionController _controller;

    public TransactionControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        var service = new TransactionService(_db);
        _controller = new TransactionController(service);
    }

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreatedResponse()
    {
        var result = await _controller.Create(new CreateTransactionRequest
        {
            CustomerName = "Budi",
            TableNumber = "5"
        });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionResponse>>(created.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Budi", response.Data.CustomerName);
        Assert.Equal("5", response.Data.TableNumber);
        Assert.Equal("Created", response.Data.Status);
        Assert.StartsWith("TRX-", response.Data.TransactionCode);
    }

    [Fact]
    public async Task AddItem_WithValidRequest_ShouldReturnOkResponse()
    {
        var transactionId = await CreateTransactionAsync();
        SeedMenu();

        var result = await _controller.AddItem(transactionId, new AddItemRequest
        {
            MenuId = 1,
            Quantity = 2
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.Single(response.Data!.Items);
        Assert.Equal(2, response.Data.Items[0].Quantity);
    }

    [Fact]
    public async Task AddItem_WhenMenuNotFound_ShouldThrow()
    {
        var transactionId = await CreateTransactionAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.AddItem(transactionId, new AddItemRequest
        {
            MenuId = 999,
            Quantity = 1
        }));
    }

    [Fact]
    public async Task RemoveItem_WithValidRequest_ShouldReturnOkResponse()
    {
        var transactionId = await CreateTransactionAsync();
        SeedMenu();
        await _controller.AddItem(transactionId, new AddItemRequest { MenuId = 1, Quantity = 2 });

        var result = await _controller.RemoveItem(transactionId, 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.Empty(response.Data!.Items);
    }

    [Fact]
    public async Task UpdateItemQuantity_WithValidRequest_ShouldReturnOkResponse()
    {
        var transactionId = await CreateTransactionAsync();
        SeedMenu();
        await _controller.AddItem(transactionId, new AddItemRequest { MenuId = 1, Quantity = 2 });

        var result = await _controller.UpdateItemQuantity(transactionId, 1, new UpdateItemRequest { Quantity = 5 });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.Equal(5, response.Data!.Items[0].Quantity);
    }

    private async Task<int> CreateTransactionAsync()
    {
        var result = await _controller.Create(new CreateTransactionRequest());
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionResponse>>(created.Value);
        return response.Data!.Id;
    }

    private void SeedMenu()
    {
        _db.Menus.Add(new Menu
        {
            Id = 1,
            Name = "Nasi Goreng",
            Price = 25_000m,
            Category = "Makanan",
            IsAvailable = true
        });
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
