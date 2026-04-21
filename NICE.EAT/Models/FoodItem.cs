using NICE.EAT.Interfaces;

namespace NICE.EAT.Models;

/// <summary>
/// Concrete menu item for food (non-drink) items.
/// LEVEL 3 of 3-level inheritance: BaseEntity → MenuItem → FoodItem.
/// Also implements IRecommendable (Interface #1, implementation #2).
/// </summary>
public class FoodItem : MenuItem, IRecommendable
{
    public string PortionSize { get; set; } = "Regular"; // Small, Regular, Large

    // ── Overrides from MenuItem (POLYMORPHISM) ──────────────────────────────

    public override string GetItemType() => "Food";

    public override string GetDescription()
    {
        string spicy = SpicyLevel > 0 ? $" | Spicy: {SpicyLevel}/5" : "";
        string vegan = IsVegan ? " | 🌿 Vegan" : "";
        return $"[{Category}] {Name} ({PortionSize}){spicy}{vegan} — Rp {Price:N0}";
    }

    // ── IRecommendable implementation ───────────────────────────────────────

    public string GetDisplayInfo()
    {
        string veganTag = IsVegan ? " 🌿" : "";
        return $"🍽️ {Name}{veganTag} — Rp {Price:N0} (from: {RestaurantName})";
    }

    public double GetRecommendationScore(decimal budget, int spicyPreference)
    {
        double score = 50; // base
        if (Price <= budget) score += 30;
        if (Math.Abs(SpicyLevel - spicyPreference) <= 1) score += 20;
        if (IsVegan) score += 5; // small bonus for healthy option
        return Math.Min(score, 100);
    }
}
