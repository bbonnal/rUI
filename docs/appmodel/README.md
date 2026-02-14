# rUI.AppModel

`rUI.AppModel` is the contract package for application-level state in rUI.

It defines boundaries for:
- application settings persistence,
- session/workspace recovery,
- storage and serialization abstractions.

This package intentionally contains interfaces and neutral data envelopes only.
Runtime policies and concrete providers belong to implementation packages.

## Objectives

- Keep app-state architecture consistent across all future rUI modules.
- Avoid framework lock-in in core contracts.
- Enable replaceable providers (file, database, cloud, secure stores).
- Keep settings persistence simple for small apps (best-effort load, sane defaults).

## Current Surface

- `Settings`
  - `ISettingsStore<TSettings>`
  - `ISettingsService<TSettings>`
  - `SettingsEnvelope<TSettings>`
- `Recovery`
  - `IRecoveryStore<TState>`
  - `IRecoveryService<TState>`
  - `RecoveryEnvelope<TState>`
- `Storage`
  - `IDocumentStore`
- `Serialization`
  - `IAppModelSerializer`
- `Contracts`
  - `PersistenceKey`

## Next Related Packages

- `rUI.AppModel.Json` (default JSON serializer + file document store)
- `rUI.AppModel.SecureStorage` (secrets and encrypted payloads)
- `rUI.AppModel.Avalonia` (lifecycle helpers and autosave orchestration)

## References

- `docs/appmodel/01-goals-and-non-goals.md`
- `docs/appmodel/02-domain-model.md`
- `docs/appmodel/03-settings-lifecycle.md`
- `docs/appmodel/04-recovery-lifecycle.md`
- `docs/appmodel/05-storage-and-serialization.md`
- `docs/appmodel/implementation-roadmap.md`
- `docs/appmodel/decision-checkpoints.md`
- `docs/appmodel/selected-decisions.md`
- `docs/appmodel/persistence.md`
- `docs/appmodel/adrs/ADR-0001-appmodel-boundaries.md`
- `docs/appmodel/adrs/ADR-0002-contract-versioning.md`
- `docs/appmodel/json/README.md`
- `docs/appmodel/appmodeltester-persistence-showcase.md`
