namespace rUI.AppModel.Settings;

public interface ISettingsService<TSettings> where TSettings : class
{
    Task<TSettings> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(TSettings settings, CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}
