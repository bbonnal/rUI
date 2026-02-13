using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using rUIAvaloniaDesktopTester.ViewModels;

namespace rUIAvaloniaDesktopTester;

/// <summary>
/// Given a view model, returns the corresponding view from explicit mappings.
/// </summary>
public class ViewLocator : IDataTemplate
{
    private static IReadOnlyDictionary<Type, Func<Control>> _mappings = new Dictionary<Type, Func<Control>>();

    public static void Configure(IReadOnlyDictionary<Type, Func<Control>> mappings)
    {
        _mappings = mappings;
    }

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var vmType = param.GetType();
        if (_mappings.TryGetValue(vmType, out var factory))
            return factory();

        return new TextBlock { Text = "Not Found: " + vmType.FullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
