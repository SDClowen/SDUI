using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ProgressBar : UIElementBase
{
    private long _value = 0;
    public long Value
    {
        get => _value;
        set
        {
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
            _maximum = value <= 0 ? 1 : value;
            Invalidate();
        }
    }

    private Color[] _gradient = new Color[2];
    public Color[] Gradient
    {
        get => _gradient;
        set
        {
            _gradient = value;
            Invalidate();
        }
    }

    private bool _showAsPercent = false;
    public bool ShowAsPercent
    {
        get => _showAsPercent;
        set
        {
            _showAsPercent = value;
            Invalidate();
        }
    }

    private int _percentIndices = 2;
    public int PercentIndices
    {
        get => _percentIndices;
        set
        {
            _percentIndices = value;
            Invalidate();
        }
    }

    private float _maxPercentShowValue = 100;
    public float MaxPercentShowValue
    {
        get => _maxPercentShowValue;
        set
        {
            _maxPercentShowValue = value;
            Invalidate();
        }
    }

    private bool _showValue = false;
    public bool ShowValue
    {
        get => _showValue;
        set
        {
            _showValue = value;
            Invalidate();
        }
    }

    private int _radius = 4;
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value <= 0 ? 1 : value;
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

    private HatchStyle _hatchType = HatchStyle.Percent10;
    public HatchStyle HatchType
    {
        get => _hatchType;
        set
        {
            _hatchType = HatchStyle.Percent10;
            Invalidate();
        }
    }

    public ProgressBar()
    {
        BackColor = Color.Transparent;
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var intValue = ((_value / (float)_maximum) * Width);
        var percent = ((100.0f * Value) / Maximum);

        var rect = new SKRect(0, 0, Width, Height);

        // Arka plan çizimi
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, paint);
        }

        // İlerleme çubuğu çizimi
        if (intValue > 0)
        {
            var progressRect = new SKRect(0, 0, intValue, Height);

            // Gradient oluştur
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, Height),
                new[] { _gradient[0].ToSKColor(), _gradient[1].ToSKColor() },
                null,
                SKShaderTileMode.Clamp);

            using (var paint = new SKPaint
            {
                Shader = shader,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRoundRect(progressRect, _radius, _radius, paint);
            }

            // Hatch pattern çizimi
            if (_drawHatch)
            {
                using var hatchPaint = new SKPaint
                {
                    Color = SKColors.White.WithAlpha(50),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1,
                    PathEffect = SKPathEffect.Create2DLine(2, new SKMatrix(1, 1, 0, -1, 1, 0, 0, 0, 1))
                };

                canvas.DrawRoundRect(progressRect, _radius, _radius, hatchPaint);
            }
        }

        // Çerçeve çizimi
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, paint);
        }

        // Değer metni çizimi
        if (ShowValue)
        {
            string text;
            if (_showAsPercent)
            {
                if (percent == 100)
                    percent = _maxPercentShowValue;

                text = percent.ToString($"0.{"0".PadRight(_percentIndices, '0')}") + "%";
            }
            else
            {
                text = $"{_value} / {_maximum}";
            }

            var textColor = percent > 50 ? SKColors.White : ColorScheme.ForeColor.ToSKColor();
            var shadowColor = percent > 50 ? SKColors.Black : ColorScheme.ForeColor.Determine().ToSKColor();

            // Gölge metni
            using (var paint = new SKPaint
            {
                Color = shadowColor,
                TextSize = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            })
            {
                var textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);
                canvas.DrawText(text, Width / 2f + 1, (Height + textBounds.Height) / 2f + 1, paint);
            }

            // Ana metin
            using (var paint = new SKPaint
            {
                Color = textColor,
                TextSize = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            })
            {
                var textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);
                canvas.DrawText(text, Width / 2f, (Height + textBounds.Height) / 2f, paint);
            }
        }
    }
}
