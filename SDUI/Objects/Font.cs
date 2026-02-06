using System;
using SkiaSharp;

namespace SDUI;

/// <summary>
/// A lightweight Skia-aware Font representation used by SDUI.
/// Designed to be independent from System.Drawing types so it can be used in rendering logic.
/// </summary>
public sealed class Font : IDisposable
{
    public string Name { get; }
    public float Size { get; }
    public bool Bold { get; }
    public bool Italic { get; }
    public SKFontStyle SkiaStyle { get; }
    public SKTypeface SKTypeface => SKTypeface.FromFamilyName(Name, SkiaStyle);

    public Font(string family, float size) : this(family, size, SKFontStyle.Normal)
    {
    }

    public Font(string family, float size, SKFontStyle skStyle)
    {
        Name = family ?? throw new ArgumentNullException(nameof(family));
        Size = size;
        SkiaStyle = skStyle;

        Bold = ((SKFontStyleWeight)skStyle.Weight) >= SKFontStyleWeight.Bold;
        Italic = skStyle.Slant == SKFontStyleSlant.Italic;
    }

    public Font(string family, float size, SKFontStyleWeight weight, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
        : this(family, size, new SKFontStyle(weight, SKFontStyleWidth.Normal, slant))
    {
        
    }

    /// <summary>
    /// Create from flags for simple consumers who don't have SKFontStyle ready.
    /// </summary>
    public Font(string family, float size, bool bold, bool italic)
        : this(family, size, new SKFontStyle(bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
            italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright))
    {
    }

    public override string ToString() => $"{Name}, {Size}pt{(Bold ? ", Bold" : "")}{(Italic ? ", Italic" : "")}";

    public void Dispose()
    {
        
    }
}

