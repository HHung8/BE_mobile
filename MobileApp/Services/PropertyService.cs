using MobileApp.Data;
using MobileApp.Models;
using MobileApp.Services.Interfaces;
using Dapper;

namespace MobileApp.Services;

public class PropertyService : IPropertyService
{
    private readonly DatabaseConnection _db;
    public PropertyService(DatabaseConnection db) {
        _db = db;
    }

    public async Task<PropertyResponse> GetAllAsync(PropertyFilterRequest filter)
    {
        using var conn = _db.CreateConnection();
        // Build dynamic where
        var conditions = new List<string>();
        var parameters = new DynamicParameters();
        if (!string.IsNullOrEmpty(filter.City))
        {
            conditions.Add("LOWER(city) = LOWER(@City)");
            parameters.Add("City", filter.City);
        }

        if (!string.IsNullOrEmpty(filter.Type))
        {
            conditions.Add("type=@Type");
            parameters.Add("Type", filter.Type);
        }

        if (filter.MinPrice.HasValue)
        {
            conditions.Add("price >=@MinPrice");
            parameters.Add("MinPrice", filter.MinPrice);
        }

        if (filter.MaxPrice.HasValue)
        {
            conditions.Add("price <=@MaxPrice");
            parameters.Add("MaxPrice", filter.MaxPrice);
        }

        if (filter.Bedrooms.HasValue)
        {
            conditions.Add("bedrooms >=@Bedrooms");
            parameters.Add("Bedrooms", filter.Bedrooms);
        }

        if (!string.IsNullOrEmpty(filter.Search))
        {
            conditions.Add("(LOWER(title) LIKE @Search OR LOWER(address) LIKE @Search OR LOWER(city) LIKE @Search)");
            parameters.Add("Search", $"%{filter.Search.ToLower()}%");
        }
        
        var where = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : string.Empty;
        // Count total
        var countSql = $"SELECT COUNT(*) FROM properties {where}"; 
        var total = await conn.ExecuteScalarAsync<int>(countSql, parameters);

        // Paginate
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Limit", filter.PageSize);
        parameters.Add("Offset", offset);


        var sql = $@"
                  SELECT id, title, description, price, type, bedrooms, 
                       bathrooms, area_sqft AS AreaSqft, address, city, latitude, longitude,
                       images, is_featured AS IsFeatured,
                       is_sold AS IsSold, created_at AS CreatedAt 
                  FROM properties
                  {where}
                  ORDER BY created_at DESC
                  LIMIT @Limit OFFSET @Offset";
        var items = (await conn.QueryAsync<Property>(sql, parameters)).ToList();
        return new PropertyResponse
        {
            Items = items,
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)total / filter.PageSize),
        };
    }

    public async Task<List<Property>> GetFeaturedAsync()
    {
       using var conn = _db.CreateConnection();
       var sql = @"SELECT id, title, description, price, type, bedrooms, 
                        bathrooms, area_sqft AS AreaSqft, address, city, latitude, longitude,
                        images, is_featured AS IsFeatured,
                        is_sold AS IsSold, created_at AS CreatedAt
                FROM properties
                WHERE is_featured = TRUE AND is_sold = FALSE
                ORDER BY created_at DESC";
       return (await conn.QueryAsync<Property>(sql)).ToList();
    }
    
    public async Task<Property?> GetByIdAsync(Guid id)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
            SELECT id, title, description, price, type,
                   bedrooms, bathrooms, area_sqft AS AreaSqft,
                   address, city, latitude, longitude,
                   images, is_featured AS IsFeatured,
                   is_sold AS IsSold, created_at AS CreatedAt
            FROM properties
            WHERE id = @Id";
        return await conn.QueryFirstOrDefaultAsync<Property>(sql, new { Id = id }); 
    }

    public async Task<Property> CreateAsync(CreatePropertyRequest request)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
            INSERT INTO properties
            (title, description, price, type, bedrooms, bathrooms,
             area_sqft, address, city, latitude, longitude, images, is_featured)     
            VALUES 
            (@Title, @Description, @Price, @Type, @Bedrooms, @Bathrooms,
             @AreaSqft, @Address, @City, @Latitude, @Longitude, @Images, @IsFeatured)
            RETURNING
                id, title, description, price, type, bedrooms, bathrooms,
                area_sqft AS AreaSqft, address, city, latitude, longitude,
                images, is_featured AS IsFeatured, is_sold AS IsSold, 
                created_at AS CreatedAt 
             ";
        return await conn.QueryFirstAsync<Property>(sql, request);
    }

    public async Task<Property?> UpdateAsync(Guid id, UpdatePropertyRequest request)
    {
        using var conn = _db.CreateConnection();
        var sql = @"
            UPDATE properties SET
                                  title = @Title,
                                  description = @Description,
                                  price = @Price,
                                  type = @Type,
                                  bedrooms = @Bedrooms,
                                  bathrooms = @Bathrooms,
                                  area_sqft = @AreaSqft,
                                  address = @Address,
                                  city = @City,
                                  latitude = @Latitude,
                                  longitude = @Longitude,
                                  images = @Images,
                                  is_featured = @IsFeatured,
                                  is_sold = @IsSold
            Where id = @Id
            RETURNING
                id, title, description, price, type, bedrooms, bathrooms,
                area_sqft AS AreaSqft, address, city, latitude, longitude,
                images, is_featured AS IsFeatured, is_sold AS IsSold,
                created_at AS CreatedAt                        
            ";
        return await conn.QueryFirstOrDefaultAsync<Property>(sql, new
        {
            request.Title, request.Description, request.Price, request.Type,
            request.Bedrooms, request.Bathrooms, request.AreaSqft,
            request.Address, request.City, request.Latitude, request.Longitude,
            request.Images, request.IsFeatured, request.IsSold,
            Id = id
        });
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        using var conn = _db.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "DELETE FROM properties WHERE id = @Id",
            new { Id = id }
        );
        return affected > 0;
    }
}