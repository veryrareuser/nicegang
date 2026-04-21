using Microsoft.Data.Sqlite;
using NICE.EAT.Models;

namespace NICE.EAT.Data;

/// <summary>
/// Data access for the orders table.
/// Returns Order or DeliveryOrder based on order_type discriminator column.
/// </summary>
public class OrderRepository
{
    /// <summary>Records a new order. Returns the generated order ID.</summary>
    public int AddOrder(int userId, int restaurantId, int menuItemId, string? note = null,
                        bool isDelivery = false, string? deliveryAddress = null, decimal deliveryFee = 0)
    {
        string orderType = isDelivery ? "Delivery" : "DineIn";
        const string sql = @"
            INSERT INTO orders (user_id, restaurant_id, menu_item_id, note,
                                order_type, delivery_address, delivery_fee)
            VALUES (@uid, @rid, @mid, @note, @type, @addr, @fee);
            SELECT last_insert_rowid();";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@uid",  userId);
        cmd.Parameters.AddWithValue("@rid",  restaurantId);
        cmd.Parameters.AddWithValue("@mid",  menuItemId);
        cmd.Parameters.AddWithValue("@note", (object?)note ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@type", orderType);
        cmd.Parameters.AddWithValue("@addr", (object?)deliveryAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@fee",  deliveryFee);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>
    /// Returns order history. Factory method produces Order or DeliveryOrder
    /// based on order_type discriminator.
    /// </summary>
    public List<Order> GetOrderHistory(int userId)
    {
        var results = new List<Order>();
        const string sql = @"
            SELECT  o.id, o.user_id, o.restaurant_id, o.menu_item_id,
                    o.ordered_at, o.note, o.order_type,
                    o.delivery_address, o.delivery_fee,
                    r.name  AS restaurant_name,
                    mi.name AS menu_item_name,
                    mi.price AS menu_item_price
            FROM    orders      o
            JOIN    restaurants r  ON r.id  = o.restaurant_id
            JOIN    menu_items  mi ON mi.id = o.menu_item_id
            WHERE   o.user_id = @uid
            ORDER BY o.ordered_at DESC";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@uid", userId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add(MapOrder(reader));

        return results;
    }

    /// <summary>Returns user's favourite restaurant by GROUP BY count.</summary>
    public (string RestaurantName, int OrderCount)? GetFavoriteRestaurant(int userId)
    {
        const string sql = @"
            SELECT  r.name       AS restaurant_name,
                    COUNT(o.id)  AS order_count
            FROM    orders      o
            JOIN    restaurants r ON r.id = o.restaurant_id
            WHERE   o.user_id = @uid
            GROUP BY o.restaurant_id
            ORDER BY order_count DESC
            LIMIT 1";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@uid", userId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return (reader.GetString("restaurant_name"), reader.GetInt32("order_count"));
    }

    /// <summary>
    /// Factory: maps DB row to Order or DeliveryOrder based on order_type.
    /// </summary>
    private static Order MapOrder(SqliteDataReader r)
    {
        string orderType = r.GetString("order_type");
        string? note = r.IsDBNull(r.GetOrdinal("note")) ? null : r.GetString("note");

        if (orderType == "Delivery")
        {
            return new DeliveryOrder
            {
                Id              = r.GetInt32("id"),
                UserId          = r.GetInt32("user_id"),
                RestaurantId    = r.GetInt32("restaurant_id"),
                MenuItemId      = r.GetInt32("menu_item_id"),
                CreatedAt       = r.GetDateTime("ordered_at"),
                Note            = note,
                RestaurantName  = r.GetString("restaurant_name"),
                MenuItemName    = r.GetString("menu_item_name"),
                MenuItemPrice   = r.GetDecimal("menu_item_price"),
                DeliveryAddress = r.IsDBNull(r.GetOrdinal("delivery_address")) ? "" : r.GetString("delivery_address"),
                DeliveryFee     = r.GetDecimal("delivery_fee")
            };
        }

        return new Order
        {
            Id             = r.GetInt32("id"),
            UserId         = r.GetInt32("user_id"),
            RestaurantId   = r.GetInt32("restaurant_id"),
            MenuItemId     = r.GetInt32("menu_item_id"),
            CreatedAt      = r.GetDateTime("ordered_at"),
            Note           = note,
            RestaurantName = r.GetString("restaurant_name"),
            MenuItemName   = r.GetString("menu_item_name"),
            MenuItemPrice  = r.GetDecimal("menu_item_price")
        };
    }
}
