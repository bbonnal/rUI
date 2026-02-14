using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using rUI.Avalonia.Desktop;
using rUI.Avalonia.Desktop.Translation;
using rUIAppModelTester.AppModel;
using rUIAppModelTester.Services;

namespace rUIAppModelTester.ViewModels;

public class SettingsPageViewModel : ViewModelBase, INavigationViewModel
{
    private readonly ITranslationService _translations;
    private readonly ILogger<SettingsPageViewModel> _logger;
    private readonly IAppSettingsRuntime _runtime;
    private string _selectedLanguageCode = "en";
    private string _selectedThemeMode = AppThemeModes.System;
    private bool _hasUnsavedChanges;
    private bool _isSubscribed;

    public SettingsPageViewModel(
        ITranslationService translations,
        ILogger<SettingsPageViewModel> logger,
        IAppSettingsRuntime runtime)
    {
        _translations = translations;
        _logger = logger;
        _runtime = runtime;

        SetEnglishCommand = new RelayCommand(() => SetCulture("en"));
        SetFrenchCommand = new RelayCommand(() => SetCulture("fr"));
        SetThemeSystemCommand = new RelayCommand(() => SetTheme(AppThemeModes.System));
        SetThemeLightCommand = new RelayCommand(() => SetTheme(AppThemeModes.Light));
        SetThemeDarkCommand = new RelayCommand(() => SetTheme(AppThemeModes.Dark));

        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
        ResetDefaultsCommand = new AsyncRelayCommand(ResetDefaultsAsync);
        DeletePersistedSettingsCommand = new AsyncRelayCommand(DeletePersistedSettingsAsync);

        RefreshTranslations();
        RefreshStatus();
    }

    public string SaveStatus
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string CurrentLanguageDisplay
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string CurrentThemeDisplay
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string? LastAction
    {
        get;
        set => SetProperty(ref field, value);
    }

    public IRelayCommand SetEnglishCommand { get; }
    public IRelayCommand SetFrenchCommand { get; }
    public IRelayCommand SetThemeSystemCommand { get; }
    public IRelayCommand SetThemeLightCommand { get; }
    public IRelayCommand SetThemeDarkCommand { get; }

    public IAsyncRelayCommand SaveSettingsCommand { get; }
    public IAsyncRelayCommand ResetDefaultsCommand { get; }
    public IAsyncRelayCommand DeletePersistedSettingsCommand { get; }

    public Task<bool> OnDisappearingAsync()
    {
        if (_isSubscribed)
        {
            _runtime.Changed -= HandleRuntimeChanged;
            _isSubscribed = false;
        }

        return Task.FromResult(true);
    }

    public async Task OnAppearingAsync()
    {
        if (!_isSubscribed)
        {
            _runtime.Changed += HandleRuntimeChanged;
            _isSubscribed = true;
        }

        await _runtime.InitializeAsync();
        ApplySettings(_runtime.Current);
    }

    private void HandleRuntimeChanged(object? sender, EventArgs e)
    {
        ApplySettings(_runtime.Current);
    }

    private void SetCulture(string cultureName)
    {
        var culture = ResolveCulture(cultureName);
        _translations.SetCulture(culture);

        _selectedLanguageCode = culture.Name;
        _hasUnsavedChanges = true;

        LastAction = "Language changed in runtime preview. Click Save to persist.";
        RefreshTranslations();
        RefreshStatus();

        _logger.LogInformation("Language preview set to {Culture}", culture.Name);
    }

    private void SetTheme(string themeMode)
    {
        _selectedThemeMode = AppThemeModes.IsSupported(themeMode)
            ? themeMode
            : AppThemeModes.System;

        ApplyThemePreview(_selectedThemeMode);
        _hasUnsavedChanges = true;

        LastAction = "Theme changed in runtime preview. Click Save to persist.";
        RefreshTranslations();
        RefreshStatus();

        _logger.LogInformation("Theme preview set to {Theme}", _selectedThemeMode);
    }

    private async Task SaveSettingsAsync()
    {
        var settings = new AppModelTesterSettings
        {
            LanguageCode = _selectedLanguageCode,
            ThemeMode = _selectedThemeMode
        };

        await _runtime.SaveAsync(settings);
        _hasUnsavedChanges = false;
        LastAction = "Settings saved to local persistence and applied.";
        RefreshStatus();

        _logger.LogInformation("Settings saved. Language={Language} Theme={Theme}", settings.LanguageCode, settings.ThemeMode);
    }

    private async Task ResetDefaultsAsync()
    {
        await _runtime.ResetToDefaultsAsync();
        _hasUnsavedChanges = false;
        LastAction = "Defaults restored and saved.";
        RefreshStatus();

        _logger.LogInformation("Settings reset to defaults.");
    }

    private async Task DeletePersistedSettingsAsync()
    {
        await _runtime.DeletePersistedAsync();
        _hasUnsavedChanges = false;
        LastAction = "Persisted settings deleted. Runtime set to defaults.";
        RefreshStatus();

        _logger.LogInformation("Persisted settings deleted.");
    }

    private void ApplySettings(AppModelTesterSettings settings)
    {
        _selectedLanguageCode = string.IsNullOrWhiteSpace(settings.LanguageCode)
            ? "en"
            : settings.LanguageCode;

        _selectedThemeMode = AppThemeModes.IsSupported(settings.ThemeMode)
            ? settings.ThemeMode
            : AppThemeModes.System;

        _translations.SetCulture(ResolveCulture(_selectedLanguageCode));
        ApplyThemePreview(_selectedThemeMode);

        RefreshTranslations();
        RefreshStatus();
    }

    private void RefreshTranslations()
    {
        CurrentLanguageDisplay = _translations.Translate("settings.language.current", new Dictionary<string, object?>
        {
            ["Language"] = _translations.CurrentCulture.NativeName
        });

        CurrentThemeDisplay = _translations.Translate("settings.theme.current", new Dictionary<string, object?>
        {
            ["Theme"] = GetThemeDisplay(_selectedThemeMode)
        });
    }

    private void RefreshStatus()
    {
        SaveStatus = _hasUnsavedChanges
            ? "Unsaved changes"
            : "All changes are saved";
    }

    private string GetThemeDisplay(string themeMode)
    {
        return themeMode switch
        {
            AppThemeModes.Light => _translations.Translate("settings.theme.light"),
            AppThemeModes.Dark => _translations.Translate("settings.theme.dark"),
            _ => _translations.Translate("settings.theme.system")
        };
    }

    private static void ApplyThemePreview(string themeMode)
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
}
