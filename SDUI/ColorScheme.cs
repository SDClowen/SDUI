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
        public static Color BorderColor => ForeColor.Alpha(40);
    }
}
