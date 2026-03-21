using Microsoft.Data.Sqlite;

namespace BarangayConnect.Services;

public class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var configuredPath = configuration.GetConnectionString("DefaultConnection") ?? "App_Data/barangayconnect.db";
        var relativePath = configuredPath.Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(environment.ContentRootPath, relativePath);
        var directory = Path.GetDirectoryName(absolutePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = $"Data Source={absolutePath}";
    }

    public SqliteConnection CreateConnection() => new(_connectionString);
}
