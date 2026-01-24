using SkiaSharp;
using System.Collections.Generic;

namespace SDUI.Helpers;

public static class TextRenderingHelper
{
    private static readonly SKFontManager _fontManager = SKFontManager.Default;
    private static readonly Dictionary<int, SKTypeface> _fallbackCache = new();
    
    private static readonly string[] _fallbackFonts = 
    {
        "Segoe UI Emoji",
        "Segoe UI Symbol", 
        "Arial Unicode MS",
        "Noto Sans",
        "Noto Color Emoji"
    };

    public static void DrawText(SKCanvas canvas, string? text, float x, float y, SKColor color)
    {
        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true
        };

        DrawText(canvas, text, x, y, SKTextAlign.Left, null, paint);
    }

    public static void DrawText(SKCanvas canvas, string? text, float x, float y, SKPaint paint)
    {
        DrawText(canvas, text, x, y, SKTextAlign.Left, null, paint);
    }

    public static void DrawText(SKCanvas canvas, string? text, float x, float y, SKFont font, SKPaint paint)
    {
        DrawText(canvas, text, x, y, SKTextAlign.Left, font, paint);
    }

    public static void DrawText(SKCanvas canvas, string? text, float x, float y, SKTextAlign alignment, SKPaint paint)
    {
        DrawText(canvas, text, x, y, alignment, null, paint);
    }

    public static void DrawText(SKCanvas canvas, string? text, float x, float y, SKTextAlign alignment, SKFont? font,
        SKPaint paint)
    {
        if (!ShouldRender(canvas, paint, text))
            return;

        using var disposableFont = font is null ? CreateFontFromPaint(paint) : null;
        var effectiveFont = font ?? disposableFont!;
        
        DrawTextWithFallback(canvas, text!, x, y, alignment, effectiveFont, paint);
    }
    
    private static void DrawTextWithFallback(SKCanvas canvas, string text, float x, float y, 
        SKTextAlign alignment, SKFont font, SKPaint paint)
    {
        var currentX = x;
        var runs = new List<(string text, SKTypeface typeface)>();
        var primaryTypeface = font.Typeface ?? SKTypeface.Default;
        
        var currentRun = "";
        var currentTypeface = primaryTypeface;
        
        foreach (var c in text)
        {
            var glyphId = primaryTypeface.GetGlyph(c);
            SKTypeface? requiredTypeface = null;
            
            if (glyphId == 0)
            {
                requiredTypeface = GetFallbackTypeface(c);
            }
            
            var charTypeface = requiredTypeface ?? primaryTypeface;
            
            if (charTypeface != currentTypeface)
            {
                if (currentRun.Length > 0)
                {
                    runs.Add((currentRun, currentTypeface));
                }
                currentRun = c.ToString();
                currentTypeface = charTypeface;
            }
            else
            {
                currentRun += c;
            }
        }
        
        if (currentRun.Length > 0)
        {
            runs.Add((currentRun, currentTypeface));
        }
        
        if (alignment == SKTextAlign.Center || alignment == SKTextAlign.Right)
        {
            var totalWidth = 0f;
            foreach (var run in runs)
            {
                using var runFont = new SKFont(run.typeface, font.Size)
                {
                    Edging = font.Edging,
                    Hinting = font.Hinting,
                    Subpixel = font.Subpixel
                };
                totalWidth += runFont.MeasureText(run.text);
            }
            
            if (alignment == SKTextAlign.Center)
                currentX -= totalWidth / 2;
            else if (alignment == SKTextAlign.Right)
                currentX -= totalWidth;
        }
        
        foreach (var run in runs)
        {
            using var runFont = new SKFont(run.typeface, font.Size)
            {
                Edging = font.Edging,
                Hinting = font.Hinting,
                Subpixel = font.Subpixel
            };
            canvas.DrawText(run.text, currentX, y, runFont, paint);
            currentX += runFont.MeasureText(run.text);
        }
    }
    
    private static SKTypeface GetFallbackTypeface(char c)
    {
        var codepoint = (int)c;
        
        if (_fallbackCache.TryGetValue(codepoint, out var cached))
            return cached;
        
        foreach (var fontFamily in _fallbackFonts)
        {
            var typeface = SKTypeface.FromFamilyName(fontFamily);
            if (typeface != null && typeface.GetGlyph(c) != 0)
            {
                _fallbackCache[codepoint] = typeface;
                return typeface;
            }
        }
        
        var matched = _fontManager.MatchCharacter(codepoint);
        if (matched != null)
        {
            _fallbackCache[codepoint] = matched;
            return matched;
        }
        
        return SKTypeface.Default;
    }

    private static SKFont CreateFontFromPaint(SKPaint paint)
    {
#pragma warning disable CS0618 // Access legacy paint font properties for backward compatibility
        var font = new SKFont(paint.Typeface ?? SKTypeface.Default, paint.TextSize);
#pragma warning restore CS0618
        font.Edging = SKFontEdging.SubpixelAntialias;
        font.Subpixel = true;
        font.Hinting = SKFontHinting.Full;
        return font;
    }

    private static bool ShouldRender(SKCanvas canvas, SKPaint paint, string? text)
    {
        return canvas is not null && paint is not null && !string.IsNullOrEmpty(text);
    }
}