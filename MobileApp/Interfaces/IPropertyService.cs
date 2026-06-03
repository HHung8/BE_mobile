using MobileApp.Models;

namespace MobileApp.Services.Interfaces;
public interface IPropertyService
{
    Task<PropertyResponse> GetAllAsync(PropertyFilterRequest filter);
    Task<List<Property>> GetFeaturedAsync();
    Task<List<Property>> GetRecommendAsync();
    Task<Property?> GetByIdAsync(Guid id);
    Task<Property> CreateAsync(CreatePropertyRequest request);
    Task<Property?> UpdateAsync(Guid id, UpdatePropertyRequest request);
    Task<bool> DeleteAsync(Guid id);

}