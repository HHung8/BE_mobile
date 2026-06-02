using MobileApp.Models;
namespace MobileApp.Services.Interfaces;

public interface ISavedPropertyService
{
    Task<List<SavedProperty>> GetSavedAsync(Guid userId);
    Task<(bool Success, string Message)> SaveAsync(Guid userId, Guid propertyId);
    Task<(bool Success, string Message)> UnSaveAsync(Guid userId, Guid propertyId);
    Task<bool> IsSavedAsync(Guid userId, Guid propertyId);
}

