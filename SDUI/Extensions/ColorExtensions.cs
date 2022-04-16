namespace System.Drawing
{
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
    }
}
