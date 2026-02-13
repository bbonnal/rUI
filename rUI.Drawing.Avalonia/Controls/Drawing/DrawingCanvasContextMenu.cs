using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace rUI.Drawing.Avalonia.Controls.Drawing;

public sealed class DrawingCanvasContextMenu : ContextMenu
{
    private readonly MenuItem _deleteShapeItem;
    private readonly MenuItem _centerViewItem;
    protected override Type StyleKeyOverride => typeof(ContextMenu);

    public DrawingCanvasContextMenu()
    {
        _deleteShapeItem = new MenuItem { Header = "Delete shape" };
        _centerViewItem = new MenuItem { Header = "Center view" };

        _deleteShapeItem.Click += (_, _) => DeleteShapeRequested?.Invoke(this, EventArgs.Empty);
        _centerViewItem.Click += (_, _) => CenterViewRequested?.Invoke(this, EventArgs.Empty);

        ItemsSource = new object[] { _deleteShapeItem, _centerViewItem };
        Placement = PlacementMode.Pointer;
    }

    public event EventHandler? DeleteShapeRequested;
    public event EventHandler? CenterViewRequested;

    public void ConfigureForShape()
    {
        _deleteShapeItem.IsVisible = true;
        _centerViewItem.IsVisible = false;
    }

    public void ConfigureForCanvas()
    {
        _deleteShapeItem.IsVisible = false;
        _centerViewItem.IsVisible = true;
    }
}
