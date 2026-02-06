using SkiaSharp;

namespace SDUI.Helpers;

public readonly struct TextRenderOptions
{
    public TextRenderOptions()
    {
        Wrap = TextWrap.None;
        Trimming = TextTrimming.None;
        Decoration = TextDecoration.None;
        MaxWidth = float.MaxValue;
        MaxHeight = float.MaxValue;
        LineSpacing = 1.2f;
        DecorationThickness = 1f;
        DecorationColor = SKColors.Transparent;
    }

    public TextWrap Wrap { get; init; }
    public TextTrimming Trimming { get; init; }
    public TextDecoration Decoration { get; init; }
    public float MaxWidth { get; init; }
    public float MaxHeight { get; init; }
    public float LineSpacing { get; init; }
    public float DecorationThickness { get; init; }
    public SKColor DecorationColor { get; init; }
}
