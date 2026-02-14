# ADR-0002: Best-Effort Persistence (Supersedes Versioned Settings)

Status: Accepted
Date: 2026-02-14

## Context

The framework targets simple applications where a small codebase is preferred over strict migration infrastructure.

Previous direction used explicit schema versions and migrators for settings/recovery payloads.
That added complexity that was not justified for the current product goals.

## Decision

Adopt a best-effort persistence model for settings and recovery payloads.

Rules:
- keep persisted contracts small and stable,
- ignore unknown JSON fields,
- rely on defaults for missing fields,
- quarantine unreadable payloads and continue with defaults,
- avoid mandatory schema versioning and migration pipelines.

## Consequences

Positive:
- smaller API surface,
- fewer moving parts,
- easier onboarding and maintenance.

Tradeoff:
- breaking persistence model changes may reset state to defaults,
- developers must avoid frequent incompatible contract changes.

## Operational Guidance

For app updates:
- prefer additive contract changes,
- keep defaults safe,
- document rare breaking changes clearly.
