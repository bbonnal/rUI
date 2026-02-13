using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using PhosphorIconsAvalonia;
using rUI.Avalonia.Desktop;
using rUI.Avalonia.Desktop.Controls.Navigation;
using rUI.Avalonia.Desktop.Services;

namespace rUIAvaloniaDesktopTester.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _isInitialized;

    public MainWindowViewModel(
        INavigationService navigation,
        IContentDialogService dialogService,
        IOverlayService overlayService,
        IInfoBarService infoBarService)
    {
        Navigation = navigation;
        DialogService = dialogService;
        OverlayService = overlayService;
        InfoBarService = infoBarService;
        ToggleThemeCommand = new RelayCommand(ToggleTheme);

        var items = new[]
        {
            new NavigationItemControl
            {
                Header = "Keys",
                IconData = IconService.CreateGeometry(Icon.key, IconType.regular),
                PageViewModelType = typeof(GenerateKeysPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Dialogs",
                IconData = IconService.CreateGeometry(Icon.chat_circle_text, IconType.regular),
                PageViewModelType = typeof(ContentDialogTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Overlay",
                IconData = IconService.CreateGeometry(Icon.check_circle, IconType.regular),
                PageViewModelType = typeof(OverlayTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "InfoBar",
                IconData = IconService.CreateGeometry(Icon.info, IconType.regular),
                PageViewModelType = typeof(InfoBarTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Charts",
                IconData = IconService.CreateGeometry(Icon.chart_bar, IconType.regular),
                PageViewModelType = typeof(ChartsPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Expander",
                IconData = IconService.CreateGeometry(Icon.caret_circle_up_down, IconType.regular),
                PageViewModelType = typeof(ExpanderTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Schedule",
                IconData = IconService.CreateGeometry(Icon.calendar, IconType.regular),
                PageViewModelType = typeof(SchedulePageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Ribbon Canvas",
                IconData = IconService.CreateGeometry(Icon.app_window, IconType.regular),
                PageViewModelType = typeof(RibbonCanvasTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Imaging Canvas",
                IconData = IconService.CreateGeometry(Icon.app_window, IconType.regular),
                PageViewModelType = typeof(ImagingCanvasPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Docking",
                IconData = IconService.CreateGeometry(Icon.square_split_horizontal, IconType.regular),
                PageViewModelType = typeof(DockingTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Docked Canvas",
                IconData = IconService.CreateGeometry(Icon.app_window, IconType.regular),
                PageViewModelType = typeof(DockingCanvasTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Navigate",
                IconData = IconService.CreateGeometry(Icon.compass, IconType.regular),
                PageViewModelType = typeof(NavigationTestingPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Nav Cancel",
                IconData = IconService.CreateGeometry(Icon.file_x, IconType.regular),
                PageViewModelType = typeof(NavigationCancellationDemoPageViewModel)
            },
            new NavigationItemControl
            {
                Header = "Editors",
                IconData = Geometry.Parse("M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04a1 1 0 0 0 0-1.41l-2.34-2.34a1 1 0 0 0-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"),
                PageViewModelType = typeof(EditorsTestingPageViewModel)
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

    public IRelayCommand ToggleThemeCommand { get; }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        await Navigation.NavigateToAsync<GenerateKeysPageViewModel>();
    }

    private void ToggleTheme()
    {
        var app = Application.Current;
        if (app != null)
        {
            app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }
    }
}
