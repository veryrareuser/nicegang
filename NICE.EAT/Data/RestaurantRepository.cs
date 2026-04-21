using Microsoft.Data.Sqlite;
using NICE.EAT.Models;

namespace NICE.EAT.Data;

/// <summary>Data access for the restaurants table.</summary>
public class RestaurantRepository
{
    /// <summary>Returns all restaurants (without menu items loaded).</summary>
    public List<Restaurant> GetAll()
    {
        var results = new List<Restaurant>();
        const string sql = @"
            SELECT id, name, address, spicy_level, is_vegan,
                   average_price, mood_tags, rating, created_at
            FROM restaurants
            ORDER BY name";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            results.Add(MapRestaurant(reader));

        return results;
    }

    /// <summary>Returns a single restaurant by ID.</summary>
    public Restaurant? GetById(int id)
    {
        const string sql = @"
            SELECT id, name, address, spicy_level, is_vegan,
                   average_price, mood_tags, rating, created_at
            FROM restaurants
            WHERE id = @id";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapRestaurant(reader) : null;
    }

    private static Restaurant MapRestaurant(SqliteDataReader r) => new()
    {
        Id           = r.GetInt32("id"),
        Name         = r.GetString("name"),
        Address      = r.GetString("address"),
        SpicyLevel   = r.GetInt32("spicy_level"),
        IsVegan      = r.GetBoolean("is_vegan"),
        AveragePrice = r.GetDecimal("average_price"),
        MoodTags     = r.GetString("mood_tags"),
        Rating       = r.GetDecimal("rating"),
        CreatedAt    = r.GetDateTime("created_at")
    };
}
