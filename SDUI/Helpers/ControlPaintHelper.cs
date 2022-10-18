using SDUI.Extensions;
using System.Drawing;

namespace SDUI.Helpers;

internal class ControlPaintHelper
{
    internal static void DrawShadow(Graphics graphics, Rectangle rect, float size, int radius, Color color = default)
    => DrawShadow(graphics, rect.ToRectangleF(), size, radius, color);

    internal static void DrawShadow(Graphics graphics, RectangleF rect, float size, int radius, Color color = default)
    {
        if (size <= 0)
            return;

        if(color == default)
            color = ColorScheme.ShadowColor;

        for (float i = 0; i < size; i++)
        {
            var v = i * (size / 2);
            using (var pen = new Pen(color.Alpha(color.A / ((int)i + 1)), 1))
            {
                var rectF = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
                rectF.Inflate(v / size, v / size);
                using (var rectPath = rectF.Radius(radius))
                    graphics.DrawPath(pen, rectPath);
            }
        }
    }
}
