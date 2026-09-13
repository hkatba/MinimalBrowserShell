# Minimal Single-Tab Browser Shell Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Windows C#/.NET WebView2 browser shell where every window contains exactly one webpage and Ctrl+T/Ctrl+N create another window instead of a tab.

**Architecture:** Use a WPF desktop application with one WebView2 per window. Keep window creation, URL normalization, settings, and keyboard-command decisions in small testable classes; keep WPF/WebView2 event handling thin. Use the installed Microsoft Edge WebView2 runtime rather than bundling another Chromium engine.

**Tech Stack:** .NET 8, WPF, C#, Microsoft.Web.WebView2, xUnit, GitHub Actions on Windows.

**Spec:** `docs/superpowers/specs/2026-09-13-minimal-single-tab-browser-design.md`

## Global Constraints

- Windows desktop application.
- C#/.NET and Microsoft WebView2.
- One WebView2 control per window; no tab collection or tab UI.
- No extensions, bookmark bar, or full browser UI.
- Startup URL must be configurable and suitable for self-hosted Karakeep.
- Ctrl+N and Ctrl+T create a new browser window.
- Ctrl+W closes the current window.
- Ctrl+L exposes/focuses address input.
- Alt+Left/Alt+Right navigate back/forward; Ctrl+R/F5 reload.
- `target=_blank` and equivalent popup requests become new application windows.
- Settings failures fall back to defaults; navigation failures must not terminate the application.
- Unit tests cover URL normalization, keyboard commands, and new-window decisions.

---

### Task 1: Bootstrap the WPF solution and project configuration

**Files:**
- Create: `MinimalBrowserShell.sln`
- Create: `src/MinimalBrowserShell/MinimalBrowserShell.csproj`
- Create: `src/MinimalBrowserShell/App.xaml`
- Create: `src/MinimalBrowserShell/App.xaml.cs`
- Create: `tests/MinimalBrowserShell.Tests/MinimalBrowserShell.Tests.csproj`
- Create: `.gitignore`
- Create: `.github/workflows/build.yml`

**Interfaces:**
- Produces a buildable .NET 8 WPF executable project and xUnit test project.
- The application project references `Microsoft.Web.WebView2`; the test project references the application project.

- [ ] **Step 1: Create the solution and projects**

Run:
```powershell
dotnet new sln -n MinimalBrowserShell
dotnet new wpf -n MinimalBrowserShell -o src/MinimalBrowserShell --framework net8.0-windows
dotnet new xunit -n MinimalBrowserShell.Tests -o tests/MinimalBrowserShell.Tests --framework net8.0
```

- [ ] **Step 2: Add the projects to the solution**

Run:
```powershell
dotnet sln MinimalBrowserShell.sln add src/MinimalBrowserShell/MinimalBrowserShell.csproj
dotnet sln MinimalBrowserShell.sln add tests/MinimalBrowserShell.Tests/MinimalBrowserShell.Tests.csproj
dotnet add tests/MinimalBrowserShell.Tests/MinimalBrowserShell.Tests.csproj reference src/MinimalBrowserShell/MinimalBrowserShell.csproj
```

- [ ] **Step 3: Configure WPF and WebView2**

Set the application target to `net8.0-windows`, enable WPF, add `Microsoft.Web.WebView2`, and use nullable reference types and implicit usings. Keep the app self-contained only where required for the .NET runtime; do not bundle a separate Chromium runtime.

- [ ] **Step 4: Add the minimal application bootstrap**

Create `App.xaml` and `App.xaml.cs` so application startup constructs a `BrowserWindow` through the window manager after loading settings.

- [ ] **Step 5: Add Windows build CI**

Create a GitHub Actions workflow on `windows-latest` that restores, builds Release, and runs the unit tests.

- [ ] **Step 6: Verify the bootstrap**

Run:
```powershell
dotnet restore
dotnet build MinimalBrowserShell.sln -c Release
dotnet test MinimalBrowserShell.sln -c Release --no-build
```

Expected: build succeeds and the initial test project passes.

- [ ] **Step 7: Commit**

```bash
git add .
git commit -m "chore: bootstrap minimal browser shell"
```

---

### Task 2: Implement URL normalization and settings with tests first

**Files:**
- Create: `src/MinimalBrowserShell/Navigation/UrlNormalizer.cs`
- Create: `src/MinimalBrowserShell/Settings/BrowserSettings.cs`
- Create: `src/MinimalBrowserShell/Settings/SettingsStore.cs`
- Create: `tests/MinimalBrowserShell.Tests/Navigation/UrlNormalizerTests.cs`
- Create: `tests/MinimalBrowserShell.Tests/Settings/SettingsStoreTests.cs`

**Interfaces:**
- `UrlNormalizer.Normalize(string input) -> Uri`
- `BrowserSettings.StartupUrl: Uri`
- `BrowserSettings.SearchUrlTemplate: string`
- `SettingsStore.Load() -> BrowserSettings`
- `SettingsStore.Save(BrowserSettings settings)`

- [ ] **Step 1: Write failing URL tests**

```csharp
[Theory]
[InlineData("https://example.com", "https://example.com/")]
[InlineData("example.com", "https://example.com/")]
public void NormalizeAcceptsHttpUrls(string input, string expected)
{
    Assert.Equal(expected, UrlNormalizer.Normalize(input).ToString());
}

[Fact]
public void NormalizeTurnsSearchTextIntoSearchUrl()
{
    var uri = UrlNormalizer.Normalize("karakeep self hosted", "https://www.google.com/search?q={0}");
    Assert.Contains("karakeep", uri.Query, StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Run the URL tests and verify failure**

Run:
```powershell
dotnet test tests/MinimalBrowserShell.Tests/MinimalBrowserShell.Tests.csproj --filter UrlNormalizerTests
```

Expected: FAIL because `UrlNormalizer` does not exist.

- [ ] **Step 3: Implement URL normalization minimally**

Accept `http` and `https`; prepend `https://` for host-like input; otherwise URL-encode the text into the configured search template. Reject unsupported explicit schemes with `UriFormatException`.

- [ ] **Step 4: Run URL tests and verify pass**

Run the same test command. Expected: PASS.

- [ ] **Step 5: Write failing settings tests**

Test that a missing settings file returns a default startup URL, saving then loading preserves startup/search settings, and malformed settings fall back safely.

- [ ] **Step 6: Implement settings persistence**

Use JSON under `%LocalAppData%\MinimalBrowserShell\settings.json`. Default startup URL is `https://example.com/`; expose it so users can set Karakeep without recompiling.

- [ ] **Step 7: Run settings tests**

Run:
```powershell
dotnet test tests/MinimalBrowserShell.Tests/MinimalBrowserShell.Tests.csproj --filter SettingsStoreTests
```

Expected: PASS.

- [ ] **Step 8: Commit**

```bash
git add src/MinimalBrowserShell/Navigation src/MinimalBrowserShell/Settings tests/MinimalBrowserShell.Tests
 git commit -m "feat: add url normalization and settings"
```

---

### Task 3: Implement command and window-target logic with tests first

**Files:**
- Create: `src/MinimalBrowserShell/Navigation/BrowserCommand.cs`
- Create: `src/MinimalBrowserShell/Navigation/BrowserCommandResolver.cs`
- Create: `src/MinimalBrowserShell/Navigation/NewWindowRequest.cs`
- Create: `tests/MinimalBrowserShell.Tests/Navigation/BrowserCommandResolverTests.cs`

**Interfaces:**
- `BrowserCommand`: `NewWindow`, `CloseWindow`, `FocusAddress`, `Back`, `Forward`, `Reload`.
- `BrowserCommandResolver.Resolve(Key key, ModifierKeys modifiers) -> BrowserCommand?`
- `NewWindowRequest(Uri? Uri, bool UseCurrentUri)`.

- [ ] **Step 1: Write failing command tests**

```csharp
[Theory]
[InlineData(Key.T, ModifierKeys.Control, BrowserCommand.NewWindow)]
[InlineData(Key.N, ModifierKeys.Control, BrowserCommand.NewWindow)]
[InlineData(Key.W, ModifierKeys.Control, BrowserCommand.CloseWindow)]
[InlineData(Key.L, ModifierKeys.Control, BrowserCommand.FocusAddress)]
[InlineData(Key.Left, ModifierKeys.Alt, BrowserCommand.Back)]
[InlineData(Key.Right, ModifierKeys.Alt, BrowserCommand.Forward)]
[InlineData(Key.F5, ModifierKeys.None, BrowserCommand.Reload)]
public void ResolvesExpectedCommand(Key key, ModifierKeys modifiers, BrowserCommand expected)
{
    Assert.Equal(expected, BrowserCommandResolver.Resolve(key, modifiers));
}
```

- [ ] **Step 2: Run the tests and verify failure**

Run the filtered test. Expected: FAIL because the resolver is missing.

- [ ] **Step 3: Implement command resolution**

Treat Ctrl+T and Ctrl+N identically as `NewWindow`; map the remaining commands exactly as defined by the spec; return null for unrelated key combinations.

- [ ] **Step 4: Add new-window decision tests**

Cover explicit popup URIs, Ctrl+T using the current page, and new-window requests with no recoverable URI.

- [ ] **Step 5: Implement `NewWindowRequest`**

Represent either a requested URI or a signal to open the configured new-window/startup URL. Do not create a tab abstraction.

- [ ] **Step 6: Run navigation tests**

Run:
```powershell
dotnet test tests/MinimalBrowserShell.Tests/MinimalBrowserShell.Tests.csproj --filter Navigation
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/MinimalBrowserShell/Navigation tests/MinimalBrowserShell.Tests/Navigation
 git commit -m "feat: define single-window browser commands"
```

---

### Task 4: Build the single-page browser window

**Files:**
- Create: `src/MinimalBrowserShell/Browser/BrowserWindow.xaml`
- Create: `src/MinimalBrowserShell/Browser/BrowserWindow.xaml.cs`
- Create: `src/MinimalBrowserShell/Browser/BrowserWindowManager.cs`
- Create: `src/MinimalBrowserShell/Browser/NavigationController.cs`

**Interfaces:**
- `BrowserWindowManager.CreateWindow(Uri? uri = null) -> BrowserWindow`
- `NavigationController.Navigate(Uri uri)`
- `NavigationController.Back()`, `Forward()`, `Reload()`
- `BrowserWindowManager.OpenNewWindow(NewWindowRequest request)`

- [ ] **Step 1: Create the minimal WPF window layout**

Use a root grid containing a small address bar row and a single `WebView2`. Do not create a `TabControl`, tab model, bookmarks bar, or secondary browser chrome. The address bar can be hidden until Ctrl+L is pressed.

- [ ] **Step 2: Initialize WebView2**

Create one WebView2 instance per `BrowserWindow`, initialize its environment, and navigate to the configured startup URI. Keep environment options lean and avoid extensions and unnecessary features.

- [ ] **Step 3: Implement navigation controller**

Centralize navigation and back/forward/reload operations. Catch initialization/navigation exceptions so a failed page load does not terminate the process.

- [ ] **Step 4: Implement window manager**

Every `CreateWindow` call creates a new WPF window containing exactly one WebView2. `OpenNewWindow` never reuses a tab because no tab concept exists.

- [ ] **Step 5: Wire address input**

Ctrl+L reveals/focuses the address input. Enter uses `UrlNormalizer`, navigates the current WebView2, and hides the input again.

- [ ] **Step 6: Verify manually**

Run:
```powershell
dotnet run --project src/MinimalBrowserShell/MinimalBrowserShell.csproj
```

Expected: one minimal window opens with the configured startup page and no tab bar.

- [ ] **Step 7: Commit**

```bash
git add src/MinimalBrowserShell/Browser
 git commit -m "feat: add single-page browser window"
```

---

### Task 5: Wire keyboard commands and popup/new-window behavior

**Files:**
- Modify: `src/MinimalBrowserShell/Browser/BrowserWindow.xaml.cs`
- Modify: `src/MinimalBrowserShell/Browser/BrowserWindowManager.cs`
- Modify: `src/MinimalBrowserShell/Browser/NavigationController.cs`

**Interfaces:**
- BrowserWindow handles WPF preview-key events and delegates to `BrowserCommandResolver`.
- WebView2 `NewWindowRequested` events become `NewWindowRequest` instances passed to `BrowserWindowManager.OpenNewWindow`.

- [ ] **Step 1: Wire Ctrl+T/Ctrl+N**

Handle both before WebView2 receives them and call `BrowserWindowManager.CreateWindow`, using the configured startup URL. Never allow a browser tab to be created.

- [ ] **Step 2: Wire Ctrl+W**

Close the current WPF window. If it is the final window, allow the normal application shutdown behavior.

- [ ] **Step 3: Wire Ctrl+L, navigation, and reload commands**

Route Ctrl+L, Alt+Left, Alt+Right, Ctrl+R, and F5 through the command resolver and navigation controller.

- [ ] **Step 4: Handle WebView2 popup requests**

Cancel `NewWindowRequested` and create a new `BrowserWindow` for the requested URI. This covers `target=_blank` and similar popup navigation without exposing tabs.

- [ ] **Step 5: Handle navigation failures**

Show a minimal error view/message while leaving the window usable. Do not crash or silently exit.

- [ ] **Step 6: Manually smoke-test required behavior**

Verify:
1. Ctrl+T creates a second window.
2. Ctrl+N creates a second window.
3. No tab appears in either window.
4. Ctrl+W closes only the current window.
5. Ctrl+L accepts a URL.
6. Alt+Left/Alt+Right work when history exists.
7. F5/Ctrl+R reload.
8. A `target=_blank` link opens a new application window.

- [ ] **Step 7: Commit**

```bash
git add src/MinimalBrowserShell/Browser
 git commit -m "feat: enforce one webpage per browser window"
```

---

### Task 6: Add user-facing configuration and packaging documentation

**Files:**
- Create: `README.md`
- Create: `src/MinimalBrowserShell/appsettings.example.json`
- Create: `packaging/README.md`
- Modify: `src/MinimalBrowserShell/Settings/BrowserSettings.cs`

**Interfaces:**
- Configuration documents `StartupUrl` and `SearchUrlTemplate`.
- README documents Windows prerequisites, WebView2 runtime requirement, build/run commands, keyboard shortcuts, and Karakeep setup.

- [ ] **Step 1: Add example configuration**

Use:
```json
{
  "startupUrl": "http://localhost:3000",
  "searchUrlTemplate": "https://www.google.com/search?q={0}"
}
```

- [ ] **Step 2: Document the default behavior**

Explain that the app intentionally has no tabs and that Ctrl+T is mapped to a new window. Explain how to point startup URL at a self-hosted Karakeep instance.

- [ ] **Step 3: Document WebView2 prerequisites**

Explain that the Windows WebView2 Runtime is required and is not bundled as a second Chromium engine by this project.

- [ ] **Step 4: Document build and publish commands**

Include:
```powershell
dotnet restore
dotnet build MinimalBrowserShell.sln -c Release
dotnet test MinimalBrowserShell.sln -c Release
dotnet publish src/MinimalBrowserShell/MinimalBrowserShell.csproj -c Release -r win-x64 --self-contained false
```

- [ ] **Step 5: Commit**

```bash
git add README.md packaging src/MinimalBrowserShell/Settings/BrowserSettings.cs
 git commit -m "docs: document browser configuration and packaging"
```

---

### Task 7: Final verification and release build

**Files:**
- Modify only files required by verification failures.

- [ ] **Step 1: Run the complete test suite**

```powershell
dotnet test MinimalBrowserShell.sln -c Release
```

Expected: all tests pass.

- [ ] **Step 2: Run the Release build**

```powershell
dotnet build MinimalBrowserShell.sln -c Release --no-restore
```

Expected: zero errors and zero warnings introduced by the project.

- [ ] **Step 3: Run the publish build**

```powershell
dotnet publish src/MinimalBrowserShell/MinimalBrowserShell.csproj -c Release -r win-x64 --self-contained false
```

Expected: Windows publish output is produced successfully.

- [ ] **Step 4: Perform final manual smoke test**

Launch the published executable and verify the complete behavior from Task 5, including Karakeep startup when configured, popup links, and multiple independent windows.

- [ ] **Step 5: Inspect the final diff**

Run:
```bash
git status --short
git diff --check
git log --oneline -8
```

Expected: no accidental files, no whitespace errors, and commits correspond to the planned tasks.

- [ ] **Step 6: Commit any verification fixes**

```bash
git add .
git commit -m "chore: verify minimal browser release"
```
