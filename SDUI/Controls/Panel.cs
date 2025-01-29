using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace SDUI.Controls;

public class Panel : UIElementBase
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
        BackColor = Color.Transparent;
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var rect = new SKRect(0, 0, Width, Height);
        var color = BackColor == Color.Transparent ? ColorScheme.BackColor2 : BackColor;
        var borderColor = _borderColor == Color.Transparent ? ColorScheme.BorderColor : _borderColor;

        // Gölge çizimi
        if (_shadowDepth > 0)
        {
            var shadowPaint = GetPaintFromPool();
            try
            {
                shadowPaint.Color = SKColors.Black.WithAlpha(30);
                shadowPaint.ImageFilter = SKImageFilter.CreateDropShadow(
                    _shadowDepth,
                    _shadowDepth,
                    3,
                    3,
                    SKColors.Black.WithAlpha(30));
                shadowPaint.IsAntialias = true;

                if (_radius > 0)
                {
                    using var path = new SKPath();
                    path.AddRoundRect(rect, _radius * ScaleFactor, _radius * ScaleFactor);
                    canvas.DrawPath(path, shadowPaint);
                }
                else
                {
                    canvas.DrawRect(rect, shadowPaint);
                }
            }
            finally
            {
                ReturnPaintToPool(shadowPaint);
            }
        }

        // Panel arka planı
        var paint = GetPaintFromPool();
        try
        {
            paint.Color = color.ToSKColor();
            paint.IsAntialias = true;

            if (_radius > 0)
            {
                using var path = new SKPath();
                path.AddRoundRect(rect, _radius * ScaleFactor, _radius * ScaleFactor);
                canvas.DrawPath(path, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }
        finally
        {
            ReturnPaintToPool(paint);
        }

        // Kenarlık çizimi
        if (_border.All > 0 || _border.Left > 0 || _border.Top > 0 || _border.Right > 0 || _border.Bottom > 0)
        {
            paint = GetPaintFromPool();
            try
            {
                paint.Color = borderColor.ToSKColor();
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1;
                paint.IsAntialias = true;

                if (_radius > 0)
                {
                    using var path = new SKPath();
                    path.AddRoundRect(rect, _radius * ScaleFactor, _radius * ScaleFactor);

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
                            left.MoveTo(rect.Left + _radius * ScaleFactor, rect.Top);
                            left.LineTo(rect.Left + _radius * ScaleFactor, rect.Bottom);
                            canvas.DrawPath(left, paint);
                        }

                        // Üst kenarlık
                        if (_border.Top > 0)
                        {
                            paint.StrokeWidth = _border.Top;
                            var top = new SKPath();
                            top.MoveTo(rect.Left, rect.Top + _radius * ScaleFactor);
                            top.LineTo(rect.Right, rect.Top + _radius * ScaleFactor);
                            canvas.DrawPath(top, paint);
                        }

                        // Sağ kenarlık
                        if (_border.Right > 0)
                        {
                            paint.StrokeWidth = _border.Right;
                            var right = new SKPath();
                            right.MoveTo(rect.Right - _radius * ScaleFactor, rect.Top);
                            right.LineTo(rect.Right - _radius * ScaleFactor, rect.Bottom);
                            canvas.DrawPath(right, paint);
                        }

                        // Alt kenarlık
                        if (_border.Bottom > 0)
                        {
                            paint.StrokeWidth = _border.Bottom;
                            var bottom = new SKPath();
                            bottom.MoveTo(rect.Left, rect.Bottom - _radius * ScaleFactor);
                            bottom.LineTo(rect.Right, rect.Bottom - _radius * ScaleFactor);
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
            finally
            {
                ReturnPaintToPool(paint);
            }
        }

        // Debug çerçevesi
        if (ColorScheme.DrawDebugBorders)
        {
            paint = GetPaintFromPool();
            try
            {
                paint.Color = SKColors.Red;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1;
                paint.IsAntialias = true;
                canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);
            }
            finally
            {
                ReturnPaintToPool(paint);
            }
        }

        // Alt elementleri render et
        base.OnPaint(e);
    }
}
