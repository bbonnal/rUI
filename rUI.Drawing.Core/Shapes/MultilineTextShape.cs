using Flowxel.Core.Geometry.Shapes;

namespace rUI.Drawing.Core.Shapes;

public sealed class MultilineTextShape : Shape
{
    public string Text { get; set; } = "Line 1\nLine 2";

    public double FontSize { get; set; } = 16;

    public double Width { get; set; } = 240;
}
