namespace rUIAppModelTester.AppModel;

public sealed record AppModelTesterSettings
{
    public string LanguageCode { get; init; } = "en";
    public string ThemeMode { get; init; } = AppThemeModes.System;

    public static AppModelTesterSettings CreateDefaults()
    {
        return new AppModelTesterSettings();
    }
}
