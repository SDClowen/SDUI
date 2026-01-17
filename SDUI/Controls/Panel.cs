using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SkiaSharp;

namespace SDUI.Controls;

public class Panel : UIElementBase
{
    private Padding _border;

    private Color _borderColor = Color.Transparent;
    private int _radius = 10;

    private float _shadowDepth = 4;
    private float RadiusScaled => _radius * ScaleFactor;
    private float ShadowDepthScaled => _shadowDepth * ScaleFactor;

    public Panel()
    {
        BackColor = Color.Transparent;
    }

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

    public override void OnPaint(SKCanvas canvas)
    {
        var rect = new SKRect(0, 0, Width, Height);

        var color = BackColor == Color.Transparent ? ColorScheme.BackColor : BackColor;
        var borderColor = _borderColor == Color.Transparent ? ColorScheme.BorderColor : _borderColor;

        // Gölge çizimi
        if (_shadowDepth > 0)
        {
            using var shadowFilter = SKImageFilter.CreateDropShadow(
                ShadowDepthScaled,
                ShadowDepthScaled,
                0,
                0,
                SKColors.Black.WithAlpha(30));
            using var shadowPaint = new SKPaint
            {
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High,
                ImageFilter = shadowFilter
            };
            try
            {
                if (_radius > 0)
                {
                    using var path = new SKPath();
                    path.AddRoundRect(rect, RadiusScaled, RadiusScaled);
                    canvas.DrawPath(path, shadowPaint);
                }
                else
                {
                    canvas.DrawRect(rect, shadowPaint);
                }
            }
            catch
            {
            }
        }

        // Panel arka planı
        using var paint = new SKPaint
        {
            Color = color.ToSKColor(),
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        };
        try
        {
            if (_radius > 0)
            {
                using var path = new SKPath();
                path.AddRoundRect(rect, RadiusScaled, RadiusScaled);
                canvas.DrawPath(path, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }
        catch
        {
        }

        // Kenarlık çizimi
        if (_border.All > 0 || _border.Left > 0 || _border.Top > 0 || _border.Right > 0 || _border.Bottom > 0)
            try
            {
                paint.Color = borderColor.ToSKColor();
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1f * ScaleFactor;
                paint.IsAntialias = true;

                if (_radius > 0)
                {
                    using var path = new SKPath();
                    path.AddRoundRect(rect, RadiusScaled, RadiusScaled);

                    if (_border.All > 0)
                    {
                        paint.StrokeWidth = _border.All * ScaleFactor;
                        canvas.DrawPath(path, paint);
                    }
                    else
                    {
                        // Sol kenarlık
                        if (_border.Left > 0)
                        {
                            paint.StrokeWidth = _border.Left * ScaleFactor;
                            using var left = new SKPath();
                            left.MoveTo(rect.Left + RadiusScaled, rect.Top);
                            left.LineTo(rect.Left + RadiusScaled, rect.Bottom);
                            canvas.DrawPath(left, paint);
                        }

                        // Üst kenarlık
                        if (_border.Top > 0)
                        {
                            paint.StrokeWidth = _border.Top * ScaleFactor;
                            using var top = new SKPath();
                            top.MoveTo(rect.Left, rect.Top + RadiusScaled);
                            top.LineTo(rect.Right, rect.Top + RadiusScaled);
                            canvas.DrawPath(top, paint);
                        }

                        // Sağ kenarlık
                        if (_border.Right > 0)
                        {
                            paint.StrokeWidth = _border.Right * ScaleFactor;
                            using var right = new SKPath();
                            right.MoveTo(rect.Right - RadiusScaled, rect.Top);
                            right.LineTo(rect.Right - RadiusScaled, rect.Bottom);
                            canvas.DrawPath(right, paint);
                        }

                        // Alt kenarlık
                        if (_border.Bottom > 0)
                        {
                            paint.StrokeWidth = _border.Bottom * ScaleFactor;
                            using var bottom = new SKPath();
                            bottom.MoveTo(rect.Left, rect.Bottom - RadiusScaled);
                            bottom.LineTo(rect.Right, rect.Bottom - RadiusScaled);
                            canvas.DrawPath(bottom, paint);
                        }
                    }
                }
                else
                {
                    if (_border.All > 0)
                    {
                        paint.StrokeWidth = _border.All * ScaleFactor;
                        canvas.DrawRect(rect, paint);
                    }
                    else
                    {
                        // Sol kenarlık
                        if (_border.Left > 0)
                        {
                            paint.StrokeWidth = _border.Left * ScaleFactor;
                            canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Bottom, paint);
                        }

                        // Üst kenarlık
                        if (_border.Top > 0)
                        {
                            paint.StrokeWidth = _border.Top * ScaleFactor;
                            canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Top, paint);
                        }

                        // Sağ kenarlık
                        if (_border.Right > 0)
                        {
                            paint.StrokeWidth = _border.Right * ScaleFactor;
                            canvas.DrawLine(rect.Right, rect.Top, rect.Right, rect.Bottom, paint);
                        }

                        // Alt kenarlık
                        if (_border.Bottom > 0)
                        {
                            paint.StrokeWidth = _border.Bottom * ScaleFactor;
                            canvas.DrawLine(rect.Left, rect.Bottom, rect.Right, rect.Bottom, paint);
                        }
                    }
                }
            }
            catch
            {
            }

        // Debug çerçevesi
        if (ColorScheme.DrawDebugBorders)
            try
            {
                paint.Color = SKColors.Red;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1;
                paint.IsAntialias = true;
                canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);
            }
            catch
            {
            }

        // Alt elementleri render et
        base.OnPaint(canvas);
    }
}