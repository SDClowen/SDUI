using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Concurrent;

namespace SDUI.Controls
{
    public class UIElementEventArgs : EventArgs
    {
        public UIElementBase Element { get; }

        public UIElementEventArgs(UIElementBase element)
        {
            Element = element;
        }
    }

    public class UILayoutEventArgs : EventArgs
    {
        public UIElementBase AffectedElement { get; }

        public UILayoutEventArgs(UIElementBase affectedElement)
        {
            AffectedElement = affectedElement;
        }
    }

    public delegate void UIElementEventHandler(object sender, UIElementEventArgs e);
    public delegate void UILayoutEventHandler(object sender, UILayoutEventArgs e);

    public class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> _objects;
        private readonly Func<T> _objectGenerator;

        public ObjectPool(Func<T> objectGenerator)
        {
            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        }

        public T Get() => _objects.TryTake(out T item) ? item : _objectGenerator();

        public void Return(T item)
        {
            if (item != null)
                _objects.Add(item);
        }
    }

    public abstract class UIElementBase : IDisposable
    {
        public Bitmap Image { get; set; }
        protected SKSurface Surface { get; private set; }

        protected SKCanvas Canvas => Surface?.Canvas;

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

        public Rectangle ClientRectangle => new Rectangle(Point.Empty, Size);

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
        public virtual int DeviceDpi => DpiHelper.GetSystemDpi();

        [Browsable(false)]
        public virtual float ScaleFactor => DpiHelper.GetScaleFactor();

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
        protected bool IsDisposed => _isDisposed;

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

        private readonly UIElementCollection _controls;

        protected SKImage _cachedImage;
        protected bool _needsRedraw = true;
        protected Rectangle _dirtyRegion;
        protected bool _isLayoutSuspended;
        protected static readonly ObjectPool<SKPaint> _paintPool = new ObjectPool<SKPaint>(() => new SKPaint());

        public UIElementBase()
        {
            IsDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            _controls = new UIElementCollection(this);
            _font = new Font("Segoe UI", 9f);
            _cursor = Cursors.Default;
        }

        #region Virtual Methods

        public virtual void OnPaint(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            if (_needsRedraw || _dirtyRegion != Rectangle.Empty)
            {
                using (var tempSurface = SKSurface.Create(e.Info))
                {
                    if (tempSurface != null)
                    {
                        var tempCanvas = tempSurface.Canvas;
                        
                        // Sadece dirty region'ı temizle
                        if (!_dirtyRegion.IsEmpty)
                        {
                            tempCanvas.Save();
                            tempCanvas.ClipRect(SKRect.Create(_dirtyRegion.X, _dirtyRegion.Y, _dirtyRegion.Width, _dirtyRegion.Height));
                            tempCanvas.Clear(SKColors.Transparent);
                            tempCanvas.Restore();
                        }
                        else
                        {
                            tempCanvas.Clear(SKColors.Transparent);
                        }

                        OnRender(tempCanvas, e.Info);
                        
                        _cachedImage?.Dispose();
                        _cachedImage = tempSurface.Snapshot();
                        _needsRedraw = false;
                        _dirtyRegion = Rectangle.Empty;
                    }
                }
            }

            if (_cachedImage != null)
            {
                canvas.DrawImage(_cachedImage, 0, 0);
            }
        }

        protected virtual void PaintBackground(SKCanvas canvas)
        {
            if (BackColor.A > 0)  // Sadece arkaplan rengi şeffaf değilse çiz
            {
                canvas.Clear(BackColor.ToSKColor());
            }
            else
            {
                canvas.Clear(SKColors.Transparent);
            }
        }

        protected virtual void PaintContent(SKCanvas canvas)
        {
            // Türetilen sınıflar bu metodu override ederek kendi çizimlerini yapabilir
        }

        public virtual void Invalidate()
        {
            CheckDisposed();
            if (Parent is UIWindowBase window)
            {
                window.Invalidate();
            }
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

        protected virtual void OnClick(EventArgs e)
        {
            if (!Enabled || !Visible)
                return;

            Click?.Invoke(this, e);
        }

        protected virtual void OnSizeChanged(EventArgs e)
        {
            SizeChanged?.Invoke(this, e);
            Invalidate();
        }

        internal virtual void OnDoubleClick(EventArgs e) => DoubleClick?.Invoke(this, e);

        internal virtual void OnMouseMove(MouseEventArgs e) => MouseMove?.Invoke(this, e);

        internal virtual void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && ContextMenuStrip != null)
            {
                var point = PointToScreen(e.Location);
                ContextMenuStrip.Show(this, point);
            }

            MouseDown?.Invoke(this, e);
        }

        internal virtual void OnMouseUp(MouseEventArgs e) => MouseUp?.Invoke(this, e);

        internal virtual void OnMouseClick(MouseEventArgs e) => MouseClick?.Invoke(this, e);

        internal virtual void OnMouseDoubleClick(MouseEventArgs e) =>
            MouseDoubleClick?.Invoke(this, e);

        internal virtual void OnMouseEnter(EventArgs e) => MouseEnter?.Invoke(this, e);

        internal virtual void OnMouseLeave(EventArgs e) => MouseLeave?.Invoke(this, e);

        internal virtual void OnMouseHover(EventArgs e) => MouseHover?.Invoke(this, e);

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

        internal virtual void OnKeyDown(KeyEventArgs e) => KeyDown?.Invoke(this, e);

        internal virtual void OnKeyUp(KeyEventArgs e) => KeyUp?.Invoke(this, e);

        internal virtual void OnKeyPress(KeyPressEventArgs e) => KeyPress?.Invoke(this, e);

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
            if (Parent is UIWindowBase parentWindow)
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

        internal virtual void OnDpiChanged(EventArgs e)
        {
            DpiChanged?.Invoke(this, e);

            // Alt kontrollere DPI değişikliğini bildir
            foreach (var control in _controls)
            {
                control.OnDpiChanged(e);
            }

            // DPI değişikliğinde boyut ve konumu güncelle
            if (AutoSize)
            {
                AdjustSize();
            }
            else
            {
                // Mevcut boyutu yeni DPI'ya göre ölçekle
                var scaleFactor = DpiHelper.GetScaleFactor();
                Size = new Size(
                    (int)(Width * scaleFactor),
                    (int)(Height * scaleFactor)
                );
            }

            // Yeniden çizim
            Invalidate();
        }

        #endregion

        public object Parent { get; internal set; }

        public UIWindowBase ParentWindow => Parent as UIWindowBase;

        public UIElementBase ParentElement => Parent as UIElementBase;

        public bool HasParent => Parent != null;

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
            if (Parent is UIWindowBase window)
            {
                window.BringToFront(this);
            }
        }

        public void SendToBack()
        {
            if (Parent is UIWindowBase window)
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

            if (Parent is UIWindowBase parentWindow)
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
                    _font?.Dispose();
                    _cursor?.Dispose();
                    Surface?.Dispose();
                }

                // Yönetilmeyen kaynakları temizle
                Surface = null;
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
        public UIElementCollection Controls => _controls;

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

            if (Parent is UIWindowBase window)
            {
                var clientArea = window.ClientRectangle;
                var remainingArea = clientArea;

                // Dock işlemleri
                foreach (var control in _controls.Where(c => c.Visible && c.Dock != DockStyle.None))
                {
                    switch (control.Dock)
                    {
                        case DockStyle.Top:
                            control.Size = new Size(remainingArea.Width, control.Size.Height);
                            control.Location = new Point(remainingArea.Left, remainingArea.Top);
                            remainingArea = new Rectangle(
                                remainingArea.Left,
                                remainingArea.Top + control.Size.Height,
                                remainingArea.Width,
                                remainingArea.Height - control.Size.Height
                            );
                            break;

                        case DockStyle.Bottom:
                            control.Size = new Size(remainingArea.Width, control.Size.Height);
                            control.Location = new Point(remainingArea.Left, remainingArea.Bottom - control.Size.Height);
                            remainingArea = new Rectangle(
                                remainingArea.Left,
                                remainingArea.Top,
                                remainingArea.Width,
                                remainingArea.Height - control.Size.Height
                            );
                            break;

                        case DockStyle.Left:
                            control.Size = new Size(control.Size.Width, remainingArea.Height);
                            control.Location = new Point(remainingArea.Left, remainingArea.Top);
                            remainingArea = new Rectangle(
                                remainingArea.Left + control.Size.Width,
                                remainingArea.Top,
                                remainingArea.Width - control.Size.Width,
                                remainingArea.Height
                            );
                            break;

                        case DockStyle.Right:
                            control.Size = new Size(control.Size.Width, remainingArea.Height);
                            control.Location = new Point(remainingArea.Right - control.Size.Width, remainingArea.Top);
                            remainingArea = new Rectangle(
                                remainingArea.Left,
                                remainingArea.Top,
                                remainingArea.Width - control.Size.Width,
                                remainingArea.Height
                            );
                            break;

                        case DockStyle.Fill:
                            control.Location = new Point(remainingArea.Left, remainingArea.Top);
                            control.Size = new Size(remainingArea.Width, remainingArea.Height);
                            break;
                    }
                }

                // Anchor işlemleri
                foreach (var control in _controls.Where(c => c.Visible && c.Dock == DockStyle.None))
                {
                    var anchor = control.Anchor;
                    var location = control.Location;
                    var size = control.Size;

                    // Yatay konumlandırma
                    if ((anchor & AnchorStyles.Left) != 0 && (anchor & AnchorStyles.Right) != 0)
                    {
                        size.Width = clientArea.Width - (location.X + (clientArea.Width - (location.X + size.Width)));
                    }
                    else if ((anchor & AnchorStyles.Right) != 0)
                    {
                        location.X = clientArea.Width - (clientArea.Width - location.X);
                    }

                    // Dikey konumlandırma
                    if ((anchor & AnchorStyles.Top) != 0 && (anchor & AnchorStyles.Bottom) != 0)
                    {
                        size.Height = clientArea.Height - (location.Y + (clientArea.Height - (location.Y + size.Height)));
                    }
                    else if ((anchor & AnchorStyles.Bottom) != 0)
                    {
                        location.Y = clientArea.Height - (clientArea.Height - location.Y);
                    }

                    control.Location = location;
                    control.Size = size;
                }
            }

            Invalidate();
        }

        internal virtual void OnControlAdded(UIElementEventArgs e)
        {
            if (e.Element != null)
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
            }
        }

        internal virtual void OnControlRemoved(UIElementEventArgs e)
        {
            if (e.Element != null)
            {
                if (e.Element.Parent == Parent)
                {
                    e.Element.Parent = null;
                }
                ControlRemoved?.Invoke(this, e);
                PerformLayout();
            }
        }

        public void SuspendLayout()
        {
            _isLayoutSuspended = true;
        }

        public void ResumeLayout(bool unused = false)
        {
            _isLayoutSuspended = false;
            PerformLayout();
        }

        protected void InvalidateCache()
        {
            _needsRedraw = true;
            _cachedImage?.Dispose();
            _cachedImage = null;
        }

        protected void InvalidateRegion(Rectangle region)
        {
            if (_dirtyRegion.IsEmpty)
                _dirtyRegion = region;
            else
                _dirtyRegion = Rectangle.Union(_dirtyRegion, region);
            
            _needsRedraw = true;
        }

        protected SKPaint GetPaintFromPool()
        {
            var paint = _paintPool.Get();
            paint.Reset();
            return paint;
        }

        protected void ReturnPaintToPool(SKPaint paint)
        {
            if (paint != null)
                _paintPool.Return(paint);
        }

        protected virtual void OnRender(SKCanvas canvas, SKImageInfo info)
        {
            // Alt sınıflar bu metodu override ederek kendi çizimlerini yapacak
        }
        #endregion

        public void AddControl(UIElementBase element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            Controls.Add(element);
        }

        public void RemoveControl(UIElementBase element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            Controls.Remove(element);
        }
    }

    public abstract class ValidationRule
    {
        public string ErrorMessage { get; set; }
        public abstract bool Validate(UIElementBase element, out string errorMessage);
    }

    public class RequiredFieldValidationRule : ValidationRule
    {
        public override bool Validate(UIElementBase element, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(element.Text))
            {
                errorMessage = ErrorMessage ?? "Bu alan boş bırakılamaz.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }
    }

    public class MinLengthValidationRule : ValidationRule
    {
        public int MinLength { get; set; }

        public override bool Validate(UIElementBase element, out string errorMessage)
        {
            if (element.Text.Length < MinLength)
            {
                errorMessage = ErrorMessage ?? $"Bu alan en az {MinLength} karakter olmalıdır.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }
    }

    public class MaxLengthValidationRule : ValidationRule
    {
        public int MaxLength { get; set; }

        public override bool Validate(UIElementBase element, out string errorMessage)
        {
            if (element.Text.Length > MaxLength)
            {
                errorMessage = ErrorMessage ?? $"Bu alan en fazla {MaxLength} karakter olmalıdır.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }
    }

    public class RegexValidationRule : ValidationRule
    {
        public string Pattern { get; set; }

        public override bool Validate(UIElementBase element, out string errorMessage)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(element.Text, Pattern))
            {
                errorMessage = ErrorMessage ?? "Geçersiz format.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }
    }

    public class CustomValidationRule : ValidationRule
    {
        private readonly Func<UIElementBase, (bool isValid, string errorMessage)> _validationFunc;

        public CustomValidationRule(Func<UIElementBase, (bool isValid, string errorMessage)> validationFunc)
        {
            _validationFunc = validationFunc;
        }

        public override bool Validate(UIElementBase element, out string errorMessage)
        {
            var result = _validationFunc(element);
            errorMessage = result.errorMessage;
            return result.isValid;
        }
    }

    public class UIElementCollection : IList<UIElementBase>
    {
        private readonly List<UIElementBase> _items = new(32);
        private readonly UIElementBase _owner;

        public UIElementCollection(UIElementBase owner)
        {
            _owner = owner;
        }

        public UIElementBase this[int index]
        {
            get => _items[index];
            set
            {
                var oldItem = _items[index];
                if (oldItem != value)
                {
                    if (oldItem != null)
                    {
                        _owner.OnControlRemoved(new UIElementEventArgs(oldItem));
                    }
                    _items[index] = value;
                    if (value != null)
                    {
                        _owner.OnControlAdded(new UIElementEventArgs(value));
                    }
                }
            }
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public void Add(UIElementBase item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _items.Add(item);
            _owner.OnControlAdded(new UIElementEventArgs(item));
        }

        public void AddRange(UIElementBase[] items)
        {
            foreach (var item in items)
                this.Add(item);
        }

        public void Clear()
        {
            var itemsToRemove = _items.ToList();
            _items.Clear();
            foreach (var item in itemsToRemove)
            {
                _owner.OnControlRemoved(new UIElementEventArgs(item));
            }
        }

        public bool Contains(UIElementBase item) => _items.Contains(item);
        public void CopyTo(UIElementBase[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public IEnumerator<UIElementBase> GetEnumerator() => _items.GetEnumerator();
        public int IndexOf(UIElementBase item) => _items.IndexOf(item);

        public void Insert(int index, UIElementBase item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _items.Insert(index, item);
            _owner.OnControlAdded(new UIElementEventArgs(item));
        }

        public bool Remove(UIElementBase item)
        {
            if (item == null)
                return false;

            var result = _items.Remove(item);
            if (result)
            {
                _owner.OnControlRemoved(new UIElementEventArgs(item));
            }
            return result;
        }

        public void RemoveAt(int index)
        {
            var item = _items[index];
            _items.RemoveAt(index);
            _owner.OnControlRemoved(new UIElementEventArgs(item));
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
