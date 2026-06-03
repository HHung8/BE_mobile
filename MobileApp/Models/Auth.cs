namespace MobileApp.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
}

// ── Request Models ──────────────────────────────
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
 
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
 
public class RefreshTokenRequest
{
    public string? RefreshToken { get; set; } = string.Empty;
}
 
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
 
public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}
 
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

// ── Response Models ─────────────────────────────
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}