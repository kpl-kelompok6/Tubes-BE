using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs.Auth;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new ApiResponse<AuthResponse>
            {
                Message = "Registrasi berhasil.",
                Data = response
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (DbUpdateException)
        {
            return Conflict(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Username sudah digunakan.",
                Data = null
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(new ApiResponse<AuthResponse>
            {
                Message = "Login berhasil.",
                Data = response
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Username atau password salah.",
                Data = null
            });
        }
    }
}
