using System;
using System.Drawing;

namespace SDUI;

public class ColorScheme
{
    private static Color _backColor = Color.White;

    /// <summary>
    /// Event fired when the theme color changes
    /// </summary>
    public static event EventHandler ThemeChanged;

    /// <summary>
    /// Gets or Sets to the theme back color
    /// </summary>
    public static Color BackColor
    {
        get => _backColor;
        set
        {
            if (_backColor != value)
            {
                _backColor = value;
                ThemeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Get Determined theme forecolor from backcolor
    /// </summary>
    public static Color ForeColor => BackColor.Determine();

    /// <summary>
    /// Gets theme border color
    /// </summary>
    public static Color BorderColor => ForeColor.Alpha(BackColor.IsDark() ? 10 : 40);

    /// <summary>
    /// Gets theme back color 2
    /// </summary>
    public static Color BackColor2 => ForeColor.Alpha(10);

    /// <summary>
    /// Gets theme back color 2
    /// </summary>
    public static Color ShadowColor => ForeColor.Alpha(10);

    /// <summary>
    /// Gets theme accent color
    /// </summary>
    public static Color AccentColor => Color.FromArgb(0, 92, 252);

    /// <summary>
    /// Gets or sets the debug borders
    /// </summary>
    public static bool DrawDebugBorders;
}
