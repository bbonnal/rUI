using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using rUI.Avalonia.Desktop.Controls.Navigation;
using rUI.Avalonia.Desktop.Services;
using PhosphorIconsAvalonia;
using rUIAvaloniaDesktopTester.Views;

namespace rUIAvaloniaDesktopTester.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        DialogService = new ContentDialogService();
        OverlayService = new OverlayService();
        InfoBarService = new InfoBarService();
        ToggleThemeCommand = new RelayCommand(ToggleTheme);

        var items = new[]
        {
            new NavigationItemControl
            {
                Header = "Keys",
                IconData = IconService.CreateGeometry(Icon.key, IconType.regular),
                Factory = () => new GenerateKeysPageView { DataContext = new GenerateKeysPageViewModel() },
                PageType = typeof(GenerateKeysPageView)
            },
            new NavigationItemControl
            {
                Header = "Dialogs",
                IconData = IconService.CreateGeometry(Icon.chat_circle_text, IconType.regular),
                Factory = () => new ContentDialogTestingPageView { DataContext = new ContentDialogTestingPageViewModel(DialogService) },
                PageType = typeof(ContentDialogTestingPageView)
            },
            new NavigationItemControl
            {
                Header = "Overlay",
                IconData = IconService.CreateGeometry(Icon.check_circle, IconType.regular),
                Factory = () => new OverlayTestingPageView { DataContext = new OverlayTestingPageViewModel(OverlayService) },
                PageType = typeof(OverlayTestingPageView)
            },
            new NavigationItemControl
            {
                Header = "InfoBar",
                IconData = IconService.CreateGeometry(Icon.info, IconType.regular),
                Factory = () => new InfoBarTestingPageView { DataContext = new InfoBarTestingPageViewModel(InfoBarService) },
                PageType = typeof(InfoBarTestingPageView)
            },
            new NavigationItemControl
            {
                Header = "Charts",
                IconData = IconService.CreateGeometry(Icon.chart_bar, IconType.regular),
                Factory = () => new ChartsPageView { DataContext = new ChartsPageViewModel() },
                PageType = typeof(ChartsPageView)
            },
            new NavigationItemControl
            {
                Header = "Expander",
                IconData = IconService.CreateGeometry(Icon.caret_circle_up_down, IconType.regular),
                Factory = () => new ExpanderTestingPageView { DataContext = new ExpanderTestingPageViewModel() },
                PageType = typeof(ExpanderTestingPageView)
            },
            new NavigationItemControl
            {
                Header = "Schedule",
                IconData = IconService.CreateGeometry(Icon.calendar, IconType.regular),
                Factory = () => new SchedulePageView { DataContext = new SchedulePageViewModel() },
                PageType = typeof(SchedulePageView)
            },
            new NavigationItemControl
            {
                Header = "Ribbon",
                IconData = IconService.CreateGeometry(Icon.app_window, IconType.regular),
                Factory = () => new RibbonTestingPageView { DataContext = new RibbonTestingPageViewModel(DialogService) },
                PageType = typeof(RibbonTestingPageView)
            },
            new NavigationItemControl
            {
                Header = "Docking",
                IconData = IconService.CreateGeometry(Icon.square_split_horizontal, IconType.regular),
                Factory = () => new DockingTestingPageView { DataContext = new DockingTestingPageViewModel() },
                PageType = typeof(DockingTestingPageView)
            },
            new NavigationItemControl
            {
                Header = "Navigate",
                IconData = IconService.CreateGeometry(Icon.compass, IconType.regular),
                Factory = () => new NavigationTestingPageView { DataContext = new NavigationTestingPageViewModel(Navigation!) },
                PageType = typeof(NavigationTestingPageView)
            },
            new NavigationItemControl
            {
                Header = "Nav Cancel",
                IconData = IconService.CreateGeometry(Icon.file_x, IconType.regular),
                Factory = () => new NavigationCancellationDemoPageView { DataContext = new NavigationCancellationDemoPageViewModel(DialogService) },
                PageType = typeof(NavigationCancellationDemoPageView)
            },
            new NavigationItemControl
            {
                Header = "Editors",
                IconData = Geometry.Parse("M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04a1 1 0 0 0 0-1.41l-2.34-2.34a1 1 0 0 0-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"),
                Factory = () => new EditorsTestingPageView { DataContext = new EditorsTestingPageViewModel() },
                PageType = typeof(EditorsTestingPageView)
            },
        };

        var footerItems = new[]
        {
            new NavigationItemControl
            {
                Header = "Settings",
                IconData = IconService.CreateGeometry(Icon.gear, IconType.regular),
                Factory = () => new SettingsPageView { DataContext = new SettingsPageViewModel() },
                PageType = typeof(SettingsPageView)
            },
        };

        Logo = new Avalonia.Controls.PathIcon
        {
            Data = Geometry.Parse("M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"),
            Width = 28,
            Height = 28,
            Foreground = new SolidColorBrush(Color.FromRgb(99, 102, 241))
        };

        Navigation = new NavigationService(items, footerItems);
        Navigation.NavigateToAsync(new GenerateKeysPageView{DataContext = new GenerateKeysPageViewModel()}).GetAwaiter().GetResult();
    }

    public NavigationService Navigation { get; }
    public ContentDialogService DialogService { get; }
    public OverlayService OverlayService { get; }
    public InfoBarService InfoBarService { get; }
    public object Logo { get; }

    public IRelayCommand ToggleThemeCommand { get; }

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
