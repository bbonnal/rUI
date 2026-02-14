# Corruption Quarantine

When a settings/recovery payload cannot be deserialized or fails contract/scope validation:

1. payload is copied to quarantine directory,
2. failure reason is written as metadata text file,
3. original persisted file is deleted,
4. caller receives `null`/default flow instead of crash.

## Quarantine Location

- `<root>/_quarantine/`

Files:
- `<safe-key>-<timestamp>.json`
- `<safe-key>-<timestamp>.meta.txt`

## Why This Matters

- App startup and restore remain resilient.
- Broken state is preserved for diagnostics.
- Support/debug workflows can inspect quarantined artifacts.
