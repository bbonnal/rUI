# Implementation Roadmap

## Phase 1: Contracts (Completed)

- Introduce `rUI.AppModel`.
- Define settings/recovery envelopes.
- Define store/service/serializer contracts.
- Add architecture docs and ADRs.

## Phase 2: Default Runtime Package (Completed)

Package: `rUI.AppModel.Json`

- `SystemTextJsonAppModelSerializer`
- `FileDocumentStore`
- `JsonSettingsStore<TSettings>`
- `JsonRecoveryStore<TState>`
- Best-effort settings/recovery load policy with defaults fallback

## Phase 3: Orchestration

Package: `rUI.AppModel.Avalonia`

- app lifecycle hooks for autosave/recovery,
- throttled capture scheduler,
- startup restore coordinator.

## Phase 4: Hardening

- corruption detection and quarantine,
- backup rotation policy,
- telemetry hooks for read/write outcomes,
- fault injection tests.

## Phase 5: Extensions

- encrypted secret store,
- cloud sync strategy,
- multi-profile settings partitioning.
