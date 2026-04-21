using System.Data.Common;

namespace NICE.EAT.Data;

/// <summary>
/// Extension methods for DbDataReader to provide Get...() methods that accept column names,
/// matching the signature of MySqlDataReader extensions, to easily migrate to Microsoft.Data.Sqlite.
/// </summary>
public static class DbDataReaderExtensions
{
    public static string GetString(this DbDataReader reader, string columnName)
        => reader.GetString(reader.GetOrdinal(columnName));

    public static int GetInt32(this DbDataReader reader, string columnName)
        => reader.GetInt32(reader.GetOrdinal(columnName));

    public static decimal GetDecimal(this DbDataReader reader, string columnName)
        // SQLite stores decimals as REAL or TEXT, so we must safely convert it
        => Convert.ToDecimal(reader.GetValue(reader.GetOrdinal(columnName)));

    public static bool GetBoolean(this DbDataReader reader, string columnName)
        => Convert.ToBoolean(reader.GetValue(reader.GetOrdinal(columnName)));

    public static DateTime GetDateTime(this DbDataReader reader, string columnName)
        => reader.GetDateTime(reader.GetOrdinal(columnName));
}
