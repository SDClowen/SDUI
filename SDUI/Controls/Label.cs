using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Label : UIElementBase
{
    private bool _autoEllipsis;
    public bool AutoEllipsis
    {
        get => _autoEllipsis;
        set
        {
            _autoEllipsis = value;
            Invalidate();
        }
    }

    private bool _autoSize = false;
    public bool AutoSize
    {
        get => _autoSize;
        set
        {
            _autoSize = value;
            if (value) AdjustSize();
            Invalidate();
        }
    }

    private ContentAlignment _textAlign = ContentAlignment.TopLeft;
    public ContentAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            _textAlign = value;
            Invalidate();
        }
    }

    private bool _useMnemonic = true;
    public bool UseMnemonic
    {
        get => _useMnemonic;
        set
        {
            _useMnemonic = value;
            Invalidate();
        }
    }

    private BorderStyle _borderStyle = BorderStyle.None;
    public BorderStyle BorderStyle
    {
        get => _borderStyle;
        set
        {
            _borderStyle = value;
            Invalidate();
        }
    }

    public float Angle = 45;
    public bool ApplyGradient { get; set; }

    private bool _gradientAnimation;
    public bool GradientAnimation
    {
        get => _gradientAnimation;
        set
        {
            _gradientAnimation = value;
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

    public Label()
    {
        // Varsayılan gradient renkleri
        _gradient[0] = Color.Gray.ToSKColor();
        _gradient[1] = Color.Black.ToSKColor();
    }

    internal override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        if (AutoSize) AdjustSize();
    }

    internal override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (AutoSize) AdjustSize();
    }

    private void AdjustSize()
    {
        if (string.IsNullOrEmpty(Text)) return;

        using var font = new SKFont
        {
            Size = Font.Size.PtToPx(this),
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
        };

        var lines = Text.Split('\n');
        float maxWidth = 0;
        float totalHeight = 0;

        foreach (var line in lines)
        {
            SKRect bounds = new SKRect();
            font.MeasureText(line, out bounds);
            maxWidth = Math.Max(maxWidth, bounds.Width);
            totalHeight += bounds.Height;
        }

        Width = (int)Math.Ceiling(maxWidth) + Padding.Horizontal;
        Height = (int)Math.Ceiling(totalHeight) + Padding.Vertical;
    }

    private SKTextAlign GetSKTextAlign()
    {
        switch (TextAlign)
        {
            case ContentAlignment.TopCenter:
            case ContentAlignment.MiddleCenter:
            case ContentAlignment.BottomCenter:
                return SKTextAlign.Center;
            case ContentAlignment.TopRight:
            case ContentAlignment.MiddleRight:
            case ContentAlignment.BottomRight:
                return SKTextAlign.Right;
            default:
                return SKTextAlign.Left;
        }
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;

        if (GradientAnimation)
            Angle = Angle % 360 + 1;

        // Arka plan çizimi
        if (BackColor != Color.Transparent)
        {
            using var paint = new SKPaint
            {
                Color = BackColor.ToSKColor(),
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(0, 0, Width, Height, paint);
        }

        // Kenarlık çizimi
        if (BorderStyle != BorderStyle.None)
        {
            using var paint = new SKPaint
            {
                Color = ColorScheme.BorderColor.ToSKColor(),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            if (BorderStyle == BorderStyle.FixedSingle)
            {
                canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);
            }
            else if (BorderStyle == BorderStyle.Fixed3D)
            {
                // Üst ve sol kenar - açık renk
                paint.Color = Color.FromArgb(255, 240, 240, 240).ToSKColor();
                canvas.DrawLine(0, Height - 1, 0, 0, paint);
                canvas.DrawLine(0, 0, Width - 1, 0, paint);

                // Alt ve sağ kenar - koyu renk
                paint.Color = Color.FromArgb(255, 120, 120, 120).ToSKColor();
                canvas.DrawLine(0, Height - 1, Width - 1, Height - 1, paint);
                canvas.DrawLine(Width - 1, Height - 1, Width - 1, 0, paint);
            }
        }

        if (string.IsNullOrEmpty(Text)) return;

        // Metin çizimi için paint
        using var font = new SKFont
        {
            Size = Font.Size.PtToPx(this),
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
            Subpixel = true,
            Edging = SKFontEdging.SubpixelAntialias
        };

        using var textPaint = new SKPaint
        {
            IsAntialias = true
        };

        if (ApplyGradient)
        {
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint((float)(Width * Math.Cos(Angle * Math.PI / 180)),
                          (float)(Height * Math.Sin(Angle * Math.PI / 180))),
                _gradient,
                null,
                SKShaderTileMode.Clamp);

            textPaint.Shader = shader;
        }
        else
        {
            textPaint.Color = ColorScheme.ForeColor.ToSKColor();
        }

        var availableWidth = Width - Padding.Horizontal;
        var lineHeight = font.Spacing;
        var text = Text;
        var lines = new List<string>();

        // Metni satırlara böl
        while (!string.IsNullOrEmpty(text))
        {
            float measuredWidth;
            long count = font.BreakText(text, availableWidth, out measuredWidth);
            
            if (count == 0) break;

            var line = text.Substring(0, (int)count);
            if (AutoEllipsis && text.Length > count)
            {
                line = CreateEllipsisText(line, availableWidth, font);
                text = "";
            }
            else
            {
                text = text.Substring((int)count).TrimStart();
            }
            lines.Add(line);
        }

        // Dikey hizalama için y offset hesapla
        float yOffset = TextAlign switch
        {
            ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter or ContentAlignment.MiddleRight
                => (Height - (lines.Count * lineHeight)) / 2,
            ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight
                => Height - (lines.Count * lineHeight) - Padding.Bottom,
            _ => Padding.Top
        };

        var skTextAlign = GetSKTextAlign();
        // Yatay pozisyon hesapla
        float xPos = skTextAlign switch
        {
            SKTextAlign.Center => Width / 2,
            SKTextAlign.Right => Width - Padding.Right,
            _ => Padding.Left
        };

        // Her satırı çiz
        float baselineOffset = -font.Metrics.Ascent;
        for (int i = 0; i < lines.Count; i++)
        {
            TextRenderingHelper.DrawText(canvas, lines[i], xPos, yOffset + baselineOffset + (i * lineHeight), skTextAlign, font, textPaint);
        }
    }

    private string CreateEllipsisText(string text, float maxWidth, SKFont font)
    {
        const string ellipsis = "...";
        if (string.IsNullOrEmpty(text)) return text;

        var ellipsisWidth = font.MeasureText(ellipsis);
        if (font.MeasureText(text) <= maxWidth) return text;

        var length = text.Length;
        while (length > 0)
        {
            var truncated = text.Substring(0, length) + ellipsis;
            if (font.MeasureText(truncated) <= maxWidth)
                return truncated;
            length--;
        }

        return ellipsis;
    }
}