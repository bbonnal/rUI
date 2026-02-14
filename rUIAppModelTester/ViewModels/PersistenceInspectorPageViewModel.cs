using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using rUI.Avalonia.Desktop;
using rUIAppModelTester.Services;

namespace rUIAppModelTester.ViewModels;

public class PersistenceInspectorPageViewModel : ViewModelBase, INavigationViewModel
{
    private readonly IAppSettingsRuntime _runtime;
    private bool _isSubscribed;

    public PersistenceInspectorPageViewModel(IAppSettingsRuntime runtime)
    {
        _runtime = runtime;
        SettingsFilePath = _runtime.SettingsFilePath;
        ReloadFromDiskCommand = new AsyncRelayCommand(ReloadFromDiskAsync);

        RefreshSnapshot();
    }

    public string RuntimeLanguageCode
    {
        get;
        private set => SetProperty(ref field, value);
    } = "en";

    public string RuntimeThemeMode
    {
        get;
        private set => SetProperty(ref field, value);
    } = "system";

    public string SettingsFilePath
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public bool SettingsFileExists
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public string LastLoadedAtUtc
    {
        get;
        private set => SetProperty(ref field, value);
    } = "(not loaded)";

    public string LastSavedAtUtc
    {
        get;
        private set => SetProperty(ref field, value);
    } = "(never saved)";

    public string PersistedJson
    {
        get;
        private set => SetProperty(ref field, value);
    } = "(no settings file on disk)";

    public IAsyncRelayCommand ReloadFromDiskCommand { get; }

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
        await RefreshSnapshotAsync();
    }

    private void HandleRuntimeChanged(object? sender, EventArgs e)
    {
        _ = RefreshSnapshotAsync();
    }

    private async Task ReloadFromDiskAsync()
    {
        await _runtime.ReloadAsync();
        await RefreshSnapshotAsync();
    }

    private void RefreshSnapshot()
    {
        var settings = _runtime.Current;

        RuntimeLanguageCode = settings.LanguageCode;
        RuntimeThemeMode = settings.ThemeMode;

        LastLoadedAtUtc = _runtime.LastLoadedAtUtc?.ToString("u") ?? "(not loaded)";
        LastSavedAtUtc = _runtime.LastSavedAtUtc?.ToString("u") ?? "(never saved)";
        SettingsFileExists = File.Exists(SettingsFilePath);
    }

    private async Task RefreshSnapshotAsync()
    {
        RefreshSnapshot();

        if (!SettingsFileExists)
        {
            PersistedJson = "(no settings file on disk)";
            return;
        }

        try
        {
            PersistedJson = await File.ReadAllTextAsync(SettingsFilePath);
        }
        catch (IOException ex)
        {
            PersistedJson = $"(failed to read settings file: {ex.Message})";
        }
    }
}
