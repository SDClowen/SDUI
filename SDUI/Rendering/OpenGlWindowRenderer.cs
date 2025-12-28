using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace SDUI.Rendering;

internal sealed class OpenGlWindowRenderer : IWindowRenderer, IGpuWindowRenderer
{
    public RenderBackend Backend => RenderBackend.OpenGL;

    public GRContext? GrContext => _grContext;

    public bool IsSkiaGpuActive => _grContext != null && _surface != null;

    private nint _hwnd;
    private nint _hdc;
    private nint _hglrc;

    private GRContext? _grContext;
    private GRGlInterface? _glInterface;

    private GRBackendRenderTarget? _renderTarget;
    private SKSurface? _surface;

    private int _width;
    private int _height;

    public void Initialize(nint hwnd)
    {
        _hwnd = hwnd;
        _hdc = GetDC(_hwnd);
        if (_hdc == 0)
            throw new InvalidOperationException("GetDC failed.");

        SetupPixelFormat(_hdc);

        _hglrc = wglCreateContext(_hdc);
        if (_hglrc == 0)
            throw new InvalidOperationException("wglCreateContext failed.");

        if (!wglMakeCurrent(_hdc, _hglrc))
            throw new InvalidOperationException("wglMakeCurrent failed.");

        // Reduce tearing/flicker by enabling VSync when supported.
        TryEnableVSync();

        _glInterface = GRGlInterface.Create();
        if (_glInterface == null)
            throw new InvalidOperationException("GRGlInterface.Create returned null.");

        _grContext = GRContext.CreateGl(_glInterface);
        if (_grContext == null)
            throw new InvalidOperationException("GRContext.CreateGl returned null.");
    }

    private static void TryEnableVSync()
    {
        try
        {
            var proc = wglGetProcAddress("wglSwapIntervalEXT");
            if (proc == 0)
                return;

            var swapInterval = Marshal.GetDelegateForFunctionPointer<wglSwapIntervalEXTDelegate>(proc);
            _ = swapInterval(1);
        }
        catch
        {
            // ignore
        }
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        if (_width == width && _height == height && _surface != null)
            return;

        _width = width;
        _height = height;

        _surface?.Dispose();
        _surface = null;
        _renderTarget?.Dispose();
        _renderTarget = null;

        if (_grContext == null)
            return;

        // Assume default framebuffer (0) and RGBA8.
        // SkiaSharp will handle coordinate mapping via surface origin.
        var framebufferInfo = new GRGlFramebufferInfo(0, 0x8058 /* GL_RGBA8 */);
        _renderTarget = new GRBackendRenderTarget(width, height, 0, 8, framebufferInfo);

        _surface = SKSurface.Create(
            _grContext,
            _renderTarget,
            GRSurfaceOrigin.BottomLeft,
            SKColorType.Rgba8888,
            SKColorSpace.CreateSrgb());

        if (_surface == null)
            throw new InvalidOperationException("SKSurface.Create (OpenGL) returned null.");
    }

    public void Render(int width, int height, Action<SKCanvas, SKImageInfo> draw)
    {
        if (_hdc == 0 || _hglrc == 0)
            return;

        if (!wglMakeCurrent(_hdc, _hglrc))
            return;

        Resize(width, height);
        if (_surface == null || _grContext == null)
            return;

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        var canvas = _surface.Canvas;

        draw(canvas, info);

        canvas.Flush();
        _grContext.Flush();
        _grContext.Submit();

        SwapBuffers(_hdc);
    }

    public void TrimCaches()
    {
        _grContext?.PurgeResources();
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _surface = null;

        _renderTarget?.Dispose();
        _renderTarget = null;

        _grContext?.Dispose();
        _grContext = null;

        _glInterface?.Dispose();
        _glInterface = null;

        if (_hglrc != 0)
        {
            wglMakeCurrent(0, 0);
            wglDeleteContext(_hglrc);
            _hglrc = 0;
        }

        if (_hdc != 0)
        {
            ReleaseDC(_hwnd, _hdc);
            _hdc = 0;
        }

        _hwnd = 0;
    }

    private static void SetupPixelFormat(nint hdc)
    {
        var pfd = new PIXELFORMATDESCRIPTOR
        {
            nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
            iPixelType = PFD_TYPE_RGBA,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
            iLayerType = PFD_MAIN_PLANE,
        };

        int pixelFormat = ChoosePixelFormat(hdc, ref pfd);
        if (pixelFormat == 0)
            throw new InvalidOperationException("ChoosePixelFormat failed.");

        if (!SetPixelFormat(hdc, pixelFormat, ref pfd))
            throw new InvalidOperationException("SetPixelFormat failed.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits;
        public byte cRedShift;
        public byte cGreenBits;
        public byte cGreenShift;
        public byte cBlueBits;
        public byte cBlueShift;
        public byte cAlphaBits;
        public byte cAlphaShift;
        public byte cAccumBits;
        public byte cAccumRedBits;
        public byte cAccumGreenBits;
        public byte cAccumBlueBits;
        public byte cAccumAlphaBits;
        public byte cDepthBits;
        public byte cStencilBits;
        public byte cAuxBuffers;
        public sbyte iLayerType;
        public byte bReserved;
        public uint dwLayerMask;
        public uint dwVisibleMask;
        public uint dwDamageMask;
    }

    private const uint PFD_DRAW_TO_WINDOW = 0x00000004;
    private const uint PFD_SUPPORT_OPENGL = 0x00000020;
    private const uint PFD_DOUBLEBUFFER = 0x00000001;
    private const byte PFD_TYPE_RGBA = 0;
    private const sbyte PFD_MAIN_PLANE = 0;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetDC(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int ReleaseDC(nint hWnd, nint hDC);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int ChoosePixelFormat(nint hdc, ref PIXELFORMATDESCRIPTOR ppfd);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool SetPixelFormat(nint hdc, int iPixelFormat, ref PIXELFORMATDESCRIPTOR ppfd);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool SwapBuffers(nint hdc);

    [DllImport("opengl32.dll", SetLastError = true)]
    private static extern nint wglCreateContext(nint hdc);

    [DllImport("opengl32.dll", SetLastError = true)]
    private static extern bool wglDeleteContext(nint hglrc);

    [DllImport("opengl32.dll", SetLastError = true)]
    private static extern bool wglMakeCurrent(nint hdc, nint hglrc);

    [DllImport("opengl32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern nint wglGetProcAddress(string lpszProc);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int wglSwapIntervalEXTDelegate(int interval);
}
