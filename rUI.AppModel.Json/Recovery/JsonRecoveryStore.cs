using rUI.AppModel.Contracts;
using rUI.AppModel.Recovery;
using rUI.AppModel.Serialization;
using rUI.AppModel.Storage;
using rUI.AppModel.Json.Storage;

namespace rUI.AppModel.Json.Recovery;

public sealed class JsonRecoveryStore<TState>(
    string contract,
    IDocumentStore documentStore,
    IAppModelSerializer serializer) : IRecoveryStore<TState>
    where TState : class
{
    public string Contract { get; } = contract;

    public async Task<RecoveryEnvelope<TState>?> LoadAsync(string scope, CancellationToken cancellationToken = default)
    {
        var key = BuildKey(Contract, scope);
        var raw = await documentStore.ReadAsync(key, cancellationToken);
        if (raw is null)
            return null;

        try
        {
            var envelope = serializer.Deserialize<RecoveryEnvelope<TState>>(raw);
            if (!string.Equals(envelope.Contract, Contract, StringComparison.Ordinal))
                throw new InvalidOperationException($"Recovery contract mismatch. Expected '{Contract}', got '{envelope.Contract}'.");

            if (!string.Equals(envelope.Scope, scope, StringComparison.Ordinal))
                throw new InvalidOperationException($"Recovery scope mismatch. Expected '{scope}', got '{envelope.Scope}'.");

            return envelope;
        }
        catch (Exception ex)
        {
            await QuarantineIfSupportedAsync(key, raw, ex.Message, cancellationToken);
            return null;
        }
    }

    public Task SaveAsync(RecoveryEnvelope<TState> state, CancellationToken cancellationToken = default)
    {
        var key = BuildKey(Contract, state.Scope);
        var raw = serializer.Serialize(state);
        return documentStore.WriteAsync(key, raw, cancellationToken);
    }

    public Task ClearAsync(string scope, CancellationToken cancellationToken = default)
    {
        return documentStore.DeleteAsync(BuildKey(Contract, scope), cancellationToken);
    }

    private async Task QuarantineIfSupportedAsync(PersistenceKey key, string content, string reason, CancellationToken cancellationToken)
    {
        if (documentStore is IQuarantineDocumentStore quarantine)
            await quarantine.QuarantineAsync(key, content, reason, cancellationToken);
    }

    private static PersistenceKey BuildKey(string recoveryContract, string scope)
    {
        var contractSegment = KeyPath.NormalizeSegment(recoveryContract);
        var scopeSegment = KeyPath.NormalizeSegment(scope);
        return new PersistenceKey($"recovery/{contractSegment}/{scopeSegment}");
    }
}
