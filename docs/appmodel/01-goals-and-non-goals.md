# Goals And Non-Goals

## Goals

- Provide stable contracts for app settings and recovery state.
- Keep settings persistence simple for small applications.
- Keep persistence-transport independent from domain models.
- Keep API small and composable.
- Fail safe to defaults when persisted settings are invalid.

## Non-Goals

- No concrete file/database implementation in `rUI.AppModel`.
- No direct Avalonia dependencies.
- No UI-specific autosave behavior in core contracts.
- No opinionated security/encryption mechanism in core contracts.
- No mandatory settings migration framework.

## Design Constraints

- Nullable enabled and async-first APIs.
- Deterministic, serializable envelopes for persisted state.
- Contracts should remain compatible with DI but not depend on DI frameworks.
