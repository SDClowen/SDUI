using SkiaSharp;
using System;

namespace SDUI;

public static class ColorExtensions
{
    public static bool IsEmpty(this SKColor color)
    {
        return color == SKColors.Empty;
    }

    public static SKColor InterpolateColor(this SKColor start, SKColor end, float progress)
    {
        var r = (byte)(start.Red + (end.Red - start.Red) * progress);
        var g = (byte)(start.Green + (end.Green - start.Green) * progress);
        var b = (byte)(start.Blue + (end.Blue - start.Blue) * progress);
        var a = (byte)(start.Alpha + (end.Alpha - start.Alpha) * progress);
        return new SKColor(r, g, b, a);
    }

    public static SKColor Determine(this SKColor color)
    {
        var value = 0;

        var luminance = (0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue) / 255;

        if (luminance > 0.5)
            value = 0; // bright colors - black font
        else
            value = 255; // dark colors - white font

        return new SKColor((byte)value, (byte)value, (byte)value);
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
    public static SKColor Brightness(this SKColor color, float correctionFactor)
    {
        color.ToHsl(out var h, out var s, out var l);
        l = Math.Clamp(l + correctionFactor, 0, 1);

        return SKColor.FromHsl(h, s, l);
    }

    /// <summary>
    ///     Is the color dark <c>true</c>; otherwise <c>false</c>
    /// </summary>
    /// <param name="color">The color</param>
    public static bool IsDark(this SKColor color)
    {
        return 384 - color.Red - color.Green - color.Blue > 0;
    }

    /// <summary>
    ///     Removes the alpha component of a color.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static SKColor RemoveAlpha(this SKColor color)
    {
        color.WithAlpha(255);
        return color;
    }

    public static SKColor BlendWith(this SKColor backgroundColor, SKColor frontColor, double blend)
    {
        var ratio = blend / 255d;
        var invRatio = 1d - ratio;
        byte r = (byte)Math.Clamp((int)(backgroundColor.Red * invRatio + frontColor.Red * ratio),0, 255);
        byte g = (byte)Math.Clamp((int)(backgroundColor.Green * invRatio + frontColor.Green * ratio),0, 255);
        byte b = (byte)Math.Clamp((int)(backgroundColor.Blue * invRatio + frontColor.Blue * ratio),0, 255);
        byte a = (byte)Math.Clamp((int)Math.Abs(frontColor.Alpha - backgroundColor.Alpha), 0, 255);


        return new SKColor(r, g, b, a);
    }
}