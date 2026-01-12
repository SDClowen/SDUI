using SDUI.Animation;
using SDUI.Collections;
using SDUI.Extensions;
using SDUI.Helpers;
using SDUI.Layout;
using SDUI.Rendering;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace SDUI.Controls;

public partial class UIWindow : UIWindowBase, IUIElement, IArrangedElement
{
    public enum TabDesingMode
    {
        Rectangle,
        Rounded,
        Chromed
    }

    private const int TAB_HEADER_PADDING = 9;
    private const int TAB_INDICATOR_HEIGHT = 3;

    private const float HOVER_ANIMATION_SPEED = 0.1f;
    private const float HOVER_ANIMATION_OPACITY = 0.4f;

    private const int WM_ERASEBKGND = 0x0014;

    // Hot-path caches (avoid per-frame LINQ allocations)
    private readonly List<UIElementBase> _frameElements = new();
    private readonly List<UIElementBase> _hitTestElements = new();
    private readonly Dictionary<string, SKPaint> _paintCache = new();
    private readonly object _softwareCacheLock = new();
    private readonly List<ZOrderSortItem> _zOrderSortBuffer = new();

    /// <summary>
    ///     Close tab hover animation manager
    /// </summary>
    private readonly AnimationManager closeBoxHoverAnimationManager;

    /// <summary>
    ///     Whether to display the control buttons of the form
    /// </summary>
    private readonly bool controlBox = true;

    /// <summary>
    ///     Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager extendBoxHoverAnimationManager;

    /// <summary>
    ///     tab area animation manager
    /// </summary>
    private readonly AnimationManager formMenuHoverAnimationManager;

    /// <summary>
    ///     Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager maxBoxHoverAnimationManager;

    /// <summary>
    ///     Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager minBoxHoverAnimationManager;

    /// <summary>
    ///     new Tab hover animation manager
    /// </summary>
    private readonly AnimationManager newTabHoverAnimationManager;

    /// <summary>
    ///     Tab Area hover animation manager
    /// </summary>
    private readonly AnimationManager pageAreaAnimationManager;

    /// <summary>
    ///     tab area animation manager
    /// </summary>
    private readonly AnimationManager tabCloseHoverAnimationManager;

    private SKBitmap _cacheBitmap;
    private SKSurface _cacheSurface;

    /// <summary>
    ///     The rectangle of extend box
    /// </summary>
    private RectangleF _closeTabBoxRect;

    /// <summary>
    ///     The control box left value
    /// </summary>
    private float _controlBoxLeft;

    /// <summary>
    ///     The rectangle of control box
    /// </summary>
    private RectangleF _controlBoxRect;

    private Cursor _currentCursor;

    private bool _drawTabIcons;

    /// <summary>
    ///     Whether to show the title bar of the form
    /// </summary>
    private bool _drawTitleBorder = true;

    private bool _extendBox;

    /// <summary>
    ///     The rectangle of extend box
    /// </summary>
    private RectangleF _extendBoxRect;

    private UIElementBase _focusedElement;

    /// <summary>
    ///     The rectangle of extend box
    /// </summary>
    private RectangleF _formMenuRect;

    /// <summary>
    ///     If the mouse down <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _formMoveMouseDown;

    /// <summary>
    ///     Gradient header colors
    /// </summary>
    private Color[] _gradient = new[] { Color.Transparent, Color.Transparent };

    private HatchStyle _hatch = HatchStyle.Percent80;

    private float _iconWidth = 42;

    private Timer? _idleMaintenanceTimer;

    private bool _inCloseBox, _inMaxBox, _inMinBox, _inExtendBox, _inTabCloseBox, _inNewTabBox, _inFormMenuBox;

    /// <summary>
    ///     Is form active <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _isActive;

    private UIElementBase _lastHoveredElement;

    private int _layoutSuspendCount;

    /// <summary>
    ///     The position of the form when the left mouse button is pressed
    /// </summary>
    private Point _location;

    /// <summary>
    ///     The position of the window before it is maximized
    /// </summary>
    private Point _locationOfBeforeMaximized;

    /// <summary>
    /// Gets or sets whether to display the control buttons of the form
    /// </summary>
    /*public new bool ControlBox
    {
        get => controlBox;
        set
        {
            controlBox = value;
            if (!controlBox)
            {
                MinimizeBox = MaximizeBox = false;
            }

            CalcSystemBoxPos();
            Invalidate();
        }
    }*/

    /// <summary>
    ///     Whether to show the maximize button of the form
    /// </summary>
    private bool _maximizeBox = true;

    /// <summary>
    ///     The rectangle of maximize box
    /// </summary>
    private RectangleF _maximizeBoxRect;

    private int _maxZOrder;

    /// <summary>
    ///     Whether to show the minimize button of the form
    /// </summary>
    private bool _minimizeBox = true;

    /// <summary>
    ///     The rectangle of minimize box
    /// </summary>
    private RectangleF _minimizeBoxRect;

    // Element that has explicitly captured mouse input (via SetMouseCapture)
    private UIElementBase? _mouseCapturedElement;

    /// <summary>
    ///     The position of the mouse when the left mouse button is pressed
    /// </summary>
    private Point _mouseOffset;

    private bool _needsFullRedraw = true;

    /// <summary>
    ///     The rectangle of extend box
    /// </summary>
    private RectangleF _newTabBoxRect;

    private bool _newTabButton;
    private long _perfLastTimestamp;
    private double _perfSmoothedFrameMs;

    private RenderBackend _renderBackend = RenderBackend.Software;
    private IWindowRenderer? _renderer;

    private bool _showPerfOverlay;

    /// <summary>
    ///     The size of the window before it is maximized
    /// </summary>
    private Size _sizeOfBeforeMaximized;

    // Prevent Invalidate()->Update() storms in Software backend
    private bool _softwareUpdateQueued;

    private long _stickyBorderTime = 5000000;
    private int _suppressImmediateUpdateCount;


    private float _symbolSize = 24;

    private bool _tabCloseButton;

    /// <summary>
    ///     Tab desing mode
    /// </summary>
    private TabDesingMode _tabDesingMode = TabDesingMode.Rectangle;

    /// <summary>
    ///     The title height
    /// </summary>
    private float _titleHeight = 32;

    private WindowPageControl _windowPageControl;
    private Point animationSource;

    /// <summary>
    ///     The title color
    /// </summary>
    private Color borderColor = Color.Transparent;

    /// <summary>
    ///     Whether to trigger the stay event on the edge of the display
    /// </summary>
    private bool IsStayAtTopBorder;

    private List<RectangleF> pageRect;

    private int previousSelectedPageIndex;

    /// <summary>
    ///     Whether to show the title bar of the form
    /// </summary>
    private bool showMenuInsteadOfIcon;

    /*
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new FormBorderStyle FormBorderStyle
    {
        get
        {
            return base.FormBorderStyle;
        }
        set
        {
            if (!Enum.IsDefined(typeof(FormBorderStyle), value))
                throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(FormBorderStyle));
            base.FormBorderStyle = FormBorderStyle.Sizable;
        }
    }*/

    /// <summary>
    ///     Whether to show the title bar of the form
    /// </summary>
    private bool showTitle = true;

    /// <summary>
    ///     The title color
    /// </summary>
    private Color titleColor;

    /// <summary>
    ///     The time at which the display edge dwell event was triggered
    /// </summary>
    private long TopBorderStayTicks;

    /// <summary>
    ///     The contructor
    /// </summary>
    public UIWindow()
    {
        CheckForIllegalCrossThreadCalls = false;

        // WinForms double-buffering can cause visible flicker when the window is presented by
        // a GPU swapchain (OpenGL/DX). Keep it for software, disable it for GPU backends.
        ApplyRenderStyles();
        Controls = new ElementCollection(this);
        enableFullDraggable = false;

        pageAreaAnimationManager = new AnimationManager
        {
            AnimationType = AnimationType.EaseOut,
            Increment = 0.07,
            Singular = true,
            InterruptAnimation = true
        };

        minBoxHoverAnimationManager = new AnimationManager
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        maxBoxHoverAnimationManager = new AnimationManager
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        closeBoxHoverAnimationManager = new AnimationManager
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        extendBoxHoverAnimationManager = new AnimationManager
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        tabCloseHoverAnimationManager = new AnimationManager
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        newTabHoverAnimationManager = new AnimationManager
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        formMenuHoverAnimationManager = new AnimationManager
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        minBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        maxBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        closeBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        extendBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        tabCloseHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        newTabHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        pageAreaAnimationManager.OnAnimationProgress += sender => Invalidate();
        formMenuHoverAnimationManager.OnAnimationProgress += sender => Invalidate();

        //WindowsHelper.ApplyRoundCorner(this.Handle);
    }

    private float _titleHeightDPI => _titleHeight * DPI;
    private float _iconWidthDPI => _iconWidth * DPI;
    private float _symbolSizeDPI => _symbolSize * DPI;

    [DefaultValue(42)]
    [Description("Gets or sets the header bar icon width")]
    public float IconWidth
    {
        get => _iconWidth;
        set
        {
            _iconWidth = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    [DefaultValue(true)]
    [Description("Gets or sets form can movable")]
    public bool Movable { get; set; } = true;

    [DefaultValue(false)] public bool AllowAddControlOnTitle { get; set; }

    [DefaultValue(false)]
    public bool ExtendBox
    {
        get => _extendBox;
        set
        {
            _extendBox = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    [DefaultValue(false)]
    public bool DrawTabIcons
    {
        get => _drawTabIcons;
        set
        {
            _drawTabIcons = value;
            Invalidate();
        }
    }

    [DefaultValue(false)]
    public bool TabCloseButton
    {
        get => _tabCloseButton;
        set
        {
            _tabCloseButton = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    [DefaultValue(false)]
    public bool NewTabButton
    {
        get => _newTabButton;
        set
        {
            _newTabButton = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    [DefaultValue(24)]
    public float ExtendSymbolSize
    {
        get => _symbolSize;
        set
        {
            _symbolSize = Math.Max(value, 16);
            _symbolSize = Math.Min(value, 128);
            Invalidate();
        }
    }

    [DefaultValue(null)] public ContextMenuStrip ExtendMenu { get; set; }

    [DefaultValue(null)] public ContextMenuStrip FormMenu { get; set; }

    /// <summary>
    ///     Gets or sets whether to show the title bar of the form
    /// </summary>
    public bool ShowTitle
    {
        get => showTitle;
        set
        {
            showTitle = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    /// <summary>
    ///     Gets or sets whether to show the title bar of the form
    /// </summary>
    public bool ShowMenuInsteadOfIcon
    {
        get => showMenuInsteadOfIcon;
        set
        {
            showMenuInsteadOfIcon = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    /// <summary>
    ///     Gets or sets whether to show the title bar of the form
    /// </summary>
    public bool DrawTitleBorder
    {
        get => _drawTitleBorder;
        set
        {
            _drawTitleBorder = value;
            Invalidate();
        }
    }

    /// <summary>
    ///     Whether to show the maximize button of the form
    /// </summary>
    public new bool MaximizeBox
    {
        get => _maximizeBox;
        set
        {
            _maximizeBox = value;

            if (value)
                _minimizeBox = true;

            CalcSystemBoxPos();
            Invalidate();
        }
    }

    /// <summary>
    ///     Whether to show the minimize button of the form
    /// </summary>
    public new bool MinimizeBox
    {
        get => _minimizeBox;
        set
        {
            _minimizeBox = value;

            if (!value)
                _maximizeBox = false;

            CalcSystemBoxPos();
            Invalidate();
        }
    }

    /// <summary>
    ///     Gets or sets the title height
    /// </summary>
    public float TitleHeight
    {
        get => _titleHeight;
        set
        {
            _titleHeight = Math.Max(value, 31);
            Invalidate();
            CalcSystemBoxPos();
        }
    }

    public Color[] Gradient
    {
        get => _gradient;
        set
        {
            _gradient = value;
            Invalidate();
        }
    }

    /// <summary>
    ///     Gets or sets the title color
    /// </summary>
    [Description("Title color")]
    [DefaultValue(typeof(Color), "224, 224, 224")]
    public Color TitleColor
    {
        get => titleColor;
        set
        {
            titleColor = value;
            Invalidate();
        }
    }

    /// <summary>
    ///     Gets or sets the title color
    /// </summary>
    [Description("Border Color")]
    [DefaultValue(typeof(Color), "Transparent")]
    public Color BorderColor
    {
        get => borderColor;
        set
        {
            borderColor = value;

            if (value != Color.Transparent)
                WindowsHelper.ApplyBorderColor(Handle, borderColor);

            Invalidate();
        }
    }

    public TabDesingMode TitleTabDesingMode
    {
        get => _tabDesingMode;
        set
        {
            if (_tabDesingMode == value)
                return;

            _tabDesingMode = value;
            Invalidate();
        }
    }

    /// <summary>
    ///     Draw hatch brush on form
    /// </summary>
    public bool FullDrawHatch { get; set; }

    public HatchStyle Hatch
    {
        get => _hatch;
        set
        {
            if (_hatch == value)
                return;

            _hatch = value;
            Invalidate();
        }
    }

    [Description("Set or get the maximum time to stay at the edge of the display(ms)")]
    [DefaultValue(500)]
    public long StickyBorderTime
    {
        get => _stickyBorderTime / 10000;
        set => _stickyBorderTime = value * 10000;
    }

    public new ContextMenuStrip ContextMenuStrip { get; set; }

    public SKSize CanvasSize =>
        _cacheBitmap == null ? SKSize.Empty : new SKSize(_cacheBitmap.Width, _cacheBitmap.Height);

    public UIElementBase FocusedElement
    {
        get => _focusedElement;
        set
        {
            if (_focusedElement == value) return;

            var oldFocus = _focusedElement;
            _focusedElement = value;

            if (oldFocus != null)
            {
                oldFocus.Focused = false;
                oldFocus.OnLostFocus(EventArgs.Empty);
                oldFocus.OnLeave(EventArgs.Empty);
            }

            if (_focusedElement != null)
            {
                _focusedElement.Focused = true;
                _focusedElement.OnGotFocus(EventArgs.Empty);
                _focusedElement.OnEnter(EventArgs.Empty);
            }
        }
    }

    public UIElementBase LastHoveredElement
    {
        get => _lastHoveredElement;
        internal set
        {
            if (_lastHoveredElement != value)
            {
                _lastHoveredElement = value;
                UpdateCursor(value);
            }
        }
    }

    public WindowPageControl WindowPageControl
    {
        get => _windowPageControl;
        set
        {
            _windowPageControl = value;
            if (_windowPageControl == null)
                return;

            previousSelectedPageIndex = _windowPageControl.SelectedIndex;

            _windowPageControl.SelectedIndexChanged += (sender, previousIndex) =>
            {
                previousSelectedPageIndex = previousIndex;
                pageAreaAnimationManager.SetProgress(0);
                pageAreaAnimationManager.StartNewAnimation(AnimationDirection.In);
            };
            _windowPageControl.ControlAdded += delegate { Invalidate(); };
            _windowPageControl.ControlRemoved += delegate { Invalidate(); };
        }
    }

    /// <summary>
    ///     Maximum retained bytes for the software backbuffer (SKBitmap + SKSurface + GDI Bitmap wrapper).
    ///     This prevents 4K/8K windows from permanently retaining very large pixel buffers.
    ///     Set to 0 (or less) to disable the limit (unlimited).
    /// </summary>
    public static long MaxSoftwareBackBufferBytes { get; set; } = 24L * 1024 * 1024;

    public static bool EnableIdleMaintenance { get; set; } = true;

    /// <summary>
    ///     Delay (ms) after the last repaint request before trimming retained backbuffers and
    ///     asking Skia to purge resource caches.
    /// </summary>
    public static int IdleMaintenanceDelayMs { get; set; } = 1500;

    public static bool PurgeSkiaResourceCacheOnIdle { get; set; } = true;

    [DefaultValue(false)]
    [Description("Shows a small FPS/frame-time overlay for measuring renderer performance.")]
    public bool ShowPerfOverlay
    {
        get => _showPerfOverlay;
        set
        {
            if (_showPerfOverlay == value)
                return;
            _showPerfOverlay = value;
            _perfLastTimestamp = 0;
            _perfSmoothedFrameMs = 0;
            Invalidate();
        }
    }

    [DefaultValue(RenderBackend.Software)]
    [Description(
        "Selects how UIWindow presents frames: Software (GDI), OpenGL, or DirectX11 (DXGI/GDI-compatible swapchain).")]
    public RenderBackend RenderBackend
    {
        get => _renderBackend;
        set
        {
            if (_renderBackend == value)
                return;

            _renderBackend = value;
            ApplyRenderStyles();
            RecreateRenderer();
            Invalidate();
        }
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;

            if (!DesignMode && _renderBackend != RenderBackend.Software)
            {
                cp.Style |= (int)(NativeMethods.SetWindowLongFlags.WS_CLIPCHILDREN |
                                  NativeMethods.SetWindowLongFlags.WS_CLIPSIBLINGS);
                // WS_EX_NOREDIRECTIONBITMAP helps some WGL/SwapBuffers flicker scenarios,
                // but can interfere with DXGI swapchains. Apply only for OpenGL.
                if (_renderBackend == RenderBackend.OpenGL)
                    cp.ExStyle |= (int)NativeMethods.SetWindowLongFlags.WS_EX_NOREDIRECTIONBITMAP;
                cp.ExStyle &= ~(int)NativeMethods.SetWindowLongFlags.WS_EX_COMPOSITED;
            }

            return cp;
        }
    }

    public new ElementCollection Controls { get; }

    // Explicit implementations to satisfy IUIElement interface contracts that differ from internal types
    IUIElement IUIElement.FocusedElement
    {
        get => _focusedElement;
        set => FocusedElement = value as UIElementBase;
    }

    int IUIElement.ZOrder { get; set; }

    void IUIElement.OnCreateControl()
    {
        // Forward to the Form's protected OnCreateControl
        OnCreateControl();
    }

    UIWindowBase IUIElement.GetParentWindow()
    {
        return this;
    }

    void IUIElement.EnsureLoadedRecursively()
    {
        foreach (var c in Controls)
            if (c is IUIElement el)
                el.EnsureLoadedRecursively();
    }

    void IUIElement.EnsureUnloadedRecursively()
    {
        foreach (var c in Controls)
            if (c is IUIElement el)
                el.EnsureUnloadedRecursively();
    }

    public new IUIElement Parent { get; set; }
    public int LayoutSuspendCount { get; set; }
    public bool _childControlsNeedAnchorLayout { get; set; }
    public bool _forceAnchorCalculations { get; set; }

    public new void Invalidate()
    {
        base.Invalidate();
        // Avoid synchronous Update() storms (especially with multiple animations). In software
        // backend we still want snappy repaint, but we coalesce to one Update per message loop.
        if (IsHandleCreated && !IsDisposed && !Disposing && _suppressImmediateUpdateCount <= 0)
            if (_renderBackend == RenderBackend.Software && ShouldForceSoftwareUpdate())
                QueueSoftwareUpdate();
    }

    void IUIElement.Render(SKCanvas canvas)
    {
        var w = ClientSize.Width;
        var h = ClientSize.Height;
        if (w <= 0 || h <= 0)
            return;
        var info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
        RenderScene(canvas, info);
    }

    public new void PerformLayout()
    {
        if (_layoutSuspendCount > 0)
            return;

        foreach (UIElementBase element in Controls)
            element.PerformLayout();

        Invalidate();
    }

    public new void ResumeLayout()
    {
        ResumeLayout(true);
    }

    public new void SuspendLayout()
    {
        _layoutSuspendCount++;
    }

    public new void ResumeLayout(bool performLayout)
    {
        if (_layoutSuspendCount > 0)
            _layoutSuspendCount--;

        if (performLayout && _layoutSuspendCount == 0)
        {
            _needsFullRedraw = true;
            PerformLayout();
            Invalidate();
        }
    }

    public void UpdateZOrder()
    {
    }

    /// <summary>
    ///     If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnFormMenuClick;

    /// <summary>
    ///     If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnExtendBoxClick;

    /// <summary>
    ///     If extend box clicked invoke the event
    /// </summary>
    public event EventHandler<int> OnCloseTabBoxClick;

    /// <summary>
    ///     If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnNewTabBoxClick;

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        _suppressImmediateUpdateCount++;
        try
        {
            base.OnDpiChanged(e);

            // Prefer WM_DPICHANGED suggested bounds; avoid manual scaling (can cause size oscillation).
            var suggested = e.SuggestedRectangle;
            if (Bounds != suggested) Bounds = suggested;

            // Invalidate measurements recursively before DPI notification
            InvalidateMeasureRecursive();

            foreach (UIElementBase element in Controls)
                element.OnDpiChanged(e.DeviceDpiNew, e.DeviceDpiOld);

            // Invalidate layout measurements on DPI change
            PerformLayout();

            _needsFullRedraw = true;
            Invalidate();
        }
        finally
        {
            _suppressImmediateUpdateCount--;
        }
    }

    private void InvalidateMeasureRecursive()
    {
        // Recursively invalidate all children
        foreach (UIElementBase child in Controls) InvalidateMeasureRecursiveInternal(child);
    }

    private static void InvalidateMeasureRecursiveInternal(UIElementBase element)
    {
        element.InvalidateMeasure();
        foreach (UIElementBase child in element.Controls) InvalidateMeasureRecursiveInternal(child);
    }

    protected internal override void SetMouseCapture(UIElementBase element)
    {
        _mouseCapturedElement = element;
        NativeMethods.SetCapture(Handle);
    }

    protected internal override void ReleaseMouseCapture(UIElementBase element)
    {
        if (_mouseCapturedElement == element)
        {
            _mouseCapturedElement = null;
            NativeMethods.ReleaseCapture();
        }
    }

    private bool ShouldForceSoftwareUpdate()
    {
        // Only force synchronous Update() when we need smooth, real-time visuals.
        // Otherwise let WinForms coalesce paints to keep idle CPU low.
        if (_suppressImmediateUpdateCount > 0)
            return false;

        if (_showPerfOverlay)
            return true;

        return pageAreaAnimationManager.Running
               || minBoxHoverAnimationManager.Running
               || maxBoxHoverAnimationManager.Running
               || closeBoxHoverAnimationManager.Running
               || extendBoxHoverAnimationManager.Running
               || tabCloseHoverAnimationManager.Running
               || newTabHoverAnimationManager.Running
               || formMenuHoverAnimationManager.Running;
    }

    private void EnsureIdleMaintenanceTimer()
    {
        if (!EnableIdleMaintenance)
            return;

        if (_idleMaintenanceTimer == null)
        {
            _idleMaintenanceTimer = new Timer();
            _idleMaintenanceTimer.Interval = IdleMaintenanceDelayMs;
            _idleMaintenanceTimer.Tick += IdleMaintenanceTimer_Tick;
        }
    }

    private void ArmIdleMaintenance()
    {
        if (!EnableIdleMaintenance)
            return;

        EnsureIdleMaintenanceTimer();
        if (_idleMaintenanceTimer != null)
        {
            _idleMaintenanceTimer.Stop();
            _idleMaintenanceTimer.Start();
        }
    }

    private void IdleMaintenanceTimer_Tick(object? sender, EventArgs e)
    {
        _idleMaintenanceTimer?.Stop();

        // 1. Trim renderer caches (DirectX / OpenGL)
        _renderer?.TrimCaches();

        // 2. Trim software backbuffer if using software rendering
        if (_renderer == null)
        {
            DisposeSoftwareBackBuffer();
            _needsFullRedraw = true;
        }

        // 3. Purge global Skia resource cache if requested
        if (PurgeSkiaResourceCacheOnIdle) SKGraphics.PurgeResourceCache();
    }

    private void QueueSoftwareUpdate()
    {
        if (_softwareUpdateQueued)
            return;

        if (!IsHandleCreated || IsDisposed || Disposing)
            return;

        _softwareUpdateQueued = true;
        try
        {
            BeginInvoke((Action)(() =>
            {
                _softwareUpdateQueued = false;
                if (!IsHandleCreated || IsDisposed || Disposing)
                    return;

                if (_renderBackend == RenderBackend.Software)
                    Update();
            }));
        }
        catch
        {
            _softwareUpdateQueued = false;
        }
    }

    private void StableSortByZOrderAscending(List<UIElementBase> list)
    {
        _zOrderSortBuffer.Clear();
        for (var i = 0; i < list.Count; i++)
        {
            var element = list[i];
            _zOrderSortBuffer.Add(new ZOrderSortItem(element, element.ZOrder, i));
        }

        _zOrderSortBuffer.Sort(static (a, b) =>
        {
            var cmp = a.ZOrder.CompareTo(b.ZOrder);
            return cmp != 0 ? cmp : a.Sequence.CompareTo(b.Sequence);
        });

        for (var i = 0; i < list.Count; i++)
            list[i] = _zOrderSortBuffer[i].Element;
    }

    private void StableSortByZOrderDescending(List<UIElementBase> list)
    {
        _zOrderSortBuffer.Clear();
        for (var i = 0; i < list.Count; i++)
        {
            var element = list[i];
            _zOrderSortBuffer.Add(new ZOrderSortItem(element, element.ZOrder, i));
        }

        _zOrderSortBuffer.Sort(static (a, b) =>
        {
            var cmp = b.ZOrder.CompareTo(a.ZOrder);
            return cmp != 0 ? cmp : a.Sequence.CompareTo(b.Sequence);
        });

        for (var i = 0; i < list.Count; i++)
            list[i] = _zOrderSortBuffer[i].Element;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyRenderStyles();
        RecreateRenderer();
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        try
        {
            _renderer?.Dispose();
        }
        finally
        {
            _renderer = null;
            base.OnHandleDestroyed(e);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (!DesignMode && _renderBackend != RenderBackend.Software)
            return;

        base.OnPaintBackground(e);
    }

    private void RecreateRenderer()
    {
        if (DesignMode || IsDisposed || Disposing)
            return;

        _renderer?.Dispose();
        _renderer = null;

        if (!IsHandleCreated)
            return;

        if (_renderBackend == RenderBackend.Software)
            return;

        try
        {
            _renderer = _renderBackend switch
            {
                RenderBackend.OpenGL => new OpenGlWindowRenderer(),
                RenderBackend.DirectX11 => new DirectX11WindowRenderer(),
                _ => null
            };

            _renderer?.Initialize(Handle);
            _renderer?.Resize(ClientSize.Width, ClientSize.Height);
        }
        catch
        {
            _renderer?.Dispose();
            _renderer = null;
            _renderBackend = RenderBackend.Software;
        }
    }

    private static MouseEventArgs CreateChildMouseEvent(MouseEventArgs source, UIElementBase element)
    {
        // Kaynak koordinat� pencere tabanl�; elementi pencere tabanl� dikd�rtgene �evir
        var elementWindowRect = GetWindowRelativeBoundsStatic(element);
        return new MouseEventArgs(
            source.Button,
            source.Clicks,
            source.X - elementWindowRect.X,
            source.Y - elementWindowRect.Y,
            source.Delta);
    }

    private static Rectangle GetWindowRelativeBoundsStatic(UIElementBase element)
    {
        if (element?.Parent == null)
            return new Rectangle(element?.Location ?? Point.Empty, element?.Size ?? Size.Empty);

        if (element.Parent is UIWindowBase window && !window.IsDisposed)
        {
            var screenLoc = element.PointToScreen(Point.Empty);
            var clientLoc = window.PointToClient(screenLoc);
            return new Rectangle(clientLoc, element.Size);
        }

        if (element.Parent is UIElementBase parentElement)
        {
            var screenLoc = element.PointToScreen(Point.Empty);
            // Pencereyi zincirden bul
            UIWindowBase parentWindow = null;
            var current = parentElement;
            while (current != null && parentWindow == null)
            {
                if (current.Parent is UIWindowBase w)
                {
                    parentWindow = w;
                    break;
                }

                current = current.Parent as UIElementBase;
            }

            if (parentWindow != null)
            {
                var clientLoc = parentWindow.PointToClient(screenLoc);
                return new Rectangle(clientLoc, element.Size);
            }
        }

        return new Rectangle(element.Location, element.Size);
    }

    private void CreateOrUpdateCache(SKImageInfo info)
    {
        if (info.Width <= 0 || info.Height <= 0)
        {
            DisposeSoftwareBackBuffer();
            return;
        }

        if (_cacheBitmap == null || _cacheBitmap.Width != info.Width || _cacheBitmap.Height != info.Height)
        {
            _cacheSurface?.Dispose();
            _cacheBitmap?.Dispose();

            _cacheBitmap = new SKBitmap(info);
            var pixels = _cacheBitmap.GetPixels();
            _cacheSurface = SKSurface.Create(info, pixels, _cacheBitmap.RowBytes);
            _needsFullRedraw = true;
        }
    }

    private static long EstimateBackBufferBytes(SKImageInfo info)
    {
        // Estimate: BGRA8888/RGBA8888 => 4 bytes per pixel
        var bytes = (long)info.Width * info.Height * 4;
        return bytes > 0 ? bytes : 0;
    }

    private static bool ShouldCacheSoftwareBackBuffer(SKImageInfo info)
    {
        var maxBytes = MaxSoftwareBackBufferBytes;
        if (maxBytes <= 0)
            return true;

        var estimated = EstimateBackBufferBytes(info);
        return estimated > 0 && estimated <= maxBytes;
    }

    private void DisposeSoftwareBackBuffer()
    {
        _cacheSurface?.Dispose();
        _cacheSurface = null;

        _cacheBitmap?.Dispose();
        _cacheBitmap = null;
    }

    private void RenderSoftwareFrameUncached(SKImageInfo info, Graphics graphics)
    {
        using var skBitmap = new SKBitmap(info);
        var pixels = skBitmap.GetPixels();

        using var surface = SKSurface.Create(info, pixels, skBitmap.RowBytes);
        if (surface == null)
            return;

        var canvas = surface.Canvas;
        RenderScene(canvas, info);
        canvas.Flush();
        surface.Flush();

        using var gdiBitmap =
            new Bitmap(info.Width, info.Height, skBitmap.RowBytes, PixelFormat.Format32bppPArgb, pixels);
        graphics.DrawImageUnscaled(gdiBitmap, 0, 0);
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);

        if (ShowTitle && !AllowAddControlOnTitle && e.Control.Top < TitleHeight) e.Control.Top = Padding.Top;
    }

    protected override void OnControlRemoved(ControlEventArgs e)
    {
        base.OnControlRemoved(e);
    }

    private void CalcSystemBoxPos()
    {
        _controlBoxLeft = Width;

        if (controlBox)
        {
            _controlBoxRect = new RectangleF(Width - _iconWidthDPI, 0, _iconWidthDPI, _titleHeightDPI);
            _controlBoxLeft = _controlBoxRect.Left - 2;

            if (MaximizeBox)
            {
                _maximizeBoxRect = new RectangleF(_controlBoxRect.Left - _iconWidthDPI, _controlBoxRect.Top,
                    _iconWidthDPI, _titleHeightDPI);
                _controlBoxLeft = _maximizeBoxRect.Left - 2;
            }
            else
            {
                _maximizeBoxRect = new RectangleF(Width + 1, Height + 1, 1, 1);
            }

            if (MinimizeBox)
            {
                _minimizeBoxRect =
                    new RectangleF(
                        MaximizeBox
                            ? _maximizeBoxRect.Left - _iconWidthDPI - 2
                            : _controlBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top, _iconWidthDPI,
                        _titleHeightDPI);
                _controlBoxLeft = _minimizeBoxRect.Left - 2;
            }
            else
            {
                _minimizeBoxRect = new Rectangle(Width + 1, Height + 1, 1, 1);
            }

            if (ExtendBox)
            {
                if (MinimizeBox)
                    _extendBoxRect = new RectangleF(_minimizeBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top,
                        _iconWidthDPI, _titleHeightDPI);
                else
                    _extendBoxRect = new RectangleF(_controlBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top,
                        _iconWidthDPI, _titleHeightDPI);
            }
        }
        else
        {
            _extendBoxRect = _maximizeBoxRect =
                _minimizeBoxRect = _controlBoxRect = new Rectangle(Width + 1, Height + 1, 1, 1);
        }

        var titleIconSize = 24 * DPI;
        _formMenuRect = new RectangleF(10, _titleHeightDPI / 2 - titleIconSize / 2, titleIconSize, titleIconSize);

        Padding = new Padding(Padding.Left, (int)(showTitle ? _titleHeightDPI : 0), Padding.Right, Padding.Bottom);
    }

    private void HandleTabKey(bool isShift)
    {
        var tabbableElements = Controls.OfType<UIElementBase>()
            .Where(e => e.Visible && e.Enabled && e.TabStop)
            .OrderBy(e => e.TabIndex)
            .ToList();

        if (tabbableElements.Count == 0) return;

        var currentIndex = _focusedElement != null ? tabbableElements.IndexOf(_focusedElement) : -1;

        if (isShift)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = tabbableElements.Count - 1;
        }
        else
        {
            currentIndex++;
            if (currentIndex >= tabbableElements.Count) currentIndex = 0;
        }

        FocusedElement = tabbableElements[currentIndex];
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // If the focused element is a TextBox that accepts tabs, treat Tab as input (not navigation).
        if (keyData == Keys.Tab && _focusedElement is TextBox tb && tb.AcceptsTab)
        {
            tb.OnKeyPress(new KeyPressEventArgs('\t'));
            return true;
        }

        if (keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift))
            if (FocusManager.ProcessKeyNavigation(new KeyEventArgs(keyData)))
                return true;

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_focusedElement != null) _focusedElement.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (_focusedElement != null) _focusedElement.OnKeyUp(e);
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);

        if (_focusedElement != null) _focusedElement.OnKeyPress(e);
    }

    private void BuildHitTestList(bool requireEnabled)
    {
        _hitTestElements.Clear();
        for (var i = 0; i < Controls.Count; i++)
        {
            if (Controls[i] is not UIElementBase element)
                continue;
            if (!element.Visible)
                continue;
            if (requireEnabled && !element.Enabled)
                continue;
            _hitTestElements.Add(element);
        }

        // Stable ordering prevents subtle behavior changes when ZOrder ties exist.
        StableSortByZOrderDescending(_hitTestElements);
    }


    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        BuildHitTestList(true);
        for (var i = 0; i < _hitTestElements.Count; i++)
        {
            var element = _hitTestElements[i];
            if (!GetWindowRelativeBoundsStatic(element).Contains(e.Location))
                continue;
            var localEvent = CreateChildMouseEvent(e, element);
            element.OnMouseClick(localEvent);
            break;
        }

        if (!ShowTitle)
            return;

        if (_inCloseBox)
        {
            _inCloseBox = false;
            Close();
        }

        if (_inMinBox)
        {
            _inMinBox = false;
            WindowState = FormWindowState.Minimized;
        }

        if (_inMaxBox)
        {
            _inMaxBox = false;
            ShowMaximize();
        }

        if (_inExtendBox)
        {
            _inExtendBox = false;
            if (ExtendMenu != null)
                ExtendMenu.Show(PointToScreen(new Point(Convert.ToInt32(_extendBoxRect.Left),
                    Convert.ToInt32(_titleHeightDPI - 1))));
            else
                OnExtendBoxClick?.Invoke(this, EventArgs.Empty);
        }

        if (_inFormMenuBox)
        {
            _inFormMenuBox = false;
            if (FormMenu != null)
                FormMenu.Show(PointToScreen(new Point(Convert.ToInt32(_formMenuRect.Left),
                    Convert.ToInt32(_titleHeightDPI - 1))));
            else
                OnFormMenuClick?.Invoke(this, EventArgs.Empty);
        }

        if (_inTabCloseBox)
        {
            _inTabCloseBox = false;

            OnCloseTabBoxClick?.Invoke(this, _windowPageControl.SelectedIndex);
        }

        if (_inNewTabBox)
        {
            _inNewTabBox = false;

            OnNewTabBoxClick?.Invoke(this, EventArgs.Empty);
        }

        if (pageRect == null)
            UpdateTabRects();

        if (_formMoveMouseDown && !MousePosition.Equals(_mouseOffset))
            return;

        for (var i = 0; i < pageRect.Count; i++)
            if (pageRect[i].Contains(e.Location))
            {
                _windowPageControl.SelectedIndex = i;

                if (_tabCloseButton && e.Button == MouseButtons.Middle)
                    OnCloseTabBoxClick?.Invoke(null, i);

                break;
            }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        // Make sure this Form receives keyboard input.
        if (CanFocus)
            Focus();

        var elementClicked = false;
        // Z-order'a g�re tersten kontrol et (�stteki elementten ba�la)
        BuildHitTestList(true);
        for (var i = 0; i < _hitTestElements.Count; i++)
        {
            var element = _hitTestElements[i];
            if (!GetWindowRelativeBoundsStatic(element).Contains(e.Location))
                continue;

            elementClicked = true;

            // If the element (or its descendants) doesn't set focus, fall back to focusing this element.
            var prevFocus = FocusedElement;

            var localEvent = CreateChildMouseEvent(e, element);
            element.OnMouseDown(localEvent);

            if (FocusedElement == prevFocus)
            {
                static bool IsDescendantOf(UIElementBase? maybeChild, UIElementBase ancestor)
                {
                    var current = maybeChild;
                    while (current != null)
                    {
                        if (ReferenceEquals(current, ancestor))
                            return true;
                        current = current.Parent as UIElementBase;
                    }

                    return false;
                }

                // If focus stayed on an existing descendant (common when clicking inside an already-focused TextBox),
                // don't steal focus back to the container.
                if (prevFocus == null || !IsDescendantOf(prevFocus, element))
                    FocusedElement = element;
            }

            // T�klanan elementi en �ste getir
            BringToFront(element);
            break; // �lk t�klanan elementten sonra di�erlerini kontrol etmeye gerek yok
        }

        if (!elementClicked)
        {
            FocusManager.SetFocus(null);
            FocusedElement = null;
        }

        if (enableFullDraggable && e.Button == MouseButtons.Left)
            //right = e.Button == MouseButtons.Right;
            //location = e.Location;
            DragForm(Handle);

        // NOTE: Window context menus should open on MouseUp (standard behavior).
        // Showing on MouseDown can lead to double menus when the mouse moves slightly
        // and an element handles right-click on MouseUp.

        if (_inCloseBox || _inMaxBox || _inMinBox || _inExtendBox || _inTabCloseBox || _inNewTabBox || _inFormMenuBox)
            return;

        if (!ShowTitle)
            return;

        if (e.Y > Padding.Top)
            return;

        if (e.Button == MouseButtons.Left && Movable)
        {
            _formMoveMouseDown = true;
            _location = Location;
            _mouseOffset = MousePosition;
        }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        var elementClicked = false;
        // Z-order'a g�re tersten kontrol et (�stteki elementten ba�la)
        BuildHitTestList(true);
        for (var i = 0; i < _hitTestElements.Count; i++)
        {
            var element = _hitTestElements[i];
            if (!GetWindowRelativeBoundsStatic(element).Contains(e.Location))
                continue;

            elementClicked = true;

            var localEvent = CreateChildMouseEvent(e, element);
            element.OnMouseDoubleClick(localEvent);
            // T�klanan elementi en �ste getir
            BringToFront(element);
            break; // �lk t�klanan elementten sonra di�erlerini kontrol etmeye gerek yok
        }

        if (!elementClicked)
        {
            FocusManager.SetFocus(null);
            FocusedElement = null;
        }

        if (!MaximizeBox)
            return;

        var inCloseBox = e.Location.InRect(_controlBoxRect);
        var inMaxBox = e.Location.InRect(_maximizeBoxRect);
        var inMinBox = e.Location.InRect(_minimizeBoxRect);
        var inExtendBox = e.Location.InRect(_extendBoxRect);
        var inCloseTabBox = _tabCloseButton && e.Location.InRect(_closeTabBoxRect);
        var inNewTabBox = _newTabButton && e.Location.InRect(_newTabBoxRect);
        var inFormMenuBox = e.Location.InRect(_formMenuRect);

        if (inCloseBox || inMaxBox || inMinBox || inExtendBox || inCloseTabBox || inNewTabBox || inFormMenuBox)
            return;

        if (!ShowTitle)
            return;

        if (e.Y > Padding.Top)
            return;

        ShowMaximize();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        // If an element captured the mouse, forward the mouse up to it and release capture if left button
        if (_mouseCapturedElement != null)
        {
            var captured = _mouseCapturedElement;
            var bounds = GetWindowRelativeBoundsStatic(captured);
            var localEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - bounds.X, e.Y - bounds.Y, e.Delta);
            captured.OnMouseUp(localEvent);
            if (e.Button == MouseButtons.Left) ReleaseMouseCapture(captured);
        }

        base.OnMouseUp(e);

        if (!IsDisposed && _formMoveMouseDown)
        {
            //int screenIndex = GetMouseInScreen(PointToScreen(e.Location));
            var screen = Screen.FromPoint(MousePosition);
            if (MousePosition.Y == screen.WorkingArea.Top && MaximizeBox) ShowMaximize(true);

            if (Top < screen.WorkingArea.Top) Top = screen.WorkingArea.Top;

            if (Top > screen.WorkingArea.Bottom - TitleHeight)
                Top = Convert.ToInt32(screen.WorkingArea.Bottom - _titleHeightDPI);
        }

        IsStayAtTopBorder = false;
        Cursor.Clip = new Rectangle();
        _formMoveMouseDown = false;

        animationSource = e.Location;

        // Z-order'a g�re tersten kontrol et
        var elementClicked = false;
        UIElementBase? hitElement = null;
        BuildHitTestList(true);
        for (var i = 0; i < _hitTestElements.Count; i++)
        {
            var element = _hitTestElements[i];
            if (!GetWindowRelativeBoundsStatic(element).Contains(e.Location))
                continue;

            elementClicked = true;
            hitElement = element;
            var localEvent = CreateChildMouseEvent(e, element);
            element.OnMouseUp(localEvent);
            break;
        }

        if (e.Button == MouseButtons.Right && ContextMenuStrip != null)
        {
            static bool HasContextMenuInChain(UIElementBase? start)
            {
                var current = start;
                while (current != null)
                {
                    if (current.ContextMenuStrip != null)
                        return true;
                    current = current.Parent as UIElementBase;
                }

                return false;
            }

            // If nothing was hit, show the window menu.
            // If an element was hit but no element/parent has a menu, fall back to the window menu.
            // Exception: TextBox can show a native menu fallback; don't show window menu on top.
            var shouldShowWindowMenu = !elementClicked;

            if (!shouldShowWindowMenu && hitElement != null)
            {
                var isTextBox = hitElement is TextBox;
                if (!isTextBox && !HasContextMenuInChain(hitElement))
                    shouldShowWindowMenu = true;
            }

            if (shouldShowWindowMenu)
            {
                var point = PointToScreen(e.Location);
                ContextMenuStrip.Show(point);
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        // If an element has captured mouse, forward all mouse move events to it (so dragging continues even when cursor leaves its bounds)
        if (_mouseCapturedElement != null)
        {
            var captured = _mouseCapturedElement;
            var bounds = GetWindowRelativeBoundsStatic(captured);
            var localEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - bounds.X, e.Y - bounds.Y, e.Delta);
            captured.OnMouseMove(localEvent);
            return;
        }

        if (_formMoveMouseDown && !MousePosition.Equals(_mouseOffset))
        {
            if (WindowState == FormWindowState.Maximized)
            {
                var maximizedWidth = Width;
                var locationX = Left;
                ShowMaximize();

                var offsetXRatio = 1 - (float)Width / maximizedWidth;
                _mouseOffset.X -= (int)((_mouseOffset.X - locationX) * offsetXRatio);
            }

            var offsetX = _mouseOffset.X - MousePosition.X;
            var offsetY = _mouseOffset.Y - MousePosition.Y;
            var _workingArea = Screen.GetWorkingArea(this);

            if (MousePosition.Y - _workingArea.Top == 0)
            {
                if (!IsStayAtTopBorder)
                {
                    Cursor.Clip = _workingArea;
                    TopBorderStayTicks = DateTime.Now.Ticks;
                    IsStayAtTopBorder = true;
                }
                else if (DateTime.Now.Ticks - TopBorderStayTicks > _stickyBorderTime)
                {
                    Cursor.Clip = new Rectangle();
                }
            }

            Location = new Point(_location.X - offsetX, _location.Y - offsetY);
        }
        else
        {
            var inCloseBox = e.Location.InRect(_controlBoxRect);
            var inMaxBox = e.Location.InRect(_maximizeBoxRect);
            var inMinBox = e.Location.InRect(_minimizeBoxRect);
            var inExtendBox = e.Location.InRect(_extendBoxRect);
            var inCloseTabBox = _tabCloseButton && e.Location.InRect(_closeTabBoxRect);
            var inNewTabBox = _newTabButton && e.Location.InRect(_newTabBoxRect);
            var inFormMenuBox = e.Location.InRect(_formMenuRect);
            var isChange = false;

            if (inCloseBox != _inCloseBox)
            {
                _inCloseBox = inCloseBox;
                isChange = true;
                if (inCloseBox)
                    closeBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    closeBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inMaxBox != _inMaxBox)
            {
                _inMaxBox = inMaxBox;
                isChange = true;
                if (inMaxBox)
                    maxBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    maxBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inMinBox != _inMinBox)
            {
                _inMinBox = inMinBox;
                isChange = true;
                if (inMinBox)
                    minBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    minBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inExtendBox != _inExtendBox)
            {
                _inExtendBox = inExtendBox;
                isChange = true;
                if (inExtendBox)
                    extendBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    extendBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inCloseTabBox != _inTabCloseBox)
            {
                _inTabCloseBox = inCloseTabBox;
                isChange = true;
                if (inCloseTabBox)
                    tabCloseHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    tabCloseHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inNewTabBox != _inNewTabBox)
            {
                _inNewTabBox = inNewTabBox;
                isChange = true;
                if (inNewTabBox)
                    newTabHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    newTabHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inFormMenuBox != _inFormMenuBox)
            {
                _inFormMenuBox = inFormMenuBox;
                isChange = true;
                if (inFormMenuBox)
                    formMenuHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    formMenuHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (isChange)
                Invalidate();
        }

        UIElementBase hoveredElement = null;

        // Z-order'a g�re tersten kontrol et
        foreach (var element in Controls.OfType<UIElementBase>().OrderByDescending(el => el.ZOrder)
                     .Where(el => el.Visible && el.Enabled))
            if (GetWindowRelativeBoundsStatic(element).Contains(e.Location))
            {
                hoveredElement = element;
                var localEvent = CreateChildMouseEvent(e, element);
                element.OnMouseMove(localEvent);
                break; // �lk hover edilen elementten sonra di�erlerini kontrol etmeye gerek yok
            }

        // Cursor should reflect the deepest hovered child (e.g., TextBox -> IBeam)
        var cursorElement = hoveredElement;
        while (cursorElement?.LastHoveredElement != null)
            cursorElement = cursorElement.LastHoveredElement;
        UpdateCursor(cursorElement);

        if (hoveredElement != _lastHoveredElement)
        {
            _lastHoveredElement?.OnMouseLeave(EventArgs.Empty);
            hoveredElement?.OnMouseEnter(EventArgs.Empty);
            LastHoveredElement = hoveredElement;
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _inExtendBox = _inCloseBox = _inMaxBox = _inMinBox = _inTabCloseBox = _inNewTabBox = _inFormMenuBox = false;
        closeBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        minBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        maxBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        extendBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        tabCloseHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        newTabHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        formMenuHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);

        _lastHoveredElement?.OnMouseLeave(e);
        LastHoveredElement = null;

        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);

        // Z-order'a g�re tersten kontrol et
        foreach (var element in Controls.OfType<UIElementBase>().OrderByDescending(el => el.ZOrder)
                     .Where(el => el.Visible && el.Enabled))
        {
            var mousePos = PointToClient(MousePosition);
            if (GetWindowRelativeBoundsStatic(element).Contains(mousePos))
            {
                element.OnMouseEnter(e);
                break;
            }
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        // Mouse pozisyonunu window client koordinatlar�na �evir
        var mousePos = PointToClient(MousePosition);

        // Recursive olarak do�ru child'� bul ve wheel olay�n� ilet
        if (PropagateMouseWheel(Controls.OfType<UIElementBase>(), mousePos, e))
            return; // Event i�lendi
    }

    /// <summary>
    ///     Recursive olarak child elementlere mouse wheel olay�n� iletir
    /// </summary>
    private bool PropagateMouseWheel(IEnumerable<UIElementBase> elements, Point windowMousePos, MouseEventArgs e)
    {
        // Z-order'a g�re tersten kontrol et - en �stteki element �nce
        foreach (var element in elements.OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            var elementBounds = GetWindowRelativeBoundsStatic(element);
            if (!elementBounds.Contains(windowMousePos))
                continue;

            // �nce bu elementin child'lar�n� kontrol et (daha spesifik -> daha genel)
            if (element.Controls != null && element.Controls.Count > 0)
            {
                var childElements = element.Controls.OfType<UIElementBase>();
                if (PropagateMouseWheel(childElements, windowMousePos, e))
                    return true; // Child i�ledi
            }

            // Child i�lemediyse bu elemente g�nder
            var localEvent = new MouseEventArgs(
                e.Button,
                e.Clicks,
                windowMousePos.X - elementBounds.X,
                windowMousePos.Y - elementBounds.Y,
                e.Delta);

            element.OnMouseWheel(localEvent);
            return true; // Event i�lendi
        }

        return false; // Hi�bir element i�lemedi
    }

    private void ShowMaximize(bool IsOnMoving = false)
    {
        var screen = Screen.FromPoint(MousePosition);
        base.MaximumSize = screen.WorkingArea.Size;
        if (screen.Primary)
            MaximizedBounds = screen.WorkingArea;
        else
            MaximizedBounds = new Rectangle(0, 0, 0, 0);

        if (WindowState == FormWindowState.Normal)
        {
            _sizeOfBeforeMaximized = Size;
            _locationOfBeforeMaximized = IsOnMoving ? _location : Location;
            WindowState = FormWindowState.Maximized;
        }
        else if (WindowState == FormWindowState.Maximized)
        {
            if (_sizeOfBeforeMaximized.Width == 0 || _sizeOfBeforeMaximized.Height == 0)
            {
                var w = 800;
                if (MinimumSize.Width > 0) w = MinimumSize.Width;
                var h = 600;
                if (MinimumSize.Height > 0) h = MinimumSize.Height;
                _sizeOfBeforeMaximized = new Size(w, h);
            }

            Size = _sizeOfBeforeMaximized;
            if (_locationOfBeforeMaximized.X == 0 && _locationOfBeforeMaximized.Y == 0)
                _locationOfBeforeMaximized = new Point(
                    screen.Bounds.Left + screen.Bounds.Width / 2 - _sizeOfBeforeMaximized.Width / 2,
                    screen.Bounds.Top + screen.Bounds.Height / 2 - _sizeOfBeforeMaximized.Height / 2);

            Location = _locationOfBeforeMaximized;
            WindowState = FormWindowState.Normal;
        }

        Invalidate();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _isActive = true;
        Invalidate();
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        _isActive = false;
        Invalidate();
    }

    protected override void NotifyInvalidate(Rectangle invalidatedArea)
    {
        base.NotifyInvalidate(invalidatedArea);
        _needsFullRedraw = true;
    }

    protected override void WndProc(ref Message m)
    {
        if (!DesignMode && _renderBackend != RenderBackend.Software && m.Msg == WM_ERASEBKGND)
        {
            m.Result = 1;
            return;
        }

        base.WndProc(ref m);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        var clientArea = ClientRectangle;
        var clientPadding = Padding;

        clientArea.X += clientPadding.Left;
        clientArea.Y += clientPadding.Top;
        clientArea.Width -= clientPadding.Horizontal;
        clientArea.Height -= clientPadding.Vertical;

        var remainingArea = clientArea;

        // WinForms dock order: Reverse z-order (last added first) in a single pass
        // This matches WinForms DefaultLayout behavior where docking is z-order dependent
        // and processed in reverse (children.Count - 1 down to 0)
        for (var i = Controls.Count - 1; i >= 0; i--)
            if (Controls[i] is UIElementBase control && control.Visible)
                PerformDefaultLayout(control, clientArea, ref remainingArea);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var w = ClientSize.Width;
        var h = ClientSize.Height;
        if (w <= 0 || h <= 0)
            return;

        if (!DesignMode && _renderBackend != RenderBackend.Software && _renderer != null)
        {
            _renderer.Render(w, h, RenderScene);
            ArmIdleMaintenance();
            return;
        }

        var info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
        if (ShouldCacheSoftwareBackBuffer(info))
        {
            var bmp = RenderSoftwareFrameToGdiBitmap(info);
            if (bmp != null)
            {
                e.Graphics.DrawImageUnscaled(bmp, 0, 0);
            }
            else
            {
                // If cached conversion failed for any reason, fallback to uncached rendering
                if (DebugSettings.EnableRenderLogging)
                    DebugSettings.Log(
                        $"UIWindow: RenderSoftwareFrameToGdiBitmap returned null for size {info.Width}x{info.Height}, falling back to uncached rendering");
                RenderSoftwareFrameUncached(info, e.Graphics);
            }
        }
        else
        {
            // Free any previously retained backbuffer (e.g., after resizing to a large size).
            DisposeSoftwareBackBuffer();
            RenderSoftwareFrameUncached(info, e.Graphics);
        }

        base.OnPaint(e);
        ArmIdleMaintenance();
    }

    private void ApplyRenderStyles()
    {
        if (DesignMode)
            return;

        var gpu = _renderBackend != RenderBackend.Software;

        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.ResizeRedraw, true);

        SetStyle(ControlStyles.DoubleBuffer, !gpu);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, !gpu);
        SetStyle(ControlStyles.SupportsTransparentBackColor, !gpu);
        SetStyle(ControlStyles.Opaque, gpu);

        UpdateStyles();

        ApplyNativeWindowStyles(gpu);
    }

    private void ApplyNativeWindowStyles(bool gpu)
    {
        if (!IsHandleCreated)
            return;

        var hwnd = Handle;

        // Window styles
        var stylePtr = NativeMethods.GetWindowLong(hwnd, NativeMethods.WindowLongIndexFlags.GWL_STYLE);
        var style = stylePtr;
        var clipFlags = (nint)(uint)(NativeMethods.SetWindowLongFlags.WS_CLIPCHILDREN |
                                     NativeMethods.SetWindowLongFlags.WS_CLIPSIBLINGS);
        style = gpu ? style | clipFlags : style & ~clipFlags;

        // Extended styles
        var exStylePtr = NativeMethods.GetWindowLong(hwnd, NativeMethods.WindowLongIndexFlags.GWL_EXSTYLE);
        var exStyle = exStylePtr;
        var noRedirect = (nint)(uint)NativeMethods.SetWindowLongFlags.WS_EX_NOREDIRECTIONBITMAP;
        var composited = (nint)(uint)NativeMethods.SetWindowLongFlags.WS_EX_COMPOSITED;
        if (gpu)
        {
            if (_renderBackend == RenderBackend.OpenGL)
                exStyle |= noRedirect;
            else
                exStyle &= ~noRedirect;
            exStyle &= ~composited;
        }
        else
        {
            exStyle &= ~noRedirect;
        }

        if (IntPtr.Size == 8)
        {
            NativeMethods.SetWindowLongPtr64(hwnd, (int)NativeMethods.WindowLongIndexFlags.GWL_STYLE, style);
            NativeMethods.SetWindowLongPtr64(hwnd, (int)NativeMethods.WindowLongIndexFlags.GWL_EXSTYLE, exStyle);
        }
        else
        {
            NativeMethods.SetWindowLong32(hwnd, (int)NativeMethods.WindowLongIndexFlags.GWL_STYLE, (int)style);
            NativeMethods.SetWindowLong32(hwnd, (int)NativeMethods.WindowLongIndexFlags.GWL_EXSTYLE, (int)exStyle);
        }

        // Re-apply non-client metrics.
        NativeMethods.SetWindowPos(
            hwnd,
            IntPtr.Zero,
            0,
            0,
            0,
            0,
            NativeMethods.SetWindowPosFlags.SWP_NOMOVE |
            NativeMethods.SetWindowPosFlags.SWP_NOSIZE |
            NativeMethods.SetWindowPosFlags.SWP_NOZORDER |
            NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE |
            NativeMethods.SetWindowPosFlags.SWP_FRAMECHANGED);
    }

    private Bitmap? RenderSoftwareFrameToGdiBitmap(SKImageInfo info)
    {
        CreateOrUpdateCache(info);
        if (DebugSettings.EnableRenderLogging)
            DebugSettings.Log(
                $"UIWindow.RenderSoftwareFrameToGdiBitmap: enter {info.Width}x{info.Height} cacheSurface={(_cacheSurface != null ? "present" : "null")} cacheBitmap={(_cacheBitmap != null ? "present" : "null")}");

        if (_cacheSurface == null || _cacheBitmap == null)
            return null;

        lock (_softwareCacheLock)
        {
            var canvas = _cacheSurface.Canvas;
            RenderScene(canvas, info);
            canvas.Flush();
            _cacheSurface.Flush();

            // Create a temporary GDI bitmap copy from SKBitmap pixels and return a cloned copy
            // so that the resulting System.Drawing.Bitmap owns its pixel data (no pointer into SKBitmap).
            try
            {
                var pixels = _cacheBitmap.GetPixels();
                using var wrapper = new Bitmap(info.Width, info.Height, _cacheBitmap.RowBytes,
                    PixelFormat.Format32bppPArgb, pixels);
                var copy = new Bitmap(wrapper);
                return copy;
            }
            catch (Exception ex)
            {
                // If something went wrong copying the SKBitmap pixel block (rare, may be due to native race),
                // return null so caller can fall back to uncached rendering instead of crashing the process.
                if (DebugSettings.EnableRenderLogging)
                    DebugSettings.Log(
                        $"UIWindow: RenderSoftwareFrameToGdiBitmap failed to copy pixels ({info.Width}x{info.Height}): {ex}");
                return null;
            }
        }
    }

    private void RenderScene(SKCanvas canvas, SKImageInfo info)
    {
        GRContext? gr = null;
        
        // Only use GPU context if the renderer is actually actively using it for this frame.
        // Prevents UIElementBase from creating GPU surfaces when we are falling back to CPU rendering
        // (which causes slow readbacks "weak rendering" and potential access violations).
        if (_renderer is DirectX11WindowRenderer dx && dx.IsSkiaGpuActive)
        {
            gr = dx.GrContext;
        }
        else if (_renderer is OpenGlWindowRenderer gl && gl.IsSkiaGpuActive)
        {
            gr = gl.GrContext;
        }
        else if (!(_renderer is DirectX11WindowRenderer) && !(_renderer is OpenGlWindowRenderer))
        {
             // Fallback for other renderers
             gr = (_renderer as IGpuWindowRenderer)?.GrContext;
        }

        var gpuScope = gr != null ? UIElementBase.PushGpuContext(gr) : null;
        try
        {
            canvas.Save();
            canvas.ResetMatrix();
            canvas.ClipRect(SKRect.Create(info.Width, info.Height));
            canvas.Clear(SKColors.Transparent);
            PaintSurface(canvas, info);
            canvas.Restore();

            if (_needsFullRedraw)
            {
                for (var i = 0; i < Controls.Count; i++)
                    if (Controls[i] is UIElementBase child)
                        child.InvalidateRenderTree();
                _needsFullRedraw = false;
            }

            _frameElements.Clear();
            for (var i = 0; i < Controls.Count; i++)
                if (Controls[i] is UIElementBase element)
                    _frameElements.Add(element);

            // Match LINQ OrderBy stability (ties keep original order).
            StableSortByZOrderAscending(_frameElements);
            
            // DEBUG: Count renders
            var renderedCount = 0;
            var needsRedrawBefore = 0;
            var needsRedrawAfter = 0;
            var usesCacheCount = 0;
            
            for (var i = 0; i < _frameElements.Count; i++)
            {
                var element = _frameElements[i];
                if (!element.Visible || element.Width <= 0 || element.Height <= 0)
                    continue;
                
                renderedCount++;
                if (element.NeedsRedraw) needsRedrawBefore++;
                if (element.UseRenderCache) usesCacheCount++;
                
                element.Render(canvas);
                
                if (element.NeedsRedraw) needsRedrawAfter++;
            }

            if (_showPerfOverlay)
            {
                DrawPerfOverlay(canvas);
                // Show render stats
                var statsPaint = new SKPaint { Color = SKColors.Yellow, TextSize = 12, IsAntialias = true };
                canvas.DrawText($"Rendered: {renderedCount} | Before: {needsRedrawBefore} | After: {needsRedrawAfter} | UsesCache: {usesCacheCount}", 
                    10, info.Height - 20, statsPaint);
                statsPaint.Dispose();
            }
        }
        finally
        {
            gpuScope?.Dispose();
        }
    }

    private void DrawPerfOverlay(SKCanvas canvas)
    {
        var now = Stopwatch.GetTimestamp();
        if (_perfLastTimestamp == 0)
        {
            _perfLastTimestamp = now;
            return;
        }

        var dt = (now - _perfLastTimestamp) / (double)Stopwatch.Frequency;
        _perfLastTimestamp = now;
        if (dt <= 0)
            return;

        var frameMs = dt * 1000.0;
        _perfSmoothedFrameMs = _perfSmoothedFrameMs <= 0
            ? frameMs
            : _perfSmoothedFrameMs * 0.90 + frameMs * 0.10;

        var fps = 1000.0 / Math.Max(0.001, _perfSmoothedFrameMs);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.ForeColor.ToSKColor(),
            TextSize = 12
        };

        var backendLabel = _renderBackend.ToString();
        if (_renderBackend == RenderBackend.DirectX11 && _renderer is DirectX11WindowRenderer dx)
            backendLabel = dx.IsSkiaGpuActive ? "DX:GPU" : "DX:CPU";
        else if (_renderBackend == RenderBackend.OpenGL && _renderer is OpenGlWindowRenderer gl)
            backendLabel = gl.IsSkiaGpuActive ? "GL:GPU" : "GL";

        var text = $"{backendLabel}  {fps:0} FPS  {_perfSmoothedFrameMs:0.0} ms";
        canvas.DrawText(text, 8, 16, paint);
    }

    private void PaintSurface(SKCanvas canvas, SKImageInfo info)
    {
        if (info.Width <= 0 || info.Height <= 0)
            return;

        if (!ShowTitle)
        {
            canvas.Clear(ColorScheme.BackColor.ToSKColor());
            return;
        }

        var foreColor = ColorScheme.ForeColor.ToSKColor();
        var hoverColor = ColorScheme.BorderColor.ToSKColor();

        // Arka plan� temizle
        canvas.Clear(ColorScheme.BackColor.ToSKColor());

        if (FullDrawHatch)
        {
            using var paint = new SKPaint
            {
                Color = hoverColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                PathEffect = SKPathEffect.Create2DLine(4, SKMatrix.CreateScale(4, 4))
            };
            canvas.DrawRect(0, 0, Width, Height, paint);
        }

        if (titleColor != Color.Empty)
        {
            foreColor = titleColor.Determine().ToSKColor();
            hoverColor = foreColor.WithAlpha(20);
            using var paint = new SKPaint { Color = titleColor.ToSKColor() };
            canvas.DrawRect(0, 0, Width, _titleHeightDPI, paint);
        }
        else if (_gradient.Length == 2 &&
                 !(_gradient[0] == Color.Transparent && _gradient[1] == Color.Transparent))
        {
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(Width, _titleHeightDPI),
                new[] { _gradient[0].ToSKColor(), _gradient[1].ToSKColor() },
                null,
                SKShaderTileMode.Clamp);

            using var paint = new SKPaint { Shader = shader };
            canvas.DrawRect(0, 0, Width, _titleHeightDPI, paint);

            foreColor = _gradient[0].Determine().ToSKColor();
            hoverColor = foreColor.WithAlpha(20);
        }

        // Ba�l�k alan� d���ndaki i�eri�i tema arkaplan� ile doldur
        using (var contentBgPaint = new SKPaint { Color = ColorScheme.BackColor.ToSKColor() })
        {
            canvas.DrawRect(0, _titleHeightDPI, Width, Math.Max(0, Height - _titleHeightDPI), contentBgPaint);
        }

        // Kontrol d��meleri �izimi
        if (controlBox)
        {
            var closeHoverColor = new SKColor(232, 17, 35);

            if (_inCloseBox)
            {
                using var paint = new SKPaint
                {
                    Color = closeHoverColor.WithAlpha((byte)(closeBoxHoverAnimationManager.GetProgress() * 120)),
                    IsAntialias = true
                };
                canvas.DrawRect(_controlBoxRect.ToSKRect(), paint);
            }

            using var closePaint = new SKPaint
            {
                Color = _inCloseBox ? SKColors.White : foreColor,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            // �arp� i�areti
            var centerX = _controlBoxRect.Left + _controlBoxRect.Width / 2;
            var centerY = _controlBoxRect.Top + _controlBoxRect.Height / 2;
            var size = 5 * DPI;

            canvas.DrawLine(
                centerX - size,
                centerY - size,
                centerX + size,
                centerY + size,
                closePaint);

            canvas.DrawLine(
                centerX - size,
                centerY + size,
                centerX + size,
                centerY - size,
                closePaint);
        }

        if (MaximizeBox)
        {
            if (_inMaxBox)
            {
                using var paint = new SKPaint
                {
                    Color = hoverColor.WithAlpha((byte)(maxBoxHoverAnimationManager.GetProgress() * 80)),
                    IsAntialias = true
                };
                canvas.DrawRect(_maximizeBoxRect.ToSKRect(), paint);
            }

            using var maxPaint = new SKPaint
            {
                Color = foreColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            // Maximize simgesi
            var centerX = _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2;
            var centerY = _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2;
            var size = 5 * DPI;

            if (WindowState == FormWindowState.Maximized)
            {
                // Restore simgesi
                var offset = 2 * DPI;
                canvas.DrawRect(
                    centerX - size + offset,
                    centerY - size - offset,
                    size * 2,
                    size * 2,
                    maxPaint);

                canvas.DrawRect(
                    centerX - size - offset,
                    centerY - size + offset,
                    size * 2,
                    size * 2,
                    maxPaint);
            }
            else
            {
                canvas.DrawRect(
                    centerX - size,
                    centerY - size,
                    size * 2,
                    size * 2,
                    maxPaint);
            }
        }

        if (MinimizeBox)
        {
            if (_inMinBox)
            {
                using var paint = new SKPaint
                {
                    Color = hoverColor.WithAlpha((byte)(minBoxHoverAnimationManager.GetProgress() * 80)),
                    IsAntialias = true
                };
                canvas.DrawRect(_minimizeBoxRect.ToSKRect(), paint);
            }

            using var minPaint = new SKPaint
            {
                Color = foreColor,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            var centerX = _minimizeBoxRect.Left + _minimizeBoxRect.Width / 2;
            var centerY = _minimizeBoxRect.Top + _minimizeBoxRect.Height / 2;
            var size = 5 * DPI;

            canvas.DrawLine(
                centerX - size,
                centerY,
                centerX + size,
                centerY,
                minPaint);
        }

        // Extend Box �izimi
        if (ExtendBox)
        {
            var color = foreColor;
            if (_inExtendBox)
            {
                var hoverSize = 24 * DPI;
                using var paint = new SKPaint
                {
                    Color = hoverColor.WithAlpha((byte)(extendBoxHoverAnimationManager.GetProgress() * 60)),
                    IsAntialias = true
                };

                using var path = new SKPath();
                path.AddRoundRect(new SKRect(
                    _extendBoxRect.X + 20 * DPI,
                    _titleHeightDPI / 2 - hoverSize / 2,
                    _extendBoxRect.X + 20 * DPI + hoverSize,
                    _titleHeightDPI / 2 + hoverSize / 2
                ), 15, 15);

                canvas.DrawPath(path, paint);
            }

            var size = 16 * DPI;
            using var extendPaint = new SKPaint
            {
                Color = foreColor,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            var iconRect = new SKRect(
                _extendBoxRect.X + 24 * DPI,
                _titleHeightDPI / 2 - size / 2,
                _extendBoxRect.X + 24 * DPI + size,
                _titleHeightDPI / 2 + size / 2);

            canvas.DrawLine(
                iconRect.Left + iconRect.Width / 2 - 5 * DPI - 1,
                iconRect.Top + iconRect.Height / 2 - 2 * DPI,
                iconRect.Left + iconRect.Width / 2 - 1 * DPI,
                iconRect.Top + iconRect.Height / 2 + 3 * DPI,
                extendPaint);

            canvas.DrawLine(
                iconRect.Left + iconRect.Width / 2 + 5 * DPI - 1,
                iconRect.Top + iconRect.Height / 2 - 2 * DPI,
                iconRect.Left + iconRect.Width / 2 - 1 * DPI,
                iconRect.Top + iconRect.Height / 2 + 3 * DPI,
                extendPaint);
        }

        // Form Menu veya Icon �izimi
        var faviconSize = 16 * DPI;
        if (showMenuInsteadOfIcon)
        {
            using var paint = new SKPaint
            {
                Color = hoverColor.WithAlpha((byte)(formMenuHoverAnimationManager.GetProgress() * 60)),
                IsAntialias = true
            };

            using var path = new SKPath();
            path.AddRoundRect(_formMenuRect.ToSKRect(), 10, 10);
            canvas.DrawPath(path, paint);

            using var menuPaint = new SKPaint
            {
                Color = foreColor,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            canvas.DrawLine(
                _formMenuRect.Left + _formMenuRect.Width / 2 - 5 * DPI - 1,
                _formMenuRect.Top + _formMenuRect.Height / 2 - 2 * DPI,
                _formMenuRect.Left + _formMenuRect.Width / 2 - 1 * DPI,
                _formMenuRect.Top + _formMenuRect.Height / 2 + 3 * DPI,
                menuPaint);

            canvas.DrawLine(
                _formMenuRect.Left + _formMenuRect.Width / 2 + 5 * DPI - 1,
                _formMenuRect.Top + _formMenuRect.Height / 2 - 2 * DPI,
                _formMenuRect.Left + _formMenuRect.Width / 2 - 1 * DPI,
                _formMenuRect.Top + _formMenuRect.Height / 2 + 3 * DPI,
                menuPaint);
        }
        else
        {
            if (ShowIcon && Icon != null)
            {
                using var bitmap = Icon.ToBitmap();
                using var skBitmap = bitmap.ToSKBitmap();
                using var image = SKImage.FromBitmap(skBitmap);
                var iconRect = SKRect.Create(10, _titleHeightDPI / 2 - faviconSize / 2, faviconSize, faviconSize);
                canvas.DrawImage(image, iconRect);
            }
        }

        // Form ba�l��� �izimi
        if (_windowPageControl == null || _windowPageControl.Count == 0)
        {
            using var font = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            using var textPaint = new SKPaint
            {
                Color = foreColor,
                IsAntialias = true
            };

            var bounds = new SKRect();
            font.MeasureText(Text, out bounds);
            var textX = showMenuInsteadOfIcon
                ? _formMenuRect.X + _formMenuRect.Width + 8 * DPI
                : faviconSize + 14 * DPI;
            var textY = _titleHeightDPI / 2 + Math.Abs(font.Metrics.Ascent + font.Metrics.Descent) / 2;

            TextRenderingHelper.DrawText(canvas, Text, textX, textY, SKTextAlign.Left, font, textPaint);
        }

        // Tab kontrollerinin �izimi
        if (_windowPageControl != null && _windowPageControl.Count > 0)
        {
            if (!pageAreaAnimationManager.IsAnimating() || pageRect == null ||
                pageRect.Count != _windowPageControl.Count)
                UpdateTabRects();

            var animationProgress = pageAreaAnimationManager.GetProgress();

            // Click feedback
            if (pageAreaAnimationManager.IsAnimating())
            {
                using var ripplePaint = new SKPaint
                {
                    Color = foreColor.WithAlpha((byte)(31 - animationProgress * 30)),
                    IsAntialias = true
                };

                var rippleSize = (int)(animationProgress * pageRect[_windowPageControl.SelectedIndex].Width * 1.75);
                var rippleRect = new SKRect(
                    animationSource.X - rippleSize / 2,
                    animationSource.Y - rippleSize / 2,
                    animationSource.X + rippleSize / 2,
                    animationSource.Y + rippleSize / 2);

                canvas.Save();
                canvas.ClipRect(pageRect[_windowPageControl.SelectedIndex].ToSKRect());
                canvas.DrawOval(rippleRect, ripplePaint);
                canvas.Restore();
            }

            // fix desing time error
            if (_windowPageControl.SelectedIndex <= -1 || _windowPageControl.SelectedIndex >= _windowPageControl.Count)
                return;

            // Animate page indicator
            if (previousSelectedPageIndex == pageRect.Count)
                previousSelectedPageIndex = -1;

            var previousSelectedPageIndexIfHasOne = previousSelectedPageIndex == -1
                ? _windowPageControl.SelectedIndex
                : previousSelectedPageIndex;
            var previousActivePageRect = pageRect[previousSelectedPageIndexIfHasOne];
            var activePageRect = pageRect[_windowPageControl.SelectedIndex];

            var y = activePageRect.Bottom - 2;
            var x = previousActivePageRect.X + (activePageRect.X - previousActivePageRect.X) * (float)animationProgress;
            var width = previousActivePageRect.Width +
                        (activePageRect.Width - previousActivePageRect.Width) * (float)animationProgress;

            if (_tabDesingMode == TabDesingMode.Rectangle)
            {
                using var tabPaint = new SKPaint
                {
                    Color = ColorScheme.BackColor.ToSKColor().InterpolateColor(hoverColor, 0.15f),
                    IsAntialias = true
                };

                canvas.DrawRect(activePageRect.X, 0, width, _titleHeightDPI, tabPaint);
                canvas.DrawRect(x, 0, width, _titleHeightDPI, tabPaint);

                using var indicatorPaint = new SKPaint
                {
                    Color = SKColors.DodgerBlue,
                    IsAntialias = true
                };

                canvas.DrawRect(x, _titleHeightDPI - TAB_INDICATOR_HEIGHT, width, TAB_INDICATOR_HEIGHT, indicatorPaint);
            }
            else if (_tabDesingMode == TabDesingMode.Rounded)
            {
                if (titleColor != Color.Empty && !titleColor.IsDark())
                    hoverColor = foreColor.WithAlpha(60);

                using var tabPaint = new SKPaint
                {
                    Color = ColorScheme.BackColor.ToSKColor().InterpolateColor(hoverColor, 0.2f),
                    IsAntialias = true
                };

                var tabRect = new SKRect(x, 6, x + width, _titleHeightDPI);
                var radius = 9 * DPI;

                using var path = new SKPath();
                path.AddRoundRect(tabRect, radius, radius);
                canvas.DrawPath(path, tabPaint);
            }
            else // Chromed
            {
                if (titleColor != Color.Empty && !titleColor.IsDark())
                    hoverColor = foreColor.WithAlpha(60);

                using var tabPaint = new SKPaint
                {
                    Color = ColorScheme.BackColor.ToSKColor().InterpolateColor(hoverColor, 0.2f),
                    IsAntialias = true
                };

                var tabRect = new SKRect(x, 5, x + width, _titleHeightDPI - 7);
                var radius = 12;

                using var path = new SKPath();
                path.AddRoundRect(tabRect, radius, radius);
                canvas.DrawPath(path, tabPaint);
            }

            // Draw tab headers
            foreach (UIElementBase page in _windowPageControl.Controls)
            {
                var currentTabIndex = _windowPageControl.Controls.IndexOf(page);
                var rect = pageRect[currentTabIndex];
                var closeIconSize = 24 * DPI;

                if (_drawTabIcons)
                {
                    using var font = new SKFont
                    {
                        Size = 12f.PtToPx(this),
                        Typeface = FontManager.GetSKTypeface(Font),
                        Subpixel = true,
                        Edging = SKFontEdging.SubpixelAntialias
                    };
                    using var textPaint = new SKPaint
                    {
                        Color = foreColor,
                        IsAntialias = true
                    };

                    var startingIconBounds = new SKRect();
                    font.MeasureText("", out startingIconBounds);
                    var iconX = rect.X + TAB_HEADER_PADDING * DPI;

                    var inlinePaddingX = startingIconBounds.Width + TAB_HEADER_PADDING * DPI;
                    rect.X += inlinePaddingX;
                    rect.Width -= inlinePaddingX + closeIconSize;

                    var textY = _titleHeightDPI / 2 + Math.Abs(font.Metrics.Ascent + font.Metrics.Descent) / 2;
                    TextRenderingHelper.DrawText(canvas, "", iconX, textY, SKTextAlign.Center, font, textPaint);

                    var bounds = new SKRect();
                    font.MeasureText(page.Text, out bounds);
                    var textX = rect.X + rect.Width / 2;
                    TextRenderingHelper.DrawText(canvas, page.Text, textX, textY, SKTextAlign.Center, font, textPaint);
                }
                else
                {
                    using var font = new SKFont
                    {
                        Size = 9f.PtToPx(this),
                        Typeface = FontManager.GetSKTypeface(Font),
                        Subpixel = true,
                        Edging = SKFontEdging.SubpixelAntialias
                    };
                    using var textPaint = new SKPaint
                    {
                        Color = foreColor,
                        IsAntialias = true
                    };

                    var bounds = new SKRect();
                    font.MeasureText(page.Text, out bounds);
                    var textX = rect.X + rect.Width / 2;
                    var textY = _titleHeightDPI / 2 + Math.Abs(font.Metrics.Ascent + font.Metrics.Descent) / 2;
                    TextRenderingHelper.DrawText(canvas, page.Text, textX, textY, SKTextAlign.Center, font, textPaint);
                }
            }

            // Tab close button
            if (_tabCloseButton)
            {
                var size = 20 * DPI;
                var closeHoverColor = hoverColor;

                using var buttonPaint = new SKPaint
                {
                    Color = closeHoverColor.WithAlpha((byte)(tabCloseHoverAnimationManager.GetProgress() * 60)),
                    IsAntialias = true
                };

                _closeTabBoxRect = new RectangleF(x + width - TAB_HEADER_PADDING / 2 - size,
                    _titleHeightDPI / 2 - size / 2, size, size);
                var buttonRect = _closeTabBoxRect.ToSKRect();

                canvas.DrawCircle(buttonRect.MidX, buttonRect.MidY, size / 2, buttonPaint);

                using var linePaint = new SKPaint
                {
                    Color = foreColor,
                    StrokeWidth = 1.1f * DPI,
                    IsAntialias = true,
                    StrokeCap = SKStrokeCap.Round
                };

                size = 4f * DPI;
                canvas.DrawLine(
                    buttonRect.MidX - size,
                    buttonRect.MidY - size,
                    buttonRect.MidX + size,
                    buttonRect.MidY + size,
                    linePaint);

                canvas.DrawLine(
                    buttonRect.MidX - size,
                    buttonRect.MidY + size,
                    buttonRect.MidX + size,
                    buttonRect.MidY - size,
                    linePaint);
            }

            // New tab button
            if (_newTabButton)
            {
                var size = 24 * DPI;
                var newHoverColor = hoverColor.WithAlpha(20);

                using var buttonPaint = new SKPaint
                {
                    Color = newHoverColor.WithAlpha((byte)(newTabHoverAnimationManager.GetProgress() *
                                                           newHoverColor.Alpha)),
                    IsAntialias = true
                };

                var lastTabRect = pageRect[pageRect.Count - 1];
                _newTabBoxRect = new RectangleF(lastTabRect.X + lastTabRect.Width + size / 2,
                    _titleHeightDPI / 2 - size / 2, size, size);
                var buttonRect = _newTabBoxRect.ToSKRect();

                using var path = new SKPath();
                path.AddRoundRect(buttonRect, 4, 4);
                canvas.DrawPath(path, buttonPaint);

                using var linePaint = new SKPaint
                {
                    Color = foreColor,
                    StrokeWidth = 1.1f * DPI,
                    IsAntialias = true,
                    StrokeCap = SKStrokeCap.Round
                };

                size = 6 * DPI;
                canvas.DrawLine(
                    buttonRect.MidX - size,
                    buttonRect.MidY,
                    buttonRect.MidX + size,
                    buttonRect.MidY,
                    linePaint);

                canvas.DrawLine(
                    buttonRect.MidX,
                    buttonRect.MidY - size,
                    buttonRect.MidX,
                    buttonRect.MidY + size,
                    linePaint);
            }
        }

        // Title border
        if (_drawTitleBorder)
        {
            using var borderPaint = new SKPaint
            {
                Color = titleColor != Color.Empty
                    ? titleColor.Determine().ToSKColor().WithAlpha(30)
                    : ColorScheme.BorderColor.ToSKColor(),
                StrokeWidth = 1,
                IsAntialias = true
            };

            canvas.DrawLine(Width, _titleHeightDPI - 1, 0, _titleHeightDPI - 1, borderPaint);
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        CalcSystemBoxPos();
        PerformLayout();

        if (!DesignMode && _renderBackend != RenderBackend.Software)
        {
            _renderer?.Resize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        CalcSystemBoxPos();

        // Trigger initial layout with current DPI
        InvalidateMeasureRecursive();
        PerformLayout();
    }

    private void UpdateTabRects()
    {
        pageRect = new List<RectangleF>();

        if (_windowPageControl == null || _windowPageControl.Count == 0)
            return;

        var occupiedWidth = 44 * DPI;

        if (controlBox)
            occupiedWidth += _controlBoxRect.Width;

        if (MinimizeBox)
            occupiedWidth += _minimizeBoxRect.Width;

        if (MaximizeBox)
            occupiedWidth += _maximizeBoxRect.Width;

        if (ExtendBox)
            occupiedWidth += _extendBoxRect.Width;

        occupiedWidth += 30 * DPI;

        var availableWidth = Width - occupiedWidth;
        var maxSize = 250f * DPI;

        using var font = new SKFont
        {
            Size = (_drawTabIcons ? 12f : 9f).PtToPx(this),
            Typeface = FontManager.GetSKTypeface(Font),
            Subpixel = true,
            Edging = SKFontEdging.SubpixelAntialias
        };

        var desiredWidths = new List<float>();
        float totalDesiredWidth = 0;

        foreach (UIElementBase page in _windowPageControl.Controls)
        {
            var bounds = new SKRect();
            font.MeasureText(page.Text ?? "", out bounds);

            var width = bounds.Width + (20 * DPI);

            if (_drawTabIcons)
                width += 30 * DPI;

            if (_tabCloseButton)
                width += 24 * DPI;

            desiredWidths.Add(width);
            totalDesiredWidth += width;
        }

        float scale = 1.0f;
        float extraPerTab = 0;

        if (totalDesiredWidth > availableWidth && totalDesiredWidth > 0)
        {
            scale = availableWidth / totalDesiredWidth;
        }
        else if (totalDesiredWidth < availableWidth && _windowPageControl.Count > 0)
        {
            var extra = availableWidth - totalDesiredWidth;
            extraPerTab = extra / _windowPageControl.Count;
        }

        var currentX = 44 * DPI;

        for (int i = 0; i < desiredWidths.Count; i++)
        {
            var finalWidth = (desiredWidths[i] * scale) + extraPerTab;

            if (finalWidth > maxSize)
                finalWidth = maxSize;

            pageRect.Add(new RectangleF(currentX, 0, finalWidth, _titleHeightDPI));
            currentX += finalWidth;
        }
    }

    internal void UpdateCursor(UIElementBase element)
    {
        if (element == null || !element.Enabled || !element.Visible)
        {
            _currentCursor = Cursors.Default;
            base.Cursor = Cursors.Default;
            return;
        }

        var newCursor = element.Cursor ?? Cursors.Default;
        if (_currentCursor != newCursor)
        {
            _currentCursor = newCursor;
            base.Cursor = newCursor;
        }
    }

    public void BringToFront(UIElementBase element)
    {
        if (!Controls.Contains(element)) return;

        _maxZOrder++;
        element.ZOrder = _maxZOrder;
        InvalidateElement(element);
    }

    public void SendToBack(UIElementBase element)
    {
        if (!Controls.Contains(element)) return;

        var minZOrder = Controls.OfType<UIElementBase>().Min(e => e.ZOrder);
        element.ZOrder = minZOrder - 1;
        InvalidateElement(element);
    }

    private void InvalidateElement(UIElementBase element)
    {
        if (_layoutSuspendCount > 0) return;

        element.InvalidateRenderTree();

        if (!_needsFullRedraw)
            Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var paint in _paintCache.Values)
                paint.Dispose();
            _paintCache.Clear();

            _cacheBitmap?.Dispose();
            _cacheBitmap = null;
            _cacheSurface?.Dispose();
            _cacheSurface = null;
        }

        base.Dispose(disposing);
    }

    private readonly struct ZOrderSortItem
    {
        public readonly UIElementBase Element;
        public readonly int ZOrder;
        public readonly int Sequence;

        public ZOrderSortItem(UIElementBase element, int zOrder, int sequence)
        {
            Element = element;
            ZOrder = zOrder;
            Sequence = sequence;
        }
    }
}
