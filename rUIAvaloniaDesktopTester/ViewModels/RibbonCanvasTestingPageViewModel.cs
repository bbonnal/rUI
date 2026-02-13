using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flowxel.Core.Geometry.Shapes;
using rUI.Avalonia.Desktop.Controls;
using rUI.Avalonia.Desktop.Services;
using rUI.Drawing.Core;
using rUI.Drawing.Core.Scene;

namespace rUIAvaloniaDesktopTester.ViewModels;

public partial class RibbonCanvasTestingPageViewModel : ViewModelBase
{
    private readonly ISceneSerializer _sceneSerializer = new JsonSceneSerializer();

    public RibbonCanvasTestingPageViewModel(IContentDialogService dialogService, IInfoBarService infoBarService)
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

        SaveSceneCommand = new AsyncRelayCommand(SaveSceneAsync);
        LoadSceneCommand = new AsyncRelayCommand(LoadSceneAsync);
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
    public IAsyncRelayCommand SaveSceneCommand { get; }
    public IAsyncRelayCommand LoadSceneCommand { get; }
    public IRelayCommand ClearCanvasCommand { get; }
    public IRelayCommand ResetViewCommand { get; }

    public string StatusText =>
        $"Tool: {ActiveTool} | Shapes: {Shapes.Count} (Computed: {ComputedShapeIds.Count}) | Zoom: {Zoom:0.00}x | Pan: ({Pan.X:0.0}, {Pan.Y:0.0}) | Cursor(Avalonia): ({CursorAvaloniaPosition.X:0.0}, {CursorAvaloniaPosition.Y:0.0}) | Cursor(Canvas): ({CursorCanvasPosition.X:0.00}, {CursorCanvasPosition.Y:0.00})";

    partial void OnActiveToolChanged(DrawingTool value) => OnPropertyChanged(nameof(StatusText));

    partial void OnZoomChanged(double value) => OnPropertyChanged(nameof(StatusText));

    partial void OnPanChanged(global::Avalonia.Vector value) => OnPropertyChanged(nameof(StatusText));

    partial void OnCursorAvaloniaPositionChanged(global::Avalonia.Point value) => OnPropertyChanged(nameof(StatusText));

    partial void OnCursorCanvasPositionChanged(global::Avalonia.Point value) => OnPropertyChanged(nameof(StatusText));

    private async Task SaveSceneAsync()
    {
        var path = await PromptScenePathAsync("Save scene", "Save", "ribbon-canvas-scene.json");
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            var computed = ComputedShapeIds.ToHashSet(System.StringComparer.Ordinal);
            var scene = SceneDocumentMapper.ToDocument(Shapes, computed);
            var json = _sceneSerializer.Serialize(scene);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(path, json);

            await InfoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Success;
                infoBar.Title = "Scene saved";
                infoBar.Message = path;
            });
        }
        catch (System.Exception ex)
        {
            await InfoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Error;
                infoBar.Title = "Scene save failed";
                infoBar.Message = ex.Message;
            });
        }
    }

    private async Task LoadSceneAsync()
    {
        var path = await PromptScenePathAsync("Load scene", "Load", "ribbon-canvas-scene.json");
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Scene file not found.", path);

            var json = await File.ReadAllTextAsync(path);
            var scene = _sceneSerializer.Deserialize(json);
            var loaded = SceneDocumentMapper.FromDocument(scene);

            Shapes.Clear();
            ComputedShapeIds.Clear();

            foreach (var shape in loaded.Shapes)
                Shapes.Add(shape);

            foreach (var id in loaded.ComputedShapeIds)
                ComputedShapeIds.Add(id);

            await InfoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Success;
                infoBar.Title = "Scene loaded";
                infoBar.Message = $"{path} ({loaded.Shapes.Count} shape(s))";
            });
        }
        catch (System.Exception ex)
        {
            await InfoBarService.ShowAsync(infoBar =>
            {
                infoBar.Severity = InfoBarSeverity.Error;
                infoBar.Title = "Scene load failed";
                infoBar.Message = ex.Message;
            });
        }
    }

    private async Task<string?> PromptScenePathAsync(string title, string primaryButtonText, string defaultFileName)
    {
        var pathBox = new TextBox
        {
            Width = 560,
            Watermark = "Absolute path to .json scene file"
        };

        var defaultPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), defaultFileName);
        pathBox.Text = defaultPath;

        var result = await DialogService.ShowAsync(dialog =>
        {
            dialog.Title = title;
            dialog.Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = "Scene file path:" },
                    pathBox
                }
            };
            dialog.PrimaryButtonText = primaryButtonText;
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = DefaultButton.Primary;
        });

        if (result != DialogResult.Primary)
            return null;

        var path = pathBox.Text?.Trim();
        return string.IsNullOrWhiteSpace(path) ? null : path;
    }

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
