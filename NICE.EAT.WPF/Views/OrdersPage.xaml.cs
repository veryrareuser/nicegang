using System.Windows;
using System.Windows.Controls;
using NICE.EAT.Data;
using NICE.EAT.Models;
using NICE.EAT.Services;
using NICE.EAT.WPF.ViewModels;

namespace NICE.EAT.WPF.Views;

public class OrderViewModel
{
    public string TypeIcon       { get; init; } = "🍽";
    public string MenuItemName   { get; init; } = "";
    public string RestaurantName { get; init; } = "";
    public string TotalDisplay   { get; init; } = "";
    public string DateDisplay    { get; init; } = "";
    public string ExtraInfo      { get; init; } = "";
    public string Note           { get; init; } = "";
    public Visibility ExtraInfoVisible { get; init; } = Visibility.Collapsed;
    public Visibility NoteVisible      { get; init; } = Visibility.Collapsed;
}

public partial class OrdersPage : Page
{
    private readonly OrderService _orderSvc;
    private readonly int _userId;

    public OrdersPage()
    {
        InitializeComponent();

        _userId  = AppState.Instance.CurrentUser?.Id ?? 0;
        _orderSvc = new OrderService(new OrderRepository());

        LoadData();
    }

    private void LoadData()
    {
        // Favorite restaurant
        var fav = _orderSvc.GetFavoriteRestaurant(_userId);
        FavText.Text = fav is null
            ? "No orders yet — start ordering!"
            : $"{fav.Value.Name}  ({fav.Value.Count} orders)";

        // Order history
        var history = _orderSvc.GetHistory(_userId);

        if (history.Count == 0)
        {
            EmptyMsg.Visibility = Visibility.Visible;
            return;
        }

        EmptyMsg.Visibility = Visibility.Collapsed;
        OrdersList.ItemsSource = history.Select(o =>
        {
            bool isDelivery = o is DeliveryOrder;
            string extraInfo = "";
            Visibility extraVis = Visibility.Collapsed;

            if (o is DeliveryOrder d && !string.IsNullOrEmpty(d.DeliveryAddress))
            {
                extraInfo = $"📦 Delivery → {d.DeliveryAddress}  (+Rp {d.DeliveryFee:N0})";
                extraVis  = Visibility.Visible;
            }

            return new OrderViewModel
            {
                TypeIcon       = isDelivery ? "🚚" : "🍽",
                MenuItemName   = o.MenuItemName,
                RestaurantName = o.RestaurantName,
                TotalDisplay   = $"Rp {o.GetTotalPrice():N0}",
                DateDisplay    = o.CreatedAt.ToLocalTime().ToString("dd MMM yyyy HH:mm"),
                ExtraInfo      = extraInfo,
                ExtraInfoVisible = extraVis,
                Note           = string.IsNullOrEmpty(o.Note) ? "" : $"💬 {o.Note}",
                NoteVisible    = string.IsNullOrEmpty(o.Note) ? Visibility.Collapsed : Visibility.Visible
            };
        }).ToList();
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadData();
}
