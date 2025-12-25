using SDUI.Animation;
using SDUI.Collections;
using SDUI.Extensions;
using SDUI.Helpers;
using SDUI.Rendering;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;

namespace SDUI.Controls;

public class UIWindow : UIWindowBase, IUIElement
{
    public enum TabDesingMode
    {
        Rectangle,
        Rounded,
        Chromed,
    }

    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnFormMenuClick;

    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnExtendBoxClick;

    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler<int> OnCloseTabBoxClick;

    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnNewTabBoxClick;

    /// <summary>
    /// Is form active <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _isActive;

    /// <summary>
    /// If the mouse down <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _formMoveMouseDown;

    /// <summary>
    /// The position of the form when the left mouse button is pressed
    /// </summary>
    private Point _location;

    /// <summary>
    /// The position of the mouse when the left mouse button is pressed
    /// </summary>
    private Point _mouseOffset;

    /// <summary>
    /// The rectangle of control box
    /// </summary>
    private RectangleF _controlBoxRect;

    /// <summary>
    /// The rectangle of maximize box
    /// </summary>
    private RectangleF _maximizeBoxRect;

    /// <summary>
    /// The rectangle of minimize box
    /// </summary>
    private RectangleF _minimizeBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private RectangleF _extendBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private RectangleF _closeTabBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private RectangleF _newTabBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private RectangleF _formMenuRect;

    /// <summary>
    /// The control box left value
    /// </summary>
    private float _controlBoxLeft;

    /// <summary>
    /// The size of the window before it is maximized
    /// </summary>
    private Size _sizeOfBeforeMaximized;

    /// <summary>
    /// The position of the window before it is maximized
    /// </summary>
    private Point _locationOfBeforeMaximized;

    private float _titleHeightDPI => _titleHeight * DPI;
    private float _iconWidthDPI => _iconWidth * DPI;
    private float _symbolSizeDPI => _symbolSize * DPI;

    private float _iconWidth = 42;
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

    [DefaultValue(false)]
    public bool AllowAddControlOnTitle { get; set; }

    private bool _extendBox;

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

    private bool _drawTabIcons;

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

    private bool _tabCloseButton;

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

    private bool _newTabButton;

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


    private float _symbolSize = 24;

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

    [DefaultValue(null)]
    public ContextMenuStrip ExtendMenu { get; set; }

    [DefaultValue(null)]
    public ContextMenuStrip FormMenu { get; set; }

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
    /// Whether to show the title bar of the form
    /// </summary>
    private bool showTitle = true;

    /// <summary>
    /// Gets or sets whether to show the title bar of the form
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
    /// Whether to show the title bar of the form
    /// </summary>
    private bool showMenuInsteadOfIcon = false;

    /// <summary>
    /// Gets or sets whether to show the title bar of the form
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
    /// Whether to show the title bar of the form
    /// </summary>
    private bool _drawTitleBorder = true;

    /// <summary>
    /// Gets or sets whether to show the title bar of the form
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

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        _suppressImmediateUpdateCount++;
        try
        {
            base.OnDpiChanged(e);

            // Prefer WM_DPICHANGED suggested bounds; avoid manual scaling (can cause size oscillation).
            var suggested = e.SuggestedRectangle;
            if (Bounds != suggested)
            {
                Bounds = suggested;
            }

            foreach (UIElementBase element in Controls)
                element.OnDpiChanged(e.DeviceDpiNew, e.DeviceDpiOld);

            _needsFullRedraw = true;
            Invalidate();
        }
        finally
        {
            _suppressImmediateUpdateCount--;
        }
    }

    /// <summary>
    /// Whether to display the control buttons of the form
    /// </summary>
    private bool controlBox = true;

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
    /// Whether to show the maximize button of the form
    /// </summary>
    private bool _maximizeBox = true;

    /// <summary>
    /// Whether to show the maximize button of the form
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
    /// Whether to show the minimize button of the form
    /// </summary>
    private bool _minimizeBox = true;

    /// <summary>
    /// Whether to show the minimize button of the form
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
    /// The title height
    /// </summary>
    private float _titleHeight = 32;

    /// <summary>
    /// Gets or sets the title height
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

    /// <summary>
    /// Gradient header colors
    /// </summary>
    private Color[] _gradient = new[] { Color.Transparent, Color.Transparent };
    public Color[] Gradient
    {
        get => _gradient;
        set
        {
            _gradient = value; Invalidate();
        }
    }

    /// <summary>
    /// The title color
    /// </summary>
    private Color titleColor;

    /// <summary>
    /// Gets or sets the title color
    /// </summary>
    [Description("Title color"), DefaultValue(typeof(Color), "224, 224, 224")]
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
    /// The title color
    /// </summary>
    private Color borderColor = Color.Transparent;

    /// <summary>
    /// Gets or sets the title color
    /// </summary>
    [Description("Border Color"), DefaultValue(typeof(Color), "Transparent")]
    public Color BorderColor
    {
        get => borderColor;
        set
        {
            borderColor = value;

            if (value != Color.Transparent)
                WindowsHelper.ApplyBorderColor(this.Handle, this.borderColor);

            Invalidate();
        }
    }

    /// <summary>
    /// Tab desing mode
    /// </summary>

    private TabDesingMode _tabDesingMode = TabDesingMode.Rectangle;
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
    /// Draw hatch brush on form
    /// </summary>
    public bool FullDrawHatch { get; set; }

    private HatchStyle _hatch = HatchStyle.Percent80;
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

    /// <summary>
    /// Whether to trigger the stay event on the edge of the display
    /// </summary>
    private bool IsStayAtTopBorder;

    /// <summary>
    /// The time at which the display edge dwell event was triggered
    /// </summary>
    private long TopBorderStayTicks;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager minBoxHoverAnimationManager;

    /// <summary>
    /// Tab Area hover animation manager
    /// </summary>
    private readonly AnimationManager pageAreaAnimationManager;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager maxBoxHoverAnimationManager;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager extendBoxHoverAnimationManager;

    /// <summary>
    /// Close tab hover animation manager
    /// </summary>
    private readonly AnimationManager closeBoxHoverAnimationManager;

    /// <summary>
    /// new Tab hover animation manager
    /// </summary>
    private readonly AnimationManager newTabHoverAnimationManager;

    /// <summary>
    /// tab area animation manager
    /// </summary>
    private readonly AnimationManager tabCloseHoverAnimationManager;

    /// <summary>
    /// tab area animation manager
    /// </summary>
    private readonly AnimationManager formMenuHoverAnimationManager;

    private int previousSelectedPageIndex;
    private Point animationSource;
    private List<RectangleF> pageRect;
    private const int TAB_HEADER_PADDING = 9;
    private const int TAB_INDICATOR_HEIGHT = 3;

    private long _stickyBorderTime = 5000000;
    [Description("Set or get the maximum time to stay at the edge of the display(ms)")]
    [DefaultValue(500)]
    public long StickyBorderTime
    {
        get => _stickyBorderTime / 10000;
        set => _stickyBorderTime = value * 10000;
    }

    private const float HOVER_ANIMATION_SPEED = 0.1f;
    private const float HOVER_ANIMATION_OPACITY = 0.4f;
    private int _maxZOrder = 0;
    private Cursor _currentCursor;

    public new SDUI.Controls.ContextMenuStrip ContextMenuStrip { get; set; }

    private UIElementBase _focusedElement;
    private UIElementBase _lastHoveredElement;
    private Bitmap? _gdiBitmap;
    public SKSize CanvasSize => _cacheBitmap == null ? SKSize.Empty : new SKSize(_cacheBitmap.Width, _cacheBitmap.Height);

    public new ElementCollection Controls { get; }

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


    private WindowPageControl _windowPageControl;

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
            _windowPageControl.ControlAdded += delegate
            {
                Invalidate();
            };
            _windowPageControl.ControlRemoved += delegate
            {
                Invalidate();
            };
        }
    }

    public new IUIElement Parent { get; set; }

    private bool _layoutSuspended;
    private readonly Dictionary<string, SKPaint> _paintCache = new();
    private SKBitmap _cacheBitmap;
    private SKSurface _cacheSurface;
    private bool _needsFullRedraw = true;
    private int _suppressImmediateUpdateCount;

    private bool _showPerfOverlay;
    private long _perfLastTimestamp;
    private double _perfSmoothedFrameMs;

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

    private RenderBackend _renderBackend = RenderBackend.Software;
    private IWindowRenderer? _renderer;

    [DefaultValue(RenderBackend.Software)]
    [Description("Selects how UIWindow presents frames: Software (GDI), OpenGL, or DirectX11 (DXGI/GDI-compatible swapchain).")]
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

    /// <summary>
    /// The contructor
    /// </summary>
    public UIWindow()
        : base()
    {
        CheckForIllegalCrossThreadCalls = false;

        // WinForms double-buffering can cause visible flicker when the window is presented by
        // a GPU swapchain (OpenGL/DX). Keep it for software, disable it for GPU backends.
        ApplyRenderStyles();
        Controls = new(this);
        enableFullDraggable = false;

        pageAreaAnimationManager = new()
        {
            AnimationType = AnimationType.EaseOut,
            Increment = 0.07,
            Singular = true,
            InterruptAnimation = true
        };

        minBoxHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        maxBoxHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        closeBoxHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        extendBoxHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        tabCloseHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        newTabHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut,
            Singular = true,
            InterruptAnimation = true
        };

        formMenuHoverAnimationManager = new()
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

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;

            if (!DesignMode && _renderBackend != RenderBackend.Software)
            {
                cp.Style |= (int)(NativeMethods.SetWindowLongFlags.WS_CLIPCHILDREN | NativeMethods.SetWindowLongFlags.WS_CLIPSIBLINGS);
                // WS_EX_NOREDIRECTIONBITMAP helps some WGL/SwapBuffers flicker scenarios,
                // but can interfere with DXGI swapchains. Apply only for OpenGL.
                if (_renderBackend == RenderBackend.OpenGL)
                    cp.ExStyle |= (int)NativeMethods.SetWindowLongFlags.WS_EX_NOREDIRECTIONBITMAP;
                cp.ExStyle &= ~(int)NativeMethods.SetWindowLongFlags.WS_EX_COMPOSITED;
            }

            return cp;
        }
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

    private bool _inCloseBox, _inMaxBox, _inMinBox, _inExtendBox, _inTabCloseBox, _inNewTabBox, _inFormMenuBox;
    private static MouseEventArgs CreateChildMouseEvent(MouseEventArgs source, UIElementBase element)
    {
        // Kaynak koordinatı pencere tabanlı; elementi pencere tabanlı dikdörtgene çevir
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
            _cacheSurface?.Dispose();
            _cacheSurface = null;
            _cacheBitmap?.Dispose();
            _cacheBitmap = null;
            _gdiBitmap?.Dispose();
            _gdiBitmap = null;
            return;
        }

        if (_cacheBitmap == null || _cacheBitmap.Width != info.Width || _cacheBitmap.Height != info.Height)
        {
            _cacheSurface?.Dispose();
            _cacheBitmap?.Dispose();
            _gdiBitmap?.Dispose();

            _cacheBitmap = new SKBitmap(info);
            var pixels = _cacheBitmap.GetPixels();
            _cacheSurface = SKSurface.Create(info, pixels, _cacheBitmap.RowBytes);
            _gdiBitmap = new Bitmap(info.Width, info.Height, _cacheBitmap.RowBytes, PixelFormat.Format32bppPArgb, pixels);
            _needsFullRedraw = true;
        }
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);

        if (ShowTitle && !AllowAddControlOnTitle && e.Control.Top < TitleHeight)
        {
            e.Control.Top = Padding.Top;
        }
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
            _controlBoxRect = new(Width - _iconWidthDPI, 0, _iconWidthDPI, _titleHeightDPI);
            _controlBoxLeft = _controlBoxRect.Left - 2;

            if (MaximizeBox)
            {
                _maximizeBoxRect = new(_controlBoxRect.Left - _iconWidthDPI, _controlBoxRect.Top, _iconWidthDPI, _titleHeightDPI);
                _controlBoxLeft = _maximizeBoxRect.Left - 2;
            }
            else
            {
                _maximizeBoxRect = new(Width + 1, Height + 1, 1, 1);
            }

            if (MinimizeBox)
            {
                _minimizeBoxRect = new(MaximizeBox ? _maximizeBoxRect.Left - _iconWidthDPI - 2 : _controlBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top, _iconWidthDPI, _titleHeightDPI);
                _controlBoxLeft = _minimizeBoxRect.Left - 2;
            }
            else
            {
                _minimizeBoxRect = new Rectangle(Width + 1, Height + 1, 1, 1);
            }

            if (ExtendBox)
            {
                if (MinimizeBox)
                {
                    _extendBoxRect = new(_minimizeBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top, _iconWidthDPI, _titleHeightDPI);
                }
                else
                {
                    _extendBoxRect = new(_controlBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top, _iconWidthDPI, _titleHeightDPI);
                }
            }
        }
        else
        {
            _extendBoxRect = _maximizeBoxRect = _minimizeBoxRect = _controlBoxRect = new Rectangle(Width + 1, Height + 1, 1, 1);
        }

        var titleIconSize = 24 * DPI;
        _formMenuRect = new(10, _titleHeightDPI / 2 - (titleIconSize / 2), titleIconSize, titleIconSize);

        Padding = new Padding(Padding.Left, (int)(showTitle ? _titleHeightDPI : 0), Padding.Right, Padding.Bottom);
    }
    private void HandleTabKey(bool isShift)
    {
        var tabbableElements = Controls.OfType<UIElementBase>()
            .Where(e => e.Visible && e.Enabled && e.TabStop)
            .OrderBy(e => e.TabIndex)
            .ToList();

        if (tabbableElements.Count == 0) return;

        int currentIndex = _focusedElement != null ? tabbableElements.IndexOf(_focusedElement) : -1;

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
        if (keyData == Keys.Tab && _focusedElement is SDUI.Controls.TextBox tb && tb.AcceptsTab)
        {
            tb.OnKeyPress(new KeyPressEventArgs('\t'));
            return true;
        }

        if (keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift))
        {
            if (FocusManager.ProcessKeyNavigation(new KeyEventArgs(keyData)))
            {
                return true;
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_focusedElement != null)
        {
            _focusedElement.OnKeyDown(e);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (_focusedElement != null)
        {
            _focusedElement.OnKeyUp(e);
        }
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);

        if (_focusedElement != null)
        {
            _focusedElement.OnKeyPress(e);
        }
    }


    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        foreach (var element in Controls.OfType<UIElementBase>().OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            if (GetWindowRelativeBoundsStatic(element).Contains(e.Location))
            {
                var localEvent = CreateChildMouseEvent(e, element);
                element.OnMouseClick(localEvent);
                break;
            }
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
            {
                ExtendMenu.Show(PointToScreen(new Point(Convert.ToInt32(_extendBoxRect.Left), Convert.ToInt32(_titleHeightDPI - 1))));
            }
            else
            {
                OnExtendBoxClick?.Invoke(this, EventArgs.Empty);
            }
        }

        if (_inFormMenuBox)
        {
            _inFormMenuBox = false;
            if (FormMenu != null)
            {
                FormMenu.Show(PointToScreen(new Point(Convert.ToInt32(_formMenuRect.Left), Convert.ToInt32(_titleHeightDPI - 1))));
            }
            else
            {
                OnFormMenuClick?.Invoke(this, EventArgs.Empty);
            }
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

        for (int i = 0; i < pageRect.Count; i++)
        {
            if (pageRect[i].Contains(e.Location))
            {
                _windowPageControl.SelectedIndex = i;

                if (_tabCloseButton && e.Button == MouseButtons.Middle)
                    OnCloseTabBoxClick?.Invoke(null, i);

                break;
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        // Make sure this Form receives keyboard input.
        if (CanFocus)
            Focus();

        bool elementClicked = false;
        // Z-order'a göre tersten kontrol et (üstteki elementten başla)
        foreach (var element in Controls.OfType<UIElementBase>().OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            if (GetWindowRelativeBoundsStatic(element).Contains(e.Location))
            {
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
                // Tıklanan elementi en üste getir
                BringToFront(element);
                break; // İlk tıklanan elementten sonra diğerlerini kontrol etmeye gerek yok
            }
        }

        if (!elementClicked)
        {
            FocusManager.SetFocus(null);
            FocusedElement = null;
        }

        if (enableFullDraggable && e.Button == MouseButtons.Left)
        {
            //right = e.Button == MouseButtons.Right;
            //location = e.Location;
            DragForm(Handle);
        }

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

        bool elementClicked = false;
        // Z-order'a göre tersten kontrol et (üstteki elementten başla)
        foreach (var element in Controls.OfType<UIElementBase>().OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            if (GetWindowRelativeBoundsStatic(element).Contains(e.Location))
            {
                elementClicked = true;

                var localEvent = CreateChildMouseEvent(e, element);
                element.OnMouseDoubleClick(localEvent);
                // Tıklanan elementi en üste getir
                BringToFront(element);
                break; // İlk tıklanan elementten sonra diğerlerini kontrol etmeye gerek yok
            }
        }

        if (!elementClicked)
        {
            FocusManager.SetFocus(null);
            FocusedElement = null;
        }

        if (!MaximizeBox)
            return;

        bool inCloseBox = e.Location.InRect(_controlBoxRect);
        bool inMaxBox = e.Location.InRect(_maximizeBoxRect);
        bool inMinBox = e.Location.InRect(_minimizeBoxRect);
        bool inExtendBox = e.Location.InRect(_extendBoxRect);
        bool inCloseTabBox = _tabCloseButton && e.Location.InRect(_closeTabBoxRect);
        bool inNewTabBox = _newTabButton && e.Location.InRect(_newTabBoxRect);
        bool inFormMenuBox = e.Location.InRect(_formMenuRect);

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
        base.OnMouseUp(e);

        if (!IsDisposed && _formMoveMouseDown)
        {
            //int screenIndex = GetMouseInScreen(PointToScreen(e.Location));
            var screen = Screen.FromPoint(MousePosition);
            if (MousePosition.Y == screen.WorkingArea.Top && MaximizeBox)
            {
                ShowMaximize(true);
            }

            if (Top < screen.WorkingArea.Top)
            {
                Top = screen.WorkingArea.Top;
            }

            if (Top > screen.WorkingArea.Bottom - TitleHeight)
            {
                Top = Convert.ToInt32(screen.WorkingArea.Bottom - _titleHeightDPI);
            }
        }

        IsStayAtTopBorder = false;
        Cursor.Clip = new Rectangle();
        _formMoveMouseDown = false;

        animationSource = e.Location;

        // Z-order'a göre tersten kontrol et
        bool elementClicked = false;
        UIElementBase? hitElement = null;
        foreach (var element in Controls.OfType<UIElementBase>().OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            if (GetWindowRelativeBoundsStatic(element).Contains(e.Location))
            {
                elementClicked = true;
                hitElement = element;
                var localEvent = CreateChildMouseEvent(e, element);
                element.OnMouseUp(localEvent); 
                break;
            }
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
        if (_formMoveMouseDown && !MousePosition.Equals(_mouseOffset))
        {
            if (WindowState == FormWindowState.Maximized)
            {
                int maximizedWidth = Width;
                int locationX = Left;
                ShowMaximize();

                float offsetXRatio = 1 - (float)Width / maximizedWidth;
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
            bool inCloseBox = e.Location.InRect(_controlBoxRect);
            bool inMaxBox = e.Location.InRect(_maximizeBoxRect);
            bool inMinBox = e.Location.InRect(_minimizeBoxRect);
            bool inExtendBox = e.Location.InRect(_extendBoxRect);
            bool inCloseTabBox = _tabCloseButton && e.Location.InRect(_closeTabBoxRect);
            bool inNewTabBox = _newTabButton && e.Location.InRect(_newTabBoxRect);
            bool inFormMenuBox = e.Location.InRect(_formMenuRect);
            bool isChange = false;

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

        // Z-order'a göre tersten kontrol et
        foreach (var element in Controls.OfType<UIElementBase>().OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            if (GetWindowRelativeBoundsStatic(element).Contains(e.Location))
            {
                hoveredElement = element;
                var localEvent = CreateChildMouseEvent(e, element);
                element.OnMouseMove(localEvent);
                break; // İlk hover edilen elementten sonra diğerlerini kontrol etmeye gerek yok
            }
        }

        // Cursor should reflect the deepest hovered child (e.g., TextBox -> IBeam)
        UIElementBase cursorElement = hoveredElement;
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

        // Z-order'a göre tersten kontrol et
        foreach (var element in Controls.OfType<UIElementBase>().OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
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
        
        // Mouse pozisyonunu window client koordinatlarına çevir
        var mousePos = PointToClient(MousePosition);
        
        // Recursive olarak doğru child'ı bul ve wheel olayını ilet
        if (PropagateMouseWheel(Controls.OfType<UIElementBase>(), mousePos, e))
            return; // Event işlendi
    }
    
    /// <summary>
    /// Recursive olarak child elementlere mouse wheel olayını iletir
    /// </summary>
    private bool PropagateMouseWheel(IEnumerable<UIElementBase> elements, Point windowMousePos, MouseEventArgs e)
    {
        // Z-order'a göre tersten kontrol et - en üstteki element önce
        foreach (var element in elements.OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            var elementBounds = GetWindowRelativeBoundsStatic(element);
            if (!elementBounds.Contains(windowMousePos))
                continue;
            
            // Önce bu elementin child'larını kontrol et (daha spesifik -> daha genel)
            if (element.Controls != null && element.Controls.Count > 0)
            {
                var childElements = element.Controls.OfType<UIElementBase>();
                if (PropagateMouseWheel(childElements, windowMousePos, e))
                    return true; // Child işledi
            }
            
            // Child işlemediyse bu elemente gönder
            var localEvent = new MouseEventArgs(
                e.Button,
                e.Clicks,
                windowMousePos.X - elementBounds.X,
                windowMousePos.Y - elementBounds.Y,
                e.Delta);
            
            element.OnMouseWheel(localEvent);
            return true; // Event işlendi
        }
        
        return false; // Hiçbir element işlemedi
    }

    private void ShowMaximize(bool IsOnMoving = false)
    {
        Screen screen = Screen.FromPoint(MousePosition);
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
                int w = 800;
                if (MinimumSize.Width > 0) w = MinimumSize.Width;
                int h = 600;
                if (MinimumSize.Height > 0) h = MinimumSize.Height;
                _sizeOfBeforeMaximized = new Size(w, h);
            }

            Size = _sizeOfBeforeMaximized;
            if (_locationOfBeforeMaximized.X == 0 && _locationOfBeforeMaximized.Y == 0)
            {
                _locationOfBeforeMaximized = new Point(screen.Bounds.Left + screen.Bounds.Width / 2 - _sizeOfBeforeMaximized.Width / 2,
                  screen.Bounds.Top + screen.Bounds.Height / 2 - _sizeOfBeforeMaximized.Height / 2);
            }

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

    public new void Invalidate()
    {
        base.Invalidate();
        // Force immediate repaint for real-time responsiveness
        if (IsHandleCreated && !IsDisposed && !Disposing)
        {
            if (_suppressImmediateUpdateCount <= 0)
            {
                // In GPU-present modes, forcing synchronous Update() can increase tearing/flicker
                // and prevents WM_PAINT coalescing.
                if (_renderBackend == RenderBackend.Software)
                {
                    Update();
                }
            }
        }
    }

    private const int WM_ERASEBKGND = 0x0014;

    protected override void WndProc(ref Message m)
    {
        if (!DesignMode && _renderBackend != RenderBackend.Software && m.Msg == WM_ERASEBKGND)
        {
            m.Result = (IntPtr)1;
            return;
        }

        base.WndProc(ref m);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        Rectangle clientArea = ClientRectangle;
        Padding clientPadding = Padding;

        clientArea.X += clientPadding.Left;
        clientArea.Y += clientPadding.Top;
        clientArea.Width -= clientPadding.Horizontal;
        clientArea.Height -= clientPadding.Vertical;

        Rectangle remainingArea = clientArea;

        foreach (var control in Controls.OfType<UIElementBase>())
        {
             SDUI.LayoutEngine.Perform(control, clientArea, ref remainingArea);
        }
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
            return;
        }

        var info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
        var bmp = RenderSoftwareFrameToGdiBitmap(info);
        if (bmp != null)
        {
            e.Graphics.DrawImageUnscaled(bmp, 0, 0);
        }

        base.OnPaint(e);
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
        var style = (nint)stylePtr;
        var clipFlags = (nint)(uint)(NativeMethods.SetWindowLongFlags.WS_CLIPCHILDREN | NativeMethods.SetWindowLongFlags.WS_CLIPSIBLINGS);
        style = gpu ? (style | clipFlags) : (style & ~clipFlags);

        // Extended styles
        var exStylePtr = NativeMethods.GetWindowLong(hwnd, NativeMethods.WindowLongIndexFlags.GWL_EXSTYLE);
        var exStyle = (nint)exStylePtr;
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
            NativeMethods.SetWindowLongPtr64(hwnd, (int)NativeMethods.WindowLongIndexFlags.GWL_STYLE, (IntPtr)style);
            NativeMethods.SetWindowLongPtr64(hwnd, (int)NativeMethods.WindowLongIndexFlags.GWL_EXSTYLE, (IntPtr)exStyle);
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
        if (_cacheSurface == null)
            return null;

        var canvas = _cacheSurface.Canvas;
        RenderScene(canvas, info);
        canvas.Flush();
        _cacheSurface.Flush();

        return _gdiBitmap;
    }

    private void RenderScene(SKCanvas canvas, SKImageInfo info)
    {
        var gr = (_renderer as SDUI.Rendering.IGpuWindowRenderer)?.GrContext;
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
            foreach (var child in Controls.OfType<UIElementBase>())
            {
                child.InvalidateRenderTree();
            }
            _needsFullRedraw = false;
        }

        foreach (var element in Controls.OfType<UIElementBase>().OrderBy(el => el.ZOrder))
        {
            if (!element.Visible || element.Width <= 0 || element.Height <= 0)
                continue;

            element.Render(canvas);
        }

        if (_showPerfOverlay)
        {
            DrawPerfOverlay(canvas);
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
            : (_perfSmoothedFrameMs * 0.90) + (frameMs * 0.10);

        var fps = 1000.0 / Math.Max(0.001, _perfSmoothedFrameMs);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.ForeColor.ToSKColor(),
            TextSize = 12,
        };

        var backendLabel = _renderBackend.ToString();
        if (_renderBackend == SDUI.Rendering.RenderBackend.DirectX11 && _renderer is SDUI.Rendering.DirectX11WindowRenderer dx)
        {
            backendLabel = dx.IsSkiaGpuActive ? "DX:GPU" : "DX:CPU";
        }
        else if (_renderBackend == SDUI.Rendering.RenderBackend.OpenGL && _renderer is SDUI.Rendering.OpenGlWindowRenderer gl)
        {
            backendLabel = gl.IsSkiaGpuActive ? "GL:GPU" : "GL";
        }

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

        // Arka planı temizle
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
            hoverColor = foreColor.WithAlpha((byte)(20));
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
            hoverColor = foreColor.WithAlpha((byte)(20));
        }

        // Başlık alanı dışındaki içeriği tema arkaplanı ile doldur
        using (var contentBgPaint = new SKPaint { Color = ColorScheme.BackColor.ToSKColor() })
        {
            canvas.DrawRect(0, _titleHeightDPI, Width, Math.Max(0, Height - _titleHeightDPI), contentBgPaint);
        }

        // Kontrol düğmeleri çizimi
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

            // Çarpı işareti
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

        // Extend Box çizimi
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
                    (_titleHeightDPI / 2) - (hoverSize / 2),
                    _extendBoxRect.X + 20 * DPI + hoverSize,
                    (_titleHeightDPI / 2) + (hoverSize / 2)
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
                (_titleHeightDPI / 2) - (size / 2),
                _extendBoxRect.X + 24 * DPI + size,
                (_titleHeightDPI / 2) + (size / 2));

            canvas.DrawLine(
                iconRect.Left + iconRect.Width / 2 - (5 * DPI) - 1,
                iconRect.Top + iconRect.Height / 2 - (2 * DPI),
                iconRect.Left + iconRect.Width / 2 - (1 * DPI),
                iconRect.Top + iconRect.Height / 2 + (3 * DPI),
                extendPaint);

            canvas.DrawLine(
                iconRect.Left + iconRect.Width / 2 + (5 * DPI) - 1,
                iconRect.Top + iconRect.Height / 2 - (2 * DPI),
                iconRect.Left + iconRect.Width / 2 - (1 * DPI),
                iconRect.Top + iconRect.Height / 2 + (3 * DPI),
                extendPaint);
        }

        // Form Menu veya Icon çizimi
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
                _formMenuRect.Left + _formMenuRect.Width / 2 - (5 * DPI) - 1,
                _formMenuRect.Top + _formMenuRect.Height / 2 - (2 * DPI),
                _formMenuRect.Left + _formMenuRect.Width / 2 - (1 * DPI),
                _formMenuRect.Top + _formMenuRect.Height / 2 + (3 * DPI),
                menuPaint);

            canvas.DrawLine(
                _formMenuRect.Left + _formMenuRect.Width / 2 + (5 * DPI) - 1,
                _formMenuRect.Top + _formMenuRect.Height / 2 - (2 * DPI),
                _formMenuRect.Left + _formMenuRect.Width / 2 - (1 * DPI),
                _formMenuRect.Top + _formMenuRect.Height / 2 + (3 * DPI),
                menuPaint);
        }
        else
        {
            if (ShowIcon && Icon != null)
            {
                using var bitmap = Icon.ToBitmap();
                using var skBitmap = bitmap.ToSKBitmap();
                using var image = SKImage.FromBitmap(skBitmap);
                var iconRect = SKRect.Create(10, (_titleHeightDPI / 2) - (faviconSize / 2), faviconSize, faviconSize);
                canvas.DrawImage(image, iconRect);
            }
        }

        // Form başlığı çizimi
        if (_windowPageControl == null || _windowPageControl.Count == 0)
        {
            using var font = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
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
            var textX = showMenuInsteadOfIcon ? _formMenuRect.X + _formMenuRect.Width + (8 * DPI) : faviconSize + (14 * DPI);
            var textY = (_titleHeightDPI / 2) + (Math.Abs(font.Metrics.Ascent + font.Metrics.Descent) / 2);

            TextRenderingHelper.DrawText(canvas, Text, textX, textY, SKTextAlign.Left, font, textPaint);
        }

        // Tab kontrollerinin çizimi
        if (_windowPageControl != null && _windowPageControl.Count > 0)
        {
            if (!pageAreaAnimationManager.IsAnimating() || pageRect == null || pageRect.Count != _windowPageControl.Count)
                UpdateTabRects();

            var animationProgress = pageAreaAnimationManager.GetProgress();

            // Click feedback
            if (pageAreaAnimationManager.IsAnimating())
            {
                using var ripplePaint = new SKPaint
                {
                    Color = foreColor.WithAlpha((byte)(31 - (animationProgress * 30))),
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

            var previousSelectedPageIndexIfHasOne = previousSelectedPageIndex == -1 ? _windowPageControl.SelectedIndex : previousSelectedPageIndex;
            var previousActivePageRect = pageRect[previousSelectedPageIndexIfHasOne];
            var activePageRect = pageRect[_windowPageControl.SelectedIndex];

            var y = activePageRect.Bottom - 2;
            var x = previousActivePageRect.X + (activePageRect.X - previousActivePageRect.X) * (float)animationProgress;
            var width = previousActivePageRect.Width + (activePageRect.Width - previousActivePageRect.Width) * (float)animationProgress;

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
                        Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
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
                    var iconX = rect.X + (TAB_HEADER_PADDING * DPI);

                    var inlinePaddingX = startingIconBounds.Width + (TAB_HEADER_PADDING * DPI);
                    rect.X += inlinePaddingX;
                    rect.Width -= inlinePaddingX + closeIconSize;

                    var textY = (_titleHeightDPI / 2) + (Math.Abs(font.Metrics.Ascent + font.Metrics.Descent) / 2);
                    TextRenderingHelper.DrawText(canvas, "", iconX, textY, SKTextAlign.Center, font, textPaint);

                    var bounds = new SKRect();
                    font.MeasureText(page.Text, out bounds);
                    var textX = rect.X + (rect.Width / 2);
                    TextRenderingHelper.DrawText(canvas, page.Text, textX, textY, SKTextAlign.Center, font, textPaint);
                }
                else
                {
                    using var font = new SKFont
                    {
                        Size = 9f.PtToPx(this),
                        Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
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
                    var textX = rect.X + (rect.Width / 2);
                    var textY = (_titleHeightDPI / 2) + (Math.Abs(font.Metrics.Ascent + font.Metrics.Descent) / 2);
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

                _closeTabBoxRect = new RectangleF(x + width - TAB_HEADER_PADDING / 2 - size, _titleHeightDPI / 2 - size / 2, size, size);
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
                    Color = newHoverColor.WithAlpha((byte)(newTabHoverAnimationManager.GetProgress() * newHoverColor.Alpha)),
                    IsAntialias = true
                };

                var lastTabRect = pageRect[pageRect.Count - 1];
                _newTabBoxRect = new RectangleF(lastTabRect.X + lastTabRect.Width + size / 2, _titleHeightDPI / 2 - size / 2, size, size);
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
                Color = titleColor != Color.Empty ?
                    titleColor.Determine().ToSKColor().WithAlpha(30) :
                    ColorScheme.BorderColor.ToSKColor(),
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
    }

    private void UpdateTabRects()
    {
        pageRect = new();

        if (_windowPageControl == null || _windowPageControl.Count == 0)
            return;

        float tabAreaWidth = 44 * DPI;

        if (controlBox)
            tabAreaWidth += _controlBoxRect.Width;

        if (MinimizeBox)
            tabAreaWidth += _minimizeBoxRect.Width;

        if (MaximizeBox)
            tabAreaWidth += _maximizeBoxRect.Width;

        if (ExtendBox)
            tabAreaWidth += _extendBoxRect.Width;

        float maxSize = 250f * DPI;

        tabAreaWidth = (Width - tabAreaWidth - (30 * DPI)) / _windowPageControl.Count;
        if (tabAreaWidth > maxSize)
            tabAreaWidth = maxSize;

        pageRect.Add(new(44 * DPI, 0, tabAreaWidth, _titleHeightDPI));
        for (int i = 1; i < _windowPageControl.Count; i++)
            pageRect.Add(new(pageRect[i - 1].Right, 0, tabAreaWidth, _titleHeightDPI));
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

    public new void PerformLayout()
    {
        foreach (UIElementBase element in Controls)
            element.PerformLayout();

        Invalidate();
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
        if (_layoutSuspended) return;

        element.InvalidateRenderTree();

        if (!_needsFullRedraw)
            Invalidate();
    } 

    public new void ResumeLayout()
    {
        ResumeLayout(true);
    }

    public new void SuspendLayout()
    {
        _layoutSuspended = true;
    }

    public new void ResumeLayout(bool performLayout)
    {
        _layoutSuspended = false;
        if (performLayout)
        {
            _needsFullRedraw = true;
            PerformLayout();
            Invalidate();
        }
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
            _gdiBitmap?.Dispose();
            _gdiBitmap = null;
        }
        base.Dispose(disposing);
    }

    public void UpdateZOrder() { }
}