using Microsoft.Web.WebView2.Wpf;
using MinimalBrowserShell.Navigation;

namespace MinimalBrowserShell.Browser;

public sealed class NavigationController
{
    private readonly WebView2 _webView;
    public NavigationController(WebView2 webView) => _webView = webView;
    public void Navigate(Uri uri) { try { _webView.CoreWebView2?.Navigate(uri.ToString()); } catch { } }
    public void Back() { try { if (_webView.CanGoBack) _webView.GoBack(); } catch { } }
    public void Forward() { try { if (_webView.CanGoForward) _webView.GoForward(); } catch { } }
    public void Reload() { try { _webView.Reload(); } catch { } }
}
