using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Metadata;

namespace rUI.Avalonia.Desktop.Controls.Ribbon;

public class RibbonControl : TemplatedControl
{
    private ListBox? _tabStrip;

    [Content]
    public AvaloniaList<RibbonTab> Tabs { get; } = new();

    public static readonly StyledProperty<RibbonTab?> SelectedTabProperty =
        AvaloniaProperty.Register<RibbonControl, RibbonTab?>(
            nameof(SelectedTab),
            defaultBindingMode: BindingMode.TwoWay);

    public RibbonTab? SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_tabStrip is not null)
            _tabStrip.SelectionChanged -= OnTabStripSelectionChanged;

        _tabStrip = e.NameScope.Find<ListBox>("PART_TabStrip");

        if (_tabStrip is not null)
            _tabStrip.SelectionChanged += OnTabStripSelectionChanged;

        if (SelectedTab is null && Tabs.Count > 0)
            SelectedTab = Tabs[0];
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedTabProperty && _tabStrip is not null)
        {
            var tab = change.GetNewValue<RibbonTab?>();
            if (_tabStrip.SelectedItem != tab)
                _tabStrip.SelectedItem = tab;
        }
    }

    private void OnTabStripSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_tabStrip?.SelectedItem is RibbonTab tab)
            SelectedTab = tab;
    }
}
