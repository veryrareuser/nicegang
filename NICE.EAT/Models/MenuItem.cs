namespace NICE.EAT.Models;

/// <summary>
/// Abstract base class for all menu items.
/// PARENT CLASS #2 — extends BaseEntity, parent of FoodItem and DrinkItem.
/// 
/// INHERITANCE CHAIN: BaseEntity → MenuItem → FoodItem/DrinkItem (3-LEVEL).
/// </summary>
public abstract class MenuItem : BaseEntity
{
    public int RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int SpicyLevel { get; set; }
    public bool IsVegan { get; set; }
    public string Category { get; set; } = string.Empty;

    // ── Popularity tracking (populated by GROUP BY query) ──
    public int OrderCount { get; set; }
    public string RestaurantName { get; set; } = string.Empty;

    // ── Abstract methods — must be overridden by FoodItem / DrinkItem ──

    /// <summary>Returns "Food" or "Drink".</summary>
    public abstract string GetItemType();

    /// <summary>Returns a formatted description string (POLYMORPHISM demo).</summary>
    public abstract string GetDescription();

    // ── Virtual method with default behaviour ──

    /// <summary>Returns true if this item fits within the given budget.</summary>
    public virtual bool MatchesBudget(decimal maxPrice) => Price <= maxPrice;

    public override string GetEntityLabel() => $"MenuItem:{GetItemType()}";
}
