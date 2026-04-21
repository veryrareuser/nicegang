using NICE.EAT.Data;
using NICE.EAT.Models;

namespace NICE.EAT.Services;

/// <summary>
/// Handles order recording, history retrieval, and favorite restaurant lookup.
/// Extends BaseService (Parent #5).
///
/// Demonstrates POLYMORPHISM via User.CalculateOrderPrice() —
/// BasicUser pays full price, PremiumUser gets a discount.
/// </summary>
public class OrderService : BaseService
{
    private readonly OrderRepository _orderRepo;

    public OrderService(OrderRepository orderRepo)
        : base("OrderService")
    {
        _orderRepo = orderRepo;
    }

    /// <summary>
    /// Places an order. Uses User.CalculateOrderPrice() polymorphically —
    /// PremiumUser gets a discount applied, BasicUser pays full price.
    /// </summary>
    public int PlaceOrder(User user, int restaurantId, int menuItemId, decimal itemPrice, string? note = null,
                          bool isDelivery = false, string? deliveryAddress = null, decimal deliveryFee = 0)
    {
        // POLYMORPHISM: user.CalculateOrderPrice() behaves differently
        // based on actual runtime type (User vs PremiumUser)
        decimal finalPrice = user.CalculateOrderPrice(itemPrice);
        Log($"Price for {user.Username} ({user.GetUserType()}): Rp {itemPrice:N0} → Rp {finalPrice:N0}");

        // Award loyalty points if PremiumUser
        if (user is PremiumUser premium)
        {
            int points = (int)(finalPrice / 1000);
            premium.AddLoyaltyPoints(points);
            Log($"Awarded {points} loyalty points → Total: {premium.LoyaltyPoints}");
        }

        int orderId = _orderRepo.AddOrder(
            user.Id, restaurantId, menuItemId, note,
            isDelivery, deliveryAddress, deliveryFee);

        Log($"Order #{orderId} placed successfully.");
        return orderId;
    }

    /// <summary>Returns the user's full order history, newest first.</summary>
    public List<Order> GetHistory(int userId)
    {
        return _orderRepo.GetOrderHistory(userId);
    }

    /// <summary>Returns the user's most-visited restaurant, or null if no orders.</summary>
    public (string Name, int Count)? GetFavoriteRestaurant(int userId)
    {
        return _orderRepo.GetFavoriteRestaurant(userId);
    }
}
