using Microsoft.Data.Sqlite;
using NICE.EAT.Models;

namespace NICE.EAT.Data;

/// <summary>
/// Data access for the users table.
/// Returns User or PremiumUser based on user_type discriminator column.
/// </summary>
public class UserRepository
{
    /// <summary>Finds a user by username. Returns User or PremiumUser.</summary>
    public User? GetByUsername(string username)
    {
        const string sql = @"
            SELECT id, username, email, user_type, loyalty_points, discount_rate, created_at
            FROM users
            WHERE username = @username
            LIMIT 1";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    /// <summary>Creates a new basic user.</summary>
    public User CreateUser(string username, string email)
    {
        const string sql = @"
            INSERT INTO users (username, email, user_type)
            VALUES (@username, @email, 'Basic');
            SELECT last_insert_rowid();";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@email", email);

        int newId = Convert.ToInt32(cmd.ExecuteScalar());
        return new User { Id = newId, Username = username, Email = email, CreatedAt = DateTime.Now };
    }

    /// <summary>Creates a new premium user.</summary>
    public PremiumUser CreatePremiumUser(string username, string email, decimal discountRate = 0.10m)
    {
        const string sql = @"
            INSERT INTO users (username, email, user_type, discount_rate)
            VALUES (@username, @email, 'Premium', @discount);
            SELECT last_insert_rowid();";

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@discount", discountRate);

        int newId = Convert.ToInt32(cmd.ExecuteScalar());
        return new PremiumUser
        {
            Id = newId, Username = username, Email = email,
            DiscountRate = discountRate, LoyaltyPoints = 0, CreatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Factory: maps DB row to User or PremiumUser based on user_type discriminator.
    /// </summary>
    private static User MapUser(SqliteDataReader r)
    {
        string userType = r.GetString("user_type");

        if (userType == "Premium")
        {
            return new PremiumUser
            {
                Id            = r.GetInt32("id"),
                Username      = r.GetString("username"),
                Email         = r.GetString("email"),
                LoyaltyPoints = r.GetInt32("loyalty_points"),
                DiscountRate  = r.GetDecimal("discount_rate"),
                CreatedAt     = r.GetDateTime("created_at")
            };
        }

        return new User
        {
            Id        = r.GetInt32("id"),
            Username  = r.GetString("username"),
            Email     = r.GetString("email"),
            CreatedAt = r.GetDateTime("created_at")
        };
    }
}
