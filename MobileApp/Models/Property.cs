namespace MobileApp.Models;

public class Property
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int? AreaSqft { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string[] Images { get; set; } = [];
    public bool IsFeatured { get; set; }
    public bool IsSold { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Request Models ──────────────────────────────
public class PropertyFilterRequest
{
    public string? City { get; set; }
    public string? Type { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Bedrooms { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreatePropertyRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int? AreaSqft { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string[] Images { get; set; } = [];
    public bool IsFeatured { get; set; } = false;
}

public class UpdatePropertyRequest : CreatePropertyRequest
{
    public bool IsSold { get; set; } = false;
}

// ── Response Models ─────────────────────────────
public class PropertyResponse
{
    public List<Property> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}