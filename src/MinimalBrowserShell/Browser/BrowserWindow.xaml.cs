using System.Windows;
using System.Windows.Input;
using MinimalBrowserShell.Navigation;
using MinimalBrowserShell.Settings;

namespace MinimalBrowserShell.Browser;

public partial class BrowserWindow : Window
{
    private readonly BrowserWindowManager _manager;
    private readonly BrowserSettings _settings;
    private readonly Uri _initialUri;
    private NavigationController? _navigation;

    public BrowserWindow(BrowserWindowManager manager, BrowserSettings settings, Uri initialUri)
    {
        InitializeComponent();
        _manager = manager;
        _settings = settings;
        _initialUri = initialUri;
        Loaded += BrowserWindow_Loaded;
    }

    private async void BrowserWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await WebView.EnsureCoreWebView2Async();
            _navigation = new NavigationController(WebView);
            WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            WebView.CoreWebView2.NavigationCompleted += (_, args) =>
            {
                if (!args.IsSuccess && WebView.CoreWebView2 != null)
                    WebView.CoreWebView2.NavigateToString("<html><body style='font-family:Segoe UI;padding:32px'><h2>Navigation failed</h2><p>Unable to load this page.</p></body></html>");
            };
            _navigation.Navigate(_initialUri);
        }
        catch
        {
            Title = "Minimal Browser - WebView2 unavailable";
        }
    }

    private void CoreWebView2_NewWindowRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri))
            _manager.OpenNewWindow(new NewWindowRequest(uri));
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var command = BrowserCommandResolver.Resolve(e.Key == Key.System ? e.SystemKey : e.Key, Keyboard.Modifiers);
        if (command is null) return;
        e.Handled = true;
        switch (command)
        {
            case BrowserCommand.NewWindow:
                _manager.OpenNewWindow(NewWindowRequest.Current());
                break;
            case BrowserCommand.CloseWindow:
                Close();
                break;
            case BrowserCommand.FocusAddress:
                AddressBar.Visibility = Visibility.Visible;
                AddressInput.Text = WebView.Source?.ToString() ?? _settings.StartupUrl;
                AddressInput.Focus();
                AddressInput.SelectAll();
                break;
            case BrowserCommand.Back: _navigation?.Back(); break;
            case BrowserCommand.Forward: _navigation?.Forward(); break;
            case BrowserCommand.Reload: _navigation?.Reload(); break;
        }
    }

    private void AddressInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        try
        {
            _navigation?.Navigate(UrlNormalizer.Normalize(AddressInput.Text, _settings.SearchUrlTemplate));
            AddressBar.Visibility = Visibility.Collapsed;
            Keyboard.ClearFocus();
        }
        catch { }
    }
}
