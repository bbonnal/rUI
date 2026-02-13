using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flowxel.Core.Geometry.Shapes;
using rUI.Avalonia.Desktop.Services;
using rUI.Drawing.Core;

namespace rUIAvaloniaDesktopTester.ViewModels;

public partial class RibbonTestingPageViewModel : ViewModelBase
{
    public RibbonTestingPageViewModel(IContentDialogService dialogService, IInfoBarService infoBarService)
    {
        DialogService = dialogService;
        InfoBarService = infoBarService;

        SelectToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Select);
        SelectPointToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Point);
        SelectLineToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Line);
        SelectRectangleToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Rectangle);
        SelectCircleToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Circle);
        SelectImageToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Image);
        SelectTextBoxToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.TextBox);
        SelectArrowToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Arrow);
        SelectCenterlineRectangleToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.CenterlineRectangle);
        SelectReferentialToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Referential);
        SelectDimensionToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Dimension);
        SelectAngleDimensionToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.AngleDimension);

        ClearCanvasCommand = new RelayCommand(ClearCanvas);
        ResetViewCommand = new RelayCommand(ResetView);

        Shapes.CollectionChanged += OnShapesCollectionChanged;
        ComputedShapeIds.CollectionChanged += OnComputedShapeIdsCollectionChanged;
    }

    public IContentDialogService DialogService { get; }

    public IInfoBarService InfoBarService { get; }

    public ObservableCollection<Shape> Shapes { get; } = [];

    public ObservableCollection<string> ComputedShapeIds { get; } = [];

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
    public IRelayCommand SelectImageToolCommand { get; }
    public IRelayCommand SelectTextBoxToolCommand { get; }
    public IRelayCommand SelectArrowToolCommand { get; }
    public IRelayCommand SelectCenterlineRectangleToolCommand { get; }
    public IRelayCommand SelectReferentialToolCommand { get; }
    public IRelayCommand SelectDimensionToolCommand { get; }
    public IRelayCommand SelectAngleDimensionToolCommand { get; }
    public IRelayCommand ClearCanvasCommand { get; }
    public IRelayCommand ResetViewCommand { get; }

    public string StatusText =>
        $"Tool: {ActiveTool} | Shapes: {Shapes.Count} (Computed: {ComputedShapeIds.Count}) | Zoom: {Zoom:0.00}x | Pan: ({Pan.X:0.0}, {Pan.Y:0.0}) | Cursor(Avalonia): ({CursorAvaloniaPosition.X:0.0}, {CursorAvaloniaPosition.Y:0.0}) | Cursor(Canvas): ({CursorCanvasPosition.X:0.00}, {CursorCanvasPosition.Y:0.00})";

    partial void OnActiveToolChanged(DrawingTool value) => OnPropertyChanged(nameof(StatusText));

    partial void OnZoomChanged(double value) => OnPropertyChanged(nameof(StatusText));

    partial void OnPanChanged(global::Avalonia.Vector value) => OnPropertyChanged(nameof(StatusText));

    partial void OnCursorAvaloniaPositionChanged(global::Avalonia.Point value) => OnPropertyChanged(nameof(StatusText));

    partial void OnCursorCanvasPositionChanged(global::Avalonia.Point value) => OnPropertyChanged(nameof(StatusText));

    private void ClearCanvas()
    {
        Shapes.Clear();
        ComputedShapeIds.Clear();
        OnPropertyChanged(nameof(StatusText));
    }

    private void ResetView()
    {
        Zoom = 1d;
        Pan = default;
    }

    private void OnShapesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(StatusText));

    private void OnComputedShapeIdsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(StatusText));
}
