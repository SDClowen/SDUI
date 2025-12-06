using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using SkiaSharp;
using static SDUI.NativeMethods;

namespace SDUI.Helpers
{
    public class FontManager
    {
        private static readonly PrivateFontCollection privateFontCollection = new();
        public static Font Inter = GetFont(Resources.InterFont, 9f, FontStyle.Regular);
        public static Font Segoe = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);

        public static SKTypeface InterTypeface { get; private set; }

        static FontManager()
        {
            using var data = SKData.CreateCopy(Resources.InterFont);
            InterTypeface = SKTypeface.FromData(data);
        }

        private static Font GetFont(byte[] fontResource, float size, FontStyle style)
        {
            int dataLength = fontResource.Length;
            IntPtr fontPtr = Marshal.AllocCoTaskMem(dataLength);
            Marshal.Copy(fontResource, 0, fontPtr, dataLength);

            uint cFonts = 0;
            AddFontMemResourceEx(fontPtr, (uint)fontResource.Length, IntPtr.Zero, ref cFonts);
            privateFontCollection.AddMemoryFont(fontPtr, dataLength);

            var family = privateFontCollection.Families.FirstOrDefault(p => p.IsStyleAvailable(style));

            return new(family, size, style, GraphicsUnit.Point);
        }

        public static SKTypeface GetSKTypeface(Font font)
        {
            if (font.Name == Inter.Name && InterTypeface != null)
            {
                return InterTypeface;
            }

            SKFontStyleWeight weight = font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            SKFontStyleSlant slant = font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;

            return SKTypeface.FromFamilyName(font.Name, weight, SKFontStyleWidth.Normal, slant);
        }
    }
}
