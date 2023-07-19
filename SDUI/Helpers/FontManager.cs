using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using static SDUI.NativeMethods;

namespace SDUI.Helpers
{
    public class FontManager
    {
        private static readonly PrivateFontCollection privateFontCollection = new();
        public static Font Inter = new(LoadFont(SDUI.Resources.InterFont), 13.3333f, FontStyle.Regular, GraphicsUnit.Pixel);

        private static FontFamily LoadFont(byte[] fontResource)
        {
            int dataLength = fontResource.Length;
            IntPtr fontPtr = Marshal.AllocCoTaskMem(dataLength);
            Marshal.Copy(fontResource, 0, fontPtr, dataLength);

            uint cFonts = 0;
            AddFontMemResourceEx(fontPtr, (uint)fontResource.Length, IntPtr.Zero, ref cFonts);
            privateFontCollection.AddMemoryFont(fontPtr, dataLength);

            return privateFontCollection.Families.Last();
        }
    }
}
