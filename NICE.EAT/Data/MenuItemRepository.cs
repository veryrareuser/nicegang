using Microsoft.Data.Sqlite;
using NICE.EAT.Models;

namespace NICE.EAT.Data;

/// <summary>
/// Data access for menu_items table.
/// Returns FoodItem or DrinkItem based on the item_type discriminator column.
/// This is a FACTORY PATTERN using inheritance — the repository decides
/// which subclass to instantiate at runtime.
/// </summary>
public class MenuItemRepository
{
    /// <summary>
    /// Returns all menu items for a restaurant as FoodItem or DrinkItem instances.
    /// Uses item_type column as discriminator (INHERITANCE mapping).
    /// </summary>
    public List<MenuItem> GetByRestaurant(int restaurantId)
    {
        var results = new List<MenuItem>();
        const string sql = @"
            SELECT id, restaurant_id, name, price, spicy_level, is_vegan, category,
                   item_type, portion_size, is_cold, volume_ml, created_at
            FROM menu_items
            WHERE restaurant_id = @rid
            ORDER BY category, name";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rid", restaurantId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add(MapMenuItem(reader));

        return results;
    }

    /// <summary>
    /// Returns the top N most-ordered menu items (GROUP BY with COUNT).
    /// </summary>
    public List<MenuItem> GetMostPopular(int topN = 5)
    {
        var results = new List<MenuItem>();
        const string sql = @"
            SELECT  mi.id,
                    mi.restaurant_id,
                    mi.name,
                    mi.price,
                    mi.spicy_level,
                    mi.is_vegan,
                    mi.category,
                    mi.item_type,
                    mi.portion_size,
                    mi.is_cold,
                    mi.volume_ml,
                    mi.created_at,
                    r.name        AS restaurant_name,
                    COUNT(o.id)   AS order_count
            FROM    menu_items mi
            JOIN    orders     o  ON o.menu_item_id  = mi.id
            JOIN    restaurants r ON r.id             = mi.restaurant_id
            GROUP BY mi.id
            ORDER BY order_count DESC
            LIMIT @topN";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@topN", topN);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var item = MapMenuItem(reader);
            item.OrderCount     = reader.GetInt32("order_count");
            item.RestaurantName = reader.GetString("restaurant_name");
            results.Add(item);
        }

        return results;
    }

    /// <summary>
    /// Factory method: creates FoodItem or DrinkItem based on discriminator column.
    /// This demonstrates the connection between INHERITANCE and database mapping.
    /// </summary>
    private static MenuItem MapMenuItem(SqliteDataReader r)
    {
        string itemType = r.GetString("item_type");

        if (itemType == "Drink")
        {
            return new DrinkItem
            {
                Id           = r.GetInt32("id"),
                RestaurantId = r.GetInt32("restaurant_id"),
                Name         = r.GetString("name"),
                Price        = r.GetDecimal("price"),
                SpicyLevel   = r.GetInt32("spicy_level"),
                IsVegan      = r.GetBoolean("is_vegan"),
                Category     = r.GetString("category"),
                IsCold       = r.GetBoolean("is_cold"),
                VolumeInMl   = r.GetInt32("volume_ml"),
                CreatedAt    = r.GetDateTime("created_at")
            };
        }

        return new FoodItem
        {
            Id           = r.GetInt32("id"),
            RestaurantId = r.GetInt32("restaurant_id"),
            Name         = r.GetString("name"),
            Price        = r.GetDecimal("price"),
            SpicyLevel   = r.GetInt32("spicy_level"),
            IsVegan      = r.GetBoolean("is_vegan"),
            Category     = r.GetString("category"),
            PortionSize  = r.GetString("portion_size"),
            CreatedAt    = r.GetDateTime("created_at")
        };
    }
}
