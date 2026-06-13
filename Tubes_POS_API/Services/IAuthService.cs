using Tubes_POS_API.Models.DTOs.Auth;

namespace Tubes_POS_API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
