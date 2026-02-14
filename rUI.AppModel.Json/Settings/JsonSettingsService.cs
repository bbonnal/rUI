using rUI.AppModel.Settings;

namespace rUI.AppModel.Json.Settings;

public sealed class JsonSettingsService<TSettings>(
    ISettingsStore<TSettings> store,
    JsonSettingsServiceOptions<TSettings> options) : ISettingsService<TSettings>
    where TSettings : class
{
    public async Task<TSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        var envelope = await store.LoadAsync(cancellationToken);
        if (envelope is null)
        {
            var defaults = options.CreateDefaults();
            await TrySelfHealAsync(defaults, cancellationToken);
            return defaults;
        }
        return envelope.Data;
    }

    public Task SaveAsync(TSettings settings, CancellationToken cancellationToken = default)
    {
        var envelope = new SettingsEnvelope<TSettings>(
            Contract: store.Contract,
            UpdatedAtUtc: DateTimeOffset.UtcNow,
            Data: settings);

        return store.SaveAsync(envelope, cancellationToken);
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        return store.DeleteAsync(cancellationToken);
    }

    private async Task TrySelfHealAsync(TSettings defaults, CancellationToken cancellationToken)
    {
        try
        {
            await SaveAsync(defaults, cancellationToken);
        }
        catch
        {
            // Best-effort recovery: ignore write failures while returning defaults.
        }
    }
}
