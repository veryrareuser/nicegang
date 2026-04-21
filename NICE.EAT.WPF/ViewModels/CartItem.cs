using NICE.EAT.Models;

namespace NICE.EAT.WPF.ViewModels;

/// <summary>
/// Represents one item in the shopping cart.
/// COMPOSITION: CartItem HAS-A MenuItem, HAS-A Restaurant reference.
/// </summary>
public class CartItem
{
    public int    RestaurantId   { get; set; }
    public string RestaurantName { get; set; } = "";
    public int    MenuItemId     { get; set; }
    public string MenuItemName   { get; set; } = "";
    public decimal UnitPrice     { get; set; }
    public int    Quantity       { get; set; } = 1;
    public string? Note          { get; set; }

    /// <summary>Line total before any user discount.</summary>
    public decimal Subtotal => UnitPrice * Quantity;
}
