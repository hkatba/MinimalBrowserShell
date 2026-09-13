# Minimal Single-Tab Browser Shell Design

## Goal
Build a Windows desktop browser shell in C#/.NET using WebView2 where every application window contains exactly one webpage and opening a new tab is replaced by opening a new window.

## Requirements
- Windows desktop application.
- C#/.NET and Microsoft WebView2.
- One WebView2 control per window; no tab collection or tab UI.
- Minimal browser chrome; no tab bar or bookmarks bar.
- Configurable startup URL, suitable for self-hosted Karakeep.
- `Ctrl+N` opens a new browser window.
- `Ctrl+T` opens a new browser window rather than a tab.
- `Ctrl+W` closes the current window.
- `Ctrl+L` exposes/focuses a minimal address input.
- Back, forward, refresh and address navigation are available through keyboard shortcuts and minimal chrome.
- Requests for new tabs/windows are represented as new application windows.
- Keep WebView2 configuration lean and avoid unnecessary browser features/extensions.
- Persist basic user settings such as startup URL and window state.
- Provide automated tests for testable navigation/window-management logic.
- Provide a Windows build/verification workflow.

## Architecture
A small WPF application hosts one WebView2 per `BrowserWindow`. A `BrowserWindowManager` owns window creation and ensures that every new browsing target becomes a new window. `NavigationController` contains URL normalization and navigation decisions, while `SettingsStore` persists configuration. UI code remains thin and delegates behavior to these components.

WebView2 supplies the Chromium rendering engine already installed/managed through the Microsoft Edge WebView2 runtime. The application does not implement its own renderer or bundle a second browser engine.

## Keyboard and navigation behavior
- Ctrl+N / Ctrl+T: create a new browser window using the configured new-window URL or current navigation target as appropriate.
- Ctrl+W: close current window.
- Ctrl+L: show/focus address input.
- Alt+Left: back.
- Alt+Right: forward.
- Ctrl+R or F5: reload.
- Enter in address input: navigate the current window.
- Links with `target=_blank` or equivalent new-window requests: cancel the WebView2 popup and create a BrowserWindow for the requested URI.

## URL behavior
Accept absolute HTTP/HTTPS URLs. For non-URL text entered in the address field, use a configurable search provider. The initial implementation defaults to HTTPS URLs and a simple search fallback, while rejecting unsupported schemes unless explicitly handled by WebView2 policy.

## Error handling
Navigation failures remain visible through a minimal error state/page and do not terminate the browser process. Popup creation failures fall back to the current window only when a target URI cannot be recovered; otherwise they create a new BrowserWindow. Settings failures fall back to defaults.

## Testing
Unit tests cover URL normalization, keyboard-command mapping, and window-target decisions. Build verification compiles the WPF application and runs the test suite. Manual smoke testing covers opening Karakeep, Ctrl+T/Ctrl+N new-window behavior, popup links, navigation, and closing windows.

## Non-goals
- Multi-tab browsing.
- Extension management UI.
- Full bookmark/history/password-manager UI.
- A custom rendering engine.
- Bundling a separate Chromium runtime.
