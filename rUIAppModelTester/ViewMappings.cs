using System;
using System.Collections.Generic;
using Avalonia.Controls;
using rUIAppModelTester.ViewModels;
using rUIAppModelTester.Views;

namespace rUIAppModelTester;

/// <summary>
/// Central registry of explicit ViewModel-to-View mappings.
/// </summary>
public static class ViewMappings
{
    public static IReadOnlyDictionary<Type, Func<Control>> Create()
    {
        return new Dictionary<Type, Func<Control>>
        {
            [typeof(ControlsShowcasePageViewModel)] = static () => new ControlsShowcasePageView(),
            [typeof(RibbonCanvasTestingPageViewModel)] = static () => new RibbonCanvasTestingPageView(),
            [typeof(PersistenceInspectorPageViewModel)] = static () => new PersistenceInspectorPageView(),
            [typeof(SettingsPageViewModel)] = static () => new SettingsPageView(),
        };
    }
}
