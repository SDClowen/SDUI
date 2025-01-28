using SkiaSharp;

namespace SDUI;

public class SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
{
    public SKSurface Surface => surface;
    public SKImageInfo ImageInfo => info;
}