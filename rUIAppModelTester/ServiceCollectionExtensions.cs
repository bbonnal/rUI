using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using rUI.AppModel.Serialization;
using rUI.AppModel.Settings;
using rUI.Avalonia.Desktop;
using rUI.Avalonia.Desktop.Services;
using rUI.Avalonia.Desktop.Services.Shortcuts;
using rUI.Avalonia.Desktop.Translation;
using rUI.AppModel.Storage;
using rUI.AppModel.Json.Serialization;
using rUI.AppModel.Json.Settings;
using rUI.AppModel.Json.Storage;
using rUIAppModelTester.AppModel;
using rUIAppModelTester.Services;
using rUIAppModelTester.ViewModels;
using rUIAppModelTester.Views;

namespace rUIAppModelTester;

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
        ConfigureAppModel(services);

        _ = services.AddSingleton<MainWindow>();
        _ = services.AddSingleton<MainWindowViewModel>();

        // Page view models are transient to avoid stale state across navigations.
        _ = services.AddTransient<ControlsShowcasePageViewModel>();
        _ = services.AddTransient<RibbonCanvasTestingPageViewModel>();
        _ = services.AddTransient<PersistenceInspectorPageViewModel>();
        _ = services.AddTransient<SettingsPageViewModel>();
    }

    private static void ConfigureAppModel(IServiceCollection services)
    {
        var rootDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "rUI",
            "rUIAppModelTester");

        _ = services.AddSingleton<IAppModelSerializer, SystemTextJsonAppModelSerializer>();
        _ = services.AddSingleton(new FileDocumentStoreOptions(rootDirectory));
        _ = services.AddSingleton<FileDocumentStore>();
        _ = services.AddSingleton<IDocumentStore>(sp => sp.GetRequiredService<FileDocumentStore>());
        _ = services.AddSingleton<ISettingsStore<AppModelTesterSettings>>(sp =>
            new JsonSettingsStore<AppModelTesterSettings>(
                contract: AppModelTesterSettingsContract.Name,
                documentStore: sp.GetRequiredService<IDocumentStore>(),
                serializer: sp.GetRequiredService<IAppModelSerializer>()));
        _ = services.AddSingleton<ISettingsService<AppModelTesterSettings>>(sp =>
            new JsonSettingsService<AppModelTesterSettings>(
                sp.GetRequiredService<ISettingsStore<AppModelTesterSettings>>(),
                new JsonSettingsServiceOptions<AppModelTesterSettings>(
                    CreateDefaults: AppModelTesterSettings.CreateDefaults)));
        _ = services.AddSingleton<IAppSettingsRuntime, AppSettingsRuntime>();
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        _ = services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        _ = services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
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
            "rUIAppModelTester.Resources.Translation.");
    }
}
