using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SDUI.Controls;

public class ShapeProgressBar : UIElementBase
{
    private float _weight = 8;
    public float Weight
    {
        get => _weight;
        set
        {
            if (value < 0)
                value = 1;

            _weight = value;
            Invalidate();
        }
    }

    private long _value;
    public long Value
    {
        get => _value;
        set
        {
            if (value > _maximum)
                value = _maximum;

            _value = value;
            Invalidate();
        }
    }

    private long _maximum = 100;
    public long Maximum
    {
        get => _maximum;
        set
        {
            if (value < 1)
                value = 1;

            _maximum = value;
            Invalidate();
        }
    }

    private SKColor[] _gradient = new SKColor[2];
    public Color[] Gradient
    {
        get => new Color[] { _gradient[0].ToColor(), _gradient[1].ToColor() };
        set
        {
            _gradient[0] = value[0].ToSKColor();
            _gradient[1] = value[1].ToSKColor();
            Invalidate();
        }
    }

    private bool _drawHatch = false;
    public bool DrawHatch
    {
        get => _drawHatch;
        set
        {
            _drawHatch = value;
            Invalidate();
        }
    }

    private SKPaint _hatchPaint;
    private HatchStyle _hatchType = HatchStyle.Min;
    public HatchStyle HatchType
    {
        get => _hatchType;
        set
        {
            _hatchType = value;
            UpdateHatchPattern();
            Invalidate();
        }
    }

    private void UpdateHatchPattern()
    {
        _hatchPaint?.Dispose();

        _hatchPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _weight * 1.75f,
            Color = _gradient[0].WithAlpha(50),
            StrokeCap = SKStrokeCap.Round
        };

        // Pattern için küçük bir bitmap oluştur
        using (var surface = SKSurface.Create(new SKImageInfo(8, 8)))
        {
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            using (var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                switch (_hatchType)
                {
                    case HatchStyle.ForwardDiagonal:
                        canvas.DrawLine(0, 0, 8, 8, paint);
                        break;

                    case HatchStyle.BackwardDiagonal:
                        canvas.DrawLine(8, 0, 0, 8, paint);
                        break;

                    case HatchStyle.Cross:
                        canvas.DrawLine(4, 0, 4, 8, paint);
                        canvas.DrawLine(0, 4, 8, 4, paint);
                        break;

                    case HatchStyle.DiagonalCross:
                        canvas.DrawLine(0, 0, 8, 8, paint);
                        canvas.DrawLine(8, 0, 0, 8, paint);
                        break;

                    case HatchStyle.Horizontal:
                        canvas.DrawLine(0, 4, 8, 4, paint);
                        break;

                    case HatchStyle.Vertical:
                        canvas.DrawLine(4, 0, 4, 8, paint);
                        break;

                    default:
                        canvas.DrawLine(0, 0, 8, 8, paint);
                        break;
                }
            }

            using (var image = surface.Snapshot())
            using (var shader = SKShader.CreateImage(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat))
            {
                _hatchPaint.Shader = shader;
            }
        }
    }

    public ShapeProgressBar()
    {
        Size = new Size(100, 100);
        Font = new Font("Segoe UI", 15);
        BackColor = Color.Transparent;

        // Varsayılan gradient renkleri
        _gradient[0] = ColorScheme.AccentColor.ToSKColor();
        _gradient[1] = ColorScheme.AccentColor.ToSKColor().WithAlpha(200);

        UpdateHatchPattern();
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        // Antialiasing için yüksek kalite ayarı
        canvas.SetMatrix(SKMatrix.CreateScale(1.0f, 1.0f));

        var calc = (float)((360.0 / _maximum) * _value);

        // Merkez noktası ve boyutlar
        float centerX = Width / 2f;
        float centerY = Height / 2f;
        float size = Math.Min(Width, Height) - _weight - 2;
        float left = centerX - size / 2f;
        float top = centerY - size / 2f;

        var rect = new SKRect(left, top, left + size, top + size);

        // Ana gradient çizimi
        using (var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _weight,
            StrokeCap = SKStrokeCap.Round
        })
        {
            // Gradient shader'ı oluştur
            using (var shader = SKShader.CreateLinearGradient(
                new SKPoint(left, top),
                new SKPoint(left + size, top + size),
                new[] { _gradient[0], _gradient[1] },
                null,
                SKShaderTileMode.Clamp))
            {
                paint.Shader = shader;
                canvas.DrawArc(rect, -90, calc, false, paint);
            }
        }

        // Hatch pattern çizimi
        if (_drawHatch)
        {
            canvas.DrawArc(rect, -90, calc, false, _hatchPaint);
        }

        // İç daire çizimi
        float innerSize = size - _weight;
        float innerLeft = centerX - innerSize / 2f;
        float innerTop = centerY - innerSize / 2f;

        using (var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            Style = SKPaintStyle.Fill
        })
        {
            // İç daire için gradient
            using (var shader = SKShader.CreateLinearGradient(
                new SKPoint(innerLeft, innerTop),
                new SKPoint(innerLeft, innerTop + innerSize),
                new[] { ColorScheme.BackColor.ToSKColor(), ColorScheme.BackColor2.ToSKColor() },
                null,
                SKShaderTileMode.Clamp))
            {
                paint.Shader = shader;
                canvas.DrawCircle(centerX, centerY, innerSize / 2f, paint);
            }
        }

        // Yüzde metni çizimi
        var percent = (100 / (float)_maximum) * _value;
        var percentString = percent.ToString("0");

        using (var paint = new SKPaint
        {
            Color = ColorScheme.ForeColor.ToSKColor(),
            IsAntialias = true
        })
        using (var font = new SKFont
        {
            Size = Math.Min(Width, Height) * 0.2f,
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
            Subpixel = true
        })
        {
            var textBounds = new SKRect();
            font.MeasureText(percentString, out textBounds);
            canvas.DrawText(percentString, centerX, centerY + textBounds.Height / 3f, SKTextAlign.Center, font, paint);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hatchPaint?.Dispose();
        }
        base.Dispose(disposing);
    }
}