namespace MinimalBrowserShell.Settings;

public sealed class BrowserSettings
{
    public string StartupUrl { get; set; } = "https://example.com/";
    public string SearchUrlTemplate { get; set; } = "https://www.google.com/search?q={0}";
}
