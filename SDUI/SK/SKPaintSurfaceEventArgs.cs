using SkiaSharp;

namespace SDUI.SK;

public class SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
{
    public SKSurface Surface => surface;
    public SKImageInfo ImageInfo => info;
}