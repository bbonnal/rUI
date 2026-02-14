using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using rUI.AppModel.Settings;
using rUI.AppModel.Json.Storage;
using rUI.Avalonia.Desktop.Services.Logging;
using rUI.Avalonia.Desktop.Translation;
using rUIAppModelTester.AppModel;

namespace rUIAppModelTester.Services;

public sealed class AppSettingsRuntime : IAppSettingsRuntime
{
    private readonly ISettingsService<AppModelTesterSettings> _settingsService;
    private readonly ITranslationService _translations;
    private readonly IRuiLogger<AppSettingsRuntime> _logger;

    public AppSettingsRuntime(
        ISettingsService<AppModelTesterSettings> settingsService,
        ITranslationService translations,
        IRuiLogger<AppSettingsRuntime> logger,
        FileDocumentStoreOptions storeOptions)
    {
        _settingsService = settingsService;
        _translations = translations;
        _logger = logger;

        SettingsFilePath = Path.Combine(
            storeOptions.RootDirectory,
            "settings",
            $"{AppModelTesterSettingsContract.Name}.json");

        Current = AppModelTesterSettings.CreateDefaults();
    }

    public event EventHandler? Changed;

    public AppModelTesterSettings Current { get; private set; }
    public bool IsInitialized { get; private set; }
    public DateTimeOffset? LastLoadedAtUtc { get; private set; }
    public DateTimeOffset? LastSavedAtUtc { get; private set; }
    public string SettingsFilePath { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized)
            return;

        await ReloadCoreAsync(cancellationToken);
        IsInitialized = true;

        _logger.Information("App settings initialized. Language={Language} Theme={Theme}", Current.LanguageCode, Current.ThemeMode);
    }

    public async Task SaveAsync(AppModelTesterSettings settings, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(settings);

        await _settingsService.SaveAsync(normalized, cancellationToken);
        LastSavedAtUtc = DateTimeOffset.UtcNow;

        Apply(normalized);
        _logger.Information("App settings saved and applied. Language={Language} Theme={Theme}", normalized.LanguageCode, normalized.ThemeMode);
    }

    public async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        var defaults = AppModelTesterSettings.CreateDefaults();
        await _settingsService.SaveAsync(defaults, cancellationToken);
        LastSavedAtUtc = DateTimeOffset.UtcNow;

        Apply(defaults);
        _logger.Information("App settings reset to defaults and persisted.");
    }

    public async Task DeletePersistedAsync(CancellationToken cancellationToken = default)
    {
        await _settingsService.ResetAsync(cancellationToken);

        var defaults = AppModelTesterSettings.CreateDefaults();
        Apply(defaults);
        LastSavedAtUtc = null;

        _logger.Information("Persisted app settings deleted. Defaults applied to runtime.");
    }

    public Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        return ReloadCoreAsync(cancellationToken);
    }

    private async Task ReloadCoreAsync(CancellationToken cancellationToken)
    {
        var loaded = await _settingsService.GetAsync(cancellationToken);
        LastLoadedAtUtc = DateTimeOffset.UtcNow;

        Apply(loaded);
        _logger.Information("App settings loaded from persistence. Language={Language} Theme={Theme}", Current.LanguageCode, Current.ThemeMode);
    }

    private void Apply(AppModelTesterSettings settings)
    {
        Current = Normalize(settings);

        var culture = ResolveCulture(Current.LanguageCode);
        _translations.SetCulture(culture);
        ApplyTheme(Current.ThemeMode);

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static AppModelTesterSettings Normalize(AppModelTesterSettings settings)
    {
        var languageCode = string.IsNullOrWhiteSpace(settings.LanguageCode)
            ? "en"
            : settings.LanguageCode;

        var themeMode = AppThemeModes.IsSupported(settings.ThemeMode)
            ? settings.ThemeMode
            : AppThemeModes.System;

        return settings with
        {
            LanguageCode = languageCode,
            ThemeMode = themeMode
        };
    }

    private static CultureInfo ResolveCulture(string cultureName)
    {
        try
        {
            return CultureInfo.GetCultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.GetCultureInfo("en");
        }
    }

    private static void ApplyTheme(string themeMode)
    {
        var app = Application.Current;
        if (app is null)
            return;

        app.RequestedThemeVariant = themeMode switch
        {
            AppThemeModes.Light => ThemeVariant.Light,
            AppThemeModes.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }
}
