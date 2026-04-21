using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NICE.EAT.Data;
using NICE.EAT.Models;
using NICE.EAT.Services;
using NICE.EAT.Services.Filters;
using NICE.EAT.WPF.ViewModels;

namespace NICE.EAT.WPF.Views;

// View-model for a single menu item row (with add-to-cart data)
public class MenuItemVM
{
    public int    RestaurantId   { get; init; }
    public string RestaurantName { get; init; } = "";
    public int    MenuItemId     { get; init; }
    public string MenuItemName   { get; init; } = "";
    public decimal Price          { get; init; }
    public string DisplayText     { get; init; } = "";
}

// View-model for a restaurant result card
public class RestaurantViewModel
{
    public string Name           { get; init; } = "";
    public string Address        { get; init; } = "";
    public string RatingDisplay  { get; init; } = "";
    public string PriceDisplay   { get; init; } = "";
    public string SpicyDisplay   { get; init; } = "";
    public string VeganDisplay   { get; init; } = "";
    public string WaitDisplay    { get; init; } = "";
    public List<MenuItemVM> MenuItems { get; init; } = new();
}

public partial class RecommendPage : Page
{
    private readonly RecommendationService _svc;
    private readonly MainWindow _mainWindow;
    private DispatcherTimer? _toastTimer;

    public RecommendPage(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;

        var restaurantRepo = new RestaurantRepository();
        var menuRepo       = new MenuItemRepository();
        var waitService    = new WaitTimeService();
        _svc = new RecommendationService(restaurantRepo, menuRepo, waitService);
    }

    private void SpicySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SpicyLabel is not null)
            SpicyLabel.Text = $"MAX SPICY: {(int)e.NewValue}";
    }

    private void BtnSearch_Click(object sender, RoutedEventArgs e)
    {
        EmptyMsg.Visibility = Visibility.Collapsed;

        if (!decimal.TryParse(BudgetBox.Text.Trim(), out decimal budget) || budget <= 0)
        {
            EmptyMsg.Text       = "Please enter a valid budget (e.g. 30000).";
            EmptyMsg.Visibility = Visibility.Visible;
            return;
        }

        int spicyLevel = (int)SpicySlider.Value;
        string mood    = (MoodBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "happy";
        bool veganOnly = VeganCheck.IsChecked == true;

        _svc.ClearFilters();
        _svc.AddFilter(new BudgetFilter(budget));
        _svc.AddFilter(new SpicyFilter(spicyLevel));
        _svc.AddFilter(new MoodFilter(mood));
        if (veganOnly) _svc.AddFilter(new VeganFilter());

        List<Restaurant> results = _svc.GetRecommendations();

        if (results.Count == 0)
        {
            EmptyMsg.Text       = "No restaurants match your criteria. Try relaxing the filters.";
            EmptyMsg.Visibility = Visibility.Visible;
            ResultsList.ItemsSource = null;
            return;
        }

        ResultsList.ItemsSource = results.Select(r => new RestaurantViewModel
        {
            Name         = r.Name,
            Address      = r.Address,
            RatingDisplay  = $"⭐ {r.Rating:F1}",
            PriceDisplay   = $"Rp {r.AveragePrice:N0} avg",
            SpicyDisplay   = SpicyEmoji(r.SpicyLevel),
            VeganDisplay   = r.IsVegan ? "🌿 Vegan" : "🍖 Non-Vegan",
            WaitDisplay    = $"⏱ ~{r.WaitTimeMinutes} min",
            MenuItems      = r.Menu.Take(6).Select(m => new MenuItemVM
            {
                RestaurantId   = r.Id,
                RestaurantName = r.Name,
                MenuItemId     = m.Id,
                MenuItemName   = m.Name,
                Price          = m.Price,
                DisplayText    = $"• {m.Name}  —  Rp {m.Price:N0}"
            }).ToList()
        }).ToList();
    }

    // ── Add-to-cart handler ──────────────────────────────────────────────────
    private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not MenuItemVM item) return;

        AppState.Instance.AddToCart(
            item.RestaurantId, item.RestaurantName,
            item.MenuItemId, item.MenuItemName, item.Price);

        _mainWindow.UpdateCartBadge();
        ShowToast($"✓ {item.MenuItemName} added to cart");
    }

    private void ShowToast(string msg)
    {
        ToastText.Text      = msg;
        Toast.Visibility    = Visibility.Visible;

        _toastTimer?.Stop();
        _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
        _toastTimer.Tick += (_, _) => { Toast.Visibility = Visibility.Collapsed; _toastTimer.Stop(); };
        _toastTimer.Start();
    }

    private static string SpicyEmoji(int level) => level switch
    {
        0 => "🔵 Not spicy",
        1 => "🌶 Mild",
        2 => "🌶🌶 Medium",
        3 => "🌶🌶🌶 Hot",
        4 => "🌶🌶🌶🌶 Very hot",
        _ => "🌶🌶🌶🌶🌶 Extreme"
    };
}
