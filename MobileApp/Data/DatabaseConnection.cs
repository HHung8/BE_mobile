using System.Data;
using Npgsql;

namespace MobileApp.Data;

public class DatabaseConnection
{
    private readonly string _connectionString;
    // IConfiguration giúp đọc appsettings.json
    public DatabaseConnection(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new Exception(
                                "Connection string 'DefaultConnection' Không tìm thấy trong appsettings.json");
    }
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}