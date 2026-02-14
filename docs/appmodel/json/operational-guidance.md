# Operational Guidance

## Backup

- Back up `RootDirectory` before major upgrades.
- Keep at least one previous version in deployment rollback plans.

## Monitoring

- Count quarantine file creation events.
- Alert on rapid increase (signals serializer regressions or contract breaks).

## Release Discipline (Best Effort Settings)

- Prefer additive settings changes over breaking changes.
- Keep durable settings contracts small and stable.
- If a breaking change is necessary, document that old settings may reset to defaults.

## Recovery UX Policy

Current recommendation is prompt-based restore.

Implement in host startup:
1. check recoverable snapshot,
2. ask user whether to restore/discard,
3. apply chosen action.
