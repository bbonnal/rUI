namespace rUI.AppModel.Settings;

public sealed record SettingsEnvelope<TSettings>(
    string Contract,
    DateTimeOffset UpdatedAtUtc,
    TSettings Data);
