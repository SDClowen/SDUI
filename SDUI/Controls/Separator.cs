using System.Drawing;
using SkiaSharp;

namespace SDUI.Controls;

public class Separator : UIElementBase
{
    private bool _isVertical;

    public Separator()
    {
        Size = new Size((int)(120 * ScaleFactor), (int)(6 * ScaleFactor));
    }

    public bool IsVertical
    {
        get => _isVertical;
        set
        {
            _isVertical = value;
            Invalidate();
        }
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        using var paint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f * ScaleFactor
        };

        if (_isVertical)
        {
            var x = Width / 2f;
            canvas.DrawLine(x, 0, x, Height, paint);
        }
        else
        {
            var y = Height / 2f;
            canvas.DrawLine(0, y, Width, y, paint);
        }
    }
}