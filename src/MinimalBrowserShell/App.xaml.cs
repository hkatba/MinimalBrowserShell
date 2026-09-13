using System.Windows;
using MinimalBrowserShell.Browser;
using MinimalBrowserShell.Settings;

namespace MinimalBrowserShell;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var settings = new SettingsStore().Load();
        var manager = new BrowserWindowManager(settings);
        manager.CreateWindow();
    }
}
