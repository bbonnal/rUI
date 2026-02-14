using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rUI.Avalonia.Desktop.Controls;
using rUI.Avalonia.Desktop.Services;

namespace rUIAppModelTester.ViewModels;

public partial class ControlsShowcasePageViewModel : ViewModelBase
{
    private readonly IContentDialogService _dialogService;
    private readonly IOverlayService _overlayService;
    private readonly IInfoBarService _infoBarService;

    public ControlsShowcasePageViewModel(
        IContentDialogService dialogService,
        IOverlayService overlayService,
        IInfoBarService infoBarService)
    {
        _dialogService = dialogService;
        _overlayService = overlayService;
        _infoBarService = infoBarService;

        ShowDialogCommand = new AsyncRelayCommand(ShowDialogAsync);
        ShowOverlayCommand = new AsyncRelayCommand(ShowOverlayAsync);
        HideOverlayCommand = new AsyncRelayCommand(HideOverlayAsync);
        ShowInfoBarCommand = new AsyncRelayCommand(ShowInfoBarAsync);
        ShowSuccessBarCommand = new AsyncRelayCommand(ShowSuccessBarAsync);
    }

    [ObservableProperty]
    private string? textValue = "rUI AppModel tester";

    [ObservableProperty]
    private double? numberValue = 42.5;

    [ObservableProperty]
    private int? countValue = 7;

    [ObservableProperty]
    private string? notesValue = "Use this page to validate visual consistency and interaction behavior.";

    [ObservableProperty]
    private string? lastAction;

    public IAsyncRelayCommand ShowDialogCommand { get; }
    public IAsyncRelayCommand ShowOverlayCommand { get; }
    public IAsyncRelayCommand HideOverlayCommand { get; }
    public IAsyncRelayCommand ShowInfoBarCommand { get; }
    public IAsyncRelayCommand ShowSuccessBarCommand { get; }

    private async Task ShowDialogAsync()
    {
        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "rUI ContentDialog";
            dialog.Content = "This is the AppModel tester controls showcase.";
            dialog.PrimaryButtonText = "Confirm";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = DefaultButton.Primary;
        });

        LastAction = $"Dialog result: {result}";
    }

    private async Task ShowOverlayAsync()
    {
        await _overlayService.ShowAsync(overlay =>
        {
            overlay.Title = "Running";
            overlay.Message = "Simulating a long operation...";
            overlay.IsIndeterminate = true;
        });

        LastAction = "Overlay shown";
    }

    private async Task HideOverlayAsync()
    {
        await _overlayService.HideAsync();
        LastAction = "Overlay hidden";
    }

    private async Task ShowInfoBarAsync()
    {
        await _infoBarService.ShowAsync(infoBar =>
        {
            infoBar.Title = "Info";
            infoBar.Message = "This is a sample info message.";
            infoBar.Severity = InfoBarSeverity.Info;
        });

        LastAction = "InfoBar shown";
    }

    private async Task ShowSuccessBarAsync()
    {
        await _infoBarService.ShowAsync(infoBar =>
        {
            infoBar.Title = "Success";
            infoBar.Message = "Everything is wired correctly.";
            infoBar.Severity = InfoBarSeverity.Success;
        });

        LastAction = "Success InfoBar shown";
    }
}
