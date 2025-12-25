using SkiaSharp;
using System;
using System.Collections.Generic;
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
    private bool _swapChainIsGdiCompatible;

    public string? LastInitError { get; private set; }

    private GRContext? _grContext;
    private GRBackendRenderTarget? _grRenderTarget;
    private SKSurface? _grSurface;
    private ID3D11Texture2D? _backBuffer;
    private bool _useSkiaGpu;

    private bool _useCpuUploadPath;
    private int _cpuUploadWidth;
    private int _cpuUploadHeight;
    private ID3D11Texture2D? _cpuUploadTexture;
    private ID3D11Texture2D? _presentBackBuffer;

    private SKBitmap? _cacheBitmap;
    private SKSurface? _cacheSurface;
    private Bitmap? _gdiBitmap;

    private int _width;
    private int _height;

    public void Initialize(nint hwnd)
    {
        _hwnd = hwnd;

        LastInitError = null;

        // Create D3D11 device
        var flags = DeviceCreationFlags.BgraSupport;
        var featureLevels = new[]
        {
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
        };

        try
        {
            (_device, _context) = CreateDeviceWithFallback(flags, featureLevels);
        }
        catch (Exception ex)
        {
            LastInitError = ex.Message;
            throw;
        }

        _useSkiaGpu = TryInitializeSkiaD3D();

        try
        {
            _swapChain = CreateSwapChainForHwnd(width: 1, height: 1, gdiCompatible: !_useSkiaGpu);
            _swapChainIsGdiCompatible = !_useSkiaGpu;
        }
        catch (Exception ex)
        {
            LastInitError = ex.Message;
            throw;
        }

        Resize(1, 1);
    }

    private IDXGISwapChain1 CreateSwapChainForHwnd(int width, int height, bool gdiCompatible)
    {
        if (_device == null)
            throw new InvalidOperationException("D3D device is not initialized.");
        if (_hwnd == 0)
            throw new InvalidOperationException("HWND is not initialized.");

        width = Math.Max(1, width);
        height = Math.Max(1, height);

        using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
        using var adapter = dxgiDevice.GetAdapter();
        using var factory = adapter.GetParent<IDXGIFactory2>();

        var desc = gdiCompatible
            ? new SwapChainDescription1
            {
                Width = width,
                Height = height,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                BufferUsage = Usage.RenderTargetOutput,
                // GDI-compatible swapchains must use the legacy "blt" model.
                // With SwapEffect.Discard, DXGI requires BufferCount == 1.
                BufferCount = 1,
                // Legacy swap effects use the blt model; Stretch is the safest value.
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.Discard,
                AlphaMode = AlphaMode.Ignore,
                Flags = SwapChainFlags.GdiCompatible,
            }
            : new SwapChainDescription1
            {
                Width = width,
                Height = height,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                BufferUsage = Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.None,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore,
                Flags = SwapChainFlags.None,
            };

        var sc = factory.CreateSwapChainForHwnd(_device, _hwnd, desc);
        factory.MakeWindowAssociation(_hwnd, WindowAssociationFlags.IgnoreAltEnter);
        return sc;
    }

    private bool EnsureCpuSwapChain(int width, int height)
    {
        if (_swapChain == null)
            return false;

        if (_swapChainIsGdiCompatible)
            return false;

        try
        {
            _gdiSurface?.Dispose();
            _gdiSurface = null;

            _swapChain?.Dispose();
            _swapChain = null;

            _swapChain = CreateSwapChainForHwnd(width, height, gdiCompatible: true);
            _swapChainIsGdiCompatible = true;

            CreateGdiResources(width, height);
            return true;
        }
        catch (Exception ex)
        {
            LastInitError = $"Failed to recreate CPU/GDI swapchain: {ex.Message}";
            throw;
        }
    }

    private void CreateGdiResources(int width, int height)
    {
        if (_swapChain == null)
            return;

        try
        {
            _gdiSurface = _swapChain.GetBuffer<IDXGISurface1>(0);
        }
        catch (Exception ex)
        {
            LastInitError = $"GetBuffer(IDXGISurface1) failed: {ex.Message}";
            throw;
        }

        CreateCpuCacheResources(width, height);
    }

    private void CreateCpuCacheResources(int width, int height)
    {
        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        _cacheBitmap = new SKBitmap(info);
        var pixels = _cacheBitmap.GetPixels();
        _cacheSurface = SKSurface.Create(info, pixels, _cacheBitmap.RowBytes);

        // Only needed for the legacy GDI blit path.
        _gdiBitmap = new Bitmap(width, height, _cacheBitmap.RowBytes, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, pixels);
    }

    private void EnsureCpuUploadResources(int width, int height)
    {
        if (_device == null || _context == null || _swapChain == null)
            throw new InvalidOperationException("D3D device/context/swapchain is not initialized.");

        if (_cpuUploadTexture != null && _presentBackBuffer != null && _cpuUploadWidth == width && _cpuUploadHeight == height)
            return;

        _presentBackBuffer?.Dispose();
        _presentBackBuffer = null;
        _cpuUploadTexture?.Dispose();
        _cpuUploadTexture = null;

        _presentBackBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);

        var desc = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Dynamic,
            BindFlags = BindFlags.None,
            CPUAccessFlags = CpuAccessFlags.Write,
            MiscFlags = ResourceOptionFlags.None,
        };

        _cpuUploadTexture = _device.CreateTexture2D(desc);
        _cpuUploadWidth = width;
        _cpuUploadHeight = height;
    }

    private unsafe void PresentFromCpuCacheViaD3D11Upload(int width, int height)
    {
        if (_context == null || _swapChain == null || _cacheBitmap == null)
            return;

        EnsureCpuUploadResources(width, height);
        if (_cpuUploadTexture == null || _presentBackBuffer == null)
            return;

        var mapped = _context.Map(_cpuUploadTexture, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
        try
        {
            var srcBase = (byte*)_cacheBitmap.GetPixels().ToPointer();
            var dstBase = (byte*)mapped.DataPointer.ToPointer();

            var srcStride = _cacheBitmap.RowBytes;
            var dstStride = mapped.RowPitch;
            var rowBytes = width * 4;

            for (var y = 0; y < height; y++)
            {
                Buffer.MemoryCopy(srcBase + (y * srcStride), dstBase + (y * dstStride), dstStride, rowBytes);
            }
        }
        finally
        {
            _context.Unmap(_cpuUploadTexture, 0);
        }

        _context.CopyResource(_presentBackBuffer, _cpuUploadTexture);
        _swapChain.Present(1, PresentFlags.None);
    }

    private static (ID3D11Device device, ID3D11DeviceContext context) CreateDeviceWithFallback(DeviceCreationFlags flags, FeatureLevel[] featureLevels)
    {
        var errors = new List<Exception>(capacity: 2);

        // Try real hardware first.
        if (TryCreateDevice(DriverType.Hardware, flags, featureLevels, out var device, out var context, out var error))
            return (device, context);
        if (error != null)
            errors.Add(error);

        // Common failure scenario: running over RDP / no proper GPU driver.
        if (TryCreateDevice(DriverType.Warp, flags, featureLevels, out device, out context, out error))
            return (device, context);
        if (error != null)
            errors.Add(error);

        // As a last resort, broaden feature levels and retry WARP.
        var wideFeatureLevels = featureLevels.Concat(new[]
        {
            FeatureLevel.Level_9_3,
            FeatureLevel.Level_9_2,
            FeatureLevel.Level_9_1,
        }).Distinct().ToArray();

        if (TryCreateDevice(DriverType.Warp, flags, wideFeatureLevels, out device, out context, out error))
            return (device, context);
        if (error != null)
            errors.Add(error);

        var message = "Failed to create a D3D11 device (Hardware and WARP).";
        if (errors.Count > 0)
            message += " Last error: " + errors[^1].Message;
        throw new InvalidOperationException(message, errors.Count > 0 ? errors[^1] : null);
    }

    private static bool TryCreateDevice(
        DriverType driverType,
        DeviceCreationFlags flags,
        FeatureLevel[] featureLevels,
        out ID3D11Device device,
        out ID3D11DeviceContext context,
        out Exception? error)
    {
        device = null!;
        context = null!;
        error = null;

        try
        {
            D3D11.D3D11CreateDevice(
                null,
                driverType,
                flags,
                featureLevels,
                out device,
                out context);
            return true;
        }
        catch (Exception ex)
        {
            error = ex;
            return false;
        }
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

        _presentBackBuffer?.Dispose();
        _presentBackBuffer = null;
        _cpuUploadTexture?.Dispose();
        _cpuUploadTexture = null;
        _cpuUploadWidth = 0;
        _cpuUploadHeight = 0;

        _cacheSurface?.Dispose();
        _cacheSurface = null;
        _cacheBitmap?.Dispose();
        _cacheBitmap = null;
        _gdiBitmap?.Dispose();
        _gdiBitmap = null;

        // If GPU init was attempted and later disabled, we may still have a flip-model swapchain.
        // The GDI blit path requires a GDI-compatible swapchain; recreate it if needed.
        if (!_useSkiaGpu && EnsureCpuSwapChain(width, height))
            return;

        try
        {
            // ResizeBuffers flags must be 0; the GDI-compatible flag is specified at creation time.
            _swapChain.ResizeBuffers(0, width, height, Format.Unknown, SwapChainFlags.None);
        }
        catch (Exception ex)
        {
            LastInitError = $"ResizeBuffers failed: {ex.Message}";
            throw;
        }

        if (_useSkiaGpu && _grContext != null)
        {
            TryCreateSkiaBackBufferSurface(width, height);
            return;
        }

        // Prefer the legacy GDI blit path unless it fails at runtime.
        // If we already switched to upload mode, do not require a GDI surface.
        if (_useCpuUploadPath)
        {
            CreateCpuCacheResources(width, height);
            return;
        }

        try
        {
            CreateGdiResources(width, height);
        }
        catch (Exception ex)
        {
            // Some systems/drivers still fail the GDI swapchain surface path.
            // Switch to a CPU->D3D11 upload path to keep DirectX presenting.
            LastInitError = $"GDI surface init failed; switching to D3D upload: {ex.Message}";
            _useCpuUploadPath = true;
            _gdiSurface?.Dispose();
            _gdiSurface = null;
            _gdiBitmap?.Dispose();
            _gdiBitmap = null;
            _cacheSurface?.Dispose();
            _cacheSurface = null;
            _cacheBitmap?.Dispose();
            _cacheBitmap = null;
            CreateCpuCacheResources(width, height);
        }
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
        catch (Exception ex)
        {
            // If anything fails, fall back to the CPU/GDI blit path for this session.
            LastInitError = $"Skia D3D backbuffer init failed: {ex.Message}";
            _useSkiaGpu = false;
            _grSurface?.Dispose();
            _grSurface = null;
            _grRenderTarget?.Dispose();
            _grRenderTarget = null;
            _backBuffer?.Dispose();
            _backBuffer = null;

            // Important: if the swapchain was created as flip-model for GPU, recreate it as GDI-compatible
            // before we ever call IDXGISurface1.GetDC.
            if (EnsureCpuSwapChain(width, height))
                return;
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

        if (_cacheSurface == null || _cacheBitmap == null)
            return;

        {
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            draw(_cacheSurface.Canvas, info);
            _cacheSurface.Canvas.Flush();

            if (_useCpuUploadPath)
            {
                try
                {
                    PresentFromCpuCacheViaD3D11Upload(width, height);
                }
                catch (Exception ex)
                {
                    LastInitError = $"D3D11 upload present failed: {ex.Message}";
                }

                return;
            }

            if (_gdiSurface == null || _gdiBitmap == null)
                return;

            try
            {
                nint hdc;
                try
                {
                    hdc = _gdiSurface.GetDC(true);
                }
                catch (Exception ex)
                {
                    // This is the observed failure (DXGI_ERROR_INVALID_CALL) on some drivers.
                    LastInitError = $"GetDC failed; switching to D3D upload: {ex.Message}";
                    _useCpuUploadPath = true;
                    PresentFromCpuCacheViaD3D11Upload(width, height);
                    return;
                }

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
            catch (Exception ex)
            {
                // Never crash the app; record the error and keep the session alive.
                LastInitError = $"DirectX11 CPU present failed: {ex.Message}";
            }
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

        _presentBackBuffer?.Dispose();
        _presentBackBuffer = null;

        _cpuUploadTexture?.Dispose();
        _cpuUploadTexture = null;

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
