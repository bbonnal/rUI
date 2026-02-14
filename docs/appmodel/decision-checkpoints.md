# Decision Checkpoints

These are the key architectural decisions to keep simple app persistence consistent.

## 1) Settings Compatibility Policy

Question: strict migrations or best-effort settings load?

Options:
- best-effort load (recommended): smallest codebase, defaults fallback.
- strict versioned migrations: more control, more maintenance.

Impact:
- affects code size and update discipline.

## 2) Persistence Partitioning

Question: should settings/recovery be partitioned by user profile from day one?

Options:
- single-profile first (recommended): fastest path.
- profile-aware keys now: avoids future key migration.

Impact:
- affects `PersistenceKey` layout and default path structure.

## 3) Recovery Restore Policy

Question: should restore be automatic or prompt-based by default?

Options:
- prompt-based (recommended): safer UX, avoids surprising state overwrite.
- automatic restore: fastest startup and continuity.

Impact:
- affects `rUI.AppModel.Avalonia` startup coordinator behavior.

## 4) Store Format

Question: JSON only initially, or pluggable from day one?

Options:
- JSON first with contracts already pluggable (recommended).
- implement at least two providers immediately (JSON + SQLite).

Impact:
- affects delivery speed vs early extensibility testing.

## 5) Corruption Handling

Question: on deserialization failure, should the system auto-reset or quarantine and retry?

Options:
- quarantine bad payload + use defaults (recommended).
- hard fail and block startup.

Impact:
- affects reliability, diagnostics, and user support posture.
