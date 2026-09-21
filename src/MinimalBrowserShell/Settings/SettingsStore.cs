using System.IO;
using System.Text.Json;

namespace MinimalBrowserShell.Settings;

public sealed class SettingsStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public SettingsStore(string? path = null)
    {
        _path = path ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MinimalBrowserShell", "settings.json");
    }

    public BrowserSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new BrowserSettings();
            return JsonSerializer.Deserialize<BrowserSettings>(File.ReadAllText(_path), Options) ?? new BrowserSettings();
        }
        catch { return new BrowserSettings(); }
    }

    public void Save(BrowserSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(settings, Options));
        }
        catch { /* Settings are optional; retain in-memory defaults. */ }
    }
}
