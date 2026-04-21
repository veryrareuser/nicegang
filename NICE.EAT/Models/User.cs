namespace NICE.EAT.Models;

/// <summary>
/// Base user class.
/// PARENT CLASS #3 — extends BaseEntity, parent of PremiumUser.
/// 2-LEVEL INHERITANCE: User → PremiumUser.
///
/// COMPOSITION: has-many Order (List — order history collection).
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // ── COMPOSITION: User HAS-MANY Order ─────────────────────────────────────
    /// <summary>Collection of past orders for this user.</summary>
    public List<Order> OrderHistory { get; set; } = new();

    // ── Virtual methods for polymorphism ─────────────────────────────────────

    /// <summary>Returns the user type label. Overridden by PremiumUser.</summary>
    public virtual string GetUserType() => "Basic";

    /// <summary>
    /// Calculates the final order price. Base user pays full price.
    /// PremiumUser overrides this to apply a discount (POLYMORPHISM).
    /// </summary>
    public virtual decimal CalculateOrderPrice(decimal originalPrice)
    {
        return originalPrice;
    }

    /// <summary>Returns a display summary of this user.</summary>
    public virtual string GetProfileSummary()
    {
        return $"👤 {Username} ({GetUserType()}) — Orders: {OrderHistory.Count}";
    }

    public override string GetEntityLabel() => $"User:{GetUserType()}";
}
