using System.Drawing;
using SDUI.Controls;
using SkiaSharp;

namespace SDUI.Designer;

/// <summary>
/// Panel to show selection with resize handles
/// </summary>
internal class SelectionOverlay : SDUI.Controls.UIElementBase
{
    private const int HandleSize = 8;

    public SelectionOverlay()
    {
        BackColor = Color.Transparent;
        Visible = false;
    }

    public void ShowSelection(Rectangle bounds)
    {
        Bounds = bounds;
        Visible = true;
        BringToFront();
        Invalidate();
    }

    public void Clear()
    {
        Visible = false;
    }

    public override void OnPaint(SKCanvas canvas)
    {
        // Do NOT call base.OnPaint to skip background drawing
        // base.OnPaint(canvas);

        if (!Visible) return;

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = ColorScheme.Primary.ToSKColor(),
            StrokeWidth = 2,
            PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0)
        };

        var rect = new SKRect(0, 0, Width, Height);
        canvas.DrawRect(rect, paint);

        // Draw resize handles
        paint.PathEffect = null;
        paint.Style = SKPaintStyle.Fill;
        paint.Color = SKColors.White;

        var handlePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = ColorScheme.Primary.ToSKColor(),
            StrokeWidth = 1
        };

        DrawHandle(canvas, 0, 0, paint, handlePaint); // TopLeft
        DrawHandle(canvas, Width / 2, 0, paint, handlePaint); // Top
        DrawHandle(canvas, Width, 0, paint, handlePaint); // TopRight
        DrawHandle(canvas, Width, Height / 2, paint, handlePaint); // Right
        DrawHandle(canvas, Width, Height, paint, handlePaint); // BottomRight
        DrawHandle(canvas, Width / 2, Height, paint, handlePaint); // Bottom
        DrawHandle(canvas, 0, Height, paint, handlePaint); // BottomLeft
        DrawHandle(canvas, 0, Height / 2, paint, handlePaint); // Left

        paint.Dispose();
        handlePaint.Dispose();
    }

    private void DrawHandle(SKCanvas canvas, float x, float y, SKPaint fillPaint, SKPaint strokePaint)
    {
        float half = HandleSize / 2;
        var rect = new SKRect(x - half, y - half, x + half, y + half);
        canvas.DrawRect(rect, fillPaint);
        canvas.DrawRect(rect, strokePaint);
    }
}
