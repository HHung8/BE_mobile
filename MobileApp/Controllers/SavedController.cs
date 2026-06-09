using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileApp.Services.Interfaces;

namespace MobileApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedController : ControllerBase
{
    private readonly ISavedPropertyService _savedService;
    public SavedController(ISavedPropertyService savedService)
    {
        _savedService = savedService;
    }
    // Lấy userId từ JWT token
    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
    
    // GET /api/Saved
    [HttpGet]
    public async Task<IActionResult> GetSaved()
    {
        var result = await _savedService.GetSavedAsync(GetUserId());
        return Ok(result);
    }
    
    // POST /api/Saved/{propertyId}
    [HttpPost("{propertyId}")]
    public async Task<IActionResult> Save(Guid propertyId)
    {
        var (success, message) = await _savedService.SaveAsync(GetUserId(), propertyId);
        if (!success) return BadRequest(new { message });
        return Ok(new {message});
    }
    
    
    // DELETE /api/Saved/{propertyId}
    [HttpDelete("{propertyId}")]
    public async Task<IActionResult> UnSave(Guid propertyId)
    {
        var (success, message) = await _savedService.UnSaveAsync(GetUserId(), propertyId);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
    
    // GET /api/Saved/check/{propertyId}
    [HttpGet("{propertyId}")]
    public async Task<IActionResult> CheckSaved(Guid propertyId)
    {
        var userId = GetUserId();
        Console.WriteLine($"=== CheckSaved ===");
        Console.WriteLine($"userId: {userId}");
        Console.WriteLine($"propertyId: {propertyId}");
    
        var isSaved = await _savedService.IsSavedAsync(userId, propertyId);
        Console.WriteLine($"isSaved: {isSaved}");
        return Ok(new { isSaved });
    }
    
}