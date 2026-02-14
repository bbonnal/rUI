using rUI.AppModel.Recovery;

namespace rUI.AppModel.Json.Recovery;

public sealed class JsonRecoveryService<TState>(
    IRecoveryStore<TState> store) : IRecoveryService<TState>
    where TState : class
{
    public async Task<TState?> TryRestoreAsync(string scope, CancellationToken cancellationToken = default)
    {
        var envelope = await store.LoadAsync(scope, cancellationToken);
        if (envelope is null)
            return null;

        return envelope.Data;
    }

    public Task CaptureAsync(string scope, TState state, CancellationToken cancellationToken = default)
    {
        var envelope = new RecoveryEnvelope<TState>(
            Contract: store.Contract,
            Scope: scope,
            CapturedAtUtc: DateTimeOffset.UtcNow,
            Data: state);

        return store.SaveAsync(envelope, cancellationToken);
    }

    public Task ClearAsync(string scope, CancellationToken cancellationToken = default)
    {
        return store.ClearAsync(scope, cancellationToken);
    }
}
