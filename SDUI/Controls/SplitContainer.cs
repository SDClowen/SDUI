using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SkiaSharp;

namespace SDUI.Controls;

public class SplitContainer : UIElementBase
{
    private bool _dragging;
    private Point _dragStart;
    private int _initialDistance;
    private Orientation _orientation = Orientation.Vertical;
    private int _panel1MinSize = 30;
    private int _panel2MinSize = 30;
    private int _splitterDistance; // 0 means auto-center until first layout
    private int _splitterWidth = 6;

    public SplitContainer()
    {
        Panel1 = new Panel { BackColor = Color.Transparent };
        Panel2 = new Panel { BackColor = Color.Transparent };

        Controls.Add(Panel1);
        Controls.Add(Panel2);

        // make keyboard focusable
        TabStop = true;
    }

    [Category("Layout")]
    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation == value) return;
            _orientation = value;
            Invalidate();
            LayoutPanels();
        }
    }

    [Category("Layout")]
    public int SplitterWidth
    {
        get => _splitterWidth;
        set
        {
            if (value <= 0) return;
            _splitterWidth = value;
            Invalidate();
            LayoutPanels();
        }
    }

    [Category("Layout")]
    public int SplitterDistance
    {
        get => _splitterDistance;
        set
        {
            _splitterDistance = value;
            if (Parent is not null)
                LayoutPanels();
            Invalidate();
        }
    }

    [Category("Layout")]
    public int Panel1MinSize
    {
        get => _panel1MinSize;
        set => _panel1MinSize = Math.Max(0, value);
    }

    [Category("Layout")]
    public int Panel2MinSize
    {
        get => _panel2MinSize;
        set => _panel2MinSize = Math.Max(0, value);
    }

    [Browsable(false)] public Panel Panel1 { get; }

    [Browsable(false)] public Panel Panel2 { get; }

    public event EventHandler? SplitterMoving;
    public event EventHandler? SplitterMoved;

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        LayoutPanels();
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var splitter = GetSplitterRect();
        using var paint = new SKPaint
            { IsAntialias = true, Color = ColorScheme.BorderColor.ToSKColor(), Style = SKPaintStyle.Fill };
        canvas.DrawRect(new SKRect(splitter.X, splitter.Y, splitter.Right, splitter.Bottom), paint);

        // draw a small grabber
        using var grab = new SKPaint
            { IsAntialias = true, Color = ColorScheme.OnSurface.ToSKColor(), Style = SKPaintStyle.Fill };
        if (Orientation == Orientation.Vertical)
        {
            var lineX = splitter.X + splitter.Width / 2f;
            var centerY = splitter.Y + splitter.Height / 2f;
            var gap = 6 * ScaleFactor;
            var lineHeight = Math.Min(12 * ScaleFactor, splitter.Height - 6);
            canvas.DrawRoundRect(lineX - 1.5f, centerY - lineHeight / 2f, 3, lineHeight, 1.5f, 1.5f, grab);
        }
        else
        {
            var lineY = splitter.Y + splitter.Height / 2f;
            var centerX = splitter.X + splitter.Width / 2f;
            var gap = 6 * ScaleFactor;
            var lineWidth = Math.Min(12 * ScaleFactor, splitter.Width - 6);
            canvas.DrawRoundRect(centerX - lineWidth / 2f, lineY - 1.5f, lineWidth, 3, 1.5f, 1.5f, grab);
        }

        base.OnPaint(e);
    }

    private Rectangle GetSplitterRect()
    {
        if (Orientation == Orientation.Vertical)
        {
            var dist = _splitterDistance <= 0 ? Width / 2 : _splitterDistance;
            dist = Math.Max(Panel1MinSize, Math.Min(dist, Width - Panel2MinSize - SplitterWidth));
            return new Rectangle(dist, 0, SplitterWidth, Height);
        }
        else
        {
            var dist = _splitterDistance <= 0 ? Height / 2 : _splitterDistance;
            dist = Math.Max(Panel1MinSize, Math.Min(dist, Height - Panel2MinSize - SplitterWidth));
            return new Rectangle(0, dist, Width, SplitterWidth);
        }
    }

    private void LayoutPanels()
    {
        if (Orientation == Orientation.Vertical)
        {
            var dist = _splitterDistance <= 0 ? Width / 2 : _splitterDistance;
            dist = Math.Max(Panel1MinSize, Math.Min(dist, Width - Panel2MinSize - SplitterWidth));

            Panel1.Bounds = new Rectangle(0, 0, dist, Height);
            Panel2.Bounds = new Rectangle(dist + SplitterWidth, 0, Width - (dist + SplitterWidth), Height);
            _splitterDistance = dist; // store the clamped value
        }
        else
        {
            var dist = _splitterDistance <= 0 ? Height / 2 : _splitterDistance;
            dist = Math.Max(Panel1MinSize, Math.Min(dist, Height - Panel2MinSize - SplitterWidth));

            Panel1.Bounds = new Rectangle(0, 0, Width, dist);
            Panel2.Bounds = new Rectangle(0, dist + SplitterWidth, Width, Height - (dist + SplitterWidth));
            _splitterDistance = dist; // store the clamped value
        }

        Invalidate();
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        var splitter = GetSplitterRect();
        if (e.Button == MouseButtons.Left && splitter.Contains(e.Location))
        {
            _dragging = true;
            _dragStart = e.Location;
            _initialDistance = _splitterDistance <= 0
                ? Orientation == Orientation.Vertical ? Width / 2 : Height / 2
                : _splitterDistance;
        }
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var splitter = GetSplitterRect();
        if (_dragging)
        {
            var delta = Orientation == Orientation.Vertical ? e.X - _dragStart.X : e.Y - _dragStart.Y;
            var newDist = _initialDistance + delta;
            // clamp
            if (Orientation == Orientation.Vertical)
                newDist = Math.Max(Panel1MinSize, Math.Min(newDist, Width - Panel2MinSize - SplitterWidth));
            else
                newDist = Math.Max(Panel1MinSize, Math.Min(newDist, Height - Panel2MinSize - SplitterWidth));

            if (newDist != _splitterDistance)
            {
                _splitterDistance = newDist;
                LayoutPanels();
                SplitterMoving?.Invoke(this, EventArgs.Empty);
            }

            Cursor = Orientation == Orientation.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
            return;
        }

        // hover cursor only when over splitter
        Cursor = splitter.Contains(e.Location)
            ? Orientation == Orientation.Vertical ? Cursors.SizeWE : Cursors.SizeNS
            : Cursors.Default;
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_dragging && e.Button == MouseButtons.Left)
        {
            _dragging = false;
            SplitterMoved?.Invoke(this, EventArgs.Empty);
        }
    }
}