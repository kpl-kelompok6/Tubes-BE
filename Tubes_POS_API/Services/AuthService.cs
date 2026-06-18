using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models.DTOs.Auth;

namespace Tubes_POS_API.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var username = request.Username.Trim();

        if (await _db.Employees.AnyAsync(e => e.Username == username))
            throw new ArgumentException("Username sudah digunakan.");

        var employee = new Employee
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return BuildAuthResponse(employee);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Username == request.Username)
            ?? throw new UnauthorizedAccessException("Username atau password salah.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash))
            throw new UnauthorizedAccessException("Username atau password salah.");

        return BuildAuthResponse(employee);
    }

    private AuthResponse BuildAuthResponse(Employee employee)
    {
        return new AuthResponse
        {
            Token = GenerateJwtToken(employee),
            EmployeeId = employee.Id,
            Username = employee.Username,
            DisplayName = employee.DisplayName,
            Role = employee.Role.ToString()
        };
    }

    private string GenerateJwtToken(Employee employee)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("employeeId", employee.Id.ToString()),
            new Claim(ClaimTypes.Name, employee.Username),
            new Claim(ClaimTypes.GivenName, employee.DisplayName),
            new Claim(ClaimTypes.Role, employee.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
