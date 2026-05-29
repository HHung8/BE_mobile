using MobileApp.Data;
using MobileApp.Models;
using MobileApp.Services.Interfaces;
using Dapper;

namespace MobileApp.Services;

public class AuthService : IAuthService
{
    private readonly DatabaseConnection _db;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    
    public AuthService(DatabaseConnection db, ITokenService tokenService, IConfiguration config, IEmailService emailService)
    {
        _db = db;
        _tokenService = tokenService;
        _config = config;
        _emailService = emailService;
    }
    
    // Đăng ký
    public async Task<(bool Success, string Message, AuthResponse? Data)> RegisterAsync(RegisterRequest request)
    {
        using var conn = _db.CreateConnection();
        // 1. Check email early 
        var existingUser = await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE email = @Email OR username = @Username",
            new {request.Email, request.Username}
        );
        if (existingUser != null)
        {
            if (existingUser.Email == request.Email)
                return (false, "Email đã được sử dụng", null);
            else                                                    // ✅ Sửa: thêm else
                return (false, "Username đã được sử dụng", null);
        }
        // 2. Mã hoá mật khẩu băng Bcrypt ()
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        // 3. Lưu user vào database
        var newUser = await conn.QueryFirstAsync<User>(
            @"INSERT INTO users (username, email, password_hash)
              VALUES (@Username, @Email, @PasswordHash)
              RETURNING id, username, email,
                        password_hash AS PasswordHash,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt",   // ✅ Sửa: thêm AS thay vì RETURNING *
            new { request.Username, request.Email, PasswordHash = passwordHash }
        );
        return await CreateAuthResponseAsync(newUser, conn);
    }
    
    // Đăng nhập
    public async Task<(bool Success, string Message, AuthResponse? Data)> LoginAsync(LoginRequest request)
    {
        using var conn = _db.CreateConnection();
        var user = await conn.QueryFirstOrDefaultAsync<User>(
            @"SELECT id, username, email, 
                    password_hash AS PasswordHash,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM users WHERE email = @Email
            ",
            new { request.Email }
        );
        if (user == null) return (false, "Email hoặc mật khẩu không đúng", null);
        // 2. Kiểm tra mật khẩu
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid) return (false, "Email hoặc mật khẩu không đúng", null);
        // 3. Tạo token và trả về
        return await CreateAuthResponseAsync(user, conn);
    }
    
    // Làm mới token khi token hết hạn
    public async Task<(bool Success, string Message, AuthResponse? Data)> RefreshTokenAsync(string refreshToken)
    {
        using var conn = _db.CreateConnection();
        // 1. Tìm refresh token trong database
        var storedToken = await conn.QueryFirstOrDefaultAsync<RefreshToken>(
            @"SELECT id, user_id AS UserId, token,
                     expires_at AS ExpiresAt,
                     created_at AS CreatedAt,
                     is_revoked AS IsRevoked
              FROM refresh_tokens WHERE token = @Token",   // ✅ Sửa: thêm AS
            new { Token = refreshToken }
        );
        if (storedToken == null)
            return (false, "Refresh token không hợp lệ", null);
        if (storedToken.IsRevoked)
            return (false, "Refresh token đã bị thu hồi", null);
        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return (false, "Refresh token đã hết hạn", null);
        var user = await conn.QueryFirstOrDefaultAsync<User>(
            @"SELECT id, username, email,
                     password_hash AS PasswordHash,
                     created_at AS CreatedAt,
                     updated_at AS UpdatedAt
              FROM users WHERE id = @Id",
            new { Id = storedToken.UserId }
        );
        if(user == null) return (false, "User not found", null);
        await conn.ExecuteAsync(
            "UPDATE refresh_tokens SET is_revoked = TRUE WHERE id = @id",
            new { id = storedToken.Id }
        );
        return await  CreateAuthResponseAsync(user, conn);
    }
    
    // Đăng xuất
    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        using var conn = _db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "UPDATE refresh_tokens SET is_revoked = TRUE WHERE token = @Token",
            new { Token = refreshToken }
        );
        return affected > 0;
    }
    
    // Tạo AuthResponse
    private async Task<(bool Success, string Message, AuthResponse? Data)> CreateAuthResponseAsync(User user, System.Data.IDbConnection conn)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        var refreshExpirationDays = int.Parse(_config["JwtSettings:RefreshTokenExpirationDays"] ?? "7"); 
        var refreshExpiration = DateTime.UtcNow.AddDays(refreshExpirationDays);
        
        // Save refreshToken in database
        await conn.ExecuteAsync(
            @"INSERT INTO refresh_tokens (user_id, token, expires_at)
                VALUES (@user_id, @token, @expires_at)",
            new { user_id = user.Id, token = refreshToken, expires_at = refreshExpiration}
        );
        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
            }
        };
        return (true, "Thành công", response);
    }
    
    // Quên mật khẩu
    public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
    {
        using var conn = _db.CreateConnection();
        var user = await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE email = @Email",
            new { Email = email }
        );
        if (user == null) return (false, "Email không tồn tại");
        // Tạo OTP
        var otp = new Random().Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(10);
        await conn.ExecuteAsync(
            "DELETE FROM password_reset_otps WHERE email = @Email",
            new { Email = email }
        );
        await conn.ExecuteAsync(
            @"INSERT INTO password_reset_otps (email, otp, expires_at) 
                  VALUES (@Email, @Otp, @Expires_at)",
            new { Email = email, Otp = otp, Expires_at = expiresAt }
        );
        await _emailService.SendOtpEmailAsync(email, otp);
        return (true, "Đã gửi mã OTP");
    }
    
    // ResetPassword
    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
    {
        using var conn = _db.CreateConnection();
        // Check OTP
        var otpRecord = await conn.QueryFirstOrDefaultAsync(
            @"SELECT * FROM password_reset_otps 
                  WHERE email = @Email AND otp = @Otp AND is_used = FALSE",
            new { Email = request.Email, Otp = request.Otp }
        );
        if (otpRecord == null) return (false, "Mã OTP không đúng");
        if (otpRecord.expires_at < DateTime.UtcNow) return (false, "Mã OTP đã hết hạn");
        // Đổi mật 
        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await conn.ExecuteAsync(
            "UPDATE users SET password_hash = @Hash WHERE email = @Email",
            new { Hash = newHash, Email = request.Email }
        );
        await conn.ExecuteAsync(
            "UPDATE password_reset_otps SET is_used = TRUE WHERE email = @Email",
            new { Email = request.Email }
        );
        return (true, "Đổi mật khẩu thành công");
    }

    public async Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequest request)
    {
        using var conn = _db.CreateConnection();
        var otpRecord = await conn.QueryFirstOrDefaultAsync(
            @"SELECT * FROM password_reset_otps 
                  WHERE email = @Email AND otp = @Otp AND is_used = FALSE",
            new { Email = request.Email, Otp = request.Otp }
        );
        if (otpRecord == null) return (false, "Mã OTP không đúng");
        if (otpRecord.expires_at < DateTime.UtcNow) return (false, "Mã OTP đã hết hạn");
        return (true, "Mã OTP hợp lệ");
    }
    
    
}