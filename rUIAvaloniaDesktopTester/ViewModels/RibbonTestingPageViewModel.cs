using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rUI.Drawing.Core;
using Flowxel.Core.Geometry.Shapes;

namespace rUIAvaloniaDesktopTester.ViewModels;

public partial class RibbonTestingPageViewModel : ViewModelBase
{
    public RibbonTestingPageViewModel()
    {
        SelectToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Select);
        SelectPointToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Point);
        SelectLineToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Line);
        SelectRectangleToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Rectangle);
        SelectCircleToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Circle);

        ClearCanvasCommand = new RelayCommand(ClearCanvas);
        ResetViewCommand = new RelayCommand(ResetView);
        Shapes.CollectionChanged += OnShapesCollectionChanged;
    }

    public ObservableCollection<Shape> Shapes { get; } = [];

    [ObservableProperty]
    private DrawingTool activeTool = DrawingTool.Select;

    [ObservableProperty]
    private double zoom = 1d;

    [ObservableProperty]
    private global::Avalonia.Vector pan;

    [ObservableProperty]
    private global::Avalonia.Point cursorAvaloniaPosition;

    [ObservableProperty]
    private global::Avalonia.Point cursorCanvasPosition;

    public IRelayCommand SelectToolCommand { get; }
    public IRelayCommand SelectPointToolCommand { get; }
    public IRelayCommand SelectLineToolCommand { get; }
    public IRelayCommand SelectRectangleToolCommand { get; }
    public IRelayCommand SelectCircleToolCommand { get; }
    public IRelayCommand ClearCanvasCommand { get; }
    public IRelayCommand ResetViewCommand { get; }

    public string StatusText =>
        $"Tool: {ActiveTool} | Shapes: {Shapes.Count} | Zoom: {Zoom:0.00}x | Pan: ({Pan.X:0.0}, {Pan.Y:0.0}) | Cursor(Avalonia): ({CursorAvaloniaPosition.X:0.0}, {CursorAvaloniaPosition.Y:0.0}) | Cursor(Canvas): ({CursorCanvasPosition.X:0.00}, {CursorCanvasPosition.Y:0.00})";

    partial void OnActiveToolChanged(DrawingTool value) => OnPropertyChanged(nameof(StatusText));

    partial void OnZoomChanged(double value) => OnPropertyChanged(nameof(StatusText));

    partial void OnPanChanged(global::Avalonia.Vector value) => OnPropertyChanged(nameof(StatusText));

    partial void OnCursorAvaloniaPositionChanged(global::Avalonia.Point value) => OnPropertyChanged(nameof(StatusText));

    partial void OnCursorCanvasPositionChanged(global::Avalonia.Point value) => OnPropertyChanged(nameof(StatusText));

    private void ClearCanvas()
    {
        Shapes.Clear();
        OnPropertyChanged(nameof(StatusText));
    }

    private void ResetView()
    {
        Zoom = 1d;
        Pan = default;
    }

    private void OnShapesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(StatusText));
}
