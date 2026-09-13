using System.Net;

namespace MinimalBrowserShell.Navigation;

public static class UrlNormalizer
{
    public static Uri Normalize(string input, string searchTemplate = "https://www.google.com/search?q={0}")
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new UriFormatException("A URL or search query is required.");

        var value = input.Trim();
        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute))
        {
            if (absolute.Scheme is "http" or "https") return absolute;
            throw new UriFormatException($"Unsupported URI scheme: {absolute.Scheme}");
        }

        if (Uri.TryCreate("https://" + value, UriKind.Absolute, out var hostUri) && hostUri.Host.Contains('.'))
            return hostUri;

        var encoded = Uri.EscapeDataString(value);
        return new Uri(string.Format(searchTemplate, encoded), UriKind.Absolute);
    }
}
