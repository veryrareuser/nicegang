using NICE.EAT.Interfaces;

namespace NICE.EAT.Models;

/// <summary>
/// Represents a restaurant entity.
/// Extends BaseEntity. Implements IRecommendable (Interface #1, implementation #1).
///
/// COMPOSITION: has-many MenuItem (List — "many" relation).
/// </summary>
public class Restaurant : BaseEntity, IRecommendable
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    /// <summary>Spicy scale: 0 (not spicy) to 5 (very spicy).</summary>
    public int SpicyLevel { get; set; }

    public bool IsVegan { get; set; }
    public decimal AveragePrice { get; set; }

    /// <summary>Comma-separated mood tags, e.g. "happy,bored,angry".</summary>
    public string MoodTags { get; set; } = string.Empty;

    public decimal Rating { get; set; }

    /// <summary>Simulated wait time injected by WaitTimeService (not in DB).</summary>
    public int WaitTimeMinutes { get; set; }

    // ── COMPOSITION: Restaurant HAS-MANY MenuItem ────────────────────────────
    /// <summary>Collection of menu items belonging to this restaurant.</summary>
    public List<MenuItem> Menu { get; set; } = new();

    // ── Helper ───────────────────────────────────────────────────────────────

    public List<string> GetMoodTagList() =>
        MoodTags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .ToList();

    // ── IRecommendable implementation ────────────────────────────────────────

    public string GetDisplayInfo()
    {
        string vegan = IsVegan ? " 🌿" : "";
        return $"🏪 {Name}{vegan} — Rp {AveragePrice:N0} avg | ⭐ {Rating:F1} | 📍 {Address}";
    }

    public double GetRecommendationScore(decimal budget, int spicyPreference)
    {
        double score = (double)Rating * 15; // rating contributes up to 75
        if (AveragePrice <= budget) score += 15;
        if (Math.Abs(SpicyLevel - spicyPreference) <= 1) score += 10;
        return Math.Min(score, 100);
    }

    public override string GetEntityLabel() => "Restaurant";
}
