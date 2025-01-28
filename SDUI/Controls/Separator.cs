using SkiaSharp;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Separator : UIElementBase
{
    private bool _isVertical = false;

    public bool IsVertical
    {
        get => _isVertical;
        set
        {
            _isVertical = value;
            Invalidate();
        }
    }

    public Separator()
    {
        this.Size = new Size(120, 6);
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        using var paint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
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
