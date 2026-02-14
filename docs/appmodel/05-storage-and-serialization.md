# Storage And Serialization

## `IDocumentStore`

Lowest-level storage contract:
- read raw string by key,
- write raw string by key,
- delete by key.

The store is intentionally text-based to keep cross-provider compatibility.

## `IAppModelSerializer`

Serializer contract for typed payload conversion:
- `Serialize<T>(T value)`
- `Deserialize<T>(string payload)`

This lets implementations swap `System.Text.Json`, protobuf wrappers, or encrypted formats.

## Separation Benefits

- Store implementations can stay dumb and fast.
- Serializer policy can be changed without rewriting store.
- Testing can isolate storage failures from serialization failures.

## Suggested First Implementation Package

`rUI.AppModel.Json`:
- `SystemTextJsonAppModelSerializer`
- `FileDocumentStore` with atomic write strategy
  - write temp,
  - fsync,
  - rename.
