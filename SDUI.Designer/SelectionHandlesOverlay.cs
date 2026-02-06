using System;

using SDUI.Controls;
using SDUI.Extensions;
using SkiaSharp;

namespace SDUI.Designer;

/// <summary>
/// SDUI overlay for drawing selection handles with SkiaSharp
/// </summary>
internal class SelectionHandlesOverlay : UIElementBase
{
    private Rectangle _bounds = Rectangle.Empty;
    private ResizeHandle _activeHandle = ResizeHandle.None;
    private SKPoint _dragStart;
    private Rectangle _originalBounds;

    public event EventHandler<Rectangle>? BoundsResized;

    public SelectionHandlesOverlay()
    {
        BackColor = Color.Transparent;
        Enabled = true;
        Dock = System.Windows.Forms.DockStyle.Fill;
        
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        MouseLeave += (s, e) => Cursor = System.Windows.Forms.Cursors.Default;
    }

    public void SetBounds(Rectangle bounds)
    {
        _bounds = bounds;
        Visible = true;
        BringToFront();
        Invalidate();
    }

    public void Clear()
    {
        _bounds = Rectangle.Empty;
        Visible = false;
        Invalidate();
    }

    private void OnMouseDown(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (_bounds.IsEmpty || e.Button != System.Windows.Forms.MouseButtons.Left)
            return;

        _activeHandle = GetHandleAtPoint(e.Location);
        if (_activeHandle != ResizeHandle.None)
        {
            _dragStart = e.Location;
            _originalBounds = _bounds;
        }
    }

    private void OnMouseMove(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (_bounds.IsEmpty)
            return;

        if (_activeHandle != ResizeHandle.None)
        {
            var deltaX = e.X - _dragStart.X;
            var deltaY = e.Y - _dragStart.Y;
            
            var newBounds = _originalBounds;

            switch (_activeHandle)
            {
                case ResizeHandle.TopLeft:
                    newBounds = new Rectangle(
                        _originalBounds.X + deltaX,
                        _originalBounds.Y + deltaY,
                        _originalBounds.Width - deltaX,
                        _originalBounds.Height - deltaY);
                    break;
                case ResizeHandle.Top:
                    newBounds = new Rectangle(
                        _originalBounds.X,
                        _originalBounds.Y + deltaY,
                        _originalBounds.Width,
                        _originalBounds.Height - deltaY);
                    break;
                case ResizeHandle.TopRight:
                    newBounds = new Rectangle(
                        _originalBounds.X,
                        _originalBounds.Y + deltaY,
                        _originalBounds.Width + deltaX,
                        _originalBounds.Height - deltaY);
                    break;
                case ResizeHandle.Right:
                    newBounds = new Rectangle(
                        _originalBounds.X,
                        _originalBounds.Y,
                        _originalBounds.Width + deltaX,
                        _originalBounds.Height);
                    break;
                case ResizeHandle.BottomRight:
                    newBounds = new Rectangle(
                        _originalBounds.X,
                        _originalBounds.Y,
                        _originalBounds.Width + deltaX,
                        _originalBounds.Height + deltaY);
                    break;
                case ResizeHandle.Bottom:
                    newBounds = new Rectangle(
                        _originalBounds.X,
                        _originalBounds.Y,
                        _originalBounds.Width,
                        _originalBounds.Height + deltaY);
                    break;
                case ResizeHandle.BottomLeft:
                    newBounds = new Rectangle(
                        _originalBounds.X + deltaX,
                        _originalBounds.Y,
                        _originalBounds.Width - deltaX,
                        _originalBounds.Height + deltaY);
                    break;
                case ResizeHandle.Left:
                    newBounds = new Rectangle(
                        _originalBounds.X + deltaX,
                        _originalBounds.Y,
                        _originalBounds.Width - deltaX,
                        _originalBounds.Height);
                    break;
            }

            if (newBounds.Width > 20 && newBounds.Height > 20)
            {
                _bounds = newBounds;
                BoundsResized?.Invoke(this, newBounds);
                Invalidate();
            }
        }
        else
        {
            var handle = GetHandleAtPoint(e.Location);
            Cursor = GetCursorForHandle(handle);
        }
    }

    private void OnMouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        _activeHandle = ResizeHandle.None;
    }

    private ResizeHandle GetHandleAtPoint(SKPoint point)
    {
        const int handleSize = 8;
        
        if (IsInHandle(point, _bounds.Left, _bounds.Top, handleSize))
            return ResizeHandle.TopLeft;
        if (IsInHandle(point, _bounds.Right, _bounds.Top, handleSize))
            return ResizeHandle.TopRight;
        if (IsInHandle(point, _bounds.Left, _bounds.Bottom, handleSize))
            return ResizeHandle.BottomLeft;
        if (IsInHandle(point, _bounds.Right, _bounds.Bottom, handleSize))
            return ResizeHandle.BottomRight;
        if (IsInHandle(point, _bounds.Left + _bounds.Width / 2, _bounds.Top, handleSize))
            return ResizeHandle.Top;
        if (IsInHandle(point, _bounds.Left + _bounds.Width / 2, _bounds.Bottom, handleSize))
            return ResizeHandle.Bottom;
        if (IsInHandle(point, _bounds.Left, _bounds.Top + _bounds.Height / 2, handleSize))
            return ResizeHandle.Left;
        if (IsInHandle(point, _bounds.Right, _bounds.Top + _bounds.Height / 2, handleSize))
            return ResizeHandle.Right;

        return ResizeHandle.None;
    }

    private bool IsInHandle(SKPoint point, int centerX, int centerY, int handleSize)
    {
        var halfSize = handleSize / 2;
        return point.X >= centerX - halfSize && point.X <= centerX + halfSize &&
               point.Y >= centerY - halfSize && point.Y <= centerY + halfSize;
    }

    private System.Windows.Forms.Cursor GetCursorForHandle(ResizeHandle handle)
    {
        return handle switch
        {
            ResizeHandle.TopLeft or ResizeHandle.BottomRight => System.Windows.Forms.Cursors.SizeNWSE,
            ResizeHandle.TopRight or ResizeHandle.BottomLeft => System.Windows.Forms.Cursors.SizeNESW,
            ResizeHandle.Top or ResizeHandle.Bottom => System.Windows.Forms.Cursors.SizeNS,
            ResizeHandle.Left or ResizeHandle.Right => System.Windows.Forms.Cursors.SizeWE,
            _ => System.Windows.Forms.Cursors.Default
        };
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        if (_bounds.IsEmpty)
            return;

        var scale = ScaleFactor;
        var skBounds = new SKRect(
            _bounds.Left * scale,
            _bounds.Top * scale,
            _bounds.Right * scale,
            _bounds.Bottom * scale
        );

        // Draw dashed selection rectangle
        using var paint = new SKPaint
        {
            Color = ColorScheme.Primary,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2 * scale,
            PathEffect = SKPathEffect.CreateDash(new[] { 5f * scale, 5f * scale }, 0),
            IsAntialias = true
        };

        canvas.DrawRect(skBounds, paint);

        // Draw resize handles
        var handleSize = 6 * scale;
        using var handlePaint = new SKPaint
        {
            Color = ColorScheme.Primary,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        // Corner handles
        DrawHandle(canvas, handlePaint, skBounds.Left, skBounds.Top, handleSize);
        DrawHandle(canvas, handlePaint, skBounds.Right, skBounds.Top, handleSize);
        DrawHandle(canvas, handlePaint, skBounds.Left, skBounds.Bottom, handleSize);
        DrawHandle(canvas, handlePaint, skBounds.Right, skBounds.Bottom, handleSize);

        // Side handles
        DrawHandle(canvas, handlePaint, skBounds.MidX, skBounds.Top, handleSize);
        DrawHandle(canvas, handlePaint, skBounds.MidX, skBounds.Bottom, handleSize);
        DrawHandle(canvas, handlePaint, skBounds.Left, skBounds.MidY, handleSize);
        DrawHandle(canvas, handlePaint, skBounds.Right, skBounds.MidY, handleSize);
    }

    private void DrawHandle(SKCanvas canvas, SKPaint paint, float x, float y, float size)
    {
        var halfSize = size / 2;
        canvas.DrawRect(x - halfSize, y - halfSize, size, size, paint);
    }
}

internal enum ResizeHandle
{
    None,
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
}
