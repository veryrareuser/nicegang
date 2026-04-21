using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NICE.EAT.Data;
using NICE.EAT.Models;
using NICE.EAT.Services;
using NICE.EAT.WPF.ViewModels;

namespace NICE.EAT.WPF.Views;

// Display wrapper for cart list binding
public class CartItemVM
{
    public CartItem Source          { get; init; } = null!;
    public string   MenuItemName   => Source.MenuItemName;
    public string   RestaurantName => Source.RestaurantName;
    public string   FromDisplay    => $"from {Source.RestaurantName}";
    public int      Quantity       => Source.Quantity;
    public string   UnitPriceDisplay  => $"Rp {Source.UnitPrice:N0} each";
    public string   SubtotalDisplay   => $"Rp {Source.Subtotal:N0}";
}

// Display wrapper for quick-add restaurant combo
public class QARestaurantDisplay
{
    public Restaurant Restaurant { get; init; } = null!;
    public override string ToString() => Restaurant.Name;
}

// Display wrapper for quick-add menu combo
public class QAMenuDisplay
{
    public NICE.EAT.Models.MenuItem Item { get; init; } = null!;
    public override string ToString() => $"{Item.Name}  —  Rp {Item.Price:N0}";
}

public partial class CartPage : Page
{
    private readonly MainWindow           _mainWindow;
    private readonly RestaurantRepository _restaurantRepo = new();
    private readonly MenuItemRepository   _menuRepo       = new();
    private readonly OrderService         _orderSvc       = new(new OrderRepository());

    public CartPage(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        LoadQuickAdd();
        RefreshCart();
    }

    // ── Quick-add combos ────────────────────────────────────────────────────
    private void LoadQuickAdd()
    {
        var restaurants = _restaurantRepo.GetAll();
        QARestaurantBox.ItemsSource = restaurants.Select(r => new QARestaurantDisplay { Restaurant = r }).ToList();
        if (restaurants.Count > 0) QARestaurantBox.SelectedIndex = 0;
    }

    private void QARestaurantBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (QARestaurantBox.SelectedItem is not QARestaurantDisplay rd) return;
        var items = _menuRepo.GetByRestaurant(rd.Restaurant.Id);
        QAMenuBox.ItemsSource = items.Select(m => new QAMenuDisplay { Item = m }).ToList();
        if (items.Count > 0) QAMenuBox.SelectedIndex = 0;
    }

    private void BtnQuickAdd_Click(object sender, RoutedEventArgs e)
    {
        if (QARestaurantBox.SelectedItem is not QARestaurantDisplay rd) return;
        if (QAMenuBox.SelectedItem is not QAMenuDisplay md) return;

        var r = rd.Restaurant;
        var m = md.Item;
        AppState.Instance.AddToCart(r.Id, r.Name, m.Id, m.Name, m.Price);
        _mainWindow.UpdateCartBadge();
        RefreshCart();
    }

    // ── Qty controls ────────────────────────────────────────────────────────
    private void BtnIncrement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CartItemVM vm)
        { vm.Source.Quantity++; RefreshCart(); _mainWindow.UpdateCartBadge(); }
    }

    private void BtnDecrement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CartItemVM vm)
        {
            if (vm.Source.Quantity > 1) vm.Source.Quantity--;
            else AppState.Instance.RemoveFromCart(vm.Source);
            RefreshCart(); _mainWindow.UpdateCartBadge();
        }
    }

    private void BtnRemove_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CartItemVM vm)
        {
            AppState.Instance.RemoveFromCart(vm.Source);
            RefreshCart(); _mainWindow.UpdateCartBadge();
        }
    }

    // ── Delivery toggle ─────────────────────────────────────────────────────
    private void DeliveryCheck_Changed(object sender, RoutedEventArgs e)
    {
        bool isDel = DeliveryCheck.IsChecked == true;
        DeliveryPanel.Visibility = isDel ? Visibility.Visible : Visibility.Collapsed;
        DeliveryRow.Visibility   = isDel ? Visibility.Visible : Visibility.Collapsed;
        AppState.Instance.IsDelivery = isDel;
        UpdateSummary();
    }

    // ── Refresh display ─────────────────────────────────────────────────────
    private void RefreshCart()
    {
        var cart = AppState.Instance.Cart;
        bool empty = cart.Count == 0;

        EmptyMsg.Visibility     = empty ? Visibility.Visible : Visibility.Collapsed;
        BtnCheckout.IsEnabled   = !empty;

        CartList.ItemsSource = cart.Select(c => new CartItemVM { Source = c }).ToList();
        HeaderSub.Text = empty
            ? "Add items from Recommendations or use Quick Add below"
            : $"{AppState.Instance.CartItemCount} item(s) from {cart.Select(c => c.RestaurantName).Distinct().Count()} restaurant(s)";

        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var user     = AppState.Instance.CurrentUser!;
        decimal sub  = AppState.Instance.CartSubtotal;
        decimal disc = sub - user.CalculateOrderPrice(sub); // POLYMORPHISM
        decimal del  = DeliveryCheck.IsChecked == true ? 5000 : 0;
        decimal total = sub - disc + del;

        SubtotalText.Text = $"Rp {sub:N0}";

        if (disc > 0)
        {
            DiscountRow.Visibility = Visibility.Visible;
            DiscountLabel.Text     = $"{user.GetUserType()} Discount";
            DiscountText.Text      = $"- Rp {disc:N0}";
        }
        else
        {
            DiscountRow.Visibility = Visibility.Collapsed;
        }

        TotalText.Text = $"Rp {total:N0}";
    }

    // ── Checkout ─────────────────────────────────────────────────────────────
    private void BtnCheckout_Click(object sender, RoutedEventArgs e)
    {
        var state = AppState.Instance;
        if (state.Cart.Count == 0) return;

        if (state.IsDelivery && string.IsNullOrWhiteSpace(DeliveryAddr.Text))
        {
            ShowMsg("Please enter a delivery address.", false);
            return;
        }

        state.DeliveryAddress = DeliveryAddr.Text.Trim();

        var user      = state.CurrentUser!;
        bool isDel    = state.IsDelivery;
        string? addr  = isDel ? state.DeliveryAddress : null;
        decimal fee   = isDel ? 5000 : 0;
        var orderIds  = new List<int>();

        try
        {
            foreach (var item in state.Cart)
            {
                // Place an order per line item qty
                for (int i = 0; i < item.Quantity; i++)
                {
                    int id = _orderSvc.PlaceOrder(
                        user, item.RestaurantId, item.MenuItemId, item.UnitPrice,
                        item.Note, isDel, addr, fee);
                    orderIds.Add(id);
                }
            }

            // Navigate to invoice
            var invoiceData = new InvoiceData
            {
                Username       = user.Username,
                UserType       = user.GetUserType(),
                Items          = state.Cart.ToList(),
                Subtotal       = state.CartSubtotal,
                Discount       = state.CartSubtotal - user.CalculateOrderPrice(state.CartSubtotal),
                IsDelivery     = isDel,
                DeliveryAddress = addr ?? "",
                DeliveryFee    = fee,
                OrderIds       = orderIds,
                Timestamp      = DateTime.Now
            };

            state.ClearCart();
            _mainWindow.UpdateCartBadge();
            _mainWindow.NavigateToInvoice(invoiceData);
        }
        catch (Exception ex)
        {
            ShowMsg($"Error: {ex.Message}", false);
        }
    }

    private void ShowMsg(string msg, bool success)
    {
        StatusMsg.Text       = msg;
        StatusMsg.Foreground = success
            ? (Brush)FindResource("GreenBrush")
            : (Brush)FindResource("AccentBrush");
        StatusMsg.Visibility = Visibility.Visible;
    }
}
