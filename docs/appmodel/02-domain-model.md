# Domain Model

## Core Concepts

1. `Contract`
- Stable logical name of persisted payload family.
- Example: `rui.app.settings.main-window`.

2. `Scope`
- Context partition for recovery state.
- Example: workspace id, profile id, tenant id.

3. `Envelope`
- Metadata + payload container.
- Avoids hidden serializer metadata.

## Settings Envelope

`SettingsEnvelope<TSettings>`
- `Contract`
- `UpdatedAtUtc`
- `Data`

## Recovery Envelope

`RecoveryEnvelope<TState>`
- `Contract`
- `Scope`
- `CapturedAtUtc`
- `Data`

## Abstraction Layers

1. High-level services
- `ISettingsService<TSettings>`
- `IRecoveryService<TState>`

2. Persistence-specific stores
- `ISettingsStore<TSettings>`
- `IRecoveryStore<TState>`

3. Transport/infrastructure
- `IDocumentStore`
- `IAppModelSerializer`
