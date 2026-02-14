# Recovery Lifecycle

## Capture Path

1. App computes current recoverable state snapshot.
2. Service wraps snapshot in `RecoveryEnvelope<TState>`.
3. Store persists by `(Contract, Scope)`.

## Restore Path

1. App requests restore for given `Scope`.
2. Store returns envelope if available.
3. Service validates contract.
4. App decides whether to hydrate immediately or prompt user.

## Clear Path

1. App calls clear when recovery is consumed or invalidated.
2. Store removes snapshot document for given scope.

## Recommended Triggers

- Capture:
  - periodic autosave,
  - before process exit,
  - before destructive operation.
- Restore:
  - startup bootstrap.
- Clear:
  - successful explicit save,
  - user discards recovery.
