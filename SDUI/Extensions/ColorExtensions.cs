using System.Drawing;
using SkiaSharp;

namespace SDUI;

public static class ColorExtensions
{
    public static SKColor ToSKColor(this Color color)
    {
        return (uint)color.ToArgb();
    }

    public static Color ToColor(this SKColor color)
    {
        return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    }

    public static SKColor InterpolateColor(this SKColor start, SKColor end, float progress)
    {
        var r = (byte)(start.Red + (end.Red - start.Red) * progress);
        var g = (byte)(start.Green + (end.Green - start.Green) * progress);
        var b = (byte)(start.Blue + (end.Blue - start.Blue) * progress);
        var a = (byte)(start.Alpha + (end.Alpha - start.Alpha) * progress);
        return new SKColor(r, g, b, a);
    }
}