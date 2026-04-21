namespace NICE.EAT.Models;

/// <summary>
/// Concrete menu item for drinks (beverages).
/// LEVEL 3 of 3-level inheritance: BaseEntity → MenuItem → DrinkItem.
/// </summary>
public class DrinkItem : MenuItem
{
    public bool IsCold { get; set; } = true;
    public int VolumeInMl { get; set; } = 300;

    // ── Overrides from MenuItem (POLYMORPHISM) ──────────────────────────────

    public override string GetItemType() => "Drink";

    public override string GetDescription()
    {
        string temp = IsCold ? "🧊 Cold" : "☕ Hot";
        string vegan = IsVegan ? " | 🌿 Vegan" : "";
        return $"[Drink] {Name} ({VolumeInMl}ml, {temp}){vegan} — Rp {Price:N0}";
    }

    /// <summary>Drinks have a smaller budget threshold by default.</summary>
    public override bool MatchesBudget(decimal maxPrice) => Price <= maxPrice;
}
