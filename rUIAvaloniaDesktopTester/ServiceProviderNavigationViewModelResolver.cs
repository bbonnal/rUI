using System;
using Microsoft.Extensions.DependencyInjection;
using rUI.Avalonia.Desktop;

namespace rUIAvaloniaDesktopTester;

/// <summary>
/// Adapter that resolves navigation ViewModels from the host DI container.
/// </summary>
public sealed class ServiceProviderNavigationViewModelResolver(IServiceProvider serviceProvider) : INavigationViewModelResolver
{
    public object Resolve(Type viewModelType)
        => serviceProvider.GetService(viewModelType)
           ?? throw new InvalidOperationException($"ViewModel type '{viewModelType.FullName}' is not registered in DI.");
}
