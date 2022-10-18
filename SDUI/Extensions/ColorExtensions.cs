namespace System.Drawing;

public static class ColorExtentions
{
    public static Color Determine(this Color color)
    {
        var value = 0;

        double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

        if (luminance > 0.5)
            value = 0; // bright colors - black font
        else
            value = 255; // dark colors - white font

        return Color.FromArgb(value, value, value);
    }

    /// <summary>
    /// Creates color with corrected brightness.
    /// </summary>
    /// <param name="color">Color to correct.</param>
    /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
    /// Negative values produce darker colors.</param>
    /// <returns>
    /// Corrected <see cref="Color"/> structure.
    /// </returns>
    public static Color Brightness(this Color color, float correctionFactor)
    {
        float red = (float)color.R;
        float green = (float)color.G;
        float blue = (float)color.B;

        if (correctionFactor < 0)
        {
            correctionFactor = 1 + correctionFactor;
            red *= correctionFactor;
            green *= correctionFactor;
            blue *= correctionFactor;
        }
        else
        {
            red = (255 - red) * correctionFactor + red;
            green = (255 - green) * correctionFactor + green;
            blue = (255 - blue) * correctionFactor + blue;
        }

        return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
    }

    /// <summary>
    /// Is the color dark <c>true</c>; otherwise <c>false</c>
    /// </summary>
    /// <param name="color">The color</param>
    public static bool IsDark(this Color color)
    {
        return (384 - color.R - color.G - color.B) > 0;
    }

    /// <summary>
    /// Set alpha value for this color
    /// </summary>
    /// <param name="color">The color</param>
    /// <param name="alpha">The alpha</param>
    public static Color Alpha(this Color color, int alpha)
    {
        alpha = Math.Max(0, alpha);
        alpha = Math.Min(255, alpha);
        return Color.FromArgb(alpha, color);
    }
}