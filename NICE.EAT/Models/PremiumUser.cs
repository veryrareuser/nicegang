namespace NICE.EAT.Models;

/// <summary>
/// Premium user with loyalty points and discount.
/// 2-LEVEL INHERITANCE: User → PremiumUser.
/// Overrides CalculateOrderPrice() to demonstrate POLYMORPHISM.
/// </summary>
public class PremiumUser : User
{
    public int LoyaltyPoints { get; set; }

    /// <summary>Discount rate, e.g., 0.10 = 10% off.</summary>
    public decimal DiscountRate { get; set; }

    // ── Overrides (POLYMORPHISM) ─────────────────────────────────────────────

    public override string GetUserType() => "Premium";

    /// <summary>
    /// Applies a discount to the order price.
    /// This is the POLYMORPHISM demonstration —
    /// base User returns full price, PremiumUser applies a discount.
    /// </summary>
    public override decimal CalculateOrderPrice(decimal originalPrice)
    {
        return originalPrice * (1m - DiscountRate);
    }

    public override string GetProfileSummary()
    {
        return $"⭐ {Username} (Premium) — 💎 {LoyaltyPoints} pts | 🏷️ {DiscountRate:P0} discount | Orders: {OrderHistory.Count}";
    }

    /// <summary>Awards loyalty points after placing an order.</summary>
    public void AddLoyaltyPoints(int points)
    {
        LoyaltyPoints += points;
    }
}
