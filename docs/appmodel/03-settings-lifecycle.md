# Settings Lifecycle

## Read Path (Best Effort)

1. Service requests settings from `ISettingsStore<TSettings>`.
2. Store loads document envelope.
3. Store/service validates `Contract`.
4. If deserialization succeeds, typed settings are returned.
5. If load/parse fails, defaults are returned.

## Write Path

1. Service receives typed settings from caller.
2. Envelope is created with:
- current contract,
- `UpdatedAtUtc = now`.
3. Envelope is persisted atomically by store.

## Reset Path

1. Service clears persisted document.
2. Service returns defaults on next `GetAsync`.

## Validation Points

- Contract exact match.
- Deserialization failure handling policy.
- Settings type defaults are valid and safe.
