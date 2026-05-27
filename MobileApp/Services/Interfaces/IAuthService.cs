using MobileApp.Models;

namespace MobileApp.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, AuthResponse? Data)> RegisterAsync(RegisterRequest request);

    Task<(bool Success, string Message, AuthResponse? Data)> LoginAsync(LoginRequest request);

    Task<(bool Success, string Message, AuthResponse? Data)> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
}