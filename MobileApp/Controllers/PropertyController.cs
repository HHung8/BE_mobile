using Microsoft.AspNetCore.Mvc;
using MobileApp.Models;
using MobileApp.Services.Interfaces;

namespace MobileApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    public PropertyController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PropertyFilterRequest filter)
    {
        var result = await _propertyService.GetAllAsync(filter);
        return  Ok(result);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var result = await _propertyService.GetFeaturedAsync();
        return Ok(result);
    }

    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommend()
    {
        var result = await _propertyService.GetRecommendAsync();
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _propertyService.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy property" });
        return Ok(result);
    }
}