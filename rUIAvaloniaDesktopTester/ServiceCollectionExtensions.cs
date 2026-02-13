using Microsoft.Extensions.DependencyInjection;
using rUI.Avalonia.Desktop;
using rUI.Avalonia.Desktop.Services;
using rUIAvaloniaDesktopTester.ViewModels;
using rUIAvaloniaDesktopTester.Views;

namespace rUIAvaloniaDesktopTester;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        // Core app services are shared and host-backed.
        _ = services.AddSingleton<NavigationService>();
        _ = services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());
        _ = services.AddSingleton<IContentDialogService, ContentDialogService>();
        _ = services.AddSingleton<IInfoBarService, InfoBarService>();
        _ = services.AddSingleton<IOverlayService, OverlayService>();

        _ = services.AddSingleton<MainWindow>();
        _ = services.AddSingleton<MainWindowViewModel>();

        // Page view models are transient to avoid stale state across navigations.
        _ = services.AddTransient<GenerateKeysPageViewModel>();
        _ = services.AddTransient<ContentDialogTestingPageViewModel>();
        _ = services.AddTransient<OverlayTestingPageViewModel>();
        _ = services.AddTransient<InfoBarTestingPageViewModel>();
        _ = services.AddTransient<ChartsPageViewModel>();
        _ = services.AddTransient<ExpanderTestingPageViewModel>();
        _ = services.AddTransient<SchedulePageViewModel>();
        _ = services.AddTransient<RibbonCanvasTestingPageViewModel>();
        _ = services.AddTransient<ImagingCanvasPageViewModel>();
        _ = services.AddTransient<DockingTestingPageViewModel>();
        _ = services.AddTransient<DockingCanvasTestingPageViewModel>();
        _ = services.AddTransient<NavigationTestingPageViewModel>();
        _ = services.AddTransient<NavigationCancellationDemoPageViewModel>();
        _ = services.AddTransient<EditorsTestingPageViewModel>();
        _ = services.AddTransient<DummyPageViewModel>();
        _ = services.AddTransient<SettingsPageViewModel>();
    }
}
