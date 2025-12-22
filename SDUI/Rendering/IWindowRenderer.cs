using SkiaSharp;
using System;

namespace SDUI.Rendering;

internal interface IWindowRenderer : IDisposable
{
    RenderBackend Backend { get; }

    void Initialize(nint hwnd);

    void Resize(int width, int height);

    void Render(int width, int height, Action<SKCanvas, SKImageInfo> draw);
}
