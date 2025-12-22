using SkiaSharp;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SDUI.Rendering;

/// <summary>
/// DirectX11 presenter that uses a GDI-compatible swapchain.
/// Rendering is still done with the existing Skia software path into a bitmap,
/// then blitted onto the swapchain backbuffer via IDXGISurface1.GetDC.
/// </summary>
internal sealed class DirectX11WindowRenderer : IWindowRenderer, IGpuWindowRenderer
{
    public RenderBackend Backend => RenderBackend.DirectX11;

    // If SkiaSharp has a D3D backend in this build, we render directly to the swapchain backbuffer.
    // Otherwise we fall back to the existing CPU->GDI blit path.
    public GRContext? GrContext => _grContext;

    public bool IsSkiaGpuActive => _useSkiaGpu && _grContext != null && _grSurface != null;

    private nint _hwnd;

    private ID3D11Device? _device;
    private ID3D11DeviceContext? _context;
    private IDXGISwapChain1? _swapChain;
    private IDXGISurface1? _gdiSurface;

    private GRContext? _grContext;
    private GRBackendRenderTarget? _grRenderTarget;
    private SKSurface? _grSurface;
    private ID3D11Texture2D? _backBuffer;
    private bool _useSkiaGpu;

    private SKBitmap? _cacheBitmap;
    private SKSurface? _cacheSurface;
    private Bitmap? _gdiBitmap;

    private int _width;
    private int _height;

    public void Initialize(nint hwnd)
    {
        _hwnd = hwnd;

        // Create D3D11 device
        var flags = DeviceCreationFlags.BgraSupport;
        var featureLevels = new[]
        {
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
        };

        ID3D11Device device;
        ID3D11DeviceContext context;

        D3D11.D3D11CreateDevice(
            null,
            DriverType.Hardware,
            flags,
            featureLevels,
            out device,
            out context);

        _device = device;
        _context = context;

        using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
        using var adapter = dxgiDevice.GetAdapter();
        using var factory = adapter.GetParent<IDXGIFactory2>();

        _useSkiaGpu = TryInitializeSkiaD3D();

        var swapChainDesc = _useSkiaGpu
            ? new SwapChainDescription1
            {
                Width = 1,
                Height = 1,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                BufferUsage = Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.None,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore,
                Flags = SwapChainFlags.None,
            }
            : new SwapChainDescription1
            {
                Width = 1,
                Height = 1,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                BufferUsage = Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.None,
                SwapEffect = SwapEffect.Discard,
                AlphaMode = AlphaMode.Ignore,
                Flags = SwapChainFlags.GdiCompatible,
            };

        _swapChain = factory.CreateSwapChainForHwnd(_device, _hwnd, swapChainDesc);
        factory.MakeWindowAssociation(_hwnd, WindowAssociationFlags.IgnoreAltEnter);

        Resize(1, 1);
    }

    private bool TryInitializeSkiaD3D()
    {
        try
        {
            if (_device == null || _context == null)
                return false;

            // SkiaSharp D3D backend APIs are not available in all builds; use reflection.
            var skiaAsm = typeof(GRContext).Assembly;
            var backendContextType = skiaAsm.GetType("SkiaSharp.GRD3DBackendContext", throwOnError: false);
            if (backendContextType == null)
                return false;

            var backendContext = Activator.CreateInstance(backendContextType);
            if (backendContext == null)
                return false;

            var devicePtr = _device.NativePointer;
            var contextPtr = _context.NativePointer;

            SetPropertyIfExists(backendContext, "Device", devicePtr);
            SetPropertyIfExists(backendContext, "Context", contextPtr);
            SetPropertyIfExists(backendContext, "Queue", contextPtr);

            var create = typeof(GRContext).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "CreateDirect3D" && m.GetParameters().Length >= 1 && m.GetParameters()[0].ParameterType == backendContextType);

            if (create == null)
                return false;

            var gr = create.GetParameters().Length == 1
                ? (GRContext?)create.Invoke(null, new[] { backendContext })
                : (GRContext?)create.Invoke(null, new[] { backendContext, null });

            if (gr == null)
                return false;

            _grContext = gr;
            return true;
        }
        catch
        {
            _grContext = null;
            return false;
        }
    }

    private static void SetPropertyIfExists(object target, string name, nint value)
    {
        var prop = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null || !prop.CanWrite)
            return;

        try
        {
            if (prop.PropertyType == typeof(IntPtr) || prop.PropertyType == typeof(nint))
                prop.SetValue(target, (IntPtr)value);
            else if (prop.PropertyType == typeof(long))
                prop.SetValue(target, (long)value);
            else if (prop.PropertyType == typeof(ulong))
                prop.SetValue(target, (ulong)value);
        }
        catch
        {
            // ignore
        }
    }

    public void Resize(int width, int height)
    {
        if (_swapChain == null)
            return;

        width = Math.Max(1, width);
        height = Math.Max(1, height);

        if (_width == width && _height == height && _gdiSurface != null)
            return;

        _width = width;
        _height = height;

        _gdiSurface?.Dispose();
        _gdiSurface = null;

        _grSurface?.Dispose();
        _grSurface = null;
        _grRenderTarget?.Dispose();
        _grRenderTarget = null;
        _backBuffer?.Dispose();
        _backBuffer = null;

        _cacheSurface?.Dispose();
        _cacheSurface = null;
        _cacheBitmap?.Dispose();
        _cacheBitmap = null;
        _gdiBitmap?.Dispose();
        _gdiBitmap = null;

        _swapChain.ResizeBuffers(0, width, height, Format.Unknown, _useSkiaGpu ? SwapChainFlags.None : SwapChainFlags.GdiCompatible);

        if (_useSkiaGpu && _grContext != null)
        {
            TryCreateSkiaBackBufferSurface(width, height);
            return;
        }

        _gdiSurface = _swapChain.GetBuffer<IDXGISurface1>(0);

        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        _cacheBitmap = new SKBitmap(info);
        var pixels = _cacheBitmap.GetPixels();
        _cacheSurface = SKSurface.Create(info, pixels, _cacheBitmap.RowBytes);
        _gdiBitmap = new Bitmap(width, height, _cacheBitmap.RowBytes, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, pixels);
    }

    private void TryCreateSkiaBackBufferSurface(int width, int height)
    {
        try
        {
            if (_swapChain == null || _grContext == null)
                return;

            _backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
            var skiaAsm = typeof(GRContext).Assembly;
            var texInfoType = skiaAsm.GetType("SkiaSharp.GRD3DTextureInfo", throwOnError: false);
            if (texInfoType == null)
                throw new MissingMemberException("SkiaSharp.GRD3DTextureInfo not found.");

            var texInfo = Activator.CreateInstance(texInfoType) ?? throw new InvalidOperationException("Failed to create GRD3DTextureInfo.");
            SetPropertyIfExists(texInfo, "Texture", _backBuffer.NativePointer);
            SetPropertyIfExists(texInfo, "Resource", _backBuffer.NativePointer);

            // Some Skia builds require a DXGI format value.
            var formatProp = texInfoType.GetProperty("Format", BindingFlags.Public | BindingFlags.Instance);
            if (formatProp != null && formatProp.CanWrite)
            {
                try
                {
                    var dxgiFormat = (int)Format.B8G8R8A8_UNorm;
                    if (formatProp.PropertyType == typeof(int)) formatProp.SetValue(texInfo, dxgiFormat);
                    else if (formatProp.PropertyType == typeof(uint)) formatProp.SetValue(texInfo, (uint)dxgiFormat);
                }
                catch { }
            }

            // Create a backend render target wrapping the swapchain buffer.
            var ctor = typeof(GRBackendRenderTarget).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 5 && p[0].ParameterType == typeof(int) && p[1].ParameterType == typeof(int) && p[4].ParameterType == texInfoType;
                });

            if (ctor == null)
                throw new MissingMemberException("GRBackendRenderTarget ctor(width,height,samples,stencil,GRD3DTextureInfo) not found.");

            _grRenderTarget = (GRBackendRenderTarget)ctor.Invoke(new object[] { width, height, 1, 8, texInfo });
            _grSurface = SKSurface.Create(
                _grContext,
                _grRenderTarget,
                GRSurfaceOrigin.TopLeft,
                SKColorType.Bgra8888,
                SKColorSpace.CreateSrgb());
        }
        catch
        {
            // If anything fails, fall back to GDI-compatible path for this session.
            _useSkiaGpu = false;
            _grSurface?.Dispose();
            _grSurface = null;
            _grRenderTarget?.Dispose();
            _grRenderTarget = null;
            _backBuffer?.Dispose();
            _backBuffer = null;
        }
    }

    public void Render(int width, int height, Action<SKCanvas, SKImageInfo> draw)
    {
        if (_swapChain == null)
            return;

        Resize(width, height);

        if (_useSkiaGpu && _grSurface != null && _grContext != null)
        {
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            draw(_grSurface.Canvas, info);
            _grSurface.Canvas.Flush();
            _grContext.Flush();
            _grContext.Submit();
            _swapChain.Present(1, PresentFlags.None);
            return;
        }

        if (_gdiSurface == null || _cacheSurface == null || _cacheBitmap == null || _gdiBitmap == null)
            return;

        {
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            draw(_cacheSurface.Canvas, info);
            _cacheSurface.Canvas.Flush();

            var hdc = _gdiSurface.GetDC(true);
            try
            {
                using var gfx = Graphics.FromHdc(hdc);
                gfx.DrawImageUnscaled(_gdiBitmap, 0, 0);
            }
            finally
            {
                _gdiSurface.ReleaseDC(null);
            }

            _swapChain.Present(1, PresentFlags.None);
        }
    }

    public void Dispose()
    {
        _grSurface?.Dispose();
        _grSurface = null;

        _grRenderTarget?.Dispose();
        _grRenderTarget = null;

        _backBuffer?.Dispose();
        _backBuffer = null;

        _grContext?.Dispose();
        _grContext = null;

        _gdiBitmap?.Dispose();
        _gdiBitmap = null;

        _cacheSurface?.Dispose();
        _cacheSurface = null;

        _cacheBitmap?.Dispose();
        _cacheBitmap = null;

        _gdiSurface?.Dispose();
        _gdiSurface = null;

        _swapChain?.Dispose();
        _swapChain = null;

        _context?.Dispose();
        _context = null;

        _device?.Dispose();
        _device = null;

        _hwnd = 0;
    }
}
