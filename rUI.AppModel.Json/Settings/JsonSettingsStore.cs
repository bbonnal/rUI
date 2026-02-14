using rUI.AppModel.Contracts;
using rUI.AppModel.Serialization;
using rUI.AppModel.Settings;
using rUI.AppModel.Storage;
using rUI.AppModel.Json.Storage;

namespace rUI.AppModel.Json.Settings;

public sealed class JsonSettingsStore<TSettings>(
    string contract,
    IDocumentStore documentStore,
    IAppModelSerializer serializer) : ISettingsStore<TSettings>
    where TSettings : class
{
    public string Contract { get; } = contract;

    public async Task<SettingsEnvelope<TSettings>?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var key = BuildKey(Contract);
        var raw = await documentStore.ReadAsync(key, cancellationToken);
        if (raw is null)
            return null;

        try
        {
            var envelope = serializer.Deserialize<SettingsEnvelope<TSettings>>(raw);
            if (!string.Equals(envelope.Contract, Contract, StringComparison.Ordinal))
                throw new InvalidOperationException($"Settings contract mismatch. Expected '{Contract}', got '{envelope.Contract}'.");

            return envelope;
        }
        catch (Exception ex)
        {
            await QuarantineIfSupportedAsync(key, raw, ex.Message, cancellationToken);
            return null;
        }
    }

    public Task SaveAsync(SettingsEnvelope<TSettings> settings, CancellationToken cancellationToken = default)
    {
        var key = BuildKey(Contract);
        var raw = serializer.Serialize(settings);
        return documentStore.WriteAsync(key, raw, cancellationToken);
    }

    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        return documentStore.DeleteAsync(BuildKey(Contract), cancellationToken);
    }

    private async Task QuarantineIfSupportedAsync(PersistenceKey key, string content, string reason, CancellationToken cancellationToken)
    {
        if (documentStore is IQuarantineDocumentStore quarantine)
            await quarantine.QuarantineAsync(key, content, reason, cancellationToken);
    }

    private static PersistenceKey BuildKey(string settingsContract)
    {
        var segment = KeyPath.NormalizeSegment(settingsContract);
        return new PersistenceKey($"settings/{segment}");
    }
}
