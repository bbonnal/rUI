# Settings Load Policy (Best Effort)

Settings persistence now uses a best-effort loading model.

## Rules

1. Load settings JSON using the configured serializer.
2. If JSON contains unknown fields, ignore them.
3. If fields are missing, rely on defaults in your settings type.
4. If deserialization/contract validation fails, quarantine payload and return defaults.
5. On next save, rewrite the payload using the current settings contract shape.

## Why

- keeps codebase small,
- avoids migration boilerplate for simple apps,
- keeps apps functional across small additive changes.

## Developer Responsibility

This approach assumes developers avoid frequent breaking persistence changes.
If a breaking change is unavoidable, prefer one-time manual reset guidance.
