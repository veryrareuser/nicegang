using System.Windows;
using System.Windows.Controls;
using NICE.EAT.Data;
using NICE.EAT.WPF.ViewModels;

namespace NICE.EAT.WPF.Views;

public partial class LoginPage : Page
{
    private readonly MainWindow     _mainWindow;
    private readonly UserRepository _userRepo = new();
    private bool _isLoginTab = true;

    public LoginPage(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    // ── Tab switching ─────────────────────────────────────────────────────────
    private void TabLoginBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_isLoginTab) return;
        _isLoginTab = true;

        LoginPanel.Visibility    = Visibility.Visible;
        RegisterPanel.Visibility = Visibility.Collapsed;
        TabLoginBtn.Style        = (Style)FindResource("PrimaryButton");
        TabRegisterBtn.Style     = (Style)FindResource("TabInactiveButton");
        StatusMsg.Visibility     = Visibility.Collapsed;
    }

    private void TabRegisterBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!_isLoginTab) return;
        _isLoginTab = false;

        LoginPanel.Visibility    = Visibility.Collapsed;
        RegisterPanel.Visibility = Visibility.Visible;
        TabRegisterBtn.Style     = (Style)FindResource("PrimaryButton");
        TabLoginBtn.Style        = (Style)FindResource("TabInactiveButton");
        StatusMsg.Visibility     = Visibility.Collapsed;
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    private void BtnSignIn_Click(object sender, RoutedEventArgs e)
    {
        string username = LoginUsername.Text.Trim();
        if (string.IsNullOrEmpty(username))
        {
            ShowMsg("Please enter a username.");
            return;
        }

        try
        {
            var user = _userRepo.GetByUsername(username);
            if (user is null)
            {
                ShowMsg("Username not found. Try 'demo_user' or 'vip_member', or register.");
                return;
            }

            AppState.Instance.CurrentUser = user;
            _mainWindow.OnLoginSuccess();
        }
        catch (Exception ex) { ShowMsg($"Error: {ex.Message}"); }
    }

    // ── Register ──────────────────────────────────────────────────────────────
    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        string username = RegUsername.Text.Trim();
        string email    = RegEmail.Text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
        {
            ShowMsg("Username and email are required.");
            return;
        }

        try
        {
            var user = RegPremium.IsChecked == true
                ? (NICE.EAT.Models.User)_userRepo.CreatePremiumUser(username, email)
                : _userRepo.CreateUser(username, email);

            AppState.Instance.CurrentUser = user;
            _mainWindow.OnLoginSuccess();
        }
        catch (Exception ex) { ShowMsg($"Registration failed: {ex.Message}"); }
    }

    private void ShowMsg(string msg)
    {
        StatusMsg.Text       = msg;
        StatusMsg.Visibility = Visibility.Visible;
    }
}
