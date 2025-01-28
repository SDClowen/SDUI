using SDUI.Controls;
using SkiaSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Extensions;

public static class SKCanvasExtensions
{
    public static float PtToPx(this float pt, Control control) => (pt * 1.333f) * control.DeviceDpi / 96f;
    public static float PtToPx(this float pt, Controls.UIElementBase control) => (pt * 1.333f);

    public static SKPaint CreateTextPaint(this SKCanvas canvas, Font font, Color color, Control control, ContentAlignment alignment = ContentAlignment.MiddleCenter)
    {
        var paint = new SKPaint
        {
            Color = color.ToSKColor(),
            TextSize = font.Size.PtToPx(control),
            IsAntialias = true,
            TextAlign = alignment.ToSKTextAlign(),
            Typeface = SKTypeface.FromFamilyName(font.FontFamily.Name, SKFontStyle.Normal),
            SubpixelText = true,
            LcdRenderText = true
        };

        return paint;
    }
    public static SKPaint CreateTextPaint(this SKCanvas canvas, Font font, Color color, UIElementBase control, ContentAlignment alignment = ContentAlignment.MiddleCenter)
    {
        var paint = new SKPaint
        {
            Color = color.ToSKColor(),
            TextSize = font.Size.PtToPx(control),
            IsAntialias = true,
            TextAlign = alignment.ToSKTextAlign(),
            Typeface = SKTypeface.FromFamilyName(font.FontFamily.Name, SKFontStyle.Normal),
            SubpixelText = true,
            LcdRenderText = true
        };

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

    public static void DrawTextWithEllipsis(this SKCanvas canvas, string text, SKPaint paint, float x, float y, float maxWidth)
    {
        var displayText = text;
        if (paint.MeasureText(text) > maxWidth)
        {
            while (paint.MeasureText(displayText) > maxWidth && displayText.Length > 3)
            {
                displayText = displayText[..^4] + "...";
            }
        }
        canvas.DrawText(displayText, x, y, paint);
    }

    public static void DrawTextWithMnemonic(this SKCanvas canvas, string text, SKPaint paint, float x, float y)
    {
        if (!text.Contains('&'))
        {
            canvas.DrawText(text, x, y, paint);
            return;
        }

        var parts = text.Split('&');
        float currentX = x;
        var originalAlign = paint.TextAlign;
        paint.TextAlign = SKTextAlign.Left;

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
                canvas.DrawText(parts[i], currentX, y, paint);
                currentX += paint.MeasureText(parts[i]);
            }
        }

        paint.TextAlign = originalAlign;
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