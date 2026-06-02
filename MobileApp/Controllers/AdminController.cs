using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileApp.Models;
using MobileApp.Services.Interfaces;

namespace MobileApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    public AdminController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpPost("properties")]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.City))
            return BadRequest(new { message = " Title and City is required " });
        var result = await _propertyService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("properties/{id}")]
    public async Task<IActionResult> UpdateProperty(Guid id, [FromBody] UpdatePropertyRequest request)
    {
        var result = await _propertyService.UpdateAsync(id, request);
        if (result == null) return NotFound(new { message = "Not found property" });
        return Ok(result);
    }

    [HttpDelete("properties/{id}")]
    public async Task<IActionResult> DeleteProperty(Guid id)
    {
        var result = await _propertyService.DeleteAsync(id);
        if(!result) return NotFound(new { message = "Not found property" });
        return Ok(new { message = "Successfully deleted property" });
    }

    [HttpGet("properties/{id}")]
    public async Task<IActionResult> GetProperty(Guid id)
    {
        var result = await _propertyService.GetByIdAsync(id);
        if(result == null) return NotFound();
        return Ok(result);
    }
}