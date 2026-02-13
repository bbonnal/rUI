using System;
using System.Linq;
using rUI.Avalonia.Desktop;
using rUI.Avalonia.Desktop.Services;
using rUIAvaloniaDesktopTester;
using rUIAvaloniaDesktopTester.ViewModels;
using Xunit;

namespace rUI.ArchitectureTests;

public class ArchitectureTests
{
    [Fact]
    public void DesktopAssembly_DoesNotReferenceDependencyInjectionFramework()
    {
        var references = typeof(INavigationService).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        Assert.DoesNotContain("Microsoft.Extensions.DependencyInjection", references);
    }

    [Fact]
    public void NavigationService_UsesResolverAbstraction()
    {
        var constructors = typeof(NavigationService).GetConstructors();
        Assert.Single(constructors);

        var parameters = constructors[0].GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(INavigationViewModelResolver), parameters[0].ParameterType);
    }

    [Fact]
    public void ViewMappings_CoverAllPageViewModels()
    {
        var mapped = ViewMappings.Create().Keys.ToHashSet();

        var pageViewModels = typeof(ViewModelBase).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => typeof(ViewModelBase).IsAssignableFrom(t))
            .Where(t => t.Name.EndsWith("PageViewModel", StringComparison.Ordinal))
            .ToArray();

        var missing = pageViewModels.Where(vm => !mapped.Contains(vm)).Select(vm => vm.FullName).ToArray();
        Assert.True(missing.Length == 0, $"Missing mappings for: {string.Join(", ", missing)}");
    }
}
