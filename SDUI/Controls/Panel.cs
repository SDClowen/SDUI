using SDUI.Extensions;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Panel : SKControl
{
    private int _radius = 10;
    [Category("Appearance")]
    public int Radius
    {
        get => _radius;
        set
        {
            if (_radius == value)
                return;

            _radius = value;
            Invalidate();
        }
    }

    private Padding _border;
    [Category("Appearance")]
    public Padding Border
    {
        get => _border;
        set
        {
            if (_border == value)
                return;

            _border = value;
            Invalidate();
        }
    }

    private Color _borderColor = Color.Transparent;
    [Category("Appearance")]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor == value)
                return;

            _borderColor = value;
            Invalidate();
        }
    }

    private float _shadowDepth = 4;
    [Category("Appearance")]
    public float ShadowDepth
    {
        get => _shadowDepth;
        set
        {
            if (_shadowDepth == value)
                return;

            _shadowDepth = value;
            Invalidate();
        }
    }

    public Panel()
    {
        SetStyle(ControlStyles.Selectable, true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var rect = new SKRect(0, 0, Width, Height);
        var color = BackColor == Color.Transparent ? ColorScheme.BackColor2 : BackColor;
        var borderColor = _borderColor == Color.Transparent ? ColorScheme.BorderColor : _borderColor;

        // Gölge çizimi
        if (_shadowDepth > 0)
        {
            using var shadowPaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha(30),
                ImageFilter = SKImageFilter.CreateDropShadow(
                    _shadowDepth,
                    _shadowDepth,
                    3,
                    3,
                    SKColors.Black.WithAlpha(30)),
                IsAntialias = true
            };

            if (_radius > 0)
            {
                using var path = new SKPath();
                path.AddRoundRect(rect, _radius * DPI, _radius * DPI);
                canvas.DrawPath(path, shadowPaint);
            }
            else
            {
                canvas.DrawRect(rect, shadowPaint);
            }
        }

        // Panel arka planı
        using (var paint = new SKPaint
        {
            Color = color.ToSKColor(),
            IsAntialias = true
        })
        {
            if (_radius > 0)
            {
                using var path = new SKPath();
                path.AddRoundRect(rect, _radius * DPI, _radius * DPI);
                canvas.DrawPath(path, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }

        // Kenarlık çizimi
        if (_border.All > 0 || _border.Left > 0 || _border.Top > 0 || _border.Right > 0 || _border.Bottom > 0)
        {
            using var paint = new SKPaint
            {
                Color = borderColor.ToSKColor(),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            if (_radius > 0)
            {
                using var path = new SKPath();
                path.AddRoundRect(rect, _radius * DPI, _radius * DPI);

                if (_border.All > 0)
                {
                    paint.StrokeWidth = _border.All;
                    canvas.DrawPath(path, paint);
                }
                else
                {
                    // Sol kenarlık
                    if (_border.Left > 0)
                    {
                        paint.StrokeWidth = _border.Left;
                        var left = new SKPath();
                        left.MoveTo(rect.Left + _radius * DPI, rect.Top);
                        left.LineTo(rect.Left + _radius * DPI, rect.Bottom);
                        canvas.DrawPath(left, paint);
                    }

                    // Üst kenarlık
                    if (_border.Top > 0)
                    {
                        paint.StrokeWidth = _border.Top;
                        var top = new SKPath();
                        top.MoveTo(rect.Left, rect.Top + _radius * DPI);
                        top.LineTo(rect.Right, rect.Top + _radius * DPI);
                        canvas.DrawPath(top, paint);
                    }

                    // Sağ kenarlık
                    if (_border.Right > 0)
                    {
                        paint.StrokeWidth = _border.Right;
                        var right = new SKPath();
                        right.MoveTo(rect.Right - _radius * DPI, rect.Top);
                        right.LineTo(rect.Right - _radius * DPI, rect.Bottom);
                        canvas.DrawPath(right, paint);
                    }

                    // Alt kenarlık
                    if (_border.Bottom > 0)
                    {
                        paint.StrokeWidth = _border.Bottom;
                        var bottom = new SKPath();
                        bottom.MoveTo(rect.Left, rect.Bottom - _radius * DPI);
                        bottom.LineTo(rect.Right, rect.Bottom - _radius * DPI);
                        canvas.DrawPath(bottom, paint);
                    }
                }
            }
            else
            {
                if (_border.All > 0)
                {
                    paint.StrokeWidth = _border.All;
                    canvas.DrawRect(rect, paint);
                }
                else
                {
                    // Sol kenarlık
                    if (_border.Left > 0)
                    {
                        paint.StrokeWidth = _border.Left;
                        canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Bottom, paint);
                    }

                    // Üst kenarlık
                    if (_border.Top > 0)
                    {
                        paint.StrokeWidth = _border.Top;
                        canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Top, paint);
                    }

                    // Sağ kenarlık
                    if (_border.Right > 0)
                    {
                        paint.StrokeWidth = _border.Right;
                        canvas.DrawLine(rect.Right, rect.Top, rect.Right, rect.Bottom, paint);
                    }

                    // Alt kenarlık
                    if (_border.Bottom > 0)
                    {
                        paint.StrokeWidth = _border.Bottom;
                        canvas.DrawLine(rect.Left, rect.Bottom, rect.Right, rect.Bottom, paint);
                    }
                }
            }
        }

        // Debug çerçevesi
        if (ColorScheme.DrawDebugBorders)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };
            canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);
        }
    }
}
