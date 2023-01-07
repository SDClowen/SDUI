using System.Drawing;

public static class IntegerExtensions
{
    /// <summary>
    /// Convert an color to integer number
    /// </summary>
    /// <returns></returns>
    public static uint ToAbgr(this Color color)
    {
        return ((uint)color.A << 24)
            | ((uint)color.B << 16)
            | ((uint)color.G << 8)
            | color.R;
    }

    /// <summary>
    /// Convert an integer number to a Color.
    /// </summary>
    /// <returns></returns>
    public static Color ToColor(this int argb)
    {
        return Color.FromArgb(
            (argb & 0xff0000) >> 16,
            (argb & 0xff00) >> 8,
             argb & 0xff);
    }

    /// <summary>
    /// Converts a 0-100 integer to a 0-255 color component.
    /// </summary>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public static int PercentageToColorComponent(this int percentage)
    {
        return (int)((percentage / 100d) * 255d);
    }
}
