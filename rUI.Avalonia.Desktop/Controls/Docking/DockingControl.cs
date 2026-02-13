using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia;

namespace rUI.Avalonia.Desktop.Controls.Docking;

public class DockingControl : TemplatedControl
{
    private ContentControl? _rootHost;
    private Border? _dropOverlay;
    private Panel? _rootPanel;
    private DockDragSession? _dragSession;

    [Content]
    public AvaloniaList<DockPane> Panes { get; } = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_rootPanel is not null)
        {
            _rootPanel.RemoveHandler(PointerMovedEvent, OnRootPointerMoved);
            _rootPanel.RemoveHandler(PointerReleasedEvent, OnRootPointerReleased);
        }

        _rootHost = e.NameScope.Find<ContentControl>("PART_RootHost");
        _dropOverlay = e.NameScope.Find<Border>("PART_DropOverlay");
        _rootPanel = e.NameScope.Find<Panel>("PART_RootPanel");

        if (_rootPanel is not null)
        {
            _rootPanel.AddHandler(PointerMovedEvent, OnRootPointerMoved, RoutingStrategies.Bubble, true);
            _rootPanel.AddHandler(PointerReleasedEvent, OnRootPointerReleased, RoutingStrategies.Bubble, true);
        }

        InitializeLayout();
    }

    private void InitializeLayout()
    {
        if (_rootHost == null || Panes.Count == 0)
            return;

        var group = new DockTabGroup();
        foreach (var pane in Panes)
            group.Panes.Add(pane);

        WireGroup(group);
        _rootHost.Content = group;
    }

    private void WireGroup(DockTabGroup group)
    {
        group.PaneDragStarted += OnPaneDragStarted;
        group.PaneCloseRequested += OnPaneCloseRequested;
    }

    private void UnwireGroup(DockTabGroup group)
    {
        group.PaneDragStarted -= OnPaneDragStarted;
        group.PaneCloseRequested -= OnPaneCloseRequested;
    }

    private void OnPaneDragStarted(object? sender, DockTabGroupEventArgs e)
    {
        if (_rootPanel == null || _dropOverlay == null)
            return;

        _dragSession = new DockDragSession(e.Pane, e.SourceGroup);
        _dropOverlay.IsVisible = false;

        e.Pointer?.Capture(_rootPanel);
    }

    private void OnRootPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragSession == null || _dropOverlay == null || _rootPanel == null)
            return;

        var position = e.GetPosition(_rootPanel);
        var targetGroup = HitTestTabGroup(position);

        if (targetGroup == null)
        {
            _dropOverlay.IsVisible = false;
            _dragSession.TargetGroup = null;
            _dragSession.TargetPosition = DockPosition.Center;
            return;
        }

        _dragSession.TargetGroup = targetGroup;

        // Determine drop zone based on pointer position relative to target
        var groupBounds = targetGroup.Bounds;
        var groupTopLeft = targetGroup.TranslatePoint(new Point(0, 0), _rootPanel);
        if (groupTopLeft == null)
        {
            _dropOverlay.IsVisible = false;
            return;
        }

        var relativePos = position - groupTopLeft.Value;
        var zone = DetermineDropZone(relativePos, groupBounds.Width, groupBounds.Height);
        _dragSession.TargetPosition = zone;

        // Position and show overlay
        ShowDropOverlay(groupTopLeft.Value, groupBounds.Width, groupBounds.Height, zone);
    }

    private void OnRootPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragSession == null)
            return;

        // Release pointer capture
        e.Pointer.Capture(null);

        var session = _dragSession;
        _dragSession = null;

        if (_dropOverlay != null)
            _dropOverlay.IsVisible = false;

        if (session.TargetGroup == null)
            return;

        ExecuteDrop(session);
    }

    private DockPosition DetermineDropZone(Point relativePos, double width, double height)
    {
        double edgeBand = 0.25;

        double leftBand = width * edgeBand;
        double rightBand = width * (1 - edgeBand);
        double topBand = height * edgeBand;
        double bottomBand = height * (1 - edgeBand);

        if (relativePos.X < leftBand)
            return DockPosition.Left;
        if (relativePos.X > rightBand)
            return DockPosition.Right;
        if (relativePos.Y < topBand)
            return DockPosition.Top;
        if (relativePos.Y > bottomBand)
            return DockPosition.Bottom;

        return DockPosition.Center;
    }

    private void ShowDropOverlay(Point groupOrigin, double groupWidth, double groupHeight, DockPosition zone)
    {
        if (_dropOverlay == null)
            return;

        double x = groupOrigin.X;
        double y = groupOrigin.Y;
        double w = groupWidth;
        double h = groupHeight;

        switch (zone)
        {
            case DockPosition.Left:
                w = groupWidth * 0.5;
                break;
            case DockPosition.Right:
                x += groupWidth * 0.5;
                w = groupWidth * 0.5;
                break;
            case DockPosition.Top:
                h = groupHeight * 0.5;
                break;
            case DockPosition.Bottom:
                y += groupHeight * 0.5;
                h = groupHeight * 0.5;
                break;
        }

        _dropOverlay.Width = w;
        _dropOverlay.Height = h;
        _dropOverlay.RenderTransform = new TranslateTransform(x, y);
        _dropOverlay.IsVisible = true;
    }

    private void ExecuteDrop(DockDragSession session)
    {
        var pane = session.Pane;
        var sourceGroup = session.SourceGroup;
        var targetGroup = session.TargetGroup!;
        var position = session.TargetPosition;

        // Drop on own group at center → no-op
        if (sourceGroup == targetGroup && position == DockPosition.Center)
            return;

        // Drop on own group at edge with single pane → no-op
        if (sourceGroup == targetGroup && sourceGroup.Panes.Count <= 1)
            return;

        // Switch selection away first so the ContentPresenter releases the
        // pane from the logical tree before we re-parent it.
        if (sourceGroup.SelectedPane == pane)
        {
            var idx = sourceGroup.Panes.IndexOf(pane);
            sourceGroup.SelectedPane = sourceGroup.Panes.Count > 1
                ? sourceGroup.Panes[idx == 0 ? 1 : idx - 1]
                : null;
        }

        sourceGroup.Panes.Remove(pane);

        if (position == DockPosition.Center)
        {
            // Move to target group as new tab
            targetGroup.Panes.Add(pane);
            targetGroup.SelectedPane = pane;
        }
        else
        {
            // Split target group
            SplitGroup(targetGroup, pane, position);
        }

        // Collapse empty groups
        if (sourceGroup.Panes.Count == 0)
            CollapseEmptyGroup(sourceGroup);
    }

    private void SplitGroup(DockTabGroup targetGroup, DockPane pane, DockPosition position)
    {
        if (_rootHost == null)
            return;

        var newGroup = new DockTabGroup();
        newGroup.Panes.Add(pane);
        newGroup.SelectedPane = pane;
        WireGroup(newGroup);

        var orientation = position is DockPosition.Left or DockPosition.Right
            ? Orientation.Horizontal
            : Orientation.Vertical;

        var split = new DockSplitContainer { Orientation = orientation };

        bool newIsFirst = position is DockPosition.Left or DockPosition.Top;

        if (newIsFirst)
        {
            split.First = newGroup;
            split.Second = targetGroup;
        }
        else
        {
            split.First = targetGroup;
            split.Second = newGroup;
        }

        ReplaceInParent(targetGroup, split);
    }

    private void CollapseEmptyGroup(DockTabGroup emptyGroup)
    {
        if (_rootHost == null)
            return;

        UnwireGroup(emptyGroup);

        // If the empty group is the root content
        if (_rootHost.Content == emptyGroup)
        {
            _rootHost.Content = null;
            return;
        }

        // Find parent DockSplitContainer
        var parent = FindParentSplit(emptyGroup);
        if (parent == null)
            return;

        // Get the surviving child
        Control? survivor = null;
        if (parent.First == emptyGroup)
            survivor = parent.Second;
        else if (parent.Second == emptyGroup)
            survivor = parent.First;

        if (survivor == null)
            return;

        // Detach survivor from the split
        parent.First = null;
        parent.Second = null;

        // Replace the split with the survivor
        ReplaceInParent(parent, survivor);
    }

    private void ReplaceInParent(Control target, Control replacement)
    {
        if (_rootHost == null)
            return;

        if (_rootHost.Content == target)
        {
            _rootHost.Content = replacement;
            return;
        }

        var parent = FindParentSplit(target);
        if (parent == null)
            return;

        if (parent.First == target)
            parent.First = replacement;
        else if (parent.Second == target)
            parent.Second = replacement;
    }

    private DockSplitContainer? FindParentSplit(Control child)
    {
        if (_rootHost?.Content is not Control root)
            return null;

        return FindParentSplitRecursive(root, child);
    }

    private DockSplitContainer? FindParentSplitRecursive(Control current, Control target)
    {
        if (current is DockSplitContainer split)
        {
            if (split.First == target || split.Second == target)
                return split;

            if (split.First != null)
            {
                var result = FindParentSplitRecursive(split.First, target);
                if (result != null) return result;
            }

            if (split.Second != null)
            {
                var result = FindParentSplitRecursive(split.Second, target);
                if (result != null) return result;
            }
        }

        return null;
    }

    private DockTabGroup? HitTestTabGroup(Point position)
    {
        if (_rootPanel == null)
            return null;

        var groups = new List<DockTabGroup>();
        CollectTabGroups(_rootHost?.Content as Control, groups);

        foreach (var group in groups)
        {
            var topLeft = group.TranslatePoint(new Point(0, 0), _rootPanel);
            if (topLeft == null) continue;

            var bounds = new Rect(topLeft.Value, group.Bounds.Size);
            if (bounds.Contains(position))
                return group;
        }

        return null;
    }

    private void CollectTabGroups(Control? control, List<DockTabGroup> groups)
    {
        if (control is DockTabGroup group)
        {
            groups.Add(group);
            return;
        }

        if (control is DockSplitContainer split)
        {
            if (split.First != null) CollectTabGroups(split.First, groups);
            if (split.Second != null) CollectTabGroups(split.Second, groups);
        }
    }

    public void ClosePane(DockPane pane)
    {
        if (_rootHost?.Content == null)
            return;

        var groups = new List<DockTabGroup>();
        CollectTabGroups(_rootHost.Content as Control, groups);

        foreach (var group in groups)
        {
            if (group.Panes.Contains(pane))
            {
                if (group.SelectedPane == pane)
                {
                    var idx = group.Panes.IndexOf(pane);
                    group.SelectedPane = group.Panes.Count > 1
                        ? group.Panes[idx == 0 ? 1 : idx - 1]
                        : null;
                }

                group.Panes.Remove(pane);

                if (group.Panes.Count == 0)
                    CollapseEmptyGroup(group);

                return;
            }
        }
    }

    private void OnPaneCloseRequested(object? sender, DockTabGroupEventArgs e)
    {
        ClosePane(e.Pane);
    }

    private class DockDragSession
    {
        public DockPane Pane { get; }
        public DockTabGroup SourceGroup { get; }
        public DockTabGroup? TargetGroup { get; set; }
        public DockPosition TargetPosition { get; set; } = DockPosition.Center;

        public DockDragSession(DockPane pane, DockTabGroup sourceGroup)
        {
            Pane = pane;
            SourceGroup = sourceGroup;
        }
    }
}
