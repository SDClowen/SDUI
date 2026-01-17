using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SDUI.Collections;
using SDUI.Helpers;
using SDUI.Layout;
using SDUI.Validations;
using SkiaSharp;

namespace SDUI.Controls;

public abstract partial class UIElementBase : IUIElement, IArrangedElement, IDisposable
{
    private static int s_globalLayoutPassId;
    public bool _childControlsNeedAnchorLayout { get; set; }
    public bool _forceAnchorCalculations { get; set; }

    // Layout pass tracking for Measure/Arrange caching
    private Size? _cachedMeasure;

    private float _currentDpi = 96f;

    private Image _image;

    // Guard to prevent layout during Arrange phase
    private bool _isArranging;

    // Guard to prevent re-entrant PerformLayout calls from causing stack overflows.
    private Size _lastMeasureConstraint;
    private int _layoutPassId;

    // Use a counter to support nested SuspendLayout/ResumeLayout like WinForms.
    // When > 0 layout is suspended; when it reaches 0 we allow layouts again.
    protected int _layoutSuspendCount;

    public int LayoutSuspendCount { get => _layoutSuspendCount; set => _layoutSuspendCount = value; }

    private IUIElement _parent;

    public bool IsHandleCreated;

    public UIElementBase()
    {
        IsDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        Controls = new ElementCollection(this);
        Properties = new();

        _font = new Font("Segoe UI", 9f);
        _cursor = Cursors.Default;
        _currentDpi = DpiHelper.GetSystemDpi();
    }

    public Image BackgroundImage { get; set; }
    public ContentAlignment ImageAlign { get; set; }
    public ImageLayout BackgroundImageLayout { get; set; }

    public RightToLeft RightToLeft { get; set; } = RightToLeft.No;
    public SizeF AutoScaleDimensions { get; set; }
    public AutoScaleMode AutoScaleMode { get; set; }

    public bool Disposing { get; set; }
    public bool CheckForIllegalCrossThreadCalls { get; set; }

    public bool InvokeRequired => false;
    public bool AutoScroll { get; set; }
    public Size AutoScrollMargin { get; set; }

    public Image Image
    {
        get => _image;
        set
        {
            if (_image == value)
                return;

            _image = value;
            InvalidateMeasure();
            Invalidate();
        }
    }

    protected bool IsDesignMode { get; }

    protected bool IsPerformingLayout { get; private set; }

    public UIWindowBase? ParentWindow
    {
        get
        {
            return _parent switch
            {
                UIWindowBase window => window,
                UIElementBase element => element.ParentWindow,
                _ => null
            };
        }
    }

    public UIElementBase? ParentElement => _parent as UIElementBase;

    public bool HasParent => _parent != null;
    public object Tag { get; set; }

    public IUIElement Parent
    {
        get => _parent;
        set
        {
            if (_parent == value)
                return;

            _parent = value;
            UpdateCurrentDpiFromParent();
            NeedsRedraw = true;
        }
    }

    public void BringToFront()
    {
        switch (Parent)
        {
            case UIWindow window:
                window.BringToFront(this);
                break;
            case UIElementBase parentElement:
                var siblings = parentElement.Controls.OfType<UIElementBase>().ToList();
                if (siblings.Count == 0) return;
                var max = siblings.Max(s => s.ZOrder);
                ZOrder = max + 1;
                parentElement.InvalidateRenderTree();
                break;
        }
    }

    [Browsable(false)]
    public ElementCollection Controls { get; }

    public void BeginInvoke(Delegate method)
    {
        method.DynamicInvoke();
    }

    public void BeginInvoke(Delegate method, params object[] args)
    {
        method.DynamicInvoke(args);
    }

    public void Invoke(Delegate method)
    {
        method.DynamicInvoke();
    }

    public void Invoke(Delegate method, params object[] args)
    {
        method.DynamicInvoke(args);
    }

    public void Show()
    {
        Visible = true;
    }

    public void Hide()
    {
        Visible = false;
    }

    private void UpdateCurrentDpiFromParent()
    {
        if (_parent is UIWindowBase window && window.IsHandleCreated)
            _currentDpi = DpiHelper.GetDpiForWindowInternal(window.Handle);
        else if (_parent is UIElementBase element)
            _currentDpi = element.ScaleFactor * 96f;
        else
            _currentDpi = DpiHelper.GetSystemDpi();
    }

    /// <summary>
    ///     Override this to clear cached font objects when DPI changes.
    /// </summary>
    protected virtual void InvalidateFontCache()
    {
        // Base implementation does nothing - derived controls override to clear their font caches
    }

    private static Padding ScalePadding(Padding padding, float scaleFactor)
    {
        if (Math.Abs(scaleFactor - 1f) < 0.001f)
            return padding;

        static int Scale(int value, float factor)
        {
            return Math.Max(0, (int)Math.Round(value * factor));
        }

        return new Padding(
            Scale(padding.Left, scaleFactor),
            Scale(padding.Top, scaleFactor),
            Scale(padding.Right, scaleFactor),
            Scale(padding.Bottom, scaleFactor));
    }

    protected bool IsChildOf(UIWindowBase window)
    {
        return ParentWindow == window;
    }

    protected bool IsChildOf(UIElementBase element)
    {
        return ParentElement == element;
    }

    public void SendToBack()
    {
        switch (Parent)
        {
            case UIWindow window:
                window.SendToBack(this);
                break;
            case UIElementBase parentElement:
                var siblings = parentElement.Controls.OfType<UIElementBase>().ToList();
                if (siblings.Count == 0) return;
                var min = siblings.Min(s => s.ZOrder);
                ZOrder = min - 1;
                parentElement.InvalidateRenderTree();
                break;
        }
    }

    #region Properties

    private Point _location;

    public virtual Point Location
    {
        get => _location;
        set
        {
            if (_location == value)
                return;

            _location = value;

            OnLocationChanged(EventArgs.Empty);
        }
    }

    [ThreadStatic] private static GRContext? s_currentGpuContext;

    private static SKColorSpace? s_srgbColorSpace;

    private static SKColorSpace? SrgbColorSpace
    {
        get
        {
            if (s_srgbColorSpace == null)
                try
                {
                    s_srgbColorSpace = SKColorSpace.CreateSrgb();
                }
                catch
                {
                    // Skia native initialization failed in this environment; fall back to null (device/default color space).
                    s_srgbColorSpace = null;
                }

            return s_srgbColorSpace;
        }
    }

    internal static IDisposable PushGpuContext(GRContext? context)
    {
        var prior = s_currentGpuContext;
        s_currentGpuContext = context;
        return new GpuContextScope(prior);
    }

    private sealed class GpuContextScope : IDisposable
    {
        private readonly GRContext? _prior;
        private bool _disposed;

        public GpuContextScope(GRContext? prior)
        {
            _prior = prior;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            s_currentGpuContext = _prior;
            _disposed = true;
        }
    }

    private Size _minimumSize;

    [Category("Layout")]
    [DefaultValue(typeof(Size), "0, 0")]
    public virtual Size MinimumSize
    {
        get => _minimumSize;
        set
        {
            if (_minimumSize == value) return;
            _minimumSize = value;
            if (Size.Width < _minimumSize.Width || Size.Height < _minimumSize.Height)
                Size = new Size(
                    Math.Max(Size.Width, _minimumSize.Width),
                    Math.Max(Size.Height, _minimumSize.Height)
                );
        }
    }

    private Size _maximumSize;

    [Category("Layout")]
    [DefaultValue(typeof(Size), "0, 0")]
    public virtual Size MaximumSize
    {
        get => _maximumSize;
        set
        {
            if (_maximumSize == value) return;
            _maximumSize = value;
            if ((_maximumSize.Width > 0 && Size.Width > _maximumSize.Width) ||
                (_maximumSize.Height > 0 && Size.Height > _maximumSize.Height))
                Size = new Size(
                    _maximumSize.Width > 0 ? Math.Min(Size.Width, _maximumSize.Width) : Size.Width,
                    _maximumSize.Height > 0 ? Math.Min(Size.Height, _maximumSize.Height) : Size.Height
                );
        }
    }

    private Size _size;

    public virtual Size Size
    {
        get => _size;
        set
        {
            var newSize = value;

            // MinimumSize kontrolü
            if (MinimumSize.Width > 0)
                newSize.Width = Math.Max(newSize.Width, MinimumSize.Width);
            if (MinimumSize.Height > 0)
                newSize.Height = Math.Max(newSize.Height, MinimumSize.Height);

            // MaximumSize kontrolü
            if (MaximumSize.Width > 0)
                newSize.Width = Math.Min(newSize.Width, MaximumSize.Width);
            if (MaximumSize.Height > 0)
                newSize.Height = Math.Min(newSize.Height, MaximumSize.Height);

            if (_size == newSize) return;
            _size = newSize;
            OnSizeChanged(EventArgs.Empty);
        }
    }

    public Rectangle Bounds
    {
        get => new(Location, Size);
        set
        {
            Location = value.Location;

            Size = value.Size;
        }
    }

    /// <summary>
    ///  Retrieves our internal property storage object. If you have a property
    ///  whose value is not always set, you should store it in here to save space.
    /// </summary>
    public PropertyStore Properties { get; }

    public Rectangle ClientRectangle => new(0, 0, Size.Width, Size.Height);

    /// <summary>
    ///     Gets the rectangle that represents the display area of the control (client area minus padding).
    ///     This is where child controls are positioned.
    /// </summary>
    public virtual Rectangle DisplayRectangle
    {
        get
        {
            var padding = Padding;
            return new Rectangle(
                padding.Left,
                padding.Top,
                Math.Max(0, Size.Width - padding.Horizontal),
                Math.Max(0, Size.Height - padding.Vertical)
            );
        }
    }

    public Size ClientSize
    {
        get => Size;
        set => Size = value;
    }

    private Size _autoScrollMinSize;

    [Category("Layout")]
    [DefaultValue(typeof(Size), "0, 0")]
    public virtual Size AutoScrollMinSize
    {
        get => _autoScrollMinSize;
        set
        {
            if (_autoScrollMinSize == value) return;
            _autoScrollMinSize = value;

            // Trigger layout so containers can re-evaluate scrollbar visibility
            if (Parent is UIWindowBase parentWindow)
                parentWindow.PerformLayout();
            else if (Parent is UIElementBase parentElement)
                parentElement.PerformLayout();
            else
                PerformLayout();
        }
    }

    private bool _visible = true;

    public virtual bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value)
                return;

            _visible = value;

            OnVisibleChanged(EventArgs.Empty);
        }
    }

    private bool _enabled = true;

    public virtual bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
                return;

            _enabled = value;

            OnEnabledChanged(EventArgs.Empty);
        }
    }

    private Color _backColor = Color.Transparent;

    public virtual Color BackColor
    {
        get => _backColor;
        set
        {
            if (_backColor == value)
                return;

            _backColor = value;

            OnBackColorChanged(EventArgs.Empty);

            Invalidate();
        }
    }

    private Color _foreColor = Color.Black;

    public virtual Color ForeColor
    {
        get => _foreColor;
        set
        {
            if (_foreColor == value)
                return;

            _foreColor = value;

            OnForeColorChanged(EventArgs.Empty);

            Invalidate();
        }
    }

    private Font _font;

    public virtual Font Font
    {
        get => _font;
        set
        {
            if (_font == value)
                return;

            _font = value;

            OnFontChanged(EventArgs.Empty);
            InvalidateMeasure();
            Invalidate();
        }
    }

    private string _text = string.Empty;

    public virtual string Text
    {
        get => _text;
        set
        {
            if (_text == value)
                return;

            _text = value;

            OnTextChanged(EventArgs.Empty);
            InvalidateMeasure();
            Invalidate();
        }
    }

    private Padding _padding;

    public virtual Padding Padding
    {
        get => _padding;
        set
        {
            if (_padding == value)
                return;

            _padding = value;

            OnPaddingChanged(EventArgs.Empty);
            InvalidateMeasure();
            Invalidate();
        }
    }

    private Padding _margin;

    public virtual Padding Margin
    {
        get => _margin;
        set
        {
            if (_margin == value)
                return;

            _margin = value;

            OnMarginChanged(EventArgs.Empty);
            InvalidateMeasure();
            Invalidate();
        }
    }

    private bool _tabStop = true;

    public virtual bool TabStop
    {
        get => _tabStop;
        set
        {
            if (_tabStop == value)
                return;

            _tabStop = value;

            OnTabStopChanged(EventArgs.Empty);
        }
    }

    private int _tabIndex;

    public virtual int TabIndex
    {
        get => _tabIndex;
        set
        {
            if (_tabIndex == value)
                return;

            _tabIndex = value;

            OnTabIndexChanged(EventArgs.Empty);
        }
    }

    private AnchorStyles _anchor = AnchorStyles.Top | AnchorStyles.Left;
    internal Layout.AnchorInfo? _anchorInfo;

    public virtual AnchorStyles Anchor
    {
        get => _anchor;
        set
        {
            if (_anchor == value)
                return;

            _anchor = value;
            // Reset anchor info when anchor style changes
            _anchorInfo = null;

            OnAnchorChanged(EventArgs.Empty);
        }
    }

    private DockStyle _dock = DockStyle.None;

    public virtual DockStyle Dock
    {
        get => _dock;
        set
        {
            if (_dock == value)
                return;

            _dock = value;

            OnDockChanged(EventArgs.Empty);
        }
    }

    private bool _autoSize;

    public virtual bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize == value)
                return;

            _autoSize = value;

            if (value)
                AdjustSize();

            OnAutoSizeChanged(EventArgs.Empty);
        }
    }

    private AutoSizeMode _autoSizeMode = AutoSizeMode.GrowAndShrink;

    public virtual AutoSizeMode AutoSizeMode
    {
        get => _autoSizeMode;
        set
        {
            if (_autoSizeMode == value)
                return;

            _autoSizeMode = value;

            if (AutoSize)
                AdjustSize();

            OnAutoSizeModeChanged(EventArgs.Empty);
        }
    }

    [Browsable(false)] internal int ZOrder { get; set; }

    public int Width
    {
        get => Size.Width;
        set => Size = new Size(value, Height);
    }

    public int Height
    {
        get => Size.Height;
        set => Size = new Size(Width, value);
    }

    private bool _canSelect = true;

    [Category("Behavior")]
    [DefaultValue(true)]
    public virtual bool CanSelect
    {
        get => _canSelect;
        set
        {
            if (_canSelect == value) return;
            _canSelect = value;
        }
    }

    private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;

    [Category("Appearance")]
    [DefaultValue(ContentAlignment.MiddleCenter)]
    public virtual ContentAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            if (_textAlign == value) return;
            _textAlign = value;
            Invalidate();
        }
    }

    private bool _useMnemonic = true;

    [Category("Behavior")]
    [DefaultValue(true)]
    public virtual bool UseMnemonic
    {
        get => _useMnemonic;
        set
        {
            if (_useMnemonic == value) return;
            _useMnemonic = value;
            Invalidate();
        }
    }

    private bool _autoEllipsis;

    [Category("Behavior")]
    [DefaultValue(false)]
    public virtual bool AutoEllipsis
    {
        get => _autoEllipsis;
        set
        {
            if (_autoEllipsis == value) return;
            _autoEllipsis = value;
            Invalidate();
        }
    }

    private bool _selectable = true;

    [Category("Behavior")]
    [DefaultValue(true)]
    public virtual bool Selectable
    {
        get => _selectable && Enabled;
        set
        {
            if (_selectable == value) return;
            _selectable = value;
            Invalidate();
        }
    }

    [Browsable(false)] public bool Focused { get; internal set; }

    private string _name = string.Empty;

    [Category("Design")]
    [DefaultValue("")]
    public virtual string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
        }
    }

    private bool _useVisualStyleBackColor = true;

    [Category("Appearance")]
    [DefaultValue(true)]
    public virtual bool UseVisualStyleBackColor
    {
        get => _useVisualStyleBackColor;
        set
        {
            if (_useVisualStyleBackColor == value) return;
            _useVisualStyleBackColor = value;
            Invalidate();
        }
    }

    private Cursor _cursor = Cursors.Default;

    [Category("Appearance")]
    [DefaultValue(typeof(Cursor), "Default")]
    public virtual Cursor Cursor
    {
        get => _cursor;
        set
        {
            if (_cursor == value) return;
            _cursor = value;
            OnCursorChanged(EventArgs.Empty);
        }
    }

    protected bool IsCreated { get; private set; }

    [Browsable(false)] public virtual int DeviceDpi => (int)Math.Round(_currentDpi);

    [Browsable(false)] public virtual float ScaleFactor => _currentDpi / 96f;

    [Browsable(false)]
    protected virtual Keys ModifierKeys
    {
        get
        {
            var modifiers = Keys.None;

            if ((GetKeyState(VK_SHIFT) & 0x8000) != 0)
                modifiers |= Keys.Shift;

            if ((GetKeyState(VK_CONTROL) & 0x8000) != 0)
                modifiers |= Keys.Control;

            if ((GetKeyState(VK_MENU) & 0x8000) != 0)
                modifiers |= Keys.Alt;

            return modifiers;
        }
    }

    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12;

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    public bool IsDisposed { get; private set; }

    private UIElementBase _focusedElement;
    private UIElementBase _lastHoveredElement;


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
            if (_lastHoveredElement != value) _lastHoveredElement = value;
            //UpdateCursor(value);
        }
    }

    private ContextMenuStrip _contextMenuStrip;

    [Category("Behavior")]
    [DefaultValue(null)]
    public ContextMenuStrip ContextMenuStrip
    {
        get => _contextMenuStrip;
        set
        {
            if (_contextMenuStrip == value) return;
            _contextMenuStrip = value;
        }
    }

    #endregion

    #region Validation Properties

    private bool _causesValidation = true;

    [Category("Behavior")]
    [DefaultValue(true)]
    public virtual bool CausesValidation
    {
        get => _causesValidation;
        set
        {
            if (_causesValidation == value) return;
            _causesValidation = value;
        }
    }

    private string _validationText = string.Empty;

    [Category("Appearance")]
    [DefaultValue("")]
    public virtual string ValidationText
    {
        get => _validationText;
        set
        {
            if (_validationText == value) return;
            _validationText = value;
            Invalidate();
        }
    }

    private bool _isValid = true;

    [Browsable(false)]
    public virtual bool IsValid
    {
        get => _isValid;
        protected set
        {
            if (_isValid == value) return;
            _isValid = value;
            Invalidate();
        }
    }

    public bool NeedsRedraw { get; set; } = true;

    private readonly List<ValidationRule> _validationRules = new();

    [Browsable(false)] public IReadOnlyList<ValidationRule> ValidationRules => _validationRules.AsReadOnly();

    #endregion

    #region Events

    public event EventHandler Click;

    public event EventHandler DoubleClick;

    public event MouseEventHandler MouseMove;

    public event MouseEventHandler MouseDown;

    public event MouseEventHandler MouseUp;

    public event MouseEventHandler MouseClick;

    public event MouseEventHandler MouseDoubleClick;

    public event EventHandler MouseEnter;

    public event EventHandler MouseLeave;

    public event EventHandler MouseHover;

    public event EventHandler<SKPaintSurfaceEventArgs> Paint;

    public event EventHandler LocationChanged;

    public event EventHandler SizeChanged;

    public event EventHandler VisibleChanged;

    public event EventHandler EnabledChanged;

    public event EventHandler TextChanged;

    public event EventHandler BackColorChanged;

    public event EventHandler ForeColorChanged;

    public event EventHandler FontChanged;

    public event EventHandler PaddingChanged;

    public event EventHandler MarginChanged;

    public event EventHandler TabStopChanged;

    public event EventHandler TabIndexChanged;

    public event EventHandler AnchorChanged;

    public event EventHandler DockChanged;

    public event EventHandler AutoSizeChanged;

    public event EventHandler AutoSizeModeChanged;

    public event KeyEventHandler KeyDown;

    public event KeyEventHandler KeyUp;

    public event KeyPressEventHandler KeyPress;

    public event EventHandler GotFocus;

    public event EventHandler LostFocus;

    public event EventHandler Enter;

    public event EventHandler Leave;

    public event EventHandler Validated;

    public event CancelEventHandler Validating;

    public event EventHandler CursorChanged;
    public event EventHandler CreateControl;

    public event UILayoutEventHandler Layout;
    public event UIElementEventHandler ControlAdded;
    public event UIElementEventHandler ControlRemoved;

    public event MouseEventHandler MouseWheel;

    public event EventHandler DpiChanged;

    // Fired when the element is loaded (parent window has finished loading). Raised once per element.
    public event EventHandler? Load;

    private bool _isLoaded;

    /// <summary>
    ///     Raises the Load event for this element. This is safe to call multiple times; the event will only fire once.
    /// </summary>
    protected virtual void OnLoad(EventArgs e)
    {
        if (_isLoaded) return;
        _isLoaded = true;
        Load?.Invoke(this, e);
    }

    /// <summary>
    ///     Ensures Load has been raised for this element and all its child elements (recursively).
    /// </summary>
    public void EnsureLoadedRecursively()
    {
        OnLoad(EventArgs.Empty);
        for (var i = 0; i < Controls.Count; i++) Controls[i].EnsureLoadedRecursively();
    }

    // Fired when the element is unloaded (parent window is closing or control removed from a loaded window).
    public event EventHandler? Unload;

    /// <summary>
    ///     Raises the Unload event for this element. This is safe to call multiple times; it will fire only when the element
    ///     was previously loaded.
    /// </summary>
    protected virtual void OnUnload(EventArgs e)
    {
        if (!_isLoaded) return; // only unload if previously loaded
        _isLoaded = false;
        Unload?.Invoke(this, e);
    }

    /// <summary>
    ///     Ensures Unload has been raised for this element and all its child elements (recursively).
    /// </summary>
    public void EnsureUnloadedRecursively()
    {
        OnUnload(EventArgs.Empty);
        for (var i = 0; i < Controls.Count; i++) Controls[i].EnsureUnloadedRecursively();
    }

    #endregion

    #region Virtual Methods

    internal Size ApplySizeConstraints(Size proposedSize)
    {
        return ApplyBoundsConstraints(0, 0, proposedSize.Width, proposedSize.Height).Size;
    }

    internal virtual Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
    {
        // COMPAT: in Everett we would allow you to set negative values in pre-handle mode
        // in Whidbey, if you've set Min/Max size we will constrain you to 0,0. Everett apps didnt
        // have min/max size on control, which is why this works.
        if (MaximumSize != Size.Empty || MinimumSize != Size.Empty)
        {
            Size maximumSize = LayoutUtils.ConvertZeroToUnbounded(MaximumSize);
            Rectangle newBounds = new(suggestedX, suggestedY, 0, 0)
            {
                // Clip the size to maximum and inflate it to minimum as necessary.
                Size = LayoutUtils.IntersectSizes(new Size(proposedWidth, proposedHeight), maximumSize)
            };
            newBounds.Size = LayoutUtils.UnionSizes(newBounds.Size, MinimumSize);

            return newBounds;
        }

        return new Rectangle(suggestedX, suggestedY, proposedWidth, proposedHeight);
    }

    public virtual void OnPaint(SKCanvas canvas)
    {

        Paint?.Invoke(this, new SKPaintSurfaceEventArgs(canvas.Surface, default));
    }

    public virtual void Invalidate()
    {
        CheckDisposed();

        MarkDirty();

        // Only propagate to window if this isn't already the window
        // This prevents cascade invalidations that kill FPS
        if (!(this is UIWindowBase))
        {
            var window = GetParentWindow();
            window?.Invalidate();
        }
    }

    protected void MarkDirty()
    {
        NeedsRedraw = true;
        
        // DEBUG: Log excessive invalidations
        if (DebugSettings.EnableRenderLogging)
        {
            DebugSettings.Log($"MarkDirty called on {GetType().Name}");
        }
    }

    internal void InvalidateRenderTree()
    {
        MarkDirty();
        for (var i = 0; i < Controls.Count; i++)
            if (Controls[i] is UIElementBase child)
                child.InvalidateRenderTree();
    }



    private SKColor ResolveBackgroundColor()
    {
        var color = BackColor;
        return color == Color.Transparent
            ? SKColors.Transparent
            : color.ToSKColor();
    }

    private void RenderChildren(SKCanvas canvas)
    {
        foreach (var child in Controls
                     .OfType<IUIElement>()
                     .OrderBy(el => el.ZOrder)
                     .ThenBy(el => el.TabIndex))
            child.Render(canvas);
    }

    private void RenderUncached(SKCanvas targetCanvas)
    {
        var saved = targetCanvas.Save();

        // Translate to element bounds
        targetCanvas.Translate(Location.X, Location.Y);

        // Draw background
        var bg = ResolveBackgroundColor();
        if (bg != SKColors.Transparent)
        {
            using var paint = new SKPaint { Color = bg };
            targetCanvas.DrawRect(0, 0, Width, Height, paint);
        }

        OnPaint(targetCanvas);
        // Paint?.Invoke(this, args); 

        // Clip children to bounds
        targetCanvas.ClipRect(SKRect.Create(0, 0, Width, Height));
        RenderChildren(targetCanvas);

        targetCanvas.RestoreToCount(saved);
    }


    public void Render(SKCanvas targetCanvas)
    {
        // Disposed, null canvas ve geçersiz boyutları kontrol et
        if (IsDisposed || targetCanvas == null || !Visible || Width <= 0 || Height <= 0)
        {
            return;
        }

        try
        {
            RenderUncached(targetCanvas);
        }
        catch (Exception ex)
        {
            if (DebugSettings.EnableRenderLogging)
                DebugSettings.Log(
                    $"UIElementBase: Render failed for element {GetType().Name} ({Width}x{Height}): {ex.GetType().Name} - {ex.Message}");
        }
    }

    public virtual void Focus()
    {
        switch (Parent)
        {
            case UIElementBase element:
                element.FocusedElement = this;
                break;
            case UIWindow window:
                window.FocusedElement = this;
                break;
            case UIWindowBase windowBase:
                Focused = true;
                windowBase.Invalidate();
                OnGotFocus(EventArgs.Empty);
                break;
            default:
                Focused = true;
                OnGotFocus(EventArgs.Empty);
                break;
        }
    }

    // Explicit interface implementations to satisfy IUIElement contract
    IUIElement IUIElement.FocusedElement
    {
        get => _focusedElement;
        set => _focusedElement = value as UIElementBase;
    }

    int IUIElement.ZOrder
    {
        get => ZOrder;
        set => ZOrder = value;
    }
    public bool IsAncestorSiteInDesignMode { get; internal set; }

    protected virtual void AdjustSize()
    {
        if (!AutoSize)
            return;

        var proposedSize = GetPreferredSize(Size.Empty);

        if (AutoSizeMode == AutoSizeMode.GrowOnly)
        {
            proposedSize.Width = Math.Max(Size.Width, proposedSize.Width);
            proposedSize.Height = Math.Max(Size.Height, proposedSize.Height);
        }

        // MinimumSize ve MaximumSize kontrolü
        proposedSize.Width = Math.Max(proposedSize.Width, MinimumSize.Width);
        if (MinimumSize.Height > 0)
            proposedSize.Height = Math.Max(proposedSize.Height, MinimumSize.Height);

        if (MaximumSize.Width > 0)
            proposedSize.Width = Math.Min(proposedSize.Width, MaximumSize.Width);
        if (MaximumSize.Height > 0)
            proposedSize.Height = Math.Min(proposedSize.Height, MaximumSize.Height);

        if (Size != proposedSize)
            Size = proposedSize;
    }

    /// <summary>
    ///     Measures the element and returns its desired size given the available space.
    ///     This is the first phase of the layout pass.
    /// </summary>
    /// <param name="availableSize">The available space that a parent element can allocate to a child element.</param>
    /// <returns>The desired size of the element.</returns>
    public virtual Size Measure(Size availableSize)
    {
        // Cache measurement within the same layout pass
        if (_cachedMeasure.HasValue && _lastMeasureConstraint == availableSize && _layoutPassId == s_globalLayoutPassId)
            return _cachedMeasure.Value;

        // Default implementation: use GetPreferredSize for backward compatibility
        var desiredSize = GetPreferredSize(availableSize);

        // Apply MinimumSize/MaximumSize constraints
        if (MinimumSize.Width > 0)
            desiredSize.Width = Math.Max(desiredSize.Width, MinimumSize.Width);
        if (MinimumSize.Height > 0)
            desiredSize.Height = Math.Max(desiredSize.Height, MinimumSize.Height);
        if (MaximumSize.Width > 0)
            desiredSize.Width = Math.Min(desiredSize.Width, MaximumSize.Width);
        if (MaximumSize.Height > 0)
            desiredSize.Height = Math.Min(desiredSize.Height, MaximumSize.Height);

        // Cache for this layout pass
        _cachedMeasure = desiredSize;
        _lastMeasureConstraint = availableSize;
        _layoutPassId = s_globalLayoutPassId;

        return desiredSize;
    }

    /// <summary>
    ///     Positions the element and determines its final size.
    ///     This is the second phase of the layout pass.
    /// </summary>
    /// <param name="finalRect">
    ///     The final area within the parent that the element should use to arrange itself and its
    ///     children.
    /// </param>
    public virtual void Arrange(Rectangle finalRect)
    {
        // Default implementation: set Bounds directly
        if (Bounds != finalRect)
        {
            _isArranging = true;
            try
            {
                Bounds = finalRect;
            }
            finally
            {
                _isArranging = false;
            }
        }
    }

    /// <summary>
    ///     Invalidates the cached measurement and triggers layout if AutoSize is enabled.
    ///     Call this when properties that affect the element's size change (Text, Font, Image, etc.).
    /// </summary>
    internal void InvalidateMeasure()
    {
        // Clear cached measurement
        _cachedMeasure = null;

        // Trigger layout if AutoSize is enabled
        if (AutoSize)
        {
            // Trigger parent layout to re-measure and re-arrange this element
            if (Parent is UIWindowBase parentWindow)
                parentWindow.PerformLayout();
            else if (Parent is UIElementBase parentElement) parentElement.PerformLayout();
        }
    }

    public virtual Size GetPreferredSize(Size proposedSize)
    {
        return Size;
    }

    #endregion

    #region Protected Event Methods

    public virtual void OnClick(EventArgs e)
    {
        if (!Enabled || !Visible)
            return;

        Click?.Invoke(this, e);
    }

    public virtual void Refresh()
    {
        if (!Enabled || !Visible)
            return;

        Invalidate();
    }

    internal virtual void OnSizeChanged(EventArgs e)
    {
        SizeChanged?.Invoke(this, e);

        // Don't trigger layout during arrange/layout operation to prevent infinite recursion
        if (!IsPerformingLayout && !_isArranging)
        {
            // If this control has children, layout them within new size
            if (Controls.Count > 0)
                PerformLayout();
        }

        Invalidate();
    }

    internal virtual void OnDoubleClick(EventArgs e)
    {
        DoubleClick?.Invoke(this, e);
    }

    internal virtual void OnMouseMove(MouseEventArgs e)
    {
        MouseMove?.Invoke(this, e);

        UIElementBase hoveredElement = null;
        // Z-order'a göre tersten kontrol et (üstteki element önce)
        foreach (var control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder)
                     .Where(c => c.Visible && c.Enabled))
            if (control.Bounds.Contains(e.Location))
            {
                hoveredElement = control;
                var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X,
                    e.Y - control.Location.Y, e.Delta);
                control.OnMouseMove(childEventArgs);
                break; // İlk eşleşenden sonra dur
            }

        if (hoveredElement != _lastHoveredElement)
        {
            _lastHoveredElement?.OnMouseLeave(EventArgs.Empty);
            hoveredElement?.OnMouseEnter(EventArgs.Empty);
            _lastHoveredElement = hoveredElement;
        }
    }

    internal virtual void OnMouseDown(MouseEventArgs e)
    {
        MouseDown?.Invoke(this, e);

        var elementClicked = false;
        // Z-order'a göre tersten kontrol et (üstteki element önce)
        foreach (var control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder)
                     .Where(c => c.Visible && c.Enabled))
            if (control.Bounds.Contains(e.Location))
            {
                elementClicked = true;
                var window = GetParentWindow();
                UIElementBase? prevWindowFocus = null;
                if (window is UIWindow uiWindow)
                    prevWindowFocus = uiWindow.FocusedElement;

                var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X,
                    e.Y - control.Location.Y, e.Delta);
                control.OnMouseDown(childEventArgs);

                // Maintain focus without overriding a deeper focus set by the child.
                if (window is UIWindow uiWindowAfter)
                {
                    // If the child didn't change window focus, focus the direct child.
                    if (uiWindowAfter.FocusedElement == prevWindowFocus)
                        uiWindowAfter.FocusedElement = control;
                }
                else if (window != null)
                {
                    // Fallback for other UIWindowBase implementations
                    window.FocusManager.SetFocus(control);
                }
                else
                {
                    // No window: manage focus locally
                    if (FocusedElement != control)
                        FocusedElement = control;
                }

                break; // İlk eşleşenden sonra dur
            }

        if (!elementClicked)
        {
            var window = GetParentWindow();
            if (window != null)
            {
                // Clicking on the element itself (no child hit) should focus *this*.
                if (CanSelect && Enabled && Visible)
                    window.FocusManager.SetFocus(this);
                else
                    window.FocusManager.SetFocus(null);
            }
            else
            {
                if (CanSelect && Enabled && Visible)
                    Focus();
            }

            if (e.Button == MouseButtons.Right)
            {
                var point = PointToScreen(e.Location);
                var current = this;
                while (current != null)
                {
                    if (current.ContextMenuStrip != null)
                    {
                        current.ContextMenuStrip.Show(this, point);
                        break;
                    }

                    current = current.Parent as UIElementBase;
                }
            }
        }
    }

    /// <summary>
    ///     Gets the parent UIWindowBase for this element
    /// </summary>
    public UIWindowBase GetParentWindow()
    {
        IUIElement current = this;
        while (current != null)
        {
            if (current is UIWindowBase window)
                return window;
            current = current.Parent;
        }

        return null;
    }

    internal virtual void OnMouseUp(MouseEventArgs e)
    {
        MouseUp?.Invoke(this, e);

        // Z-order'a göre tersten kontrol et (üstteki element önce)
        foreach (var control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder)
                     .Where(c => c.Visible && c.Enabled))
            if (control.Bounds.Contains(e.Location))
            {
                var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X,
                    e.Y - control.Location.Y, e.Delta);
                control.OnMouseUp(childEventArgs);
                break; // İlk eşleşenden sonra dur
            }
    }

    internal virtual void OnMouseClick(MouseEventArgs e)
    {
        MouseClick?.Invoke(this, e);

        // Standart WinForms davranışı: MouseClick sonrası Click olayı
        OnClick(EventArgs.Empty);

        // Z-order'a göre tersten kontrol et (üstteki element önce)
        foreach (var control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder)
                     .Where(c => c.Visible && c.Enabled))
            if (control.Bounds.Contains(e.Location))
            {
                var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X,
                    e.Y - control.Location.Y, e.Delta);
                control.OnMouseClick(childEventArgs);

                if (_focusedElement != control)
                    _focusedElement = control;
                break; // İlk eşleşenden sonra dur
            }
    }

    internal virtual void OnMouseDoubleClick(MouseEventArgs e)
    {
        MouseDoubleClick?.Invoke(this, e);

        // Z-order'a göre tersten kontrol et (üstteki element önce)
        foreach (var control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder)
                     .Where(c => c.Visible && c.Enabled))
            if (control.Bounds.Contains(e.Location))
            {
                var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X,
                    e.Y - control.Location.Y, e.Delta);
                control.OnMouseDoubleClick(childEventArgs);
                break; // İlk eşleşenden sonra dur
            }
    }

    internal virtual void OnMouseLeave(EventArgs e)
    {
        MouseLeave?.Invoke(this, e);
        //foreach (var control in Controls)
        //{
        //    if (control.Bounds.Contains(e.Location))
        //    {
        //        var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X, e.Y - control.Location.Y, e.Delta);
        //        control.OnMouseLeave(childEventArgs);
        //    }
        //}

        _lastHoveredElement?.OnMouseLeave(e);
        _lastHoveredElement = null;
    }

    internal virtual void OnMouseHover(EventArgs e)
    {
        MouseHover?.Invoke(this, e);
        //foreach (var control in Controls)
        //{
        //    if (control.Bounds.Contains(e.Location))
        //    {
        //        var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X, e.Y - control.Location.Y, e.Delta);
        //        control.OnMouseLeave(childEventArgs);
        //    }
        //}
    }

    internal virtual void OnMouseEnter(EventArgs e)
    {
        MouseEnter?.Invoke(this, e);
        //foreach (var control in Controls)
        //{
        //    if (control.Bounds.Contains(e.Location))
        //    {
        //        var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X, e.Y - control.Location.Y, e.Delta);
        //        control.OnMouseLeave(childEventArgs);
        //    }
        //}
    }

    internal virtual void OnLocationChanged(EventArgs e)
    {
        LocationChanged?.Invoke(this, e);
        Invalidate();
    }

    internal virtual void OnVisibleChanged(EventArgs e)
    {
        VisibleChanged?.Invoke(this, e);
        Invalidate();
        if (Parent is UIWindowBase parentWindow) parentWindow.PerformLayout();
        else if (Parent is UIElementBase parentElement) parentElement.PerformLayout();
    }

    internal virtual void OnEnabledChanged(EventArgs e)
    {
        EnabledChanged?.Invoke(this, e);
        Invalidate();
    }

    internal virtual void OnTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
        if (CausesValidation)
            ValidateElement();
    }

    internal virtual void OnBackColorChanged(EventArgs e)
    {
        BackColorChanged?.Invoke(this, e);
    }

    internal virtual void OnForeColorChanged(EventArgs e)
    {
        ForeColorChanged?.Invoke(this, e);
    }

    internal virtual void OnFontChanged(EventArgs e)
    {
        FontChanged?.Invoke(this, e);
    }

    internal virtual void OnPaddingChanged(EventArgs e)
    {
        PaddingChanged?.Invoke(this, e);
        Invalidate();
    }

    internal virtual void OnMarginChanged(EventArgs e)
    {
        MarginChanged?.Invoke(this, e);
        // Request parent to relayout this control on next pass
        if (Parent != null)
            Invalidate();
    }

    internal virtual void OnTabStopChanged(EventArgs e)
    {
        TabStopChanged?.Invoke(this, e);
    }

    internal virtual void OnTabIndexChanged(EventArgs e)
    {
        TabIndexChanged?.Invoke(this, e);
    }

    internal virtual void OnAnchorChanged(EventArgs e)
    {
        AnchorChanged?.Invoke(this, e);
        // Don't trigger parent layout - anchor changes will be picked up on next parent resize
        // Forcing layout here causes FPS drops
    }

    internal virtual void OnDockChanged(EventArgs e)
    {
        DockChanged?.Invoke(this, e);
        if (Parent is UIWindowBase parentWindow) parentWindow.PerformLayout();
        else if (Parent is UIElementBase parentElement) parentElement.PerformLayout();
    }

    internal virtual void OnAutoSizeChanged(EventArgs e)
    {
        AutoSizeChanged?.Invoke(this, e);
    }

    internal virtual void OnAutoSizeModeChanged(EventArgs e)
    {
        AutoSizeModeChanged?.Invoke(this, e);
    }

    private bool HandleTabKey(bool isShift)
    {
        var tabbableElements = Controls.OfType<UIElementBase>()
            .Where(e => e.Visible && e.Enabled && e.TabStop)
            .OrderBy(e => e.TabIndex)
            .ToList();

        if (tabbableElements.Count == 0) return false;

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
        return true;
    }


    internal virtual void OnKeyDown(KeyEventArgs e)
    {
        KeyDown?.Invoke(this, e);
        if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt)
            if (HandleTabKey(e.Shift))
            {
                e.Handled = true;
                return;
            }

        if (_focusedElement != null) _focusedElement.OnKeyDown(e);
    }

    internal virtual void OnKeyUp(KeyEventArgs e)
    {
        KeyUp?.Invoke(this, e);

        if (_focusedElement != null) _focusedElement.OnKeyUp(e);
    }

    internal virtual void OnKeyPress(KeyPressEventArgs e)
    {
        KeyPress?.Invoke(this, e);

        if (_focusedElement != null) _focusedElement.OnKeyPress(e);
    }

    internal virtual void OnGotFocus(EventArgs e)
    {
        GotFocus?.Invoke(this, e);
    }

    internal virtual void OnLostFocus(EventArgs e)
    {
        Focused = false;
        LostFocus?.Invoke(this, e);
        if (CausesValidation)
            ValidateElement();
    }

    internal virtual void OnEnter(EventArgs e)
    {
        Enter?.Invoke(this, e);
    }

    internal virtual void OnLeave(EventArgs e)
    {
        Leave?.Invoke(this, e);
    }

    internal virtual void OnValidated(EventArgs e)
    {
        Validated?.Invoke(this, e);
    }

    internal virtual void OnValidating(CancelEventArgs e)
    {
        Validating?.Invoke(this, e);
    }

    internal virtual void OnCursorChanged(EventArgs e)
    {
        CursorChanged?.Invoke(this, e);
        if (Parent is UIWindow parentWindow) parentWindow.UpdateCursor(this);
    }

    public virtual void OnCreateControl()
    {
        if (IsCreated) return;
        IsCreated = true;
        CreateControl?.Invoke(this, EventArgs.Empty);
    }

    internal virtual void OnMouseWheel(MouseEventArgs e)
    {
        if (!Enabled || !Visible)
            return;

        MouseWheel?.Invoke(this, e);
    }

    /// <summary>
    ///     Initialize DPI on first load without scaling (design-time sizes are for 96 DPI baseline).
    /// </summary>
    internal void InitializeDpi(float dpi)
    {
        _currentDpi = dpi > 0 ? dpi : DpiHelper.GetSystemDpi();

        // Invalidate font caches so they rebuild with correct DPI
        InvalidateFontCache();

        foreach (UIElementBase control in Controls) control.InitializeDpi(_currentDpi);

        // Trigger layout to update element sizes based on new DPI
        if (Controls.Count > 0) PerformLayout();

        NeedsRedraw = true;
    }

    internal virtual void OnDpiChanged(float newDpi, float oldDpi)
    {
        if (oldDpi <= 0)
            oldDpi = _currentDpi <= 0 ? DpiHelper.GetSystemDpi() : _currentDpi;

        if (newDpi <= 0)
            newDpi = DpiHelper.GetSystemDpi();

        var scaleFactor = oldDpi <= 0 ? 1f : newDpi / oldDpi;
        _currentDpi = newDpi;

        // Invalidate font caches BEFORE scaling so fonts rebuild with correct DPI
        InvalidateFontCache();

        var previousSize = Size;

        if (Math.Abs(scaleFactor - 1f) > 0.001f)
        {
            // Don't scale Location - layout engine will handle positioning based on parent DPI
            // Only scale Size, Padding, Margin
            if (!AutoSize)
            {
                var scaledSize = new Size(
                    Math.Max(1, (int)Math.Round(previousSize.Width * scaleFactor)),
                    Math.Max(1, (int)Math.Round(previousSize.Height * scaleFactor)));

                if (scaledSize != previousSize) Size = scaledSize;
            }

            Padding = ScalePadding(Padding, scaleFactor);
            Margin = ScalePadding(Margin, scaleFactor);
        }
        else if (AutoSize)
        {
            AdjustSize();
        }

        foreach (UIElementBase control in Controls) control.OnDpiChanged(newDpi, oldDpi);

        // Trigger layout after DPI change to reposition/resize children
        if (_parent != null && Math.Abs(scaleFactor - 1f) > 0.001f) PerformLayout();

        NeedsRedraw = true;
        DpiChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    #endregion

    #region Validation Methods

    public void AddValidationRule(ValidationRule rule)
    {
        if (!_validationRules.Contains(rule))
        {
            _validationRules.Add(rule);
            ValidateElement();
        }
    }

    public void RemoveValidationRule(ValidationRule rule)
    {
        if (_validationRules.Remove(rule)) ValidateElement();
    }

    public void ClearValidationRules()
    {
        _validationRules.Clear();
        IsValid = true;
        ValidationText = string.Empty;
    }

    protected virtual void ValidateElement()
    {
        if (!CausesValidation)
        {
            IsValid = true;
            ValidationText = string.Empty;
            return;
        }

        foreach (var rule in _validationRules)
            if (!rule.Validate(this, out var errorMessage))
            {
                IsValid = false;
                ValidationText = errorMessage;
                OnValidating(new CancelEventArgs(!IsValid));
                return;
            }

        IsValid = true;
        ValidationText = string.Empty;
        OnValidated(EventArgs.Empty);
    }

    #endregion

    #region Methods

    public Form? FindForm()
    {
        if (Parent is Form form)
            return form;
        if (Parent is UIElementBase parentElement)
            return parentElement.FindForm();

        return null;
    }

    public virtual void Select()
    {
        if (!CanSelect || !Selectable || !Enabled || !Visible)
            return;

        Focus();
    }

    protected virtual bool CanProcessMnemonic()
    {
        return UseMnemonic && Visible && Enabled;
    }

    public virtual bool ProcessMnemonic(char charCode)
    {
        if (!CanProcessMnemonic())
            return false;

        if (IsMnemonic(charCode, Text))
        {
            Select();
            PerformClick();
            return true;
        }

        return false;
    }

    protected static bool IsMnemonic(char charCode, string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var index = text.IndexOf('&');
        if (index < 0 || index >= text.Length - 1)
            return false;

        return char.ToUpper(text[index + 1]) == char.ToUpper(charCode);
    }

    protected virtual void PerformClick()
    {
        if (CanSelect)
            OnClick(EventArgs.Empty);
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Name) ? GetType().Name : Name;
    }

    public Point PointToScreen(Point p)
    {
        if (Parent == null)
            return p;

        if (Parent is UIWindowBase parentWindow && !parentWindow.IsDisposed)
            return parentWindow.PointToScreen(new Point(p.X + Location.X, p.Y + Location.Y));
        if (Parent is UIElementBase parentElement)
            return parentElement.PointToScreen(new Point(p.X + Location.X, p.Y + Location.Y));

        return p;
    }

    public Point PointToClient(Point p)
    {
        if (Parent == null)
            return p;

        Point clientPoint;
        if (Parent is UIWindowBase parentWindow)
            clientPoint = parentWindow.PointToClient(p);
        else if (Parent is UIElementBase parentElement)
            clientPoint = parentElement.PointToClient(p);
        else
            return p;

        return new Point(clientPoint.X - Location.X, clientPoint.Y - Location.Y);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                // Yönetilen kaynakları temizle
                _font?.Dispose();
                _cursor?.Dispose();
            }

            IsDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~UIElementBase()
    {
        Dispose(false);
    }

    protected void CheckDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    #endregion

    #region Layout Methods

    public virtual void PerformLayout()
    {
        if (_layoutSuspendCount > 0)
            return;

        if (IsPerformingLayout)
            return;

        try
        {
            IsPerformingLayout = true;

            // Invalidate cached measurements for new layout pass
            s_globalLayoutPassId++;
            _cachedMeasure = null;

            OnLayout(new UILayoutEventArgs(null!));
        }
        finally
        {
            IsPerformingLayout = false;
        }
    }

    public virtual void PerformLayout(UIElementBase affectedElement, string? propertyName)
    {
        var args = new UILayoutEventArgs(affectedElement, propertyName);
        PerformLayout(args);
    }

    public virtual void PerformLayout(UILayoutEventArgs args)
    {
        if (_layoutSuspendCount > 0)
            return;

        if (IsPerformingLayout)
            return;

        try
        {
            IsPerformingLayout = true;
            OnLayout(args);
        }
        finally
        {
            IsPerformingLayout = false;
        }
    }

    protected virtual void OnLayout(UILayoutEventArgs e)
    {
        Layout?.Invoke(this, e);
        // Use DisplayRectangle for child layout area (already excludes padding)
        var clientArea = DisplayRectangle;
        var remainingArea = clientArea;

        // WinForms dock order: Reverse z-order (last added first) in a single pass
        // This matches WinForms DefaultLayout behavior where docking is z-order dependent
        // and processed in reverse (children.Count - 1 down to 0)
        for (int i = Controls.Count - 1; i >= 0; i--)
        {
            var control = Controls[i];
            if (!control.Visible)
                continue;

            PerformDefaultLayout(control, clientArea, ref remainingArea);
        }
    }

    internal virtual void OnControlAdded(UIElementEventArgs e)
    {
        ControlAdded?.Invoke(this, e);

        // If the added element has a parent window that has already completed loading,
        // immediately raise Load for the newly added element (and its subtree).
        if (e.Element != null)
        {
            var parentWindow = e.Element.GetParentWindow();
            if (parentWindow != null && parentWindow.IsLoaded) e.Element.EnsureLoadedRecursively();
        }
    }

    internal virtual void OnControlRemoved(UIElementEventArgs e)
    {
        ControlRemoved?.Invoke(this, e);

        // If the removed element was part of a window that is already loaded,
        // raise Unload for that element and its subtree immediately.
        if (e.Element != null)
        {
            var parentWindow = GetParentWindow();
            if (parentWindow != null && parentWindow.IsLoaded) e.Element.EnsureUnloadedRecursively();
        }
    }

    public void SuspendLayout()
    {
        _layoutSuspendCount++;
    }

    public void ResumeLayout()
    {
        ResumeLayout(true);
    }

    public void ResumeLayout(bool performLayout)
    {
        if (_layoutSuspendCount > 0)
            _layoutSuspendCount--;

        if (performLayout && _layoutSuspendCount == 0)
            PerformLayout();
    }

    public void UpdateZOrder()
    {
    }

    #endregion
}
