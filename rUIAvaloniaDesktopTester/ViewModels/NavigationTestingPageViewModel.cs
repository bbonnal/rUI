using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using rUI.Avalonia.Desktop.Services;
using rUIAvaloniaDesktopTester.Views;

namespace rUIAvaloniaDesktopTester.ViewModels;

public class NavigationTestingPageViewModel : ViewModelBase
{
    public NavigationTestingPageViewModel(NavigationService navigation)
    {
        _navigation = navigation;
        NavigateToSettingsCommand = new AsyncRelayCommand(NavigateToSettings);
        NavigateToDummyCommand = new AsyncRelayCommand(NavigateToDummy);
    }

    private readonly NavigationService _navigation;

    public IAsyncRelayCommand NavigateToSettingsCommand { get; }
    public IAsyncRelayCommand NavigateToDummyCommand { get; }

    private async Task NavigateToSettings()
    {
        await _navigation.NavigateToAsync(new SettingsPageView { DataContext = new SettingsPageViewModel() });
    }

    private async Task NavigateToDummy()
    {
        await _navigation.NavigateToAsync(new DummyPageView { DataContext = new DummyPageViewModel() });
    }
}
