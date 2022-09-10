using System.Drawing;

namespace SDUI
{
    public class ColorScheme
    {
        /// <summary>
        /// Gets or Sets to the theme back color
        /// </summary>
        public static Color BackColor = Color.White;

        /// <summary>
        /// Get Determined theme forecolor from backcolor
        /// </summary>
        public static Color ForeColor => BackColor.Determine();

        /// <summary>
        /// Gets theme border color
        /// </summary>
        public static Color BorderColor => ForeColor.Alpha(20);

        /// <summary>
        /// Gets theme back color 2
        /// </summary>
        public static Color BackColor2 => ForeColor.Alpha(10);

        /// <summary>
        /// Gets theme back color 2
        /// </summary>
        public static Color ShadowColor => ForeColor.Alpha(10);

        /// <summary>
        /// Gets or sets the debug borders
        /// </summary>
        public static bool DrawDebugBorders;
    }
}
