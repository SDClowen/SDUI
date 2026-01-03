using SkiaSharp;

namespace SDUI.Helpers
{
    public static class TextRenderingHelper
    {
        public static void DrawText(SKCanvas canvas, string? text, float x, float y, SKColor color)
        {
            using var paint = new SKPaint
            {
                Color = color,
                IsAntialias = true,
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

        public static void DrawText(SKCanvas canvas, string? text, float x, float y, SKTextAlign alignment, SKFont? font, SKPaint paint)
        {
            if (!ShouldRender(canvas, paint, text))
                return;

            using var disposableFont = font is null ? CreateFontFromPaint(paint) : null;
            var effectiveFont = font ?? disposableFont!;
            canvas.DrawText(text, x, y, alignment, effectiveFont, paint);
        }

        private static SKFont CreateFontFromPaint(SKPaint paint)
        {
    #pragma warning disable CS0618 // Access legacy paint font properties for backward compatibility
            return new SKFont(paint.Typeface, paint.TextSize);
    #pragma warning restore CS0618
        }

        private static bool ShouldRender(SKCanvas canvas, SKPaint paint, string? text)
        {
            return canvas is not null && paint is not null && !string.IsNullOrEmpty(text);
        }
    }
}
