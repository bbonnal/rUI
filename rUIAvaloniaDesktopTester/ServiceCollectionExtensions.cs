using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using rUI.Avalonia.Desktop;
using rUI.Avalonia.Desktop.Services;
using rUI.Avalonia.Desktop.Services.Logging;
using rUI.Avalonia.Desktop.Services.Shortcuts;
using rUI.Avalonia.Desktop.Translation;
using rUIAvaloniaDesktopTester.ViewModels;
using rUIAvaloniaDesktopTester.Views;

namespace rUIAvaloniaDesktopTester;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        ConfigureLogging(services);
        ConfigureTranslation(services);

        // Core app services are shared and host-backed.
        _ = services.AddSingleton<INavigationViewModelResolver, ServiceProviderNavigationViewModelResolver>();
        _ = services.AddSingleton<NavigationService>();
        _ = services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());
        _ = services.AddSingleton<IContentDialogService, ContentDialogService>();
        _ = services.AddSingleton<IInfoBarService, InfoBarService>();
        _ = services.AddSingleton<IOverlayService, OverlayService>();
        _ = services.AddSingleton<IShortcutService, ShortcutService>();

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

    private static void ConfigureLogging(IServiceCollection services)
    {
        _ = services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        _ = services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        _ = services.AddSingleton<IRuiLoggerFactory, RuiLoggerFactory>();
        _ = services.AddSingleton(typeof(IRuiLogger<>), typeof(RuiLogger<>));
    }

    private static void ConfigureTranslation(IServiceCollection services)
    {
        var catalog = BuildCatalogFromResources();
        _ = services.AddSingleton<ITranslationService>(sp => new TranslationService(
            catalog,
            CultureInfo.CurrentUICulture,
            CultureInfo.GetCultureInfo("en")));
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> BuildCatalogFromResources()
    {
        return JsonTranslationCatalogLoader.LoadEmbeddedResourcesByPrefix(
            Assembly.GetExecutingAssembly(),
            "rUIAvaloniaDesktopTester.Resources.Translation.");
    }
}
