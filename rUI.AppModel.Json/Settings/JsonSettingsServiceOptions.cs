using rUI.AppModel.Settings;

namespace rUI.AppModel.Json.Settings;

public sealed record JsonSettingsServiceOptions<TSettings>(
    Func<TSettings> CreateDefaults)
    where TSettings : class;
