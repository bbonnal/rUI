# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**rUI.Avalonia.Desktop** is a reusable Avalonia control library. **rUIAvaloniaDesktopTester** is the companion WinExe app used to develop and test the controls.

## Build & Run

```bash
dotnet build rUI.sln                                # Build all projects
dotnet run --project rUIAvaloniaDesktopTester       # Run the tester app
```

No test projects exist yet.

## Architecture

- **Framework**: Avalonia 11.3 with FluentTheme (Dark variant), .NET 10.0
- **MVVM**: CommunityToolkit.Mvvm with compiled bindings (`AvaloniaUseCompiledBindingsByDefault=true`)
- **Icons**: PhosphorIconsAvalonia — use `IconService.CreateGeometry(Icon.xxx, IconType.regular)` for navigation item icons

### Multi-Project Structure

**rUIAvaloniaDesktopTester** — Tester application (WinExe). Contains ViewModels, Views, and app entry point. Depends on CommunityToolkit.Mvvm, Enigma.Cryptography, LiveChartsCore, PhosphorIconsAvalonia.

**rUI.Avalonia.Desktop** — Reusable control library (no CommunityToolkit dependency). Contains:
- `Controls/` — TemplatedControls organized in subdirectories: `Navigation/`, `Docking/`, `Ribbon/`, `Editors/`, `CalendarSchedule/`, plus top-level `ContentDialog`, `OverlayControl`, `InfoBarControl`, `SettingsCardControl`, `SettingsCardExpander`
- `Services/` — Each service control has a matching service + interface: `NavigationService`, `ContentDialogService`, `OverlayService`, `InfoBarService`
- `Themes/` — Control templates (AXAML). Composed via `Fluent.axaml` and included in App.axaml as `avares://rUI.Avalonia.Desktop/Themes/Fluent.axaml`

**rUI.Drawing.Core** — Drawing-domain contracts and primitives for drawing tools/features.

**rUI.Drawing.Avalonia** — Avalonia drawing controls and UI interaction layer.

### Patterns

- **ViewLocator**: Resolves views by replacing "ViewModel" with "View" in the type name. Page VMs go in `ViewModels/`, matching views in `Views/`. ViewModels must inherit from `ViewModelBase` (`ObservableObject`) to be matched.
- **Naming**: Page ViewModels are `{Name}PageViewModel`, views are `{Name}PageView`.
- **Navigation**: `NavigationService` holds `NavigationItemControl` instances. Selecting an item invokes its `Factory` (`Func<object>`) to create a page ViewModel, which `ContentControl` + `ViewLocator` renders.
- **Host pattern**: `ContentDialog`, `OverlayControl`, and `InfoBarControl` are all hosted as siblings in MainWindow's root `Panel`. Each is registered with its service via `RegisterHost()` from `MainWindow.axaml.cs` `OnDataContextChanged`. ViewModels receive services through constructor injection from `MainWindowViewModel`.
- **TemplatedControls**: Custom controls in rUI are `TemplatedControl` (not `UserControl`). C# classes go in `Controls/`, AXAML templates in `Themes/`. When adding a new control, also add its template include to `Themes/Fluent.axaml`. Use `{Binding ..., RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}` for two-way template bindings — `{TemplateBinding}` is one-way only in Avalonia.
- **OnApplyTemplate pattern**: Controls find named template parts via `e.NameScope.Find<T>("PART_Name")` in `OnApplyTemplate`. Always detach old event handlers before attaching new ones, since `OnApplyTemplate` can be called multiple times.

### Theme System

Colors are defined in `rUI.Avalonia.Desktop/Themes/Colors.axaml` using `ResourceDictionary.ThemeDictionaries` with `x:Key="Dark"` and `x:Key="Light"` variants. Brushes in `Brushes.axaml` reference colors via `{DynamicResource}` for runtime theme switching. Use `{DynamicResource rUIForegroundSecondaryBrush}` (not `StaticResource`) for theme-reactive styling. Key prefixes: `rUIBackground`, `rUISurface`, `rUIBorder`, `rUIForeground`, `rUIAccent`, `rUISuccess`, `rUIWarning`, `rUIError`.

### Gotchas

- **Namespace collision**: The `rUI.Avalonia.Desktop` namespace can make `Avalonia.Media` (and similar) resolve incorrectly as `rUI.Avalonia.Media`. Use `global::Avalonia.Media` or `using global::Avalonia.Media;` in C# files that need Avalonia sub-namespaces.
- **Compiled bindings require `x:DataType`**: Every UserControl/DataTemplate using `{Binding}` needs an explicit `x:DataType` or the build fails with AVLN2100.
- **Hit testing requires a Background**: A `Border` or `Panel` with no `Background` (null) is invisible to pointer events. Set `Background="Transparent"` on elements that need to receive clicks/hover across their full area.
