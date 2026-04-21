using NICE.EAT.Models;

namespace NICE.EAT.WPF.ViewModels;

/// <summary>
/// Singleton that holds the logged-in user and the shopping cart across all pages.
/// </summary>
public class AppState
{
    private static AppState? _instance;
    public static AppState Instance => _instance ??= new AppState();

    public User? CurrentUser { get; set; }
    public bool IsLoggedIn => CurrentUser is not null;

    // ── Shopping Cart ────────────────────────────────────────────────────────
    public List<CartItem> Cart { get; private set; } = new();
    public bool   IsDelivery      { get; set; }
    public string DeliveryAddress  { get; set; } = "";

    /// <summary>Number of distinct line items in the cart.</summary>
    public int CartItemCount => Cart.Sum(c => c.Quantity);

    /// <summary>Grand subtotal before user discount and delivery.</summary>
    public decimal CartSubtotal => Cart.Sum(c => c.Subtotal);

    /// <summary>
    /// Add a menu item to the cart. If the same item (same MenuItemId) is already
    /// present, increment its quantity instead of duplicating.
    /// </summary>
    public void AddToCart(int restaurantId, string restaurantName,
                          int menuItemId, string menuItemName, decimal unitPrice)
    {
        var existing = Cart.FirstOrDefault(c => c.MenuItemId == menuItemId);
        if (existing is not null)
        {
            existing.Quantity++;
        }
        else
        {
            Cart.Add(new CartItem
            {
                RestaurantId   = restaurantId,
                RestaurantName = restaurantName,
                MenuItemId     = menuItemId,
                MenuItemName   = menuItemName,
                UnitPrice      = unitPrice,
            });
        }
    }

    public void RemoveFromCart(CartItem item) => Cart.Remove(item);

    public void ClearCart()
    {
        Cart.Clear();
        IsDelivery      = false;
        DeliveryAddress = "";
    }
}
