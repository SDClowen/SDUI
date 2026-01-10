using System.Drawing.Drawing2D;

namespace System.Drawing;

public static class ColorExtentions
{
    public static Color Determine(this Color color)
    {
        var value = 0;

        var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

        if (luminance > 0.5)
            value = 0; // bright colors - black font
        else
            value = 255; // dark colors - white font

        return Color.FromArgb(value, value, value);
    }

    /// <summary>
    ///     Creates color with corrected brightness.
    /// </summary>
    /// <param name="color">Color to correct.</param>
    /// <param name="correctionFactor">
    ///     The brightness correction factor. Must be between -1 and 1.
    ///     Negative values produce darker colors.
    /// </param>
    /// <returns>
    ///     Corrected <see cref="Color" /> structure.
    /// </returns>
    public static Color Brightness(this Color color, float correctionFactor)
    {
        // brightnessChange: -1.0 (dark) ... 0 (nothing change) ... +1.0 (light)
        RgbToHsl(color.R / 255f, color.G / 255f, color.B / 255f, out var h, out var s, out var l);

        l = Math.Clamp(l + correctionFactor, 0, 1);

        var rgb = HslToRgb(h, s, l);
        return Color.FromArgb(color.A, rgb.R, rgb.G, rgb.B);
    }

    // RGB (0–1) → HSL (0–1)
    private static void RgbToHsl(float r, float g, float b, out float h, out float s, out float l)
    {
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        l = (max + min) / 2;

        if (delta == 0)
        {
            h = s = 0;
        }
        else
        {
            s = l > 0.5f ? delta / (2 - max - min) : delta / (max + min);
            if (max == r) h = (g - b) / delta + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / delta + 2;
            else h = (r - g) / delta + 4;
            h /= 6;
        }
    }

    // HSL (0–1) → RGB (0–255)
    private static Color HslToRgb(float h, float s, float l)
    {
        float HueToRgb(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6) return p + (q - p) * 6 * t;
            if (t < 1f / 2) return q;
            if (t < 2f / 3) return p + (q - p) * (2f / 3 - t) * 6;
            return p;
        }

        if (s == 0)
        {
            var gray = (byte)Math.Round(l * 255);
            return Color.FromArgb(gray, gray, gray);
        }

        var q = l < 0.5f ? l * (1 + s) : l + s - l * s;
        var p = 2 * l - q;

        var r = HueToRgb(p, q, h + 1f / 3);
        var g = HueToRgb(p, q, h);
        var b = HueToRgb(p, q, h - 1f / 3);

        return Color.FromArgb(
            (byte)Math.Clamp(r * 255, 0, 255),
            (byte)Math.Clamp(g * 255, 0, 255),
            (byte)Math.Clamp(b * 255, 0, 255)
        );
    }

    /// <summary>
    ///     Is the color dark <c>true</c>; otherwise <c>false</c>
    /// </summary>
    /// <param name="color">The color</param>
    public static bool IsDark(this Color color)
    {
        return 384 - color.R - color.G - color.B > 0;
    }

    /// <summary>
    ///     Set alpha value for this color
    /// </summary>
    /// <param name="color">The color</param>
    /// <param name="alpha">The alpha</param>
    public static Color Alpha(this Color color, int alpha)
    {
        alpha = Math.Max(0, alpha);
        alpha = Math.Min(255, alpha);
        return Color.FromArgb(alpha, color);
    }

    /// <summary>
    ///     Removes the alpha component of a color.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static Color RemoveAlpha(this Color color)
    {
        return Color.FromArgb(color.R, color.G, color.B);
    }

    public static Color BlendWith(this Color backgroundColor, Color frontColor, double blend)
    {
        var ratio = blend / 255d;
        var invRatio = 1d - ratio;
        var r = (int)(backgroundColor.R * invRatio + frontColor.R * ratio);
        var g = (int)(backgroundColor.G * invRatio + frontColor.G * ratio);
        var b = (int)(backgroundColor.B * invRatio + frontColor.B * ratio);
        return Color.FromArgb(Math.Abs(frontColor.A - backgroundColor.A), r, g, b);
    }

    public static SolidBrush Brush(this Color color)
    {
        return new SolidBrush(color);
    }

    public static Pen Pen(this Color color, float size = 1)
    {
        return new Pen(color, size);
    }

    public static SolidBrush Brush(this string htmlColor, int alpha = 255)
    {
        return new SolidBrush(Color.FromArgb(alpha > 255 ? 255 : alpha, ColorTranslator.FromHtml(htmlColor)));
    }

    public static Pen Pen(this string htmlColor, int alpha = 255, float size = 1, LineCap startCap = LineCap.Custom,
        LineCap endCap = LineCap.Custom)
    {
        return new Pen(Color.FromArgb(alpha > 255 ? 255 : alpha, ColorTranslator.FromHtml(htmlColor)), size)
            { StartCap = startCap, EndCap = endCap };
    }

    public static Brush GlowBrush(Color centerColor, Color[] surroundColor, PointF point, GraphicsPath gp,
        WrapMode wrapMode = WrapMode.Clamp)
    {
        return new PathGradientBrush(gp)
            { CenterColor = centerColor, SurroundColors = surroundColor, FocusScales = point, WrapMode = wrapMode };
    }

    public static Brush GlowBrush(Color centerColor, Color[] surroundColor, PointF[] point,
        WrapMode wrapMode = WrapMode.Clamp)
    {
        return new PathGradientBrush(point)
            { CenterColor = centerColor, SurroundColors = surroundColor, WrapMode = wrapMode };
    }
}