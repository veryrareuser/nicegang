namespace NICE.EAT.Models;

/// <summary>
/// Base order class.
/// PARENT CLASS #4 — extends BaseEntity, parent of DeliveryOrder.
/// 2-LEVEL INHERITANCE: Order → DeliveryOrder.
///
/// COMPOSITION: has-a User, has-a MenuItem, has-a Restaurant.
/// </summary>
public class Order : BaseEntity
{
    // ── Foreign keys (for DB mapping) ────────────────────────────────────────
    public int UserId { get; set; }
    public int RestaurantId { get; set; }
    public int MenuItemId { get; set; }
    public string? Note { get; set; }

    // ── COMPOSITION: Order HAS-A User, HAS-A MenuItem, HAS-A Restaurant ─────
    public User? OrderedBy { get; set; }
    public MenuItem? OrderedItem { get; set; }
    public Restaurant? OrderedAtRestaurant { get; set; }

    // ── Display fields (populated by JOIN queries) ───────────────────────────
    public string RestaurantName { get; set; } = string.Empty;
    public string MenuItemName { get; set; } = string.Empty;
    public decimal MenuItemPrice { get; set; }

    // ── Virtual methods for polymorphism ─────────────────────────────────────

    /// <summary>Returns the order type label. Overridden by DeliveryOrder.</summary>
    public virtual string GetOrderType() => "Dine-In";

    /// <summary>
    /// Calculates the total price for this order.
    /// DeliveryOrder overrides this to add a delivery fee (POLYMORPHISM).
    /// </summary>
    public virtual decimal GetTotalPrice()
    {
        return OrderedItem?.Price ?? MenuItemPrice;
    }

    /// <summary>Returns a one-line order summary.</summary>
    public virtual string GetOrderSummary()
    {
        return $"[{GetOrderType()}] {MenuItemName} @ {RestaurantName} — Rp {GetTotalPrice():N0}";
    }

    public override string GetEntityLabel() => $"Order:{GetOrderType()}";
}
