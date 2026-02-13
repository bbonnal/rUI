using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace rUIAvaloniaDesktopTester.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    public SettingsPageViewModel()
    {
        CardClickedCommand = new RelayCommand<string?>(CardClicked);
    }

    public string? LastAction
    {
        get;
        set => SetProperty(ref field, value);
    }

    public IRelayCommand CardClickedCommand { get; }

    private void CardClicked(string? parameter)
    {
        LastAction = $"Clicked: {parameter}";
    }
}
