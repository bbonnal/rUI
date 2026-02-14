# Persistence Model in rUI / AppModelTester

## Core Principle
The persistence layer does **not** save full views or viewmodels automatically.

Persistence only stores the explicit settings contract passed to:
- `ISettingsService<TSettings>.SaveAsync(TSettings settings)`

So persistence scope is controlled by `TSettings`.

## Current AppModelTester Contract (Minimal)
File:
- `rUIAppModelTester/AppModel/AppModelTesterSettings.cs`

Persisted fields:
- `LanguageCode`
- `ThemeMode`

This is intentional. Only durable configuration is persisted.

## What Is Not Persisted
Transient UI state stays in runtime/viewmodel state and is never serialized, for example:
- pending-change counters
- last action messages
- temporary UI feedback text

These values are useful during interaction, but should not survive app restart.

## Envelope Shape on Disk
`JsonSettingsService<TSettings>` wraps your settings in `SettingsEnvelope<TSettings>`:

```json
{
  "Contract": "rui.appmodeltester.settings",
  "UpdatedAtUtc": "2026-02-14T12:00:00.0000000+00:00",
  "Data": {
    "LanguageCode": "fr",
    "ThemeMode": "dark"
  }
}
```

Meaning:
- `Contract`: logical identity of settings payload
- `UpdatedAtUtc`: save timestamp
- `Data`: your durable settings contract only

## Best-Effort Settings Policy
For settings payloads:
- unknown fields are ignored,
- missing fields use defaults from your settings type,
- broken payloads fall back to defaults and can be quarantined.

This keeps small app codebases simple.

## Pipeline (Concrete)
1. Build durable settings object (`AppModelTesterSettings`).
2. Call `IAppSettingsRuntime.SaveAsync(...)`.
3. Runtime normalizes values and delegates to `ISettingsService<AppModelTesterSettings>`.
4. `JsonSettingsService` wraps into `SettingsEnvelope<TSettings>`.
5. `JsonSettingsStore` serializes JSON and writes file via `IDocumentStore`.
6. On load, service deserializes and returns `Data` (or defaults if invalid/unreadable).

References:
- `rUI.AppModel/Settings/ISettingsService.cs`
- `rUI.AppModel/Settings/SettingsEnvelope.cs`
- `rUI.AppModel.Json/Settings/JsonSettingsService.cs`
- `rUI.AppModel.Json/Settings/JsonSettingsStore.cs`
- `rUIAppModelTester/Services/AppSettingsRuntime.cs`

## Persisted vs Transient Decision Rule
Persist only values required to reproduce intended app behavior after restart.

Persisted examples:
- language
- theme mode
- autosave enabled
- preferred units

Transient examples:
- in-page unsaved hint text
- last clicked section name
- temporary notifications
- current dialog state

## Startup and Runtime Application
Runtime service:
- `rUIAppModelTester/Services/IAppSettingsRuntime.cs`
- `rUIAppModelTester/Services/AppSettingsRuntime.cs`

Responsibilities:
- load persisted settings once during startup path
- apply side effects (language + theme)
- expose current runtime settings and timestamps
- notify listeners on changes

Startup call site:
- `rUIAppModelTester/ViewModels/MainWindowViewModel.cs`
- `InitializeAsync()` calls `IAppSettingsRuntime.InitializeAsync()` before first navigation

## Why This Model Is Safer
- avoids accidental persistence of UI noise
- keeps stored payloads stable and understandable
- keeps implementation small for simple apps
- makes debugging simpler (inspector shows minimal payload)

## Checklist Before Adding a Persisted Field
1. Must this survive app restart?
2. Is it domain configuration rather than interaction telemetry?
3. Would stale persisted value confuse UX?
4. Are defaults safe if this field is missing?

If any answer is uncertain, keep the field transient until proven durable.
