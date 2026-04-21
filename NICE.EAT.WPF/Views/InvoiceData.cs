using NICE.EAT.WPF.ViewModels;

namespace NICE.EAT.WPF.Views;

/// <summary>
/// Data object passed from CartPage to InvoicePage after checkout.
/// </summary>
public class InvoiceData
{
    public string Username        { get; init; } = "";
    public string UserType        { get; init; } = "";
    public List<CartItem> Items   { get; init; } = new();
    public decimal Subtotal       { get; init; }
    public decimal Discount       { get; init; }
    public bool IsDelivery        { get; init; }
    public string DeliveryAddress { get; init; } = "";
    public decimal DeliveryFee    { get; init; }
    public List<int> OrderIds     { get; init; } = new();
    public DateTime Timestamp     { get; init; }

    public decimal GrandTotal => Subtotal - Discount + (IsDelivery ? DeliveryFee : 0);
}
