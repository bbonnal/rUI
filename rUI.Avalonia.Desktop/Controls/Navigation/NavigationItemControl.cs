using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia;
using Avalonia.Controls;

namespace rUI.Avalonia.Desktop.Controls.Navigation;

/// <summary>
/// Represents an item in a navigation control.
/// </summary>
public class NavigationItemControl : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Header"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<NavigationItemControl, string?>(nameof(Header));

    /// <summary>
    /// Defines the <see cref="IconData"/> property.
    /// </summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<NavigationItemControl, Geometry?>(nameof(IconData));

    /// <summary>
    /// Defines the <see cref="Factory"/> property.
    /// </summary>
    public static readonly StyledProperty<Func<Control>?> FactoryProperty =
        AvaloniaProperty.Register<NavigationItemControl, Func<Control>?>(nameof(Factory));

    /// <summary>
    /// Defines the <see cref="PageType"/> property.
    /// </summary>
    public static readonly StyledProperty<Type?> PageTypeProperty =
        AvaloniaProperty.Register<NavigationItemControl, Type?>(nameof(PageType));

    /// <summary>
    /// Gets or sets the header text for the navigation item.
    /// </summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon geometry data for the navigation item.
    /// </summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>
    /// Gets or sets the factory function used to create the page control associated with this item.
    /// </summary>
    public Func<Control>? Factory
    {
        get => GetValue(FactoryProperty);
        set => SetValue(FactoryProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of the page associated with this item.
    /// </summary>
    public Type? PageType
    {
        get => GetValue(PageTypeProperty);
        set => SetValue(PageTypeProperty, value);
    }
}
