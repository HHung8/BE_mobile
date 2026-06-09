using Dapper;
using MobileApp.Data;
using MobileApp.Models;
using MobileApp.Services.Interfaces;

namespace MobileApp.Services;

public class SavedPropertyService : ISavedPropertyService
{
    private readonly DatabaseConnection _db;
    public SavedPropertyService(DatabaseConnection db)
    {
        _db = db;
    }

    public async Task<List<SavedProperty>> GetSavedAsync(Guid userId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
           SELECT 
                sp.id, sp.user_id AS UserId, sp.property_id AS PropertyId, sp.created_at AS CreatedAt,
                p.id, p.title, p.description, p.price, p.type, p.bedrooms, p.bathrooms, p.area_sqft AS AreaSqft,
                p.address, p.city, p.latitude, p.longitude,
                p.images, p.is_featured AS IsFeatured, 
                p.is_sold AS IsSold, p.created_at AS CreatedAt
           FROM saved_properties sp
           JOIN properties p ON sp.property_id = p.id
           WHERE sp.user_id = @userId
           ORDER BY sp.created_at DESC
        ";
        var result = await conn.QueryAsync<SavedProperty, Property, SavedProperty>(
        sql,
        (saved, property) =>
        {
            saved.Property = property;
            return saved;
        },
        new {UserId = userId},
        splitOn: "id"
        );
        return result.ToList();
    }
    public async Task<(bool Success, string Message)> SaveAsync(Guid userId, Guid propertyId)
    {
        using var conn = _db.CreateConnection();
        var property = await conn.QueryFirstOrDefaultAsync(
            "SELECT id FROM properties WHERE id = @PropertyId",
            new { PropertyId = propertyId }
        );
        if (property == null) return (false, "Not found property");
        var existing = await conn.QueryFirstOrDefaultAsync(
            "SELECT id FROM saved_properties WHERE user_id = @UserId AND property_id = @PropertyId",
            new { UserId = userId, PropertyId = propertyId }
        );
       if(existing != null) return (false, "Property with same id already exists");
       await conn.ExecuteAsync(
           "INSERT INTO saved_properties (user_id, property_id) VALUES (@UserId, @PropertyId)",
           new { UserId = userId, PropertyId = propertyId }
       );
       return (true, "Save Successfully");
    }

    public async Task<(bool Success, string Message)> UnSaveAsync(Guid userId, Guid propertyId)
    {
        using var conn = _db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "DELETE FROM saved_properties WHERE user_id = @UserId AND property_id = @PropertyId",
            new { UserId = userId, PropertyId = propertyId }
        );
        return affected > 0 ? (true, "Đã bỏ lưu") : (false, "Không tìm thấy");
    }
    
    public async Task<bool> IsSavedAsync(Guid userId, Guid propertyId)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM saved_properties WHERE user_id = @userId AND property_id = @propertyId",
            new { userId, propertyId }
        );
        return count > 0;
    }
}