using MinimalBrowserShell.Navigation;
using MinimalBrowserShell.Settings;

namespace MinimalBrowserShell.Browser;

public sealed class BrowserWindowManager
{
    private readonly BrowserSettings _settings;
    private readonly List<BrowserWindow> _windows = [];

    public BrowserWindowManager(BrowserSettings settings) => _settings = settings;

    public BrowserWindow CreateWindow(Uri? uri = null)
    {
        var target = uri ?? new Uri(_settings.StartupUrl);
        var window = new BrowserWindow(this, _settings, target);
        _windows.Add(window);
        window.Closed += (_, _) => _windows.Remove(window);
        window.Show();
        return window;
    }

    public void OpenNewWindow(NewWindowRequest request)
    {
        var uri = request.Uri ?? (request.UseCurrentUri ? null : new Uri(_settings.StartupUrl));
        CreateWindow(uri);
    }
}
