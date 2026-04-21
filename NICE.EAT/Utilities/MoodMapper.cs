namespace NICE.EAT.Utilities;

/// <summary>
/// Maps user mood strings to mood tags used in restaurant filtering.
/// A single mood may map to multiple tags for broader matching.
/// </summary>
public static class MoodMapper
{
    private static readonly Dictionary<string, List<string>> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["angry"]       = new() { "angry" },
        ["bored"]       = new() { "bored", "adventurous" },
        ["happy"]       = new() { "happy" },
        ["sad"]         = new() { "sad", "happy" },
        ["stressed"]    = new() { "stressed", "calm" },
        ["calm"]        = new() { "calm" },
        ["adventurous"] = new() { "adventurous", "happy" },
        ["tired"]       = new() { "calm", "happy" }
    };

    /// <summary>
    /// Resolves a mood input to a list of SQL-matchable mood tags.
    /// Returns an empty list if the mood is unrecognized.
    /// </summary>
    public static List<string> ResolveTags(string mood)
    {
        if (_map.TryGetValue(mood.Trim(), out var tags))
            return tags;

        return new List<string>();
    }

    /// <summary>Returns all supported mood names.</summary>
    public static IEnumerable<string> SupportedMoods => _map.Keys;
}
