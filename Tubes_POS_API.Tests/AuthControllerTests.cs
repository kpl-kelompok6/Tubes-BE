using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tubes_POS_API.Controllers;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs.Auth;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class AuthControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-at-least-32-characters-long-for-hmac!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        var authService = new AuthService(_db, config);
        _controller = new AuthController(authService);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn201Created()
    {
        var result = await _controller.Register(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu",
            Role = EmployeeRole.Kasir
        });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(created.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("kasir1", response.Data.Username);
        Assert.Equal("Kasir Satu", response.Data.DisplayName);
        Assert.NotEmpty(response.Data.Token);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ShouldThrow()
    {
        await _controller.Register(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu"
        });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _controller.Register(new RegisterRequest
            {
                Username = "kasir1",
                Password = "password456",
                DisplayName = "Kasir Dua"
            }));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200Ok()
    {
        await _controller.Register(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu"
        });

        var result = await _controller.Login(new LoginRequest
        {
            Username = "kasir1",
            Password = "password123"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.NotEmpty(response.Data!.Token);
        Assert.Equal("kasir1", response.Data.Username);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        await _controller.Register(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu"
        });

        var result = await _controller.Login(new LoginRequest
        {
            Username = "kasir1",
            Password = "wrongpassword"
        });

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(unauthorized.Value);
        Assert.False(response.Success);
        Assert.Contains("salah", response.Message);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
