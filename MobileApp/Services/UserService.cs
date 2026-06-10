using Dapper;
using MobileApp.Data;
using MobileApp.Services.Interfaces;

namespace MobileApp.Services;

public class UserService : IUserService
{
    private readonly DatabaseConnection _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public UserService(DatabaseConnection db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _config = config;
    }

    public async Task<(bool Success, string Message, string? AvatarUrl)> UpdateAvatarAsync(Guid userId, IFormFile file)
    {
        if (file.Length > 5 * 1024 * 1024)
            return (false, "File không được vượt quá 5 MB", null);
        
        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadFolder);
        // 3. Delete image old if 
        using var conn = _db.CreateConnection();
        var oldUrl = await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT avatar_url From users WHERE id = @Id",
            new { Id = userId }
        );
        if (!string.IsNullOrEmpty(oldUrl))
        {
            var oldFileName = Path.GetFileName(oldUrl);
            var oldPath = Path.Combine(uploadFolder, oldFileName);
            if(File.Exists(oldPath)) File.Delete(oldPath);
        }
        // Save new file
        var ext = Path.GetExtension(file.FileName);
        var newFileName = $"{userId}_{Guid.NewGuid()}{ext}";
        var newPath = Path.Combine(uploadFolder, newFileName);

        using (var stream = new FileStream(newPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        // Create Url response
        var baseUrl = _config["AppSettings:BaseUrl"];
        var avartarUrl = $"{baseUrl}/uploads/avatars/{newFileName}";
        
        // Update Database
        await conn.ExecuteAsync(
            "UPDATE users SET avatar_url = @AvatarUrl, updated_at = NOW() WHERE id = @Id",
            new { AvatarUrl = avartarUrl, Id = userId }
        );
        return (true, "Update Successfully", avartarUrl);
    }
}