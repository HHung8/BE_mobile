namespace MobileApp.Services.Interfaces;

public interface IUserService
{
    Task<(bool Success, string Message, string? AvatarUrl)> UpdateAvatarAsync(Guid userId, IFormFile fromFile);   
}