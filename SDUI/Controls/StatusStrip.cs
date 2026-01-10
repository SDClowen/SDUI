using System;
using System.ComponentModel;
using System.Windows.Forms;
using SkiaSharp;

namespace SDUI.Controls;

// StatusStrip intentionally inherits MenuStrip to reuse item layout/docking behavior
public class StatusStrip : MenuStrip
{
    private readonly float _height = 24f;

    // Cached paints
    private SKPaint? _bgPaint;
    private SKPaint? _bottomBorderPaint;
    private Padding _gripMargin = new(3);
    private SKPaint? _gripPaint;
    private float _gripSize = 12f;
    private float _itemPadding = 6f;
    private ToolStripLayoutStyle _layoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;

    // WinForms-like features
    private bool _sizingGrip = true;

    public StatusStrip()
    {
        // Default to dock bottom like WinForms StatusStrip
        Height = (int)_height;
        Dock = DockStyle.Bottom;
        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;

        // StatusStrip shouldn't show hover effects or dropdowns
        ShowHoverEffect = false;
        ShowIcons = false;
        Stretch = false;
    }

    [Category("Layout")]
    public float ItemPadding
    {
        get => _itemPadding;
        set
        {
            if (_itemPadding == value) return;
            _itemPadding = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    [DefaultValue(true)]
    public bool SizingGrip
    {
        get => _sizingGrip;
        set
        {
            if (_sizingGrip == value) return;
            _sizingGrip = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    public Padding GripMargin
    {
        get => _gripMargin;
        set
        {
            if (_gripMargin == value) return;
            _gripMargin = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    [DefaultValue(ToolStripLayoutStyle.HorizontalStackWithOverflow)]
    public ToolStripLayoutStyle LayoutStyle
    {
        get => _layoutStyle;
        set
        {
            if (_layoutStyle == value) return;
            _layoutStyle = value;
            Invalidate();
        }
    }

    /// <summary>
    ///     Add a non-interactive status item (label-like)
    /// </summary>
    public MenuItem AddLabel(string text)
    {
        var item = new MenuItem { Text = text, Enabled = false, ForeColor = ForeColor };
        AddItem(item);
        return item;
    }

    /// <summary>
    ///     Add an existing MenuItem as a status item. Caller is responsible for disabling clicks if desired.
    /// </summary>
    public void AddStatusItem(MenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        // Status items are typically non-interactive
        item.Enabled = false;
        AddItem(item);
    }

    private void AddItem(MenuItem item)
    {
        Items.Add(item);
        item.Parent = this;
        Invalidate();
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);

        var canvas = e.Surface.Canvas;
        var bounds = ClientRectangle;

        // Draw flat background like WinForms StatusStrip
        _bgPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _bgPaint.Color = BackColor.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, bounds.Width, bounds.Height), _bgPaint);

        // subtle top border
        _bottomBorderPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
        _bottomBorderPaint.Color = ColorScheme.BorderColor.ToSKColor().WithAlpha(120);
        canvas.DrawLine(0, 0, bounds.Width, 0, _bottomBorderPaint);

        // Draw sizing grip if enabled (bottom-right)
        if (SizingGrip)
        {
            _gripPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _gripPaint.Color = ColorScheme.BorderColor.ToSKColor().WithAlpha(180);

            // Draw three diagonal dots to mimic WinForms sizing grip
            var dotSize = 3f;
            var gap = 4f;
            var startX = bounds.Width - GripMargin.Right - 4f;
            var startY = bounds.Height - GripMargin.Bottom - 4f;

            for (var row = 0; row < 3; row++)
            for (var col = 0; col <= row; col++)
            {
                var x = startX - col * gap - row * 1f;
                var y = startY - row * gap + col * 0f;
                var r = new SKRect(x - dotSize, y - dotSize, x, y);
                canvas.DrawRect(r, _gripPaint);
            }
        }
    }
}