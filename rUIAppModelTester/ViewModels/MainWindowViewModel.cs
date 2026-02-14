using System.Threading.Tasks;
using Avalonia.Media;
using PhosphorIconsAvalonia;
using rUI.Avalonia.Desktop;
using rUI.Avalonia.Desktop.Controls.Navigation;
using rUI.Avalonia.Desktop.Services;
using rUIAppModelTester.Services;

namespace rUIAppModelTester.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _isInitialized;
    private readonly IAppSettingsRuntime _runtime;

    public MainWindowViewModel(
        INavigationService navigation,
        IContentDialogService dialogService,
        IOverlayService overlayService,
        IInfoBarService infoBarService,
        IAppSettingsRuntime runtime)
    {
        Navigation = navigation;
        DialogService = dialogService;
        OverlayService = overlayService;
        InfoBarService = infoBarService;
        _runtime = runtime;

        var items = new[]
        {
            new NavigationItemControl
            {
                Header = "Controls",
                IconData = IconService.CreateGeometry(Icon.sliders_horizontal, IconType.regular),
                PageViewModelType = typeof(ControlsShowcasePageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Canvas",
                IconData = IconService.CreateGeometry(Icon.app_window, IconType.regular),
                PageViewModelType = typeof(RibbonCanvasTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Persistence",
                IconData = IconService.CreateGeometry(Icon.floppy_disk_back, IconType.regular),
                PageViewModelType = typeof(PersistenceInspectorPageViewModel)
            }
        };

        var footerItems = new[]
        {
            new NavigationItemControl
            {
                Header = "Settings",
                IconData = IconService.CreateGeometry(Icon.gear, IconType.regular),
                PageViewModelType = typeof(SettingsPageViewModel)
            }
        };

        Logo = new Avalonia.Controls.PathIcon
        {
            Data = Geometry.Parse("M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"),
            Width = 28,
            Height = 28,
            Foreground = new SolidColorBrush(Color.FromRgb(99, 102, 241))
        };

        Navigation.Initialize(items, footerItems);
    }

    public INavigationService Navigation { get; }
    public IContentDialogService DialogService { get; }
    public IOverlayService OverlayService { get; }
    public IInfoBarService InfoBarService { get; }
    public object Logo { get; }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        await _runtime.InitializeAsync();
        _isInitialized = true;
        await Navigation.NavigateToAsync<ControlsShowcasePageViewModel>();
    }
}
