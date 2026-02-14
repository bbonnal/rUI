# Quickstart

## 1) Build Infrastructure

```csharp
var serializer = new SystemTextJsonAppModelSerializer();
var docStore = new FileDocumentStore(new FileDocumentStoreOptions(
    RootDirectory: Path.Combine(appDataRoot, "state")));
```

## 2) Settings Store + Service (Best Effort)

```csharp
var settingsStore = new JsonSettingsStore<MySettings>(
    contract: "rui.settings.main",
    documentStore: docStore,
    serializer: serializer);

var settingsService = new JsonSettingsService<MySettings>(
    settingsStore,
    new JsonSettingsServiceOptions<MySettings>(
        CreateDefaults: () => new MySettings()));
```

Behavior:
- unknown JSON fields are ignored by deserialization,
- missing fields fall back to defaults in your settings type,
- unreadable payloads are quarantined and defaults are returned.

## 3) Recovery Store + Service

```csharp
var recoveryStore = new JsonRecoveryStore<MyWorkspaceState>(
    contract: "rui.recovery.workspace",
    documentStore: docStore,
    serializer: serializer);

var recoveryService = new JsonRecoveryService<MyWorkspaceState>(recoveryStore);
```

## 4) Use

```csharp
var settings = await settingsService.GetAsync();
settings.Theme = "dark";
await settingsService.SaveAsync(settings);

await recoveryService.CaptureAsync("default", currentState);
var restored = await recoveryService.TryRestoreAsync("default");
```
