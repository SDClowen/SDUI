using SDUI.Collections;
using SDUI.Helpers;
using SDUI.Validations;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public abstract class UIElementBase : IUIElement, IDisposable
    {
        public System.Drawing.ContentAlignment ImageAlign { get; set; }
        public RightToLeft RightToLeft { get; set; } = RightToLeft.No;
        public SizeF AutoScaleDimensions { get; set; }
        public AutoScaleMode AutoScaleMode { get; set; }

        public bool Disposing { get; set; }
        public virtual bool DoubleBuffered { get; set; }
        public bool CheckForIllegalCrossThreadCalls { get; set; }
        public void SetStyle(ControlStyles flag, bool value) { }
        public void SetTopLevel(bool value) { }
        public void Invoke(Delegate method) { method.DynamicInvoke(); }
        public void Invoke(Delegate method, params object[] args) { method.DynamicInvoke(args); }
        public void Show() { Visible = true; }
        public void Hide() { Visible = false; }
        public object Tag;
        public bool AutoScroll {  get; set; }
        public Size AutoScrollMargin { get; set; }

        public Image Image { get; set; }

        private float _currentDpi = 96f;
        private IUIElement _parent;

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

        private SKSurface? _renderSurface;
        private SKImageInfo _renderInfo;
    private SKImage? _renderSnapshot;

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
                {
                    Size = new Size(
                        Math.Max(Size.Width, _minimumSize.Width),
                        Math.Max(Size.Height, _minimumSize.Height)
                    );
                }
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
                {
                    Size = new Size(
                        _maximumSize.Width > 0 ? Math.Min(Size.Width, _maximumSize.Width) : Size.Width,
                        _maximumSize.Height > 0 ? Math.Min(Size.Height, _maximumSize.Height) : Size.Height
                    );
                }
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

                // Parent'a bildir
                if (Parent is UIWindowBase parentWindow)
                {
                    parentWindow.PerformLayout();
                }
            }
        }

        public Rectangle Bounds
        {
            get => new Rectangle(Location, Size);
            set
            {
                Location = value.Location;

                Size = value.Size;
            }
        }

        public Rectangle ClientRectangle => new(Location, Size);

        public Size ClientSize
        {
            get => Size;
            set => Size = value;
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

        public virtual AnchorStyles Anchor
        {
            get => _anchor;
            set
            {
                if (_anchor == value)
                    return;

                _anchor = value;

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

        private int _zOrder;

        [Browsable(false)]
        public int ZOrder
        {
            get => _zOrder;
            internal set => _zOrder = value;
        }

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

        [Browsable(false)]
        public bool Focused { get; private set; }

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

        private bool _isCreated;
        protected bool IsCreated => _isCreated;

        [Browsable(false)]
        public virtual int DeviceDpi => (int)Math.Round(_currentDpi);

        [Browsable(false)]
        public virtual float ScaleFactor => _currentDpi / 96f;

        [Browsable(false)]
        protected virtual Keys ModifierKeys
        {
            get
            {
                Keys modifiers = Keys.None;

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

        private bool _isDisposed;
        public bool IsDisposed => _isDisposed;
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
                    oldFocus.OnLostFocus(EventArgs.Empty);
                    oldFocus.OnLeave(EventArgs.Empty);
                }

                if (_focusedElement != null)
                {
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
                    //UpdateCursor(value);
                }
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

        private List<ValidationRule> _validationRules = new();
        [Browsable(false)]
        public IReadOnlyList<ValidationRule> ValidationRules => _validationRules.AsReadOnly();
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

        #endregion

        protected bool IsDesignMode { get; }

        private readonly ElementCollection _controls;

        protected bool _isLayoutSuspended;

        public bool IsHandleCreated;

        public UIElementBase()
        {
            IsDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            _controls = new(this);
            _font = new Font("Segoe UI", 9f);
            _cursor = Cursors.Default;
            _currentDpi = DpiHelper.GetSystemDpi();
        }

        #region Virtual Methods

        public virtual void OnPaint(SKPaintSurfaceEventArgs e)
        {
        }

        public virtual void Invalidate()
        {
            CheckDisposed();

            MarkDirty();

            switch (Parent)
            {
                case UIWindowBase window:
                    window.Invalidate();
                    break;
                case UIElementBase element:
                    element.Invalidate();
                    break;
            }
        }

        protected void MarkDirty()
        {
            NeedsRedraw = true;
            _renderSnapshot?.Dispose();
            _renderSnapshot = null;
        }

        internal void InvalidateRender()
        {
            MarkDirty();
        }

        internal void InvalidateRenderTree()
        {
            MarkDirty();
            foreach (var child in Controls.OfType<UIElementBase>())
            {
                child.InvalidateRenderTree();
            }
        }

        private void DisposeRenderResources()
        {
            _renderSnapshot?.Dispose();
            _renderSnapshot = null;

            _renderSurface?.Dispose();
            _renderSurface = null;
            _renderInfo = SKImageInfo.Empty;
        }

        private void EnsureRenderTarget()
        {
            if (Width <= 0 || Height <= 0)
            {
                DisposeRenderResources();
                return;
            }

            // sRGB color space ile yüksek kaliteli render
            var desiredInfo = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());

            if (_renderSurface != null &&
                _renderInfo.Width == desiredInfo.Width &&
                _renderInfo.Height == desiredInfo.Height &&
                _renderInfo.ColorType == desiredInfo.ColorType)
            {
                return;
            }

            DisposeRenderResources();
            _renderSurface = SKSurface.Create(desiredInfo);
            _renderInfo = desiredInfo;
            MarkDirty();
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
            foreach (var child in Controls.OfType<UIElementBase>().OrderBy(el => el.ZOrder))
            {
                var snapshot = child.RenderSnapshot();
                if (snapshot == null)
                    continue;

                canvas.DrawImage(snapshot, child.Location.X, child.Location.Y);
            }
        }

        internal SKImage? RenderSnapshot()
        {
            if (!Visible || Width <= 0 || Height <= 0)
            {
                DisposeRenderResources();
                return null;
            }

            EnsureRenderTarget();
            if (_renderSurface == null)
                return null;

            if (NeedsRedraw)
            {
                var canvas = _renderSurface.Canvas;
                canvas.Save();
                
                // Yüksek kaliteli render ayarları
                canvas.Clear(ResolveBackgroundColor());

                var args = new SKPaintSurfaceEventArgs(_renderSurface, _renderInfo);
                OnPaint(args);
                Paint?.Invoke(this, args);

                RenderChildren(canvas);

                canvas.Restore();
                canvas.Flush();

                _renderSnapshot?.Dispose();
                _renderSnapshot = _renderSurface.Snapshot();
                NeedsRedraw = false;
            }

            return _renderSnapshot;
        }

        internal void Render(SKCanvas targetCanvas)
        {
            var snapshot = RenderSnapshot();
            if (snapshot == null)
                return;

            targetCanvas.DrawImage(snapshot, Location.X, Location.Y);
        }

        public virtual void Focus()
        {
            if (Parent is UIWindowBase window)
            {
                Focused = true;
                OnGotFocus(EventArgs.Empty);
            }
        }

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
            if (MinimumSize.Width > 0)
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
            Invalidate();
        }

        internal virtual void OnDoubleClick(EventArgs e) => DoubleClick?.Invoke(this, e);

        internal virtual void OnMouseMove(MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);

            UIElementBase hoveredElement = null;
            // Z-order'a göre tersten kontrol et (üstteki element önce)
            foreach (UIElementBase control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder).Where(c => c.Visible && c.Enabled))
            {
                if (control.Bounds.Contains(e.Location))
                {
                    hoveredElement = control;
                    var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X, e.Y - control.Location.Y, e.Delta);
                    control.OnMouseMove(childEventArgs);
                    break; // İlk eşleşenden sonra dur
                }
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
            if (e.Button == MouseButtons.Right && ContextMenuStrip != null)
            {
                var point = PointToScreen(e.Location);
                ContextMenuStrip.Show(this, point);
            }

            MouseDown?.Invoke(this, e);

            bool elementClicked = false;
            // Z-order'a göre tersten kontrol et (üstteki element önce)
            foreach (UIElementBase control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder).Where(c => c.Visible && c.Enabled))
            {
                if (control.Bounds.Contains(e.Location))
                {
                    elementClicked = true;
                    var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X, e.Y - control.Location.Y, e.Delta);
                    control.OnMouseDown(childEventArgs);
                    if (_focusedElement != control)
                        _focusedElement = control;
                    break; // İlk eşleşenden sonra dur
                }
            }

            if (!elementClicked)
                _focusedElement = null;
        }

        internal virtual void OnMouseUp(MouseEventArgs e)
        {
            MouseUp?.Invoke(this, e);

            // Z-order'a göre tersten kontrol et (üstteki element önce)
            foreach (UIElementBase control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder).Where(c => c.Visible && c.Enabled))
            {
                if (control.Bounds.Contains(e.Location))
                {
                    var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X, e.Y - control.Location.Y, e.Delta);
                    control.OnMouseUp(childEventArgs);
                    break; // İlk eşleşenden sonra dur
                }
            }
        }

        internal virtual void OnMouseClick(MouseEventArgs e)
        {
            MouseClick?.Invoke(this, e);

            // Standart WinForms davranışı: MouseClick sonrası Click olayı
            OnClick(EventArgs.Empty);

            // Z-order'a göre tersten kontrol et (üstteki element önce)
            foreach (UIElementBase control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder).Where(c => c.Visible && c.Enabled))
            {
                if (control.Bounds.Contains(e.Location))
                {
                    var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X, e.Y - control.Location.Y, e.Delta);
                    control.OnMouseClick(childEventArgs);

                    if (_focusedElement != control)
                        _focusedElement = control;
                    break; // İlk eşleşenden sonra dur
                }
            }
        }

        internal virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            MouseDoubleClick?.Invoke(this, e);

            // Z-order'a göre tersten kontrol et (üstteki element önce)
            foreach (UIElementBase control in Controls.OfType<UIElementBase>().OrderByDescending(c => c.ZOrder).Where(c => c.Visible && c.Enabled))
            {
                if (control.Bounds.Contains(e.Location))
                {
                    var childEventArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - control.Location.X, e.Y - control.Location.Y, e.Delta);
                    control.OnMouseDoubleClick(childEventArgs);
                    break; // İlk eşleşenden sonra dur
                }
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

        internal virtual void OnBackColorChanged(EventArgs e) => BackColorChanged?.Invoke(this, e);

        internal virtual void OnForeColorChanged(EventArgs e) => ForeColorChanged?.Invoke(this, e);

        internal virtual void OnFontChanged(EventArgs e) => FontChanged?.Invoke(this, e);

        internal virtual void OnPaddingChanged(EventArgs e) => PaddingChanged?.Invoke(this, e);

        internal virtual void OnMarginChanged(EventArgs e) => MarginChanged?.Invoke(this, e);

        internal virtual void OnTabStopChanged(EventArgs e) => TabStopChanged?.Invoke(this, e);

        internal virtual void OnTabIndexChanged(EventArgs e) => TabIndexChanged?.Invoke(this, e);

        internal virtual void OnAnchorChanged(EventArgs e) => AnchorChanged?.Invoke(this, e);

        internal virtual void OnDockChanged(EventArgs e) => DockChanged?.Invoke(this, e);

        internal virtual void OnAutoSizeChanged(EventArgs e) => AutoSizeChanged?.Invoke(this, e);

        internal virtual void OnAutoSizeModeChanged(EventArgs e) =>
            AutoSizeModeChanged?.Invoke(this, e);
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

            _focusedElement = tabbableElements[currentIndex];
        }


        internal virtual void OnKeyDown(KeyEventArgs e)
        {
            KeyDown?.Invoke(this, e);
            if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt)
            {
                HandleTabKey(e.Shift);
                e.Handled = true;
                return;
            }

            if (_focusedElement != null)
            {
                _focusedElement.OnKeyDown(e);
            }
        }

        internal virtual void OnKeyUp(KeyEventArgs e)
        {
            KeyUp?.Invoke(this, e);

            if (_focusedElement != null)
            {
                _focusedElement.OnKeyUp(e);
            }
        }

        internal virtual void OnKeyPress(KeyPressEventArgs e)
        {
            KeyPress?.Invoke(this, e);

            if (_focusedElement != null)
            {
                _focusedElement.OnKeyPress(e);
            }
        }

        internal virtual void OnGotFocus(EventArgs e) => GotFocus?.Invoke(this, e);

        internal virtual void OnLostFocus(EventArgs e)
        {
            Focused = false;
            LostFocus?.Invoke(this, e);
            if (CausesValidation)
                ValidateElement();
        }

        internal virtual void OnEnter(EventArgs e) => Enter?.Invoke(this, e);

        internal virtual void OnLeave(EventArgs e) => Leave?.Invoke(this, e);

        internal virtual void OnValidated(EventArgs e) => Validated?.Invoke(this, e);

        internal virtual void OnValidating(CancelEventArgs e) => Validating?.Invoke(this, e);

        internal virtual void OnCursorChanged(EventArgs e)
        {
            CursorChanged?.Invoke(this, e);
            if (Parent is UIWindow parentWindow)
            {
                parentWindow.UpdateCursor(this);
            }
        }

        internal virtual void OnCreateControl()
        {
            if (_isCreated) return;
            _isCreated = true;
            CreateControl?.Invoke(this, EventArgs.Empty);
        }

        internal virtual void OnMouseWheel(MouseEventArgs e)
        {
            if (!Enabled || !Visible)
                return;

            MouseWheel?.Invoke(this, e);
        }

        internal virtual void OnDpiChanged(float newDpi, float oldDpi)
        {
            if (oldDpi <= 0)
                oldDpi = _currentDpi <= 0 ? DpiHelper.GetSystemDpi() : _currentDpi;

            if (newDpi <= 0)
                newDpi = DpiHelper.GetSystemDpi();

            var scaleFactor = oldDpi <= 0 ? 1f : newDpi / oldDpi;
            _currentDpi = newDpi;

            var previousLocation = Location;
            var previousSize = Size;

            if (Math.Abs(scaleFactor - 1f) > 0.001f)
            {
                var scaledLocation = new Point(
                    (int)Math.Round(previousLocation.X * scaleFactor),
                    (int)Math.Round(previousLocation.Y * scaleFactor));

                if (scaledLocation != previousLocation)
                {
                    Location = scaledLocation;
                }

                if (!AutoSize)
                {
                    var scaledSize = new Size(
                        Math.Max(1, (int)Math.Round(previousSize.Width * scaleFactor)),
                        Math.Max(1, (int)Math.Round(previousSize.Height * scaleFactor)));

                    if (scaledSize != previousSize)
                    {
                        Size = scaledSize;
                    }
                }

                Padding = ScalePadding(Padding, scaleFactor);
                Margin = ScalePadding(Margin, scaleFactor);
            }
            else if (AutoSize)
            {
                AdjustSize();
            }

            foreach (UIElementBase control in _controls)
            {
                control.OnDpiChanged(newDpi, oldDpi);
            }

            NeedsRedraw = true;
            DpiChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        #endregion

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

        public UIWindowBase ParentWindow
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

        public UIElementBase ParentElement => _parent as UIElementBase;

        public bool HasParent => _parent != null;

        private void UpdateCurrentDpiFromParent()
        {
            if (_parent is UIWindowBase window && window.IsHandleCreated)
            {
                _currentDpi = DpiHelper.GetDpiForWindow(window.Handle);
            }
            else if (_parent is UIElementBase element)
            {
                _currentDpi = element.ScaleFactor * 96f;
            }
            else
            {
                _currentDpi = DpiHelper.GetSystemDpi();
            }
        }

        private static Padding ScalePadding(Padding padding, float scaleFactor)
        {
            if (Math.Abs(scaleFactor - 1f) < 0.001f)
                return padding;

            static int Scale(int value, float factor) => Math.Max(0, (int)Math.Round(value * factor));

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

        public void BringToFront()
        {
            if (Parent is UIWindow window)
            {
                window.BringToFront(this);
            }
        }

        public void SendToBack()
        {
            if (Parent is UIWindow window)
            {
                window.SendToBack(this);
            }
        }

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
            if (_validationRules.Remove(rule))
            {
                ValidateElement();
            }
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
            {
                if (!rule.Validate(this, out string errorMessage))
                {
                    IsValid = false;
                    ValidationText = errorMessage;
                    OnValidating(new CancelEventArgs(!IsValid));
                    return;
                }
            }

            IsValid = true;
            ValidationText = string.Empty;
            OnValidated(EventArgs.Empty);
        }
        #endregion

        #region Methods
        public Form FindForm()
        {
            if (Parent is Form form)
                return form;
            else if (Parent is UIElementBase parentElement)
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

            int index = text.IndexOf('&');
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
            else if (Parent is UIElementBase parentElement)
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
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Yönetilen kaynakları temizle
                    DisposeRenderResources();
                    _font?.Dispose();
                    _cursor?.Dispose();
                }
                _isDisposed = true;
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
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
        #endregion

        [Browsable(false)]
        public ElementCollection Controls => _controls;

        #region Layout Methods
        public virtual void PerformLayout()
        {
            if (_isLayoutSuspended)
                return;

            OnLayout(new UILayoutEventArgs(null));
        }

        public virtual void PerformLayout(UIElementBase affectedElement)
        {
            if (_isLayoutSuspended)
                return;

            OnLayout(new UILayoutEventArgs(affectedElement));
        }

        protected virtual void OnLayout(UILayoutEventArgs e)
        {
            Layout?.Invoke(this, e);
            Rectangle clientArea = ClientRectangle;
            Padding clientPadding = Padding;

            if (Parent is UIWindow window)
            {
                clientArea = window.ClientRectangle;
                clientPadding = window.Padding;
            }
            else if (Parent is UIElementBase parentElement)
            {
                clientPadding = parentElement.Padding;
                clientArea = parentElement.ClientRectangle;
            }

            //clientArea.Inflate(clientPadding.Size);

            clientArea.X += clientPadding.Left;
            clientArea.Y += clientPadding.Top;
            clientArea.Width -= clientPadding.Horizontal;
            clientArea.Height -= clientPadding.Vertical;

            LayoutEngine.Perform(this, clientArea, clientPadding);

            // Dock işlemleri
            foreach (UIElementBase control in _controls)
            {
                LayoutEngine.Perform(control, clientArea, clientPadding);
                //control.PerformLayout();
            }

            Invalidate();
        }

        internal virtual void OnControlAdded(UIElementEventArgs e)
        {
            ControlAdded?.Invoke(this, e);
            /*if (e.Element != null)
            {
                if (Parent is UIWindowBase parentWindow)
                {
                    e.Element.Parent = parentWindow;
                    ControlAdded?.Invoke(this, e);
                    PerformLayout(e.Element);
                }
                else
                {
                    ControlAdded?.Invoke(this, e);
                }
            }*/
        }

        internal virtual void OnControlRemoved(UIElementEventArgs e)
        {
            ControlRemoved?.Invoke(this, e);
            //if (e.Element != null)
            //{
            //    if (e.Element.Parent == Parent)
            //    {
            //        e.Element.Parent = null;
            //    }
            //    ControlRemoved?.Invoke(this, e);
            //    PerformLayout();
            //}
        }

        public void SuspendLayout()
        {
            _isLayoutSuspended = true;
        }

        public void ResumeLayout()
        {
            ResumeLayout(true);
        }

        public void ResumeLayout(bool performLayout)
        {
            _isLayoutSuspended = false;

            if(performLayout)
            PerformLayout();
        }

        public void UpdateZOrder() { }
        #endregion
    }
}
