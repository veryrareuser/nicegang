namespace NICE.EAT.Models;

/// <summary>
/// Delivery order with additional delivery address and fee.
/// 2-LEVEL INHERITANCE: Order → DeliveryOrder.
/// Overrides GetTotalPrice() and GetOrderType() for POLYMORPHISM.
/// </summary>
public class DeliveryOrder : Order
{
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal DeliveryFee { get; set; }

    // ── Overrides (POLYMORPHISM) ─────────────────────────────────────────────

    public override string GetOrderType() => "Delivery";

    /// <summary>
    /// Adds delivery fee on top of the base item price.
    /// This is the POLYMORPHISM demonstration —
    /// base Order returns item price only, DeliveryOrder adds fee.
    /// </summary>
    public override decimal GetTotalPrice()
    {
        return base.GetTotalPrice() + DeliveryFee;
    }

    public override string GetOrderSummary()
    {
        return $"[Delivery 🚚] {MenuItemName} @ {RestaurantName} — Rp {GetTotalPrice():N0} (incl. delivery Rp {DeliveryFee:N0})";
    }
}
