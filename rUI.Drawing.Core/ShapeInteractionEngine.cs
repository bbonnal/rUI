using Flowxel.Core.Geometry.Primitives;
using Flowxel.Core.Geometry.Shapes;
using FlowPoint = Flowxel.Core.Geometry.Shapes.Point;
using FlowRectangle = Flowxel.Core.Geometry.Shapes.Rectangle;

namespace rUI.Drawing.Core;

public static class ShapeInteractionEngine
{
    public static Shape? BuildShape(DrawingTool tool, Vector start, Vector end, double minShapeSize)
    {
        var delta = end - start;

        switch (tool)
        {
            case DrawingTool.Line:
            {
                var length = delta.M;
                if (length <= minShapeSize)
                    return null;

                return new Line
                {
                    Pose = CreatePose(start.X, start.Y, delta.Normalize()),
                    Length = length
                };
            }
            case DrawingTool.Rectangle:
            {
                var width = Math.Abs(delta.X);
                var height = Math.Abs(delta.Y);
                if (width <= minShapeSize || height <= minShapeSize)
                    return null;

                return new FlowRectangle
                {
                    Pose = CreatePose((start.X + end.X) * 0.5, (start.Y + end.Y) * 0.5),
                    Width = width,
                    Height = height
                };
            }
            case DrawingTool.Circle:
            {
                var radius = delta.M;
                if (radius <= minShapeSize)
                    return null;

                return new Circle
                {
                    Pose = CreatePose(start.X, start.Y),
                    Radius = radius
                };
            }
            default:
                return null;
        }
    }

    public static bool IsShapePerimeterHit(Shape shape, Vector world, double tolerance, double pointRadius)
    {
        switch (shape)
        {
            case FlowPoint point:
                return Distance(point.Pose.Position, world) <= pointRadius + tolerance;
            case Line line:
                return DistanceToSegment(world, line.StartPoint.Position, line.EndPoint.Position) <= tolerance;
            case FlowRectangle rectangle:
            {
                var topLeft = rectangle.TopLeft.Position;
                var topRight = rectangle.TopRight.Position;
                var bottomRight = rectangle.BottomRight.Position;
                var bottomLeft = rectangle.BottomLeft.Position;
                return
                    DistanceToSegment(world, topLeft, topRight) <= tolerance ||
                    DistanceToSegment(world, topRight, bottomRight) <= tolerance ||
                    DistanceToSegment(world, bottomRight, bottomLeft) <= tolerance ||
                    DistanceToSegment(world, bottomLeft, topLeft) <= tolerance;
            }
            case Circle circle:
            {
                var distance = Distance(circle.Pose.Position, world);
                return Math.Abs(distance - circle.Radius) <= tolerance;
            }
            default:
                return false;
        }
    }

    public static IReadOnlyList<ShapeHandle> GetHandles(Shape shape)
    {
        switch (shape)
        {
            case FlowPoint point:
                return
                [
                    new ShapeHandle(ShapeHandleKind.PointPosition, point.Pose.Position)
                ];
            case Line line:
                return
                [
                    new ShapeHandle(ShapeHandleKind.LineStart, line.StartPoint.Position),
                    new ShapeHandle(ShapeHandleKind.LineEnd, line.EndPoint.Position),
                    new ShapeHandle(ShapeHandleKind.Move, Midpoint(line.StartPoint.Position, line.EndPoint.Position))
                ];
            case FlowRectangle rectangle:
                return
                [
                    new ShapeHandle(ShapeHandleKind.RectTopLeft, rectangle.TopLeft.Position),
                    new ShapeHandle(ShapeHandleKind.RectTopRight, rectangle.TopRight.Position),
                    new ShapeHandle(ShapeHandleKind.RectBottomRight, rectangle.BottomRight.Position),
                    new ShapeHandle(ShapeHandleKind.RectBottomLeft, rectangle.BottomLeft.Position),
                    new ShapeHandle(ShapeHandleKind.Move, rectangle.Pose.Position)
                ];
            case Circle circle:
                return
                [
                    new ShapeHandle(ShapeHandleKind.Move, circle.Pose.Position),
                    new ShapeHandle(ShapeHandleKind.CircleRadius, circle.Pose.Position.Translate(new Vector(circle.Radius, 0)))
                ];
            default:
                return [];
        }
    }

    public static ShapeHandleKind HitTestHandle(Shape shape, Vector world, double tolerance)
    {
        foreach (var handle in GetHandles(shape))
        {
            if (Distance(handle.Position, world) <= tolerance)
                return handle.Kind;
        }

        return ShapeHandleKind.None;
    }

    public static void ApplyHandleDrag(Shape shape, ShapeHandleKind handle, Vector world, Vector? lastWorld, double minShapeSize)
    {
        switch (shape)
        {
            case FlowPoint point:
                if (handle is ShapeHandleKind.PointPosition or ShapeHandleKind.Move)
                    point.Pose = CreatePose(world.X, world.Y);
                break;
            case Line line:
                ApplyLineHandleDrag(line, handle, world, lastWorld, minShapeSize);
                break;
            case FlowRectangle rectangle:
                ApplyRectangleHandleDrag(rectangle, handle, world, lastWorld, minShapeSize);
                break;
            case Circle circle:
                ApplyCircleHandleDrag(circle, handle, world, lastWorld, minShapeSize);
                break;
        }
    }

    private static void ApplyLineHandleDrag(Line line, ShapeHandleKind handle, Vector world, Vector? lastWorld, double minShapeSize)
    {
        var start = line.StartPoint.Position;
        var end = line.EndPoint.Position;

        switch (handle)
        {
            case ShapeHandleKind.Move when lastWorld is not null:
            {
                line.Translate(world - lastWorld.Value);
                return;
            }
            case ShapeHandleKind.LineStart:
                SetLineFromEndpoints(line, world, end, minShapeSize);
                return;
            case ShapeHandleKind.LineEnd:
                SetLineFromEndpoints(line, start, world, minShapeSize);
                return;
        }
    }

    private static void ApplyRectangleHandleDrag(
        FlowRectangle rectangle,
        ShapeHandleKind handle,
        Vector world,
        Vector? lastWorld,
        double minShapeSize)
    {
        if (handle == ShapeHandleKind.Move && lastWorld is not null)
        {
            rectangle.Translate(world - lastWorld.Value);
            return;
        }

        var topLeft = rectangle.TopLeft.Position;
        var topRight = rectangle.TopRight.Position;
        var bottomRight = rectangle.BottomRight.Position;
        var bottomLeft = rectangle.BottomLeft.Position;

        switch (handle)
        {
            case ShapeHandleKind.RectTopLeft:
                ResizeRectangle(rectangle, world, bottomRight, minShapeSize);
                return;
            case ShapeHandleKind.RectTopRight:
                ResizeRectangle(rectangle, world, bottomLeft, minShapeSize);
                return;
            case ShapeHandleKind.RectBottomRight:
                ResizeRectangle(rectangle, world, topLeft, minShapeSize);
                return;
            case ShapeHandleKind.RectBottomLeft:
                ResizeRectangle(rectangle, world, topRight, minShapeSize);
                return;
        }
    }

    private static void ApplyCircleHandleDrag(Circle circle, ShapeHandleKind handle, Vector world, Vector? lastWorld, double minShapeSize)
    {
        switch (handle)
        {
            case ShapeHandleKind.Move when lastWorld is not null:
                circle.Translate(world - lastWorld.Value);
                return;
            case ShapeHandleKind.CircleRadius:
                circle.Radius = Math.Max(Distance(circle.Pose.Position, world), minShapeSize);
                return;
        }
    }

    private static void SetLineFromEndpoints(Line line, Vector start, Vector end, double minShapeSize)
    {
        var delta = end - start;
        if (delta.M <= minShapeSize)
            return;

        line.Pose = CreatePose(start.X, start.Y, delta.Normalize());
        line.Length = delta.M;
    }

    private static void ResizeRectangle(FlowRectangle rectangle, Vector movingCorner, Vector fixedCorner, double minShapeSize)
    {
        var width = Math.Abs(movingCorner.X - fixedCorner.X);
        var height = Math.Abs(movingCorner.Y - fixedCorner.Y);
        if (width <= minShapeSize || height <= minShapeSize)
            return;

        rectangle.Pose = CreatePose(
            (movingCorner.X + fixedCorner.X) * 0.5,
            (movingCorner.Y + fixedCorner.Y) * 0.5);
        rectangle.Width = width;
        rectangle.Height = height;
    }

    private static Pose CreatePose(double x, double y, Vector? orientation = null)
        => new(new Vector(x, y), orientation ?? new Vector(1, 0));

    private static double Distance(Vector a, Vector b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static Vector Midpoint(Vector a, Vector b)
        => new((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5);

    private static double DistanceToSegment(Vector point, Vector segStart, Vector segEnd)
    {
        var dx = segEnd.X - segStart.X;
        var dy = segEnd.Y - segStart.Y;
        var segmentLenSq = (dx * dx) + (dy * dy);
        if (segmentLenSq <= 0.0000001)
            return Distance(point, segStart);

        var t = ((point.X - segStart.X) * dx + (point.Y - segStart.Y) * dy) / segmentLenSq;
        t = Math.Clamp(t, 0, 1);
        var projX = segStart.X + (t * dx);
        var projY = segStart.Y + (t * dy);
        var distanceX = point.X - projX;
        var distanceY = point.Y - projY;
        return Math.Sqrt((distanceX * distanceX) + (distanceY * distanceY));
    }
}
