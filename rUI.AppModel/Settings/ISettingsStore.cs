namespace rUI.AppModel.Settings;

public interface ISettingsStore<TSettings> where TSettings : class
{
    string Contract { get; }

    Task<SettingsEnvelope<TSettings>?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SettingsEnvelope<TSettings> settings, CancellationToken cancellationToken = default);
    Task DeleteAsync(CancellationToken cancellationToken = default);
}
