using System.Windows.Input;

namespace MinimalBrowserShell.Navigation;

public static class BrowserCommandResolver
{
    public static BrowserCommand? Resolve(Key key, ModifierKeys modifiers) =>
        (key, modifiers) switch
        {
            (Key.T or Key.N, ModifierKeys.Control) => BrowserCommand.NewWindow,
            (Key.W, ModifierKeys.Control) => BrowserCommand.CloseWindow,
            (Key.L, ModifierKeys.Control) => BrowserCommand.FocusAddress,
            (Key.Left, ModifierKeys.Alt) => BrowserCommand.Back,
            (Key.Right, ModifierKeys.Alt) => BrowserCommand.Forward,
            (Key.F5, ModifierKeys.None) or (Key.R, ModifierKeys.Control) => BrowserCommand.Reload,
            _ => null
        };
}
