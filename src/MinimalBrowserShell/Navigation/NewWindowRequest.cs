namespace MinimalBrowserShell.Navigation;

public sealed record NewWindowRequest(Uri? Uri, bool UseCurrentUri = false)
{
    public static NewWindowRequest Startup() => new(null, false);
    public static NewWindowRequest Current() => new(null, true);
}
