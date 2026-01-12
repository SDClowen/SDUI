using SDUI.Controls;

namespace SDUI.Extensions
{
    public static class ValueExtensions
    {
        public static float Topx(this float pt, UIElementBase control)
        {
            return pt * 1.333f * control.ScaleFactor;
        }

        public static float Topx(this int pt, UIElementBase control)
        {
            return ((float)pt).Topx(control);
        }
        
        public static float Topx(this double pt, UIElementBase control)
        {
            return ((float)pt).Topx(control);
        }
    }
}
