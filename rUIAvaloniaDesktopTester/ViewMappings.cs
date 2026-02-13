using System;
using System.Collections.Generic;
using Avalonia.Controls;
using rUIAvaloniaDesktopTester.ViewModels;
using rUIAvaloniaDesktopTester.Views;

namespace rUIAvaloniaDesktopTester;

/// <summary>
/// Central registry of explicit ViewModel-to-View mappings.
/// </summary>
public static class ViewMappings
{
    public static IReadOnlyDictionary<Type, Func<Control>> Create()
    {
        return new Dictionary<Type, Func<Control>>
        {
            [typeof(GenerateKeysPageViewModel)] = static () => new GenerateKeysPageView(),
            [typeof(ContentDialogTestingPageViewModel)] = static () => new ContentDialogTestingPageView(),
            [typeof(OverlayTestingPageViewModel)] = static () => new OverlayTestingPageView(),
            [typeof(InfoBarTestingPageViewModel)] = static () => new InfoBarTestingPageView(),
            [typeof(ChartsPageViewModel)] = static () => new ChartsPageView(),
            [typeof(ExpanderTestingPageViewModel)] = static () => new ExpanderTestingPageView(),
            [typeof(SchedulePageViewModel)] = static () => new SchedulePageView(),
            [typeof(RibbonCanvasTestingPageViewModel)] = static () => new RibbonCanvasTestingPageView(),
            [typeof(ImagingCanvasPageViewModel)] = static () => new ImagingCanvasPageView(),
            [typeof(DockingTestingPageViewModel)] = static () => new DockingTestingPageView(),
            [typeof(DockingCanvasTestingPageViewModel)] = static () => new DockingCanvasTestingPageView(),
            [typeof(NavigationTestingPageViewModel)] = static () => new NavigationTestingPageView(),
            [typeof(NavigationCancellationDemoPageViewModel)] = static () => new NavigationCancellationDemoPageView(),
            [typeof(EditorsTestingPageViewModel)] = static () => new EditorsTestingPageView(),
            [typeof(DummyPageViewModel)] = static () => new DummyPageView(),
            [typeof(SettingsPageViewModel)] = static () => new SettingsPageView(),
        };
    }
}
