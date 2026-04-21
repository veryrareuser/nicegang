using System.Windows;
using NICE.EAT.WPF.ViewModels;
using NICE.EAT.WPF.Views;

namespace NICE.EAT.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(new LoginPage(this));
    }

    // ── Called by LoginPage after successful login ────────────────────────────
    public void OnLoginSuccess()
    {
        var user = AppState.Instance.CurrentUser!;
        UserBadge.Text = $"👤 {user.Username} ({user.GetUserType()})";

        NavRecommend.IsEnabled = true;
        NavOrders.IsEnabled    = true;
        NavCart.IsEnabled      = true;
        BtnLogout.Visibility   = Visibility.Visible;
        NavLogin.IsEnabled     = false;

        // Navigate to recommendations after login
        MainFrame.Navigate(new RecommendPage(this));
    }

    // ── Cart badge update — called by any page after adding to cart ───────────
    public void UpdateCartBadge()
    {
        int count = AppState.Instance.CartItemCount;
        if (count > 0)
        {
            CartBadge.Visibility = Visibility.Visible;
            CartBadgeText.Text   = count.ToString();
        }
        else
        {
            CartBadge.Visibility = Visibility.Collapsed;
        }
    }

    // ── Nav button handlers ──────────────────────────────────────────────────
    private void NavLogin_Click(object sender, RoutedEventArgs e)
        => MainFrame.Navigate(new LoginPage(this));

    private void NavRecommend_Click(object sender, RoutedEventArgs e)
        => MainFrame.Navigate(new RecommendPage(this));

    private void NavOrders_Click(object sender, RoutedEventArgs e)
        => MainFrame.Navigate(new OrdersPage());

    private void NavCart_Click(object sender, RoutedEventArgs e)
        => MainFrame.Navigate(new CartPage(this));

    // ── Public navigation helpers (used by InvoicePage, CartPage) ────────────
    public void NavigateToInvoice(InvoiceData data)
        => MainFrame.Navigate(new InvoicePage(this, data));

    public void NavigateToRecommend()
        => MainFrame.Navigate(new RecommendPage(this));

    public void NavigateToOrders()
        => MainFrame.Navigate(new OrdersPage());

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        AppState.Instance.CurrentUser = null;
        AppState.Instance.ClearCart();
        UserBadge.Text = "Not logged in";

        NavRecommend.IsEnabled = false;
        NavOrders.IsEnabled    = false;
        NavCart.IsEnabled      = false;
        NavLogin.IsEnabled     = true;
        BtnLogout.Visibility   = Visibility.Collapsed;
        UpdateCartBadge();

        MainFrame.Navigate(new LoginPage(this));
    }
}
