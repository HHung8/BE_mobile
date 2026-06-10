using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileApp.Services.Interfaces;

namespace MobileApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPut("avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();
        var (success, message, avatarUrl) = await _userService.UpdateAvatarAsync(userId, file);
        if (!success) return BadRequest(new { message });
        return Ok(new {message, avatarUrl});
    }
}