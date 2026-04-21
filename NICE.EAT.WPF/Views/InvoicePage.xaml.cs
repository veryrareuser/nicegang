using System.Windows;
using System.Windows.Controls;

namespace NICE.EAT.WPF.Views;

public class InvoiceLineVM
{
    public string Name          { get; init; } = "";
    public string Restaurant    { get; init; } = "";
    public string Qty           { get; init; } = "";
    public string UnitPriceText { get; init; } = "";
    public string LineTotalText { get; init; } = "";
}

public partial class InvoicePage : Page
{
    private readonly MainWindow _mainWindow;

    public InvoicePage(MainWindow mainWindow, InvoiceData data)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        Populate(data);
    }

    private void Populate(InvoiceData d)
    {
        InvoiceIdText.Text = $"Order IDs: {string.Join(", ", d.OrderIds.Select(id => $"#{id}"))}";
        CustomerText.Text  = $"{d.Username}  ({d.UserType})";
        DateText.Text      = d.Timestamp.ToString("dd MMM yyyy  HH:mm");

        if (d.IsDelivery)
        {
            DeliveryInfo.Visibility = Visibility.Visible;
            DeliveryText.Text       = d.DeliveryAddress;
        }

        // Line items
        LineItems.ItemsSource = d.Items.Select(item => new InvoiceLineVM
        {
            Name          = item.MenuItemName,
            Restaurant    = item.RestaurantName,
            Qty           = $"x{item.Quantity}",
            UnitPriceText = $"Rp {item.UnitPrice:N0}",
            LineTotalText = $"Rp {item.Subtotal:N0}"
        }).ToList();

        // Summary
        SubtotalText.Text = $"Rp {d.Subtotal:N0}";

        if (d.Discount > 0)
        {
            DiscountRow.Visibility = Visibility.Visible;
            DiscountLabel.Text     = $"{d.UserType} Discount";
            DiscountText.Text      = $"- Rp {d.Discount:N0}";
        }

        if (d.IsDelivery)
        {
            DeliveryRow.Visibility = Visibility.Visible;
            DeliveryFeeText.Text   = $"Rp {d.DeliveryFee:N0}";
        }

        GrandTotalText.Text = $"Rp {d.GrandTotal:N0}";
    }

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        => _mainWindow.NavigateToRecommend();

    private void BtnOrders_Click(object sender, RoutedEventArgs e)
        => _mainWindow.NavigateToOrders();
}
