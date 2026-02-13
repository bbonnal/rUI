using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using Flowxel.Core.Geometry.Primitives;
using Flowxel.Core.Geometry.Shapes;
using rUI.Drawing.Core;
using AvaloniaPoint = global::Avalonia.Point;
using AvaloniaVector = global::Avalonia.Vector;
using FlowPoint = Flowxel.Core.Geometry.Shapes.Point;
using FlowRectangle = Flowxel.Core.Geometry.Shapes.Rectangle;
using FlowVector = Flowxel.Core.Geometry.Primitives.Vector;
using global::Avalonia.Media;
using Line = Flowxel.Core.Geometry.Shapes.Line;
using Shape = Flowxel.Core.Geometry.Shapes.Shape;

namespace rUI.Drawing.Avalonia.Controls.Drawing;

public class DrawingCanvasControl : Control
{
    private static readonly Cursor ArrowCursor = new(StandardCursorType.Arrow);
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);
    private static readonly Cursor GrabCursor = new(StandardCursorType.SizeAll);
    private static readonly Cursor DrawCursor = new(StandardCursorType.Cross);

    private const double MinShapeSize = 0.0001;

    private readonly DrawingCanvasContextMenu _contextMenu;

    private Shape? _previewShape;
    private Shape? _hoveredShape;
    private Shape? _selectedShape;
    private Shape? _contextMenuTargetShape;
    private FlowVector? _gestureStartWorld;
    private FlowVector? _lastDragWorld;
    private AvaloniaPoint? _gestureStartScreen;
    private AvaloniaVector _panAtGestureStart;
    private bool _isMiddlePanning;
    private bool _openContextMenuOnRightRelease;
    private ShapeHandleKind _activeHandle = ShapeHandleKind.None;

    public static readonly StyledProperty<IList<Shape>> ShapesProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IList<Shape>>(
            nameof(Shapes),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<DrawingTool> ActiveToolProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, DrawingTool>(
            nameof(ActiveTool),
            DrawingTool.Select,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, double>(
            nameof(Zoom),
            1d,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<AvaloniaVector> PanProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, AvaloniaVector>(
            nameof(Pan),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> MinZoomProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, double>(nameof(MinZoom), 0.1d);

    public static readonly StyledProperty<double> MaxZoomProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, double>(nameof(MaxZoom), 16d);

    public static readonly StyledProperty<IBrush> CanvasBackgroundProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(CanvasBackground),
            Brushes.Transparent);

    public static readonly StyledProperty<IBrush> ShapeStrokeProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(ShapeStroke),
            Brushes.DeepSkyBlue);

    public static readonly StyledProperty<IBrush> SelectedStrokeProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(SelectedStroke),
            Brushes.DeepSkyBlue);

    public static readonly StyledProperty<IBrush> PreviewStrokeProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(PreviewStroke),
            Brushes.Orange);

    public static readonly StyledProperty<IBrush> HoverStrokeProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(HoverStroke),
            Brushes.Gold);

    public static readonly StyledProperty<IBrush> HandleFillProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(HandleFill),
            Brushes.White);

    public static readonly StyledProperty<IBrush> HandleStrokeProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(HandleStroke),
            Brushes.DodgerBlue);

    public static readonly StyledProperty<double> HandleSizeProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, double>(nameof(HandleSize), 9d);

    public static readonly StyledProperty<IBrush> OriginXAxisBrushProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(OriginXAxisBrush),
            Brushes.Red);

    public static readonly StyledProperty<IBrush> OriginYAxisBrushProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, IBrush>(
            nameof(OriginYAxisBrush),
            Brushes.LimeGreen);

    public static readonly StyledProperty<double> OriginMarkerSizeProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, double>(nameof(OriginMarkerSize), 12d);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, double>(nameof(StrokeThickness), 2d);

    public static readonly StyledProperty<double> PointDisplayRadiusProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, double>(nameof(PointDisplayRadius), 4d);

    public static readonly StyledProperty<double> HitTestToleranceProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, double>(nameof(HitTestTolerance), 8d);

    public static readonly StyledProperty<AvaloniaPoint> CursorAvaloniaPositionProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, AvaloniaPoint>(
            nameof(CursorAvaloniaPosition),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<AvaloniaPoint> CursorCanvasPositionProperty =
        AvaloniaProperty.Register<DrawingCanvasControl, AvaloniaPoint>(
            nameof(CursorCanvasPosition),
            defaultBindingMode: BindingMode.TwoWay);

    public DrawingCanvasControl()
    {
        Focusable = true;
        ClipToBounds = true;
        Shapes = [];
        AttachShapesCollection(Shapes);

        _contextMenu = new DrawingCanvasContextMenu();
        _contextMenu.DeleteShapeRequested += OnDeleteShapeRequested;
        _contextMenu.CenterViewRequested += OnCenterViewRequested;
        ContextMenu = _contextMenu;
        ContextRequested += OnContextRequested;
    }

    public IList<Shape> Shapes
    {
        get => GetValue(ShapesProperty);
        set => SetValue(ShapesProperty, value);
    }

    public DrawingTool ActiveTool
    {
        get => GetValue(ActiveToolProperty);
        set => SetValue(ActiveToolProperty, value);
    }

    public double Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, Math.Clamp(value, MinZoom, MaxZoom));
    }

    public AvaloniaVector Pan
    {
        get => GetValue(PanProperty);
        set => SetValue(PanProperty, value);
    }

    public double MinZoom
    {
        get => GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    public double MaxZoom
    {
        get => GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    public IBrush CanvasBackground
    {
        get => GetValue(CanvasBackgroundProperty);
        set => SetValue(CanvasBackgroundProperty, value);
    }

    public IBrush ShapeStroke
    {
        get => GetValue(ShapeStrokeProperty);
        set => SetValue(ShapeStrokeProperty, value);
    }

    public IBrush SelectedStroke
    {
        get => GetValue(SelectedStrokeProperty);
        set => SetValue(SelectedStrokeProperty, value);
    }

    public IBrush PreviewStroke
    {
        get => GetValue(PreviewStrokeProperty);
        set => SetValue(PreviewStrokeProperty, value);
    }

    public IBrush HoverStroke
    {
        get => GetValue(HoverStrokeProperty);
        set => SetValue(HoverStrokeProperty, value);
    }

    public IBrush HandleFill
    {
        get => GetValue(HandleFillProperty);
        set => SetValue(HandleFillProperty, value);
    }

    public IBrush HandleStroke
    {
        get => GetValue(HandleStrokeProperty);
        set => SetValue(HandleStrokeProperty, value);
    }

    public double HandleSize
    {
        get => GetValue(HandleSizeProperty);
        set => SetValue(HandleSizeProperty, value);
    }

    public IBrush OriginXAxisBrush
    {
        get => GetValue(OriginXAxisBrushProperty);
        set => SetValue(OriginXAxisBrushProperty, value);
    }

    public IBrush OriginYAxisBrush
    {
        get => GetValue(OriginYAxisBrushProperty);
        set => SetValue(OriginYAxisBrushProperty, value);
    }

    public double OriginMarkerSize
    {
        get => GetValue(OriginMarkerSizeProperty);
        set => SetValue(OriginMarkerSizeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public double PointDisplayRadius
    {
        get => GetValue(PointDisplayRadiusProperty);
        set => SetValue(PointDisplayRadiusProperty, value);
    }

    public double HitTestTolerance
    {
        get => GetValue(HitTestToleranceProperty);
        set => SetValue(HitTestToleranceProperty, value);
    }

    public AvaloniaPoint CursorAvaloniaPosition
    {
        get => GetValue(CursorAvaloniaPositionProperty);
        set => SetValue(CursorAvaloniaPositionProperty, value);
    }

    public AvaloniaPoint CursorCanvasPosition
    {
        get => GetValue(CursorCanvasPositionProperty);
        set => SetValue(CursorCanvasPositionProperty, value);
    }

    public void ResetView()
    {
        Zoom = 1d;
        Pan = default;
        InvalidateVisual();
    }

    public void CenterViewOnOrigin()
    {
        Pan = new AvaloniaVector(Bounds.Width * 0.5, Bounds.Height * 0.5);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        context.FillRectangle(CanvasBackground, new Rect(Bounds.Size));
        DrawOriginMarker(context);

        foreach (var shape in Shapes)
        {
            if (ReferenceEquals(shape, _selectedShape) || ReferenceEquals(shape, _hoveredShape))
                continue;

            DrawShape(context, shape, ShapeStroke, StrokeThickness, null);
        }

        if (_hoveredShape is not null && !ReferenceEquals(_hoveredShape, _selectedShape))
            DrawShape(context, _hoveredShape, HoverStroke, StrokeThickness + 1.25, null);

        if (_selectedShape is not null)
        {
            DrawShape(context, _selectedShape, SelectedStroke, StrokeThickness + 1.5, null);
            DrawGrabHandles(context, _selectedShape);
        }

        if (_previewShape is not null)
            DrawShape(context, _previewShape, PreviewStroke, StrokeThickness, [6, 4]);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShapesProperty)
        {
            if (change.OldValue is IList<Shape> oldShapes)
                DetachShapesCollection(oldShapes);

            if (change.NewValue is IList<Shape> newShapes)
                AttachShapesCollection(newShapes);
        }

        if (change.Property == ZoomProperty ||
            change.Property == PanProperty ||
            change.Property == ActiveToolProperty ||
            change.Property == HoverStrokeProperty ||
            change.Property == SelectedStrokeProperty ||
            change.Property == HandleFillProperty ||
            change.Property == HandleStrokeProperty ||
            change.Property == HandleSizeProperty ||
            change.Property == OriginMarkerSizeProperty ||
            change.Property == OriginXAxisBrushProperty ||
            change.Property == OriginYAxisBrushProperty)
        {
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        Focus();
        var pointer = e.GetCurrentPoint(this);
        var screen = e.GetPosition(this);
        var world = ScreenToWorld(screen);
        UpdateCursorPositions(screen, world);

        if (pointer.Properties.IsRightButtonPressed)
        {
            _contextMenuTargetShape = FindHitShape(world);
            _selectedShape = _contextMenuTargetShape ?? _selectedShape;
            _hoveredShape = _contextMenuTargetShape;
            ConfigureContextMenuTarget(_contextMenuTargetShape);
            _openContextMenuOnRightRelease = true;
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        if (pointer.Properties.IsMiddleButtonPressed)
        {
            _isMiddlePanning = true;
            _gestureStartScreen = screen;
            _panAtGestureStart = Pan;
            e.Pointer.Capture(this);
            UpdateCursor();
            e.Handled = true;
            return;
        }

        if (!pointer.Properties.IsLeftButtonPressed)
            return;

        if (ActiveTool == DrawingTool.Select)
        {
            HandleSelectToolPointerPressed(e, world);
            return;
        }

        if (ActiveTool == DrawingTool.Point)
        {
            Shapes.Add(new FlowPoint { Pose = CreatePose(world.X, world.Y) });
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        _gestureStartWorld = world;
        _previewShape = ShapeInteractionEngine.BuildShape(ActiveTool, world, world, MinShapeSize);
        e.Pointer.Capture(this);
        UpdateCursor();
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var screen = e.GetPosition(this);
        var world = ScreenToWorld(screen);
        UpdateCursorPositions(screen, world);

        if (_isMiddlePanning && _gestureStartScreen is not null)
        {
            var delta = screen - _gestureStartScreen.Value;
            Pan = _panAtGestureStart + delta;
            UpdateCursor();
            e.Handled = true;
            return;
        }

        if (ActiveTool == DrawingTool.Select && _selectedShape is not null && _activeHandle != ShapeHandleKind.None)
        {
            ShapeInteractionEngine.ApplyHandleDrag(_selectedShape, _activeHandle, world, _lastDragWorld, MinShapeSize);
            _lastDragWorld = world;
            UpdateCursor();
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        if (_gestureStartWorld is not null)
        {
            _previewShape = ShapeInteractionEngine.BuildShape(ActiveTool, _gestureStartWorld.Value, world, MinShapeSize);
            UpdateCursor();
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        var previousHover = _hoveredShape;
        _hoveredShape = FindHitShape(world);
        UpdateCursor(world);
        if (!ReferenceEquals(previousHover, _hoveredShape))
            InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.InitialPressMouseButton == MouseButton.Middle)
        {
            _isMiddlePanning = false;
            _gestureStartScreen = null;
            e.Pointer.Capture(null);
            UpdateCursor();
            e.Handled = true;
            return;
        }

        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            if (_openContextMenuOnRightRelease)
            {
                _openContextMenuOnRightRelease = false;
                Dispatcher.UIThread.Post(() => _contextMenu.Open(this), DispatcherPriority.Background);
            }

            e.Handled = true;
            return;
        }

        if (e.InitialPressMouseButton == MouseButton.Left && _activeHandle != ShapeHandleKind.None)
        {
            _activeHandle = ShapeHandleKind.None;
            _lastDragWorld = null;
            e.Pointer.Capture(null);
            UpdateCursor();
            e.Handled = true;
            return;
        }

        if (e.InitialPressMouseButton != MouseButton.Left || _gestureStartWorld is null)
            return;

        var currentWorld = ScreenToWorld(e.GetPosition(this));
        var finalShape = ShapeInteractionEngine.BuildShape(ActiveTool, _gestureStartWorld.Value, currentWorld, MinShapeSize);
        if (finalShape is not null)
            Shapes.Add(finalShape);

        _gestureStartWorld = null;
        _previewShape = null;
        e.Pointer.Capture(null);
        UpdateCursor();
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_activeHandle == ShapeHandleKind.None && _gestureStartWorld is null && !_isMiddlePanning)
            _hoveredShape = null;

        UpdateCursor();
        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var cursor = e.GetPosition(this);
        var worldBeforeZoom = ScreenToWorld(cursor);
        var zoomFactor = Math.Pow(1.12, e.Delta.Y);
        Zoom = Math.Clamp(Zoom * zoomFactor, MinZoom, MaxZoom);
        Pan = new AvaloniaVector(cursor.X - (worldBeforeZoom.X * Zoom), cursor.Y - (worldBeforeZoom.Y * Zoom));

        var worldAfterZoom = ScreenToWorld(cursor);
        UpdateCursorPositions(cursor, worldAfterZoom);
        UpdateCursor(worldAfterZoom);
        InvalidateVisual();
        e.Handled = true;
    }

    private void HandleSelectToolPointerPressed(PointerPressedEventArgs e, FlowVector world)
    {
        if (_selectedShape is not null)
        {
            var handle = HitTestHandle(_selectedShape, world);
            if (handle != ShapeHandleKind.None)
            {
                _activeHandle = handle;
                _lastDragWorld = world;
                e.Pointer.Capture(this);
                UpdateCursor();
                e.Handled = true;
                return;
            }
        }

        var hitShape = FindHitShape(world);
        _hoveredShape = hitShape;

        if (hitShape is null)
        {
            _selectedShape = null;
            _activeHandle = ShapeHandleKind.None;
            _lastDragWorld = null;
            UpdateCursor();
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        if (!ReferenceEquals(hitShape, _selectedShape))
        {
            _selectedShape = hitShape;
            UpdateCursor();
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        _activeHandle = ShapeHandleKind.Move;
        _lastDragWorld = world;
        e.Pointer.Capture(this);
        UpdateCursor();
        InvalidateVisual();
        e.Handled = true;
    }

    private void ConfigureContextMenuTarget(Shape? shape)
    {
        if (shape is not null)
            _contextMenu.ConfigureForShape();
        else
            _contextMenu.ConfigureForCanvas();
    }

    private void AttachShapesCollection(IList<Shape> shapes)
    {
        if (shapes is INotifyCollectionChanged observableCollection)
            observableCollection.CollectionChanged += OnShapesCollectionChanged;
    }

    private void DetachShapesCollection(IList<Shape> shapes)
    {
        if (shapes is INotifyCollectionChanged observableCollection)
            observableCollection.CollectionChanged -= OnShapesCollectionChanged;
    }

    private void OnShapesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_hoveredShape is not null && !Shapes.Contains(_hoveredShape))
            _hoveredShape = null;

        if (_selectedShape is not null && !Shapes.Contains(_selectedShape))
            _selectedShape = null;

        InvalidateVisual();
    }

    private void OnDeleteShapeRequested(object? sender, EventArgs e)
    {
        if (_contextMenuTargetShape is null)
            return;

        Shapes.Remove(_contextMenuTargetShape);
        if (ReferenceEquals(_selectedShape, _contextMenuTargetShape))
            _selectedShape = null;

        _hoveredShape = null;
        _contextMenuTargetShape = null;
        _activeHandle = ShapeHandleKind.None;
        _lastDragWorld = null;
        InvalidateVisual();
    }

    private void OnCenterViewRequested(object? sender, EventArgs e)
        => CenterViewOnOrigin();

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (!e.TryGetPosition(this, out var position))
            position = CursorAvaloniaPosition;

        var world = ScreenToWorld(position);
        _contextMenuTargetShape = FindHitShape(world);
        if (_contextMenuTargetShape is not null)
            _selectedShape = _contextMenuTargetShape;

        ConfigureContextMenuTarget(_contextMenuTargetShape);
        InvalidateVisual();
    }

    private static Pose CreatePose(double x, double y, FlowVector? orientation = null)
        => new(new FlowVector(x, y), orientation ?? new FlowVector(1, 0));

    private void UpdateCursorPositions(AvaloniaPoint screen, FlowVector world)
    {
        CursorAvaloniaPosition = screen;
        CursorCanvasPosition = new AvaloniaPoint(world.X, world.Y);
    }

    private void UpdateCursor(FlowVector? world = null)
    {
        if (_isMiddlePanning || _activeHandle != ShapeHandleKind.None)
        {
            Cursor = GrabCursor;
            return;
        }

        if (ActiveTool != DrawingTool.Select)
        {
            Cursor = DrawCursor;
            return;
        }

        if (world is null)
        {
            Cursor = HandCursor;
            return;
        }

        if (_selectedShape is not null && HitTestHandle(_selectedShape, world.Value) != ShapeHandleKind.None)
        {
            Cursor = HandCursor;
            return;
        }

        Cursor = FindHitShape(world.Value) is null ? ArrowCursor : HandCursor;
    }

    private Shape? FindHitShape(FlowVector world)
    {
        var tolerance = HitTestTolerance / Math.Max(Zoom, MinZoom);
        var pointRadius = PointDisplayRadius / Math.Max(Zoom, MinZoom);

        for (var i = Shapes.Count - 1; i >= 0; i--)
        {
            var shape = Shapes[i];
            if (ShapeInteractionEngine.IsShapePerimeterHit(shape, world, tolerance, pointRadius))
                return shape;
        }

        return null;
    }

    private ShapeHandleKind HitTestHandle(Shape shape, FlowVector world)
    {
        var tolerance = HandleSize / Math.Max(Zoom, MinZoom);
        return ShapeInteractionEngine.HitTestHandle(shape, world, tolerance);
    }

    private void DrawOriginMarker(DrawingContext context)
    {
        var center = WorldToScreen(new FlowVector(0, 0));
        var size = OriginMarkerSize;
        var xPen = new Pen(OriginXAxisBrush, 2);
        var yPen = new Pen(OriginYAxisBrush, 2);

        context.DrawLine(xPen, new AvaloniaPoint(center.X - size, center.Y), new AvaloniaPoint(center.X + size, center.Y));
        context.DrawLine(yPen, new AvaloniaPoint(center.X, center.Y - size), new AvaloniaPoint(center.X, center.Y + size));
    }

    private void DrawGrabHandles(DrawingContext context, Shape shape)
    {
        var pen = new Pen(HandleStroke, 1.5);
        var half = HandleSize * 0.5;
        foreach (var handle in ShapeInteractionEngine.GetHandles(shape))
        {
            var screen = WorldToScreen(handle.Position);
            var rect = new Rect(screen.X - half, screen.Y - half, HandleSize, HandleSize);
            context.DrawRectangle(HandleFill, pen, rect);
        }
    }

    private void DrawShape(DrawingContext context, Shape shape, IBrush strokeBrush, double thickness, IReadOnlyList<double>? dashArray)
    {
        var pen = dashArray is null
            ? new Pen(strokeBrush, thickness)
            : new Pen(strokeBrush, thickness, dashStyle: new DashStyle(dashArray, 0));

        switch (shape)
        {
            case FlowPoint point:
            {
                var p = WorldToScreen(point.Pose.Position);
                context.DrawEllipse(strokeBrush, null, p, PointDisplayRadius, PointDisplayRadius);
                break;
            }
            case Line line:
            {
                var p1 = WorldToScreen(line.StartPoint.Position);
                var p2 = WorldToScreen(line.EndPoint.Position);
                context.DrawLine(pen, p1, p2);
                break;
            }
            case FlowRectangle rectangle:
            {
                var topLeft = WorldToScreen(rectangle.TopLeft.Position);
                var topRight = WorldToScreen(rectangle.TopRight.Position);
                var bottomRight = WorldToScreen(rectangle.BottomRight.Position);
                var bottomLeft = WorldToScreen(rectangle.BottomLeft.Position);
                context.DrawLine(pen, topLeft, topRight);
                context.DrawLine(pen, topRight, bottomRight);
                context.DrawLine(pen, bottomRight, bottomLeft);
                context.DrawLine(pen, bottomLeft, topLeft);
                break;
            }
            case Circle circle:
            {
                var center = WorldToScreen(circle.Pose.Position);
                var radius = circle.Radius * Zoom;
                context.DrawEllipse(null, pen, center, radius, radius);
                break;
            }
        }
    }

    private AvaloniaPoint WorldToScreen(FlowVector world)
        => new((world.X * Zoom) + Pan.X, (world.Y * Zoom) + Pan.Y);

    private FlowVector ScreenToWorld(AvaloniaPoint screen)
        => new((screen.X - Pan.X) / Zoom, (screen.Y - Pan.Y) / Zoom);
}
