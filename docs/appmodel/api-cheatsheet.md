# API Cheatsheet

## Settings

```csharp
public interface ISettingsStore<TSettings> where TSettings : class
{
    string Contract { get; }
    Task<SettingsEnvelope<TSettings>?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SettingsEnvelope<TSettings> settings, CancellationToken cancellationToken = default);
    Task DeleteAsync(CancellationToken cancellationToken = default);
}
```

```csharp
public interface ISettingsService<TSettings> where TSettings : class
{
    Task<TSettings> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(TSettings settings, CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}
```

## Recovery

```csharp
public interface IRecoveryStore<TState> where TState : class
{
    string Contract { get; }
    Task<RecoveryEnvelope<TState>?> LoadAsync(string scope, CancellationToken cancellationToken = default);
    Task SaveAsync(RecoveryEnvelope<TState> state, CancellationToken cancellationToken = default);
    Task ClearAsync(string scope, CancellationToken cancellationToken = default);
}
```

```csharp
public interface IRecoveryService<TState> where TState : class
{
    Task<TState?> TryRestoreAsync(string scope, CancellationToken cancellationToken = default);
    Task CaptureAsync(string scope, TState state, CancellationToken cancellationToken = default);
    Task ClearAsync(string scope, CancellationToken cancellationToken = default);
}
```

## Infrastructure

```csharp
public interface IDocumentStore
{
    Task<string?> ReadAsync(PersistenceKey key, CancellationToken cancellationToken = default);
    Task WriteAsync(PersistenceKey key, string content, CancellationToken cancellationToken = default);
    Task DeleteAsync(PersistenceKey key, CancellationToken cancellationToken = default);
}
```

```csharp
public interface IAppModelSerializer
{
    string Serialize<T>(T value);
    T Deserialize<T>(string payload);
}
```
