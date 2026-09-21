using Xunit;
using System.Windows.Input;
using MinimalBrowserShell.Navigation;

namespace MinimalBrowserShell.Tests;

public class NavigationTests
{
    [Theory]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("example.com", "https://example.com/")]
    public void NormalizesHttpUrls(string input, string expected)
        => Assert.Equal(expected, UrlNormalizer.Normalize(input).ToString());

    [Fact]
    public void ConvertsSearchTextToSearchUrl()
    {
        var uri = UrlNormalizer.Normalize("karakeep self hosted", "https://www.google.com/search?q={0}");
        Assert.Contains("karakeep", uri.Query, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(Key.T, ModifierKeys.Control, BrowserCommand.NewWindow)]
    [InlineData(Key.N, ModifierKeys.Control, BrowserCommand.NewWindow)]
    [InlineData(Key.W, ModifierKeys.Control, BrowserCommand.CloseWindow)]
    [InlineData(Key.L, ModifierKeys.Control, BrowserCommand.FocusAddress)]
    [InlineData(Key.Left, ModifierKeys.Alt, BrowserCommand.Back)]
    [InlineData(Key.Right, ModifierKeys.Alt, BrowserCommand.Forward)]
    [InlineData(Key.F5, ModifierKeys.None, BrowserCommand.Reload)]
    public void ResolvesSingleWindowShortcuts(Key key, ModifierKeys modifiers, BrowserCommand expected)
        => Assert.Equal(expected, BrowserCommandResolver.Resolve(key, modifiers));
}
