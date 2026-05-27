using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileApp.Models;
using MobileApp.Services.Interfaces;

namespace MobileApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    // Post api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new {message = "Vui lòng điền đầy đủ thông tin"});
        }

        if (request.Password.Length < 6)
            return BadRequest(new { meesagge = "Mật khẩu phải có ít nhất 6 ký tự" });
        var (success, message, data) = await _authService.RegisterAsync(request);
        if(!success) return BadRequest(new {message});
        return Ok(data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { meesage = "Vui lòng điền đầy đủ thông tin" });
        }

        var (success, message, data) = await _authService.LoginAsync(request);
        if (!success) return Unauthorized(new { message });
        return Ok(data);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token không được để trống" });
        var (success, message, data) = await _authService.RefreshTokenAsync(request.RefreshToken);
        if(!success) return Unauthorized(new { message });
        return Ok(data);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        return Ok(new { message = "Đăng xuất thành công" });
    }
    
    
}