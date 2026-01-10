using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using SkiaSharp;
using static SDUI.NativeMethods;

namespace SDUI.Helpers;

public class FontManager
{
    private static readonly PrivateFontCollection privateFontCollection = new();

    private static readonly
        ConcurrentDictionary<(string Family, SKFontStyleWeight Weight, SKFontStyleSlant Slant), SKTypeface>
        _typefaceCache = new();

    private static Font? _inter;
    public static Font Segoe = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);

    private static SKTypeface? _interTypeface;
    public static Font Inter => _inter ??= GetFont(Resources.InterFont, 9f, FontStyle.Regular);

    public static SKTypeface InterTypeface
    {
        get
        {
            if (_interTypeface == null)
                try
                {
                    using var data = SKData.CreateCopy(Resources.InterFont);
                    _interTypeface = SKTypeface.FromData(data);
                }
                catch
                {
                    // If Skia initialization fails, fall back to default typeface to avoid crashing the app during startup.
                    _interTypeface = SKTypeface.Default;
                }

            return _interTypeface;
        }
    }

    private static Font GetFont(byte[] fontResource, float size, FontStyle style)
    {
        var dataLength = fontResource.Length;
        var fontPtr = Marshal.AllocCoTaskMem(dataLength);
        Marshal.Copy(fontResource, 0, fontPtr, dataLength);

        uint cFonts = 0;
        AddFontMemResourceEx(fontPtr, (uint)fontResource.Length, IntPtr.Zero, ref cFonts);
        privateFontCollection.AddMemoryFont(fontPtr, dataLength);

        var family = privateFontCollection.Families.FirstOrDefault(p => p.IsStyleAvailable(style));

        return new Font(family, size, style, GraphicsUnit.Point);
    }

    public static SKTypeface GetSKTypeface(Font font)
    {
        if (font.Name == Inter.Name && InterTypeface != null) return InterTypeface;

        var weight = font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        var slant = font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;

        var key = (Family: font.Name, Weight: weight, Slant: slant);
        return _typefaceCache.GetOrAdd(key,
            k => SKTypeface.FromFamilyName(k.Family, k.Weight, SKFontStyleWidth.Normal, k.Slant));
    }

    public static SKTypeface GetSKTypeface(string family, SKFontStyleWeight weight = SKFontStyleWeight.Normal,
        SKFontStyleSlant slant = SKFontStyleSlant.Upright)
    {
        if (string.IsNullOrWhiteSpace(family))
            family = Segoe.Name;

        var key = (Family: family, Weight: weight, Slant: slant);
        return _typefaceCache.GetOrAdd(key,
            k => SKTypeface.FromFamilyName(k.Family, k.Weight, SKFontStyleWidth.Normal, k.Slant));
    }
}