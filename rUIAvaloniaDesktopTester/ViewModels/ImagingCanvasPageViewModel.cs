using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flowxel.Core.Geometry.Primitives;
using Flowxel.Core.Geometry.Shapes;
using Flowxel.Graph;
using Flowxel.Imaging.Operations;
using Flowxel.Imaging.Operations.Extractions;
using Flowxel.Imaging.Operations.Filters;
using Flowxel.Imaging.Operations.IO;
using OpenCvSharp;
using rUI.Avalonia.Desktop.Controls;
using rUI.Avalonia.Desktop.Controls.Processing;
using rUI.Avalonia.Desktop.Services;
using rUI.Drawing.Core;
using rUI.Drawing.Core.Shapes;
using Shape = Flowxel.Core.Geometry.Shapes.Shape;
using FlowLine = Flowxel.Core.Geometry.Shapes.Line;

namespace rUIAvaloniaDesktopTester.ViewModels;

public partial class ImagingCanvasPageViewModel : ViewModelBase
{
    private const int MaxContourSegments = 2500;

    private readonly IContentDialogService _dialogService;
    private readonly IInfoBarService _infoBarService;

    private ImageShape? _backgroundShape;

    public ImagingCanvasPageViewModel(IContentDialogService dialogService, IInfoBarService infoBarService)
    {
        _dialogService = dialogService;
        _infoBarService = infoBarService;

        SelectToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Select);
        SelectRectangleToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Rectangle);
        SelectLineToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Line);
        SelectCircleToolCommand = new RelayCommand(() => ActiveTool = DrawingTool.Circle);

        LoadBackgroundImageCommand = new AsyncRelayCommand(LoadBackgroundImageAsync);
        AddGaussianBlurOperationCommand = new RelayCommand(() => Pipeline.Add(ImagingOperationKind.GaussianBlur));
        AddWorkOperationCommand = new RelayCommand(() => Pipeline.Add(ImagingOperationKind.Work));
        AddExtractContourOperationCommand = new RelayCommand(() => Pipeline.Add(ImagingOperationKind.ExtractContour));
        AddExtractCircleOperationCommand = new RelayCommand(() => Pipeline.Add(ImagingOperationKind.ExtractCircle));
        RemoveLastOperationCommand = new RelayCommand(RemoveLastOperation);
        ClearPipelineCommand = new RelayCommand(() => Pipeline.Clear());
        RunPipelineCommand = new AsyncRelayCommand(RunPipelineAsync);
        ClearComputedResultsCommand = new RelayCommand(ClearComputedResults);

        ConnectPortsCommand = new RelayCommand(ConnectPorts);
        RemoveConnectionCommand = new RelayCommand(RemoveSelectedConnection);
        ViewResourceCommand = new AsyncRelayCommand(ViewSelectedResourceAsync);
        ClearResourcesCommand = new RelayCommand(ClearResources);

        ClearCanvasCommand = new RelayCommand(ClearCanvas);
        ResetViewCommand = new RelayCommand(ResetView);

        Shapes.CollectionChanged += OnShapesCollectionChanged;
        ComputedShapeIds.CollectionChanged += OnComputedShapeIdsCollectionChanged;
        Resources.CollectionChanged += OnResourcesCollectionChanged;
        Pipeline.CollectionChanged += OnPipelineCollectionChanged;

        RebuildProcessGraphDescriptors();
    }

    public IContentDialogService DialogService => _dialogService;

    public IInfoBarService InfoBarService => _infoBarService;

    public ObservableCollection<Shape> Shapes { get; } = [];

    public ObservableCollection<string> ComputedShapeIds { get; } = [];

    public ObservableCollection<ImagingOperationKind> Pipeline { get; } = [];

    public ObservableCollection<ProcessNodeDescriptor> ProcessNodes { get; } = [];

    public ObservableCollection<ProcessLinkDescriptor> ProcessLinks { get; } = [];

    public ObservableCollection<ResourceEntryDescriptor> Resources { get; } = [];

    public ObservableCollection<ProcessPortDescriptor> AvailableOutputPorts { get; } = [];

    public ObservableCollection<ProcessPortDescriptor> AvailableInputPorts { get; } = [];

    [ObservableProperty]
    private ProcessNodeDescriptor? selectedProcessNode;

    [ObservableProperty]
    private ProcessNodeDescriptor? selectedFromOperation;

    [ObservableProperty]
    private ProcessNodeDescriptor? selectedToOperation;

    [ObservableProperty]
    private ProcessPortDescriptor? selectedFromPort;

    [ObservableProperty]
    private ProcessPortDescriptor? selectedToPort;

    [ObservableProperty]
    private ProcessLinkDescriptor? selectedProcessLink;

    [ObservableProperty]
    private ResourceEntryDescriptor? selectedResource;

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

    [ObservableProperty]
    private string? backgroundImagePath;

    public IRelayCommand SelectToolCommand { get; }
    public IRelayCommand SelectRectangleToolCommand { get; }
    public IRelayCommand SelectLineToolCommand { get; }
    public IRelayCommand SelectCircleToolCommand { get; }

    public IAsyncRelayCommand LoadBackgroundImageCommand { get; }
    public IRelayCommand AddGaussianBlurOperationCommand { get; }
    public IRelayCommand AddWorkOperationCommand { get; }
    public IRelayCommand AddExtractContourOperationCommand { get; }
    public IRelayCommand AddExtractCircleOperationCommand { get; }
    public IRelayCommand RemoveLastOperationCommand { get; }
    public IRelayCommand ClearPipelineCommand { get; }
    public IAsyncRelayCommand RunPipelineCommand { get; }
    public IRelayCommand ClearComputedResultsCommand { get; }

    public IRelayCommand ConnectPortsCommand { get; }
    public IRelayCommand RemoveConnectionCommand { get; }
    public IAsyncRelayCommand ViewResourceCommand { get; }
    public IRelayCommand ClearResourcesCommand { get; }

    public IRelayCommand ClearCanvasCommand { get; }
    public IRelayCommand ResetViewCommand { get; }

    public string StatusText =>
        $"Tool: {ActiveTool} | Shapes: {Shapes.Count} (Computed: {ComputedShapeIds.Count}) | Ops: {Pipeline.Count} | Links: {ProcessLinks.Count} | Resources: {Resources.Count}";

    public string PipelineSummary => Pipeline.Count == 0
        ? "No operation in pipeline"
        : string.Join(" -> ", Pipeline.Select(p => p.ToString()));

    partial void OnActiveToolChanged(DrawingTool value) => OnPropertyChanged(nameof(StatusText));
    partial void OnZoomChanged(double value) => OnPropertyChanged(nameof(StatusText));
    partial void OnPanChanged(global::Avalonia.Vector value) => OnPropertyChanged(nameof(StatusText));
    partial void OnCursorCanvasPositionChanged(global::Avalonia.Point value) => OnPropertyChanged(nameof(StatusText));
    partial void OnSelectedFromOperationChanged(ProcessNodeDescriptor? value) => RefreshOutputPorts();
    partial void OnSelectedToOperationChanged(ProcessNodeDescriptor? value) => RefreshInputPorts();

    private async Task LoadBackgroundImageAsync()
    {
        var pathBox = new TextBox
        {
            Width = 520,
            Watermark = "Absolute image path (.png/.jpg/.bmp...)"
        };

        if (!string.IsNullOrWhiteSpace(BackgroundImagePath))
            pathBox.Text = BackgroundImagePath;

        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Load background image";
            dialog.Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = "Enter image path:" },
                    pathBox
                }
            };
            dialog.PrimaryButtonText = "Load";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = DefaultButton.Primary;
        });

        if (result != DialogResult.Primary)
            return;

        var path = pathBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
            return;

        BackgroundImagePath = path;

        try
        {
            using var bitmap = new Bitmap(path);
            var width = bitmap.PixelSize.Width;
            var height = bitmap.PixelSize.Height;

            if (_backgroundShape is not null)
                Shapes.Remove(_backgroundShape);

            _backgroundShape = new ImageShape
            {
                SourcePath = path,
                Width = width,
                Height = height,
                Pose = new Pose(new Vector(width * 0.5, height * 0.5), new Vector(1, 0))
            };

            Shapes.Insert(0, _backgroundShape);

            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Success;
                infoBar.Title = "Background loaded";
                infoBar.Message = path;
            });
        }
        catch (Exception ex)
        {
            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Error;
                infoBar.Title = "Failed to load background";
                infoBar.Message = ex.Message;
            });
        }
    }

    private async Task RunPipelineAsync()
    {
        if (!await EnsureOpenCvRuntimeAvailableAsync())
            return;

        if (string.IsNullOrWhiteSpace(BackgroundImagePath))
        {
            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Warning;
                infoBar.Title = "Missing background image";
                infoBar.Message = "Load a background image before running the pipeline.";
            });
            return;
        }

        if (Pipeline.Count == 0)
        {
            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Warning;
                infoBar.Title = "Empty pipeline";
                infoBar.Message = "Add one or more imaging operations in the ribbon.";
            });
            return;
        }

        ClearComputedResults();
        ClearResources();

        try
        {
            var pool = new ResourcePool();
            var graph = new Graph<IExecutableNode>();
            var nodeInstances = CreateExecutableNodes(pool, graph);

            await ValidateAndConnectGraphAsync(graph, nodeInstances);
            nodeInstances["load"].As<LoadOperation>().Parameters["Path"] = BackgroundImagePath!;

            await graph.ExecuteAsync();
            AddResourceFromNodeOutput("load", "Load", "Mat (Input)", new ImageResourceViewData
            {
                Path = BackgroundImagePath
            }, ResourceValueKind.Image, "Background path configured");

            CollectAllNodeResources(nodeInstances, pool);
            CollectComputedOverlaysFromTerminalNode(nodeInstances[ProcessNodes.Last().Id], pool);

            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Success;
                infoBar.Title = "Pipeline executed";
                infoBar.Message = $"Executed {Pipeline.Count} operations. Computed overlays: {ComputedShapeIds.Count}. Resources: {Resources.Count}.";
            });
        }
        catch (Exception ex)
        {
            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Error;
                infoBar.Title = "Pipeline execution failed";
                infoBar.Message = ex.Message;
            });
        }
    }

    private Dictionary<string, IExecutableNode> CreateExecutableNodes(ResourcePool pool, Graph<IExecutableNode> graph)
    {
        var map = new Dictionary<string, IExecutableNode>(StringComparer.Ordinal);

        foreach (var descriptor in ProcessNodes)
        {
            IExecutableNode node = descriptor.Id == "load"
                ? new LoadOperation(pool, graph)
                : descriptor.OperationType switch
                {
                    "GaussianBlurOperation" => BuildGaussianNode(pool, graph),
                    "WorkOperation" => new WorkOperation(pool, graph),
                    "ExtractContourOperation" => new ExtractContourOperation(pool, graph),
                    "ExtractCircleOperation" => new ExtractCircleOperation(pool, graph),
                    _ => throw new InvalidOperationException($"Unsupported operation type: {descriptor.OperationType}")
                };

            graph.AddNode(node);
            map[descriptor.Id] = node;
        }

        return map;
    }

    private async Task ValidateAndConnectGraphAsync(Graph<IExecutableNode> graph, IReadOnlyDictionary<string, IExecutableNode> nodes)
    {
        var incomingCounts = nodes.Keys.ToDictionary(key => key, _ => 0);

        foreach (var link in ProcessLinks)
        {
            if (!nodes.TryGetValue(link.FromNodeId, out var from) || !nodes.TryGetValue(link.ToNodeId, out var to))
                throw new InvalidOperationException($"Invalid connection: {link.DisplayLabel}");

            if (from.OutputType != to.InputType)
            {
                throw new InvalidOperationException(
                    $"Type mismatch on connection {link.DisplayLabel}: {from.OutputType.Name} -> {to.InputType.Name}");
            }

            graph.Connect(from, to);
            incomingCounts[link.ToNodeId]++;
        }

        foreach (var descriptor in ProcessNodes)
        {
            if (descriptor.Id == "load")
                continue;

            if (incomingCounts.TryGetValue(descriptor.Id, out var count) && count > 0)
                continue;

            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Warning;
                infoBar.Title = "Unconnected operation";
                infoBar.Message = $"Operation '{descriptor.Name}' has no incoming connection.";
            });
            throw new InvalidOperationException($"Operation '{descriptor.Name}' has no incoming connection.");
        }
    }

    private static GaussianBlurOperation BuildGaussianNode(ResourcePool pool, Graph<IExecutableNode> graph)
    {
        var operation = new GaussianBlurOperation(pool, graph);
        operation.Parameters["KernelSize"] = 5;
        operation.Parameters["Sigma"] = 1.0;
        return operation;
    }

    private void CollectAllNodeResources(IReadOnlyDictionary<string, IExecutableNode> nodeInstances, ResourcePool pool)
    {
        foreach (var descriptor in ProcessNodes)
        {
            if (!nodeInstances.TryGetValue(descriptor.Id, out var node))
                continue;

            CollectNodeOutputResource(descriptor, node, pool);
        }
    }

    private void CollectNodeOutputResource(ProcessNodeDescriptor descriptor, IExecutableNode node, ResourcePool pool)
    {
        if (descriptor.Id == "load")
            return;

        if (node.OutputType == typeof(Mat))
        {
            var mat = pool.Get<Mat>(node.Id);
            var bitmap = CreateBitmapFromMat(mat);
            if (bitmap is not null)
            {
                AddResourceFromNodeOutput(descriptor.Id, $"{descriptor.Name} Image", "Mat", new ImageResourceViewData
                {
                    Bitmap = bitmap,
                    Path = $"Output of {descriptor.Name}"
                }, ResourceValueKind.Image, $"{mat.Width}x{mat.Height}");
            }

            var series = BuildGraphSeries(mat);
            AddResourceFromNodeOutput(descriptor.Id, $"{descriptor.Name} Intensity Profile", "double[]", new GraphSeriesResourceViewData
            {
                Values = series
            }, ResourceValueKind.GraphSeries, $"{series.Count} values (center row)");
            return;
        }

        if (node.OutputType == typeof(OpenCvSharp.Point[][]))
        {
            var contours = pool.Get<OpenCvSharp.Point[][]>(node.Id);
            var counts = contours.Select(c => (double)c.Length).ToArray();
            AddResourceFromNodeOutput(descriptor.Id, $"{descriptor.Name} Contours", "Point[][]", new NumericArrayResourceViewData
            {
                Values = counts
            }, ResourceValueKind.NumericArray, $"{contours.Length} contours");
            return;
        }

        if (node.OutputType == typeof(Circle))
        {
            var circle = pool.Get<Circle>(node.Id);
            AddResourceFromNodeOutput(descriptor.Id, $"{descriptor.Name} Circle", "Circle", new ShapePropertyResourceViewData
            {
                ShapeType = "Circle",
                Properties =
                [
                    new KeyValuePair<string, string>("CenterX", circle.Pose.Position.X.ToString("0.###")),
                    new KeyValuePair<string, string>("CenterY", circle.Pose.Position.Y.ToString("0.###")),
                    new KeyValuePair<string, string>("Radius", circle.Radius.ToString("0.###"))
                ]
            }, ResourceValueKind.ShapeProperties, "Detected circle");
        }
    }

    private void CollectComputedOverlaysFromTerminalNode(IExecutableNode lastNode, ResourcePool pool)
    {
        if (lastNode.OutputType == typeof(OpenCvSharp.Point[][]))
        {
            var contours = pool.Get<OpenCvSharp.Point[][]>(lastNode.Id);
            AddContourOverlays(contours);
            return;
        }

        if (lastNode.OutputType == typeof(Circle))
        {
            var circle = pool.Get<Circle>(lastNode.Id);
            if (circle.Radius > 0)
            {
                Shapes.Add(circle);
                ComputedShapeIds.Add(circle.Id);
            }
        }
    }

    private void AddContourOverlays(OpenCvSharp.Point[][] contours)
    {
        var addedSegments = 0;
        foreach (var contour in contours)
        {
            if (contour.Length < 2)
                continue;

            for (var i = 1; i < contour.Length; i++)
            {
                if (addedSegments >= MaxContourSegments)
                    return;

                var start = contour[i - 1];
                var end = contour[i];
                var segment = CreateLine(start.X, start.Y, end.X, end.Y);
                Shapes.Add(segment);
                ComputedShapeIds.Add(segment.Id);
                addedSegments++;
            }
        }
    }

    private static FlowLine CreateLine(double x1, double y1, double x2, double y2)
    {
        var start = new Vector(x1, y1);
        var end = new Vector(x2, y2);
        var delta = end - start;
        if (delta.M <= 0.0001)
        {
            return new FlowLine
            {
                Pose = new Pose(start, new Vector(1, 0)),
                Length = 0.0001
            };
        }

        return new FlowLine
        {
            Pose = new Pose(start, delta.Normalize()),
            Length = delta.M
        };
    }

    private static Bitmap? CreateBitmapFromMat(Mat mat)
    {
        if (mat.Empty())
            return null;

        Cv2.ImEncode(".png", mat, out var bytes);
        using var stream = new MemoryStream(bytes);
        return new Bitmap(stream);
    }

    private static IReadOnlyList<double> BuildGraphSeries(Mat mat)
    {
        if (mat.Empty() || mat.Width <= 0 || mat.Height <= 0)
            return [];

        using var gray = mat.Channels() == 1 ? mat.Clone() : mat.CvtColor(ColorConversionCodes.BGR2GRAY);
        var row = gray.Height / 2;
        var result = new double[gray.Width];
        for (var x = 0; x < gray.Width; x++)
            result[x] = gray.At<byte>(row, x);

        return result;
    }

    private void AddResourceFromNodeOutput(string producerNode, string name, string typeName, ResourceViewData value, ResourceValueKind kind, string preview)
    {
        Resources.Add(new ResourceEntryDescriptor
        {
            Key = Guid.NewGuid().ToString(),
            ProducerNode = producerNode,
            Name = name,
            TypeName = typeName,
            ValueKind = kind,
            Value = value,
            Preview = preview
        });
    }

    private void ConnectPorts()
    {
        if (SelectedFromOperation is null || SelectedToOperation is null || SelectedFromPort is null || SelectedToPort is null)
            return;

        if (SelectedFromPort.Direction != ProcessPortDirection.Output || SelectedToPort.Direction != ProcessPortDirection.Input)
            return;

        var candidate = new ProcessLinkDescriptor
        {
            FromNodeId = SelectedFromOperation.Id,
            FromPortKey = SelectedFromPort.Key,
            ToNodeId = SelectedToOperation.Id,
            ToPortKey = SelectedToPort.Key
        };

        if (ProcessLinks.Any(existing =>
                existing.FromNodeId == candidate.FromNodeId &&
                existing.FromPortKey == candidate.FromPortKey &&
                existing.ToNodeId == candidate.ToNodeId &&
                existing.ToPortKey == candidate.ToPortKey))
            return;

        ProcessLinks.Add(candidate);
        OnPropertyChanged(nameof(StatusText));
    }

    private void RemoveSelectedConnection()
    {
        if (SelectedProcessLink is null)
            return;

        ProcessLinks.Remove(SelectedProcessLink);
        SelectedProcessLink = null;
        OnPropertyChanged(nameof(StatusText));
    }

    private async Task ViewSelectedResourceAsync()
    {
        if (SelectedResource is null)
            return;

        await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = $"Resource Viewer - {SelectedResource.Name}";
            dialog.Content = new ResourceViewerControl { Resource = SelectedResource };
            dialog.CloseButtonText = "Close";
        });
    }

    private void ClearResources()
    {
        Resources.Clear();
        SelectedResource = null;
        OnPropertyChanged(nameof(StatusText));
    }

    private async Task<bool> EnsureOpenCvRuntimeAvailableAsync()
    {
        try
        {
            _ = Cv2.GetVersionString();
            return true;
        }
        catch (TypeInitializationException ex)
        {
            var details = ex.InnerException?.Message ?? ex.Message;
            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Error;
                infoBar.Title = "OpenCV runtime not available";
                infoBar.Message =
                    $"OpenCvSharp native runtime failed to load. {details} " +
                    "On Linux you usually need libOpenCvSharpExtern.so installed/available in runtime search path.";
            });
            return false;
        }
        catch (DllNotFoundException ex)
        {
            await _infoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Error;
                infoBar.Title = "Missing native OpenCV library";
                infoBar.Message =
                    $"Could not load native dependency: {ex.Message}. " +
                    "Install OpenCvSharp native runtime for your platform or provide libOpenCvSharpExtern.so.";
            });
            return false;
        }
    }

    private void RemoveLastOperation()
    {
        if (Pipeline.Count > 0)
            Pipeline.RemoveAt(Pipeline.Count - 1);
    }

    private void ClearComputedResults()
    {
        for (var i = Shapes.Count - 1; i >= 0; i--)
        {
            if (ComputedShapeIds.Contains(Shapes[i].Id))
                Shapes.RemoveAt(i);
        }

        ComputedShapeIds.Clear();
    }

    private void ClearCanvas()
    {
        Shapes.Clear();
        ComputedShapeIds.Clear();
        Resources.Clear();
        _backgroundShape = null;
        BackgroundImagePath = null;
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

    private void OnResourcesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(StatusText));

    private void OnPipelineCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildProcessGraphDescriptors();
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(PipelineSummary));
    }

    private void RebuildProcessGraphDescriptors()
    {
        ProcessNodes.Clear();
        ProcessLinks.Clear();

        ProcessNodes.Add(new ProcessNodeDescriptor
        {
            Id = "load",
            Name = "Load",
            OperationType = "LoadOperation",
            Inputs =
            [
                new ProcessPortDescriptor { Key = "path", Name = "Path", TypeName = "string", Direction = ProcessPortDirection.Input }
            ],
            Outputs =
            [
                new ProcessPortDescriptor { Key = "out", Name = "Output", TypeName = "Mat", Direction = ProcessPortDirection.Output }
            ]
        });

        var previousNodeId = "load";
        for (var i = 0; i < Pipeline.Count; i++)
        {
            var currentNodeId = $"op-{i}";
            var descriptor = CreateDescriptorForOperation(currentNodeId, Pipeline[i]);
            ProcessNodes.Add(descriptor);

            ProcessLinks.Add(new ProcessLinkDescriptor
            {
                FromNodeId = previousNodeId,
                FromPortKey = "out",
                ToNodeId = currentNodeId,
                ToPortKey = "in"
            });

            previousNodeId = currentNodeId;
        }

        SelectedProcessNode = ProcessNodes.FirstOrDefault();
        SelectedFromOperation = ProcessNodes.FirstOrDefault();
        SelectedToOperation = ProcessNodes.Skip(1).FirstOrDefault() ?? ProcessNodes.FirstOrDefault();
        RefreshOutputPorts();
        RefreshInputPorts();
    }

    private static ProcessNodeDescriptor CreateDescriptorForOperation(string id, ImagingOperationKind kind)
    {
        return kind switch
        {
            ImagingOperationKind.GaussianBlur => new ProcessNodeDescriptor
            {
                Id = id,
                Name = "Gaussian Blur",
                OperationType = "GaussianBlurOperation",
                Inputs =
                [
                    new ProcessPortDescriptor { Key = "in", Name = "Input", TypeName = "Mat", Direction = ProcessPortDirection.Input }
                ],
                Outputs =
                [
                    new ProcessPortDescriptor { Key = "out", Name = "Output", TypeName = "Mat", Direction = ProcessPortDirection.Output }
                ]
            },
            ImagingOperationKind.Work => new ProcessNodeDescriptor
            {
                Id = id,
                Name = "Work",
                OperationType = "WorkOperation",
                Inputs =
                [
                    new ProcessPortDescriptor { Key = "in", Name = "Input", TypeName = "Mat", Direction = ProcessPortDirection.Input }
                ],
                Outputs =
                [
                    new ProcessPortDescriptor { Key = "out", Name = "Output", TypeName = "Mat", Direction = ProcessPortDirection.Output }
                ]
            },
            ImagingOperationKind.ExtractContour => new ProcessNodeDescriptor
            {
                Id = id,
                Name = "Extract Contour",
                OperationType = "ExtractContourOperation",
                Inputs =
                [
                    new ProcessPortDescriptor { Key = "in", Name = "Input", TypeName = "Mat", Direction = ProcessPortDirection.Input }
                ],
                Outputs =
                [
                    new ProcessPortDescriptor { Key = "out", Name = "Contours", TypeName = "Point[][]", Direction = ProcessPortDirection.Output }
                ]
            },
            ImagingOperationKind.ExtractCircle => new ProcessNodeDescriptor
            {
                Id = id,
                Name = "Extract Circle",
                OperationType = "ExtractCircleOperation",
                Inputs =
                [
                    new ProcessPortDescriptor { Key = "in", Name = "Input", TypeName = "Mat", Direction = ProcessPortDirection.Input }
                ],
                Outputs =
                [
                    new ProcessPortDescriptor { Key = "out", Name = "Circle", TypeName = "Circle", Direction = ProcessPortDirection.Output }
                ]
            },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    private void RefreshOutputPorts()
    {
        AvailableOutputPorts.Clear();
        if (SelectedFromOperation is null)
        {
            SelectedFromPort = null;
            return;
        }

        foreach (var port in SelectedFromOperation.Outputs)
            AvailableOutputPorts.Add(port);

        SelectedFromPort = AvailableOutputPorts.FirstOrDefault();
    }

    private void RefreshInputPorts()
    {
        AvailableInputPorts.Clear();
        if (SelectedToOperation is null)
        {
            SelectedToPort = null;
            return;
        }

        foreach (var port in SelectedToOperation.Inputs)
            AvailableInputPorts.Add(port);

        SelectedToPort = AvailableInputPorts.FirstOrDefault();
    }
}

public enum ImagingOperationKind
{
    GaussianBlur,
    Work,
    ExtractContour,
    ExtractCircle
}

internal static class ExecutableNodeCastingExtensions
{
    public static T As<T>(this IExecutableNode node) where T : class, IExecutableNode
        => node as T ?? throw new InvalidCastException($"Expected {typeof(T).Name}, got {node.GetType().Name}");
}
