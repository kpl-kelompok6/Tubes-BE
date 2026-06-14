using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models.DTOs.Auth;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthService _service;

    public AuthServiceTests()
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

        _service = new AuthService(_db, config);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnAuthResponse()
    {
        var result = await _service.RegisterAsync(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu",
            Role = EmployeeRole.Kasir
        });

        Assert.NotEmpty(result.Token);
        Assert.Equal("kasir1", result.Username);
        Assert.Equal("Kasir Satu", result.DisplayName);
        Assert.Equal("Kasir", result.Role);
        Assert.True(result.EmployeeId > 0);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ShouldThrow()
    {
        await _service.RegisterAsync(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu"
        });

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync(new RegisterRequest
            {
                Username = "kasir1",
                Password = "password456",
                DisplayName = "Kasir Dua"
            }));

        Assert.Contains("sudah digunakan", ex.Message);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnAuthResponse()
    {
        await _service.RegisterAsync(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu",
            Role = EmployeeRole.Kasir
        });

        var result = await _service.LoginAsync(new LoginRequest
        {
            Username = "kasir1",
            Password = "password123"
        });

        Assert.NotEmpty(result.Token);
        Assert.Equal("kasir1", result.Username);
        Assert.Equal("Kasir Satu", result.DisplayName);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldThrow()
    {
        await _service.RegisterAsync(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu"
        });

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(new LoginRequest
            {
                Username = "kasir1",
                Password = "wrongpassword"
            }));

        Assert.Contains("Username atau password salah", ex.Message);
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ShouldThrow()
    {
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(new LoginRequest
            {
                Username = "nobody",
                Password = "anything"
            }));

        Assert.Contains("Username atau password salah", ex.Message);
    }

    [Fact]
    public async Task Register_AdminRole_ShouldReturnAdminRole()
    {
        var result = await _service.RegisterAsync(new RegisterRequest
        {
            Username = "admin1",
            Password = "admin123",
            DisplayName = "Admin Satu",
            Role = EmployeeRole.Admin
        });

        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task Login_ShouldReturnValidJwtWithClaims()
    {
        await _service.RegisterAsync(new RegisterRequest
        {
            Username = "kasir1",
            Password = "password123",
            DisplayName = "Kasir Satu"
        });

        var result = await _service.LoginAsync(new LoginRequest
        {
            Username = "kasir1",
            Password = "password123"
        });

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);

        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Contains(jwt.Claims, c => c.Type == "employeeId" && c.Value == result.EmployeeId.ToString());
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Name && c.Value == "kasir1");
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
