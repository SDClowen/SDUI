using SkiaSharp;
using System.Drawing;

namespace SDUI.Extensions;

public static class ColorExtensions
{
    public static SKColor ToSKColor(this Color color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    public static Color ToColor(this SKColor color)
    {
        return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    }

    public static SKColor InterpolateColor(this SKColor start, SKColor end, float progress)
    {
        byte r = (byte)(start.Red + (end.Red - start.Red) * progress);
        byte g = (byte)(start.Green + (end.Green - start.Green) * progress);
        byte b = (byte)(start.Blue + (end.Blue - start.Blue) * progress);
        byte a = (byte)(start.Alpha + (end.Alpha - start.Alpha) * progress);
        return new SKColor(r, g, b, a);
    }
}