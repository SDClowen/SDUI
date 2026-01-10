using SkiaSharp;

namespace SDUI.Rendering;

internal interface IGpuWindowRenderer
{
    GRContext? GrContext { get; }
}