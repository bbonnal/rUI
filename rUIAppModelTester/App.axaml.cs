using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using rUI.Avalonia.Desktop.Translation;
using rUIAppModelTester.ViewModels;
using rUIAppModelTester.Views;
using Microsoft.Extensions.DependencyInjection;
using rUI.Avalonia.Desktop.Services;

namespace rUIAppModelTester;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var servicesCollection = new ServiceCollection();
        servicesCollection.AddCommonServices();
        var services = new DefaultServiceProviderFactory().CreateServiceProvider(servicesCollection);
        Services = services;
        TranslationBindingSource.Instance.Initialize(services.GetRequiredService<ITranslationService>());
        ViewLocator.Configure(ViewMappings.Create());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var mainWindow = services.GetRequiredService<MainWindow>();
            var vm = services.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = vm;
            
            // Register hosts directly via services
            // This is mandatory for theses services to work above the main window
            services.GetRequiredService<IContentDialogService>().RegisterHost(mainWindow.HostDialog);
            services.GetRequiredService<IOverlayService>().RegisterHost(mainWindow.HostOverlay);
            services.GetRequiredService<IInfoBarService>().RegisterHost(mainWindow.HostInfoBar);
            
            desktop.MainWindow = mainWindow;

            // Initialize app-level state (including persisted settings) at startup.
            mainWindow.Opened += async (_, _) => await vm.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
