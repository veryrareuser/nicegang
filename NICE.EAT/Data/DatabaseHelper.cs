using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace NICE.EAT.Data;

/// <summary>
/// Provides MySQL connections using the connection string from appsettings.json.
/// </summary>
public static class DatabaseHelper
{
    private static readonly string _connectionString;

    static DatabaseHelper()
    {
        string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(configPath))
            throw new FileNotFoundException($"appsettings.json not found at: {configPath}");

        string json = File.ReadAllText(configPath);
        using JsonDocument doc = JsonDocument.Parse(json);

        _connectionString = doc.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString()!;
    }

    /// <summary>Returns an open SqliteConnection. Caller must dispose.</summary>
    public static SqliteConnection GetConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }
}
