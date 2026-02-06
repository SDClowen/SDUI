using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SDUI.Controls;

public class Label : UIElementBase
{
    private readonly SKColor[] _gradient = new SKColor[2];
    private bool _autoEllipsis;

    private bool _autoSize;

    private bool _gradientAnimation;

    private ContentAlignment _textAlign = ContentAlignment.TopLeft;

    private bool _useMnemonic = true;

    public float Angle = 45;

    public Label()
    {
        _gradient[0] = SKColors.Gray;
        _gradient[1] = SKColors.Black;
    }

    public bool AutoEllipsis
    {
        get => _autoEllipsis;
        set
        {
            _autoEllipsis = value;
            Invalidate();
        }
    }

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

    public ContentAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            _textAlign = value;
            Invalidate();
        }
    }

    public bool UseMnemonic
    {
        get => _useMnemonic;
        set
        {
            _useMnemonic = value;
            Invalidate();
        }
    }

    public bool ApplyGradient { get; set; }

    public bool GradientAnimation
    {
        get => _gradientAnimation;
        set
        {
            _gradientAnimation = value;
            Invalidate();
        }
    }

    public SKColor[] Gradient
    {
        get => [_gradient[0], _gradient[1]];
        set
        {
            _gradient[0] = value[0];
            _gradient[1] = value[1];
            Invalidate();
        }
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
            Size = Font.Size.Topx(this),
            Typeface = FontManager.GetSKTypeface(Font),
            Subpixel = true,
            Edging = SKFontEdging.SubpixelAntialias
        };

        var lines = Text.Split('\n');
        float maxWidth = 0;

        foreach (var line in lines)
            // Measure advance width; more stable than bounds for layout.
            maxWidth = Math.Max(maxWidth, font.MeasureText(line));

        // Use line spacing (ascent+descent+leading) so the control height won't
        // under-estimate and clip descenders.
        var totalHeight = lines.Length * font.Spacing;

        // Add a small buffer (+2) to accommodate potential subpixel anti-aliasing overflow
        var hPadding = Padding.Horizontal + 2f * ScaleFactor;
        var vPadding = Padding.Vertical;
        Width = (int)Math.Ceiling(maxWidth + hPadding);
        Height = (int)Math.Ceiling(totalHeight + vPadding);
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

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        if (GradientAnimation)
            Angle = Angle % 360 + 1;

        // Arka plan çizimi
        if (BackColor != SKColors.Transparent)
        {
            using var paint = new SKPaint
            {
                Color = BackColor,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(0, 0, Width, Height, paint);
        }

        // Kenarlık çizimi
        if (BorderStyle != BorderStyle.None)
        {
            var strokeWidth = 1f * ScaleFactor;
            using var paint = new SKPaint
            {
                Color = ColorScheme.BorderColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                IsAntialias = true
            };

            if (BorderStyle == BorderStyle.FixedSingle)
            {
                canvas.DrawRect(0, 0, Width - strokeWidth, Height - strokeWidth, paint);
            }
            else if (BorderStyle == BorderStyle.Fixed3D)
            {
                // Üst ve sol kenar - açık renk
                paint.Color = new SKColor(255, 240, 240, 240);
                canvas.DrawLine(0, Height - 1, 0, 0, paint);
                canvas.DrawLine(0, 0, Width - 1, 0, paint);

                // Alt ve sağ kenar - koyu renk
                paint.Color = new SKColor(255, 120, 120, 120);
                canvas.DrawLine(0, Height - 1, Width - 1, Height - 1, paint);
                canvas.DrawLine(Width - 1, Height - 1, Width - 1, 0, paint);
            }
        }

        if (string.IsNullOrEmpty(Text)) return;

        // Metin çizimi için paint
        using var font = new SKFont
        {
            Size = Font.Size.Topx(this),
            Typeface = FontManager.GetSKTypeface(Font),
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
            textPaint.Color = ColorScheme.ForeColor;
        }

        var availableWidth = Width - Padding.Horizontal;
        var lineHeight = font.Spacing;
        
        // Escape karakterlerini işle
        var processedText = Text
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t");
        
        var lines = new List<string>();

        // Önce \n ile satırlara böl
        var textLines = processedText.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
        
        foreach (var textLine in textLines)
        {
            var remainingText = textLine;
            
            // Her satırı genişliğe göre böl
            while (!string.IsNullOrEmpty(remainingText))
            {
                float measuredWidth;
                long count = font.BreakText(remainingText, availableWidth, out measuredWidth);

                if (count == 0) 
                {
                    if (remainingText.Length > 0)
                    {
                        lines.Add(remainingText.Substring(0, 1));
                        remainingText = remainingText.Substring(1);
                    }
                    else
                    {
                        break;
                    }
                    continue;
                }

                var line = remainingText.Substring(0, (int)count);
                if (AutoEllipsis && remainingText.Length > count)
                {
                    line = CreateEllipsisText(line, availableWidth, font);
                    remainingText = "";
                }
                else
                {
                    remainingText = remainingText.Substring((int)count).TrimStart();
                }

                lines.Add(line);
            }
        }

        // Dikey hizalama: padding'li alan içinde hizala.
        var availableHeight = Height - Padding.Vertical;
        var textBlockHeight = lines.Count * lineHeight;
        var yOffset = TextAlign switch
        {
            ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter or ContentAlignment.MiddleRight
                => Padding.Top + Math.Max(0, (availableHeight - textBlockHeight) / 2),
            ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight
                => Height - Padding.Bottom - textBlockHeight,
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
        var baselineOffset = -font.Metrics.Ascent;
        for (var i = 0; i < lines.Count; i++)
            TextRenderingHelper.DrawText(canvas, lines[i], xPos, yOffset + baselineOffset + i * lineHeight, skTextAlign,
                font, textPaint);
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