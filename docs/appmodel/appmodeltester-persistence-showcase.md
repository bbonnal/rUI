# AppModelTester Persistence Showcase

## Purpose
`rUIAppModelTester` is a focused showcase of durable settings persistence using `rUI.AppModel` + `rUI.AppModel.Json`.

Durable settings currently demonstrated:
- `LanguageCode`
- `ThemeMode`

## Architecture
Main runtime service:
- `rUIAppModelTester/Services/IAppSettingsRuntime.cs`
- `rUIAppModelTester/Services/AppSettingsRuntime.cs`

Responsibilities:
- initialize settings once during startup flow
- apply settings side effects (translation culture + Avalonia theme variant)
- keep normalized in-memory current settings
- expose diagnostics metadata (`LastLoadedAtUtc`, `LastSavedAtUtc`, `SettingsFilePath`)
- raise `Changed` for live synchronization

## Startup Flow
Startup happens through main window VM initialization:
- `rUIAppModelTester/ViewModels/MainWindowViewModel.cs`
- `InitializeAsync()` calls `IAppSettingsRuntime.InitializeAsync()` before first page navigation

This avoids page-specific lazy loading and keeps app behavior consistent on launch.

## Settings Contract
Contract:
- `rUIAppModelTester/AppModel/AppModelTesterSettings.cs`

Contract name:
- `rUIAppModelTester/AppModel/AppModelTesterSettingsContract.cs`

No settings schema/migration pipeline is used.
Settings loading is best-effort with defaults fallback.

## UI Pages
### Settings Page
Files:
- `rUIAppModelTester/ViewModels/SettingsPageViewModel.cs`
- `rUIAppModelTester/Views/SettingsPageView.axaml`

Capabilities:
- preview language/theme changes in runtime
- `Save` persists minimal settings payload
- `Reset defaults` persists defaults
- `Delete persisted` removes settings file and applies defaults

### Persistence Inspector
Files:
- `rUIAppModelTester/ViewModels/PersistenceInspectorPageViewModel.cs`
- `rUIAppModelTester/Views/PersistenceInspectorPageView.axaml`

Displays:
- runtime `LanguageCode` and `ThemeMode`
- load/save timestamps
- resolved settings file path
- file existence
- raw persisted JSON

Includes `Reload from disk` command.

## Storage Location
- root: `%LocalAppData%/rUI/rUIAppModelTester` (platform equivalent)
- file: `<root>/settings/rui.appmodeltester.settings.json`

## Design Intent
This tester is intentionally strict:
- durable settings in persisted model
- transient UX state in viewmodels/runtime only

That separation is the key rule for future AppModel features.
