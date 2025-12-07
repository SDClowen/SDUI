using SDUI.Controls;
using SDUI.Helpers;
using SkiaSharp;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Extensions;

public static class SKCanvasExtensions
{
    public static float PtToPx(this float pt, Control control) => (pt * 1.333f) * control.DeviceDpi / 96f;
    public static float PtToPx(this float pt, Controls.UIElementBase control) => (pt * 1.333f) * control.ScaleFactor;

    public static SKPaint CreateTextPaint(this SKCanvas canvas, Font font, Color color, Control control, ContentAlignment alignment = ContentAlignment.MiddleCenter)
    {
        var paint = new SKPaint
        {
            Color = color.ToSKColor(),
            TextSize = font.Size.PtToPx(control),
            TextAlign = alignment.ToSKTextAlign(),
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(font)
        };

        bool isHighDpi = control.DeviceDpi > 96;
        paint.IsAntialias = true;
        paint.FilterQuality = isHighDpi ? SKFilterQuality.Medium : SKFilterQuality.None;
        paint.SubpixelText = isHighDpi;
        paint.LcdRenderText = isHighDpi;
        paint.HintingLevel = isHighDpi ? SKPaintHinting.Full : SKPaintHinting.Normal;

        return paint;
    }
    public static SKPaint CreateTextPaint(this SKCanvas canvas, Font font, Color color, UIElementBase control, ContentAlignment alignment = ContentAlignment.MiddleCenter)
    {
        var paint = new SKPaint
        {
            Color = color.ToSKColor(),
            TextSize = font.Size.PtToPx(control),
            TextAlign = alignment.ToSKTextAlign(),
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(font)
        };

        bool isHighScale = control.ScaleFactor > 1.0f + 0.01f;
        paint.IsAntialias = true;
        paint.FilterQuality = isHighScale ? SKFilterQuality.Medium : SKFilterQuality.None;
        paint.SubpixelText = isHighScale;
        paint.LcdRenderText = isHighScale;
        paint.HintingLevel = isHighScale ? SKPaintHinting.Full : SKPaintHinting.Normal;

        return paint;
    }

    public static SKTextAlign ToSKTextAlign(this ContentAlignment alignment)
    {
        return alignment switch
        {
            ContentAlignment.TopLeft or ContentAlignment.MiddleLeft or ContentAlignment.BottomLeft => SKTextAlign.Left,
            ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => SKTextAlign.Right,
            _ => SKTextAlign.Center,
        };
    }

    public static void DrawControlText(this SKCanvas canvas, string text, SKRect bounds, SKPaint paint, SKFont font, ContentAlignment alignment, bool autoEllipsis = false, bool useMnemonic = false)
    {
        if (string.IsNullOrEmpty(text)) return;

        var skAlignment = alignment.ToSKTextAlign();

        // Calculate X
        float x = skAlignment switch
        {
            SKTextAlign.Center => bounds.MidX,
            SKTextAlign.Right => bounds.Right,
            _ => bounds.Left
        };

        // Calculate Y (Vertical Center)
        float y = bounds.MidY - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;

        // Adjust for Top/Bottom
        if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopRight)
            y = bounds.Top - font.Metrics.Ascent + 4;
        else if (alignment == ContentAlignment.BottomLeft || alignment == ContentAlignment.BottomCenter || alignment == ContentAlignment.BottomRight)
            y = bounds.Bottom - font.Metrics.Descent - 4;

        if (autoEllipsis)
        {
            canvas.DrawTextWithEllipsis(text, x, y, bounds.Width, paint, font, skAlignment);
        }
        else if (useMnemonic)
        {
            canvas.DrawTextWithMnemonic(text, x, y, paint, font, skAlignment);
        }
        else
        {
            TextRenderingHelper.DrawText(canvas, text, x, y, skAlignment, font, paint);
        }
    }

    public static void DrawTextWithEllipsis(this SKCanvas canvas, string text, float x, float y, float maxWidth, SKPaint paint, SKFont font, SKTextAlign textAlign = SKTextAlign.Left)
    {
        var displayText = text;
        if (font.MeasureText(text) > maxWidth)
        {
            while (font.MeasureText(displayText) > maxWidth && displayText.Length > 3)
            {
                displayText = displayText[..^4] + "...";
            }
        }
        TextRenderingHelper.DrawText(canvas, displayText, x, y, textAlign, font, paint);
    }

    public static void DrawTextWithEllipsis(this SKCanvas canvas, string text, SKPaint paint, float x, float y, float maxWidth)
    {
        var displayText = text;
#pragma warning disable CS0618 // Type or member is obsolete
        if (paint.MeasureText(text) > maxWidth)
        {
            while (paint.MeasureText(displayText) > maxWidth && displayText.Length > 3)
            {
                displayText = displayText[..^4] + "...";
            }
        }
        TextRenderingHelper.DrawText(canvas, displayText, x, y, paint);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public static void DrawTextWithMnemonic(this SKCanvas canvas, string text, float x, float y, SKPaint paint, SKFont font, SKTextAlign alignment = SKTextAlign.Left)
    {
        if (!text.Contains('&'))
        {
            TextRenderingHelper.DrawText(canvas, text, x, y, alignment, font, paint);
            return;
        }

        string cleanText = text.Replace("&", "");
        float totalWidth = font.MeasureText(cleanText);

        float startX = x;
        if (alignment == SKTextAlign.Center)
            startX = x - totalWidth / 2f;
        else if (alignment == SKTextAlign.Right)
            startX = x - totalWidth;

        var parts = text.Split('&');
        float currentX = startX;
        
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0 && parts[i].Length > 0)
            {
                var underlineWidth = font.MeasureText(parts[i][0].ToString());
                using var underlinePaint = new SKPaint
                {
                    Color = paint.Color,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1
                };
                canvas.DrawLine(currentX, y + 2, currentX + underlineWidth, y + 2, underlinePaint);
            }
            if (parts[i].Length > 0)
            {
                TextRenderingHelper.DrawText(canvas, parts[i], currentX, y, SKTextAlign.Left, font, paint);
                currentX += font.MeasureText(parts[i]);
            }
        }
    }

    public static void DrawTextWithMnemonic(this SKCanvas canvas, string text, SKPaint paint, float x, float y)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        if (!text.Contains('&'))
        {
            TextRenderingHelper.DrawText(canvas, text, x, y, paint);
            return;
        }

        var parts = text.Split('&');
        float currentX = x;

        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0 && parts[i].Length > 0)
            {
                var underlineWidth = paint.MeasureText(parts[i][0].ToString());
                using var underlinePaint = new SKPaint
                {
                    Color = paint.Color,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1
                };
                canvas.DrawLine(currentX, y + 2, currentX + underlineWidth, y + 2, underlinePaint);
            }
            if (parts[i].Length > 0)
            {
                TextRenderingHelper.DrawText(canvas, parts[i], currentX, y, paint);
                currentX += paint.MeasureText(parts[i]);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public static float GetTextY(this SKPaint paint, float height, ContentAlignment alignment)
    {
        var metrics = paint.FontMetrics;
        var textHeight = metrics.Descent - metrics.Ascent;

        return alignment switch
        {
            ContentAlignment.TopLeft or ContentAlignment.TopCenter or ContentAlignment.TopRight
                => -metrics.Ascent + 4,
            ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight
                => height - metrics.Descent - 4,
            _ => (height - textHeight) / 2 - metrics.Ascent
        };
    }

    public static float GetTextX(this SKPaint paint, float width, float textWidth, ContentAlignment alignment, bool hasImage = false)
    {
        return alignment switch
        {
            ContentAlignment.TopLeft or ContentAlignment.MiddleLeft or ContentAlignment.BottomLeft
                => hasImage ? 40 : 8,
            ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight
                => width - 8,
            _ => width / 2
        };
    }

    public static SizeF MeasureText(this SKPaint paint, string text)
    {
        var metrics = paint.FontMetrics;
        var width = paint.MeasureText(text);
        return new SizeF(width, metrics.Descent - metrics.Ascent);
    }
}