# rUI.AppModel.Json

`rUI.AppModel.Json` is the first concrete runtime for `rUI.AppModel`.

It provides:
- `System.Text.Json` serializer implementation,
- atomic file-backed document store,
- settings store/service,
- recovery store/service,
- corruption quarantine support.

## What This Solves For You

Before this package:
- each feature had to invent its own save/load format and file handling,
- corruption could break startup paths,
- consistency varied by feature.

With this package:
- settings/recovery persistence is standardized,
- file writes are atomic,
- settings loading is best-effort with defaults fallback,
- bad documents are quarantined instead of crashing core flows.

## Main Types

- `SystemTextJsonAppModelSerializer`
- `FileDocumentStore`, `FileDocumentStoreOptions`
- `JsonSettingsStore<TSettings>`, `JsonSettingsService<TSettings>`
- `JsonRecoveryStore<TState>`, `JsonRecoveryService<TState>`

## Related Docs

- `docs/appmodel/json/quickstart.md`
- `docs/appmodel/json/key-layout.md`
- `docs/appmodel/json/corruption-quarantine.md`
- `docs/appmodel/json/migration-pipeline.md`
- `docs/appmodel/json/operational-guidance.md`
