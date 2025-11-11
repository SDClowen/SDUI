using SDUI.Animation;
using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ComboBox : UIElementBase
{
    private class DropDownMenu : UIElementBase
    {
        private readonly ComboBox _owner;
        private int _hoverIndex = -1;
        private readonly AnimationEngine _animation;
        private float _currentHeight;
        private float _targetHeight;
        private float _hoverY;
        private float _targetHoverY;
        private readonly AnimationEngine _hoverAnimation;

        private const int ITEM_HEIGHT = 32;
        private const int ITEM_PADDING = 8;
        private const int VERTICAL_PADDING = 8;
        private const float CORNER_RADIUS = 8;
        private const int SHADOW_OFFSET = 6;
        private const int SHADOW_BLUR = 16;

        public DropDownMenu(ComboBox owner)
        {
            _owner = owner;
            BackColor = Color.Transparent;
            Dock = DockStyle.None;
            Anchor = AnchorStyles.Top | AnchorStyles.Left;
            AutoSize = false;
            TabStop = false;

            _animation = new AnimationEngine(singular: true)
            {
            Increment = 0.18,
            SecondaryIncrement = 0.14,
            AnimationType = AnimationType.CustomQuadratic,
                InterruptAnimation = true
            };
            _animation.OnAnimationProgress += (s) =>
            {
                var progress = (float)_animation.GetProgress();
                _currentHeight = _currentHeight + (_targetHeight - _currentHeight) * progress;
                Height = (int)_currentHeight;

                if (Height <= 0 && _animation.GetDirection() == AnimationDirection.Out)
                {
                    Visible = false;
                    _owner.DroppedDown = false;
                    Parent?.Controls?.Remove(this);
                }

                Invalidate();
            };

            _hoverAnimation = new AnimationEngine(singular: true)
            {
            Increment = 0.26,
            SecondaryIncrement = 0.2,
            AnimationType = AnimationType.CustomQuadratic,
                InterruptAnimation = true
            };
            _hoverAnimation.OnAnimationProgress += (s) =>
            {
                var progress = (float)_hoverAnimation.GetProgress();
                _hoverY = _hoverY + (_targetHoverY - _hoverY) * progress;
                Invalidate();
            };
        }

        internal override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var newHoverIndex = GetItemIndexAtPoint(e.Location);
            if (newHoverIndex >= 0 && newHoverIndex < _owner.Items.Count)
            {
                if (newHoverIndex != _hoverIndex)
                {
                    _hoverIndex = newHoverIndex;
                    _targetHoverY = VERTICAL_PADDING + (_hoverIndex * ITEM_HEIGHT);
                    _hoverAnimation.StartNewAnimation(AnimationDirection.In);
                }
            }
            else
            {
                _hoverIndex = -1;
                _hoverAnimation.StartNewAnimation(AnimationDirection.Out);
            }
            Invalidate();
        }

        internal override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            _hoverAnimation.StartNewAnimation(AnimationDirection.Out);
            Invalidate();
        }

        internal override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                var itemIndex = GetItemIndexAtPoint(e.Location);
                if (itemIndex >= 0 && itemIndex < _owner.Items.Count)
                {
                    _owner.SelectedIndex = itemIndex;
                    _owner.OnSelectionChangeCommitted(EventArgs.Empty);
                    BeginClose();
                }
            }
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            if (Height <= 0) return;

            // Gölge
            using (var shadowPaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha(100),
                ImageFilter = SKImageFilter.CreateDropShadow(0, SHADOW_OFFSET, SHADOW_BLUR, SHADOW_BLUR, SKColors.Black.WithAlpha(180)),
                IsAntialias = true
            })
            {
                var shadowRect = new SKRoundRect(new SKRect(0, 0, Width, Height), CORNER_RADIUS);
                canvas.DrawRoundRect(shadowRect, shadowPaint);
            }

            // Arka plan
            using (var paint = new SKPaint
            {
                Color = ColorScheme.BackColor.ToSKColor(),
                IsAntialias = true
            })
            {
                var rect = new SKRoundRect(new SKRect(0, 0, Width, Height), CORNER_RADIUS);
                canvas.DrawRoundRect(rect, paint);
            }

            // Hover efekti
            if (_hoverIndex >= 0 && _hoverAnimation.GetProgress() > 0)
            {
                using var hoverPaint = new SKPaint
                {
                    Color = ColorScheme.AccentColor.Alpha(30).ToSKColor(),
                    IsAntialias = true
                };
                var hoverRect = new SKRoundRect(
                    new SKRect(ITEM_PADDING, _hoverY, Width - ITEM_PADDING, _hoverY + ITEM_HEIGHT), 6);
                canvas.DrawRoundRect(hoverRect, hoverPaint);
            }

            // Öğeleri çiz
            using var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                TextSize = _owner.Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(_owner.Font.FontFamily.Name),
                IsAntialias = true,
                SubpixelText = true
            };

            float y = VERTICAL_PADDING;
            for (int i = 0; i < _owner.Items.Count; i++)
            {
                var itemRect = new SKRect(ITEM_PADDING, y, Width - ITEM_PADDING, y + ITEM_HEIGHT);

                // Seçili öğe için arka plan
                if (i == _owner.SelectedIndex)
                {
                    using var selectedPaint = new SKPaint
                    {
                        Color = ColorScheme.AccentColor.Alpha(50).ToSKColor(),
                        IsAntialias = true
                    };
                    var selectedRect = new SKRoundRect(itemRect, 6);
                    canvas.DrawRoundRect(selectedRect, selectedPaint);
                }

                var text = _owner.GetItemText(_owner.Items[i]);

                // Metin rengi ayarla
                textPaint.Color = (i == _owner.SelectedIndex)
                    ? ColorScheme.AccentColor.ToSKColor()
                    : ColorScheme.ForeColor.ToSKColor();

                // Metni dikey olarak ortala
                var textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);
                float textY = y + (ITEM_HEIGHT / 2) + (Math.Abs(textPaint.FontMetrics.Ascent + textPaint.FontMetrics.Descent) / 2);
                canvas.DrawText(text, ITEM_PADDING + 4, textY, textPaint);

                y += ITEM_HEIGHT;
            }
        }

        public void BeginOpen()
        {
            // Varsayılan tam yükseklik
            var fullHeight = (_owner.Items.Count * ITEM_HEIGHT) + (VERTICAL_PADDING * 2);
            BeginOpen(fullHeight);
        }

        public void BeginOpen(int targetHeight)
        {
            // Hedef yükseklik limitli açılış
            _targetHeight = Math.Max(0, targetHeight);
            Size = new Size(_owner.DropDownWidth, 0);
            _currentHeight = 0;
            _hoverIndex = -1;
            _hoverY = 0;
            _targetHoverY = 0;

            Visible = true;
            _animation.StartNewAnimation(AnimationDirection.In);
            BringToFront();
            _owner.RestartDropDownAnimation(AnimationDirection.In);

            // Hover animasyonunu sıfırla
            //_hoverAnimation.Reset();
        }

        public void BeginClose()
        {
            _animation.StartNewAnimation(AnimationDirection.Out);
            _owner.DroppedDown = false;
            _owner.OnDropDownClosed(EventArgs.Empty);
            _owner.DetachDropDownWindowHandlers();
            _owner.RestartDropDownAnimation(AnimationDirection.Out);

            if (Parent != null)
            {
                Parent.Controls.Remove(this);
                Visible = false;
            }
        }

        private int GetItemIndexAtPoint(Point point)
        {
            if (point.Y < VERTICAL_PADDING || point.Y > Height - VERTICAL_PADDING)
                return -1;

            return (int)Math.Floor((point.Y - VERTICAL_PADDING) / (float)ITEM_HEIGHT);
        }
    }

    public class ObjectCollection : IList
    {
        private readonly List<object> _items;
        private readonly ComboBox _owner;

        public ObjectCollection(ComboBox owner)
        {
            _owner = owner;
            _items = new List<object>();
        }

        public int Count => _items.Count;
        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public object this[int index]
        {
            get => _items[index];
            set
            {
                _items[index] = value;
                _owner.Invalidate();
            }
        }

        public int Add(object value)
        {
            _items.Add(value);
            _owner.Invalidate();
            return _items.Count - 1;
        }

        public void Clear()
        {
            _items.Clear();
            _owner.Invalidate();
        }

        public bool Contains(object value) => _items.Contains(value);

        public void CopyTo(Array array, int index) => _items.CopyTo((object[])array, index);

        public IEnumerator GetEnumerator() => _items.GetEnumerator();

        public int IndexOf(object value) => _items.IndexOf(value);

        public void Insert(int index, object value)
        {
            _items.Insert(index, value);
            _owner.Invalidate();
        }

        public void Remove(object value)
        {
            _items.Remove(value);
            _owner.Invalidate();
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            _owner.Invalidate();
        }

        public void AddRange(IEnumerable items)
        {
            foreach (var item in items)
            {
                _items.Add(item);
            }
            _owner.Invalidate();
        }
    }

    private int _radius = 5;
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            Invalidate();
        }
    }

    private float _shadowDepth = 4f;
    public float ShadowDepth
    {
        get => _shadowDepth;
        set
        {
            if (_shadowDepth == value)
                return;

            _shadowDepth = value;
            Invalidate();
        }
    }

    #region ComboBox Properties
    private DrawMode _drawMode = DrawMode.Normal;
    [Browsable(true)]
    public DrawMode DrawMode
    {
        get => _drawMode;
        set
        {
            _drawMode = value;
            Invalidate();
        }
    }

    private bool _formattingEnabled;
    [Browsable(true)]
    public bool FormattingEnabled
    {
        get => _formattingEnabled;
        set => _formattingEnabled = value;
    }

    private bool _integralHeight = true;
    [Browsable(true)]
    public bool IntegralHeight
    {
        get => _integralHeight;
        set => _integralHeight = value;
    }

    private int _itemHeight = 36;
    [Browsable(true)]
    public int ItemHeight
    {
        get => _itemHeight;
        set
        {
            if (_itemHeight == value) return;
            _itemHeight = value;
            if (_dropDownMenu != null && _dropDownMenu.Visible)
            {
                var dropDownHeight = Math.Min(Items.Count * _itemHeight, MaxDropDownItems * _itemHeight);
                _dropDownMenu.Size = new Size(DropDownWidth, dropDownHeight);
            }
            Invalidate();
        }
    }

    private ObjectCollection _items;
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObjectCollection Items
    {
        get
        {
            if (_items == null)
                _items = new ObjectCollection(this);
            return _items;
        }
    }

    private object _selectedItem;
    [Browsable(true)]
    public object SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                _selectedIndex = Items.IndexOf(value);
                OnSelectedItemChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    private int _selectedIndex = -1;
    [Browsable(true)]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                _selectedItem = (_selectedIndex >= 0 && _selectedIndex < Items.Count) ? Items[_selectedIndex] : null;
                OnSelectedIndexChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    private string _displayMember;
    [Browsable(true)]
    public string DisplayMember
    {
        get => _displayMember;
        set
        {
            _displayMember = value;
            Invalidate();
        }
    }

    private string _valueMember;
    [Browsable(true)]
    public string ValueMember
    {
        get => _valueMember;
        set => _valueMember = value;
    }

    private object _dataSource;
    [Browsable(true)]
    public object DataSource
    {
        get => _dataSource;
        set
        {
            _dataSource = value;
            if (value is IList list)
            {
                Items.Clear();
                Items.AddRange(list);
            }
            Invalidate();
        }
    }

    private int _dropDownHeight = 106;
    [Browsable(true)]
    public int DropDownHeight
    {
        get => _dropDownHeight;
        set => _dropDownHeight = value;
    }

    private int _dropDownWidth;
    [Browsable(true)]
    public int DropDownWidth
    {
        get => _dropDownWidth == 0 ? Width : _dropDownWidth;
        set => _dropDownWidth = value;
    }

    private ComboBoxStyle _dropDownStyle = ComboBoxStyle.DropDownList;
    [Browsable(true)]
    public ComboBoxStyle DropDownStyle
    {
        get => _dropDownStyle;
        set
        {
            _dropDownStyle = value;
            Invalidate();
        }
    }

    private int _maxDropDownItems = 8;
    [Browsable(true)]
    public int MaxDropDownItems
    {
        get => _maxDropDownItems;
        set => _maxDropDownItems = value;
    }

    private bool _droppedDown;
    [Browsable(true)]
    public bool DroppedDown
    {
        get => _droppedDown;
        set
        {
            if (_droppedDown != value)
            {
                _droppedDown = value;
                if (value)
                    OnDropDown(EventArgs.Empty);
                else
                    OnDropDownClosed(EventArgs.Empty);
            }
        }
    }
    #endregion

    #region Events
    public event EventHandler SelectedIndexChanged;
    public event EventHandler SelectedItemChanged;
    public event EventHandler DropDown;
    public event EventHandler DropDownClosed;
    public event EventHandler SelectionChangeCommitted;
    public event DrawItemEventHandler DrawItem;
    public event MeasureItemEventHandler MeasureItem;
    #endregion

    private DropDownMenu _dropDownMenu;
    private readonly AnimationEngine _animation;
    private readonly AnimationEngine _hoverAnimation;
    private readonly AnimationEngine _dropDownAnimation;
    private UIWindow _dropDownWindow;
    private MouseEventHandler _dropDownWindowMouseDownHandler;
    private EventHandler _dropDownWindowDeactivateHandler;
    private KeyEventHandler _dropDownWindowKeyDownHandler;
    private bool _dropDownHandlersAttached;
    private bool _previousKeyPreview;

    public ComboBox()
    {
        MinimumSize = new Size(0, 23);
        Height = 23;
        _itemHeight = 36;

        _animation = new AnimationEngine(singular: true)
        {
            Increment = 0.12,
            SecondaryIncrement = 0.08,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        _animation.OnAnimationProgress += (s) => Invalidate();

        _hoverAnimation = new AnimationEngine(singular: true)
        {
            Increment = 0.09,
            SecondaryIncrement = 0.07,
            AnimationType = AnimationType.CustomQuadratic,
            InterruptAnimation = true
        };
        _hoverAnimation.OnAnimationProgress += (s) => Invalidate();

        _dropDownAnimation = new AnimationEngine(singular: true)
        {
            Increment = 0.16,
            SecondaryIncrement = 0.14,
            AnimationType = AnimationType.CustomQuadratic,
            InterruptAnimation = true
        };
        _dropDownAnimation.OnAnimationProgress += (s) => Invalidate();

        _dropDownMenu = new DropDownMenu(this);
        _dropDownMenu.Visible = false;
    }

    protected virtual void OnSelectedIndexChanged(EventArgs e)
    {
        SelectedIndexChanged?.Invoke(this, e);
    }

    protected virtual void OnSelectedItemChanged(EventArgs e)
    {
        SelectedItemChanged?.Invoke(this, e);
    }

    protected virtual void OnDropDown(EventArgs e)
    {
        DropDown?.Invoke(this, e);
    }

    protected virtual void OnDropDownClosed(EventArgs e)
    {
        DropDownClosed?.Invoke(this, e);
    }

    protected virtual void OnSelectionChangeCommitted(EventArgs e)
    {
        SelectionChangeCommitted?.Invoke(this, e);
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);

        var canvas = e.Surface.Canvas;

        var rect = new SKRect(0, 0, Width, Height);
        var pressProgress = (float)_animation.GetProgress();
        var hoverProgress = (float)_hoverAnimation.GetProgress();
        var dropDownProgress = (float)_dropDownAnimation.GetProgress();
        var dpiScale = DeviceDpi / 96f;
        var pressOffset = pressProgress * 1.5f * dpiScale;

        var accentColor = ColorScheme.AccentColor.ToSKColor();

        // Gölge çizimi
        using (var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha((byte)(80 * (1 - 0.3f * (hoverProgress + dropDownProgress)))),
            ImageFilter = SKImageFilter.CreateDropShadow(
                0,
                _shadowDepth * (1 - pressProgress * 0.5f),
                3 * (1 - pressProgress * 0.5f),
                3 * (1 - pressProgress * 0.5f),
                SKColors.Black.WithAlpha((byte)(90 * (1 - pressProgress * 0.4f)))),
            IsAntialias = true
        })
        {
            using var path = new SKPath();
            path.AddRoundRect(rect, _radius, _radius);
            canvas.DrawPath(path, shadowPaint);
        }

        var baseSkColor = ColorScheme.BackColor.Alpha(200).ToSKColor();
        var highlightBlend = Math.Clamp(hoverProgress * 0.35f + dropDownProgress * 0.45f + pressProgress * 0.25f, 0f, 0.75f);
        var backgroundColor = baseSkColor.InterpolateColor(accentColor, highlightBlend);

        // Arka plan çizimi
        using (var paint = new SKPaint
        {
            Color = backgroundColor,
            IsAntialias = true
        })
        {
            using var path = new SKPath();
            path.AddRoundRect(rect, _radius, _radius);
            canvas.DrawPath(path, paint);
        }

        // Hover parıltısı
        var glowIntensity = Math.Clamp((hoverProgress + dropDownProgress) * 0.6f, 0f, 1f);
        if (glowIntensity > 0f)
        {
            using var glowPaint = new SKPaint
            {
                Color = accentColor.WithAlpha((byte)(80 * glowIntensity)),
                IsAntialias = true
            };
            canvas.DrawRoundRect(rect, _radius, _radius, glowPaint);
        }

        // Kenarlık çizimi
        var borderBlend = Math.Clamp(hoverProgress * 0.4f + dropDownProgress * 0.4f + pressProgress * 0.2f, 0f, 0.9f);
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor().InterpolateColor(accentColor, borderBlend),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        })
        {
            using var path = new SKPath();
            path.AddRoundRect(rect, _radius, _radius);
            canvas.DrawPath(path, paint);
        }

        // Metin çizimi
        if (_selectedItem != null)
        {
            var textColor = ColorScheme.ForeColor.ToSKColor().InterpolateColor(accentColor, Math.Clamp(borderBlend + dropDownProgress * 0.2f, 0f, 1f));
            using var textPaint = new SKPaint
            {
                Color = textColor,
                TextSize = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                IsAntialias = true
            };

            string displayText = GetItemText(_selectedItem);
            var textBounds = new SKRect();
            textPaint.MeasureText(displayText, ref textBounds);

            canvas.DrawText(
                displayText,
                8f * dpiScale,
                (Height + textBounds.Height) / 2f + pressOffset,
                textPaint);
        }

        // Ok işareti çizimi
        var arrowRect = new SKRect(
            rect.Width - (24f * dpiScale),
            0,
            rect.Width - (8f * dpiScale),
            rect.Height - (4f * dpiScale) + _shadowDepth);

        var arrowColor = ColorScheme.ForeColor.ToSKColor().InterpolateColor(accentColor, Math.Clamp(hoverProgress + dropDownProgress, 0f, 1f));

        using (var paint = new SKPaint
        {
            Color = arrowColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.2f
        })
        {
            var centerX = arrowRect.Left + arrowRect.Width / 2f - (1f * dpiScale);
            var centerY = arrowRect.MidY;
            var leftX = centerX - 4f * dpiScale;
            var rightX = centerX + 4f * dpiScale;
        var topYOffset = Lerp(-2f * dpiScale, 2f * dpiScale, dropDownProgress);
        var bottomYOffset = Lerp(3f * dpiScale, -3f * dpiScale, dropDownProgress);

            canvas.DrawLine(
                leftX,
                centerY + topYOffset + pressOffset,
                centerX,
                centerY + bottomYOffset + pressOffset,
                paint);

            canvas.DrawLine(
                rightX,
                centerY + topYOffset + pressOffset,
                centerX,
                centerY + bottomYOffset + pressOffset,
                paint);
        }
    }

    internal override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        RestartAnimation(_hoverAnimation, AnimationDirection.In);
        RestartAnimation(_animation, AnimationDirection.In);
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        RestartAnimation(_hoverAnimation, AnimationDirection.Out);
        RestartAnimation(_animation, AnimationDirection.Out);
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            RestartAnimation(_animation, AnimationDirection.InOutIn);

            if (_dropDownMenu != null)
            {
                if (_dropDownMenu.Visible)
                {
                    _dropDownMenu.BeginClose();
                }
                else if (Items.Count > 0)
                {
                    ShowDropDown();
                }
            }
        }
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            RestartAnimation(_animation, AnimationDirection.Out);
        }
    }

    public override void OnClick(EventArgs e)
    {
        base.OnClick(e);
    }

    private void ShowDropDown()
    {
        if (Items.Count == 0) return;

        var window = ParentWindow as UIWindow ?? (Parent as UIWindow) ?? (FindForm() as UIWindow);
        if (window == null)
        {
            return;
        }

        if (_dropDownMenu.Parent != window)
        {
            _dropDownMenu.Parent?.Controls.Remove(_dropDownMenu);
            window.Controls.Add(_dropDownMenu);
            // Pencereye eklendiği anda üstte olduğundan emin ol
            window.BringToFront(_dropDownMenu);
        }

        // Pencereye göre mutlak konum (UIElementBase zincirini toplayarak)
        var bottomPoint = GetRelativeToWindow(this, window);
        var topPointY = bottomPoint.Y - Height; // elementin üst sınırı

        // Açılma yüksekliğini pencere yüksekliğine göre belirle
        int fullHeight = (_items?.Count ?? 0) * 32 + 16; // DropDownMenu sabitleri: ITEM_HEIGHT=32, VERTICAL_PADDING=8
        int availableBelow = Math.Max(0, window.ClientSize.Height - bottomPoint.Y);
        int availableAbove = Math.Max(0, topPointY);

        bool openUpwards = availableBelow < Math.Min(fullHeight, Math.Max(64, availableAbove)) && availableAbove > availableBelow;
        int openHeight = openUpwards ? Math.Min(fullHeight, availableAbove - 4) : Math.Min(fullHeight, availableBelow - 4);
        openHeight = Math.Max(64, openHeight); // en az birkaç öğe görünsün

        var targetLocation = openUpwards
            ? new Point(bottomPoint.X, Math.Max(0, topPointY - openHeight))
            : bottomPoint;

        _dropDownMenu.Location = targetLocation;
        _dropDownMenu.Width = DropDownWidth;
        // Açılmadan hemen önce üstte tut
        window.BringToFront(_dropDownMenu);
        DroppedDown = true;
        AttachDropDownWindowHandlers(window);
        _dropDownMenu.BeginOpen(openHeight);
    }

    private static Point GetRelativeToWindow(UIElementBase element, UIWindow window)
    {
        int x = 0, y = 0;
        UIElementBase current = element;
        while (current != null && current.Parent is not UIWindow)
        {
            x += current.Location.X;
            y += current.Location.Y;
            current = current.Parent as UIElementBase;
        }

        if (current != null && current.Parent is UIWindow w && w == window)
        {
            x += current.Location.X;
            y += current.Location.Y;
        }

        // Dropdown aşağı açılmalı
        y += element.Height;
        return new Point(x, y);
    }

    private void AttachDropDownWindowHandlers(UIWindow window)
    {
        if (_dropDownHandlersAttached && _dropDownWindow == window)
            return;

        DetachDropDownWindowHandlers();

        _dropDownWindow = window;
        _dropDownWindowMouseDownHandler ??= DropDownWindowOnMouseDown;
        _dropDownWindowDeactivateHandler ??= DropDownWindowOnDeactivate;
        _dropDownWindowKeyDownHandler ??= DropDownWindowOnKeyDown;

        _previousKeyPreview = window.KeyPreview;
        window.KeyPreview = true;

        window.MouseDown += _dropDownWindowMouseDownHandler;
        window.Deactivate += _dropDownWindowDeactivateHandler;
        window.KeyDown += _dropDownWindowKeyDownHandler;

        _dropDownHandlersAttached = true;
    }

    internal void DetachDropDownWindowHandlers()
    {
        if (!_dropDownHandlersAttached || _dropDownWindow == null)
            return;

        if (_dropDownWindowMouseDownHandler != null)
            _dropDownWindow.MouseDown -= _dropDownWindowMouseDownHandler;
        if (_dropDownWindowDeactivateHandler != null)
            _dropDownWindow.Deactivate -= _dropDownWindowDeactivateHandler;
        if (_dropDownWindowKeyDownHandler != null)
            _dropDownWindow.KeyDown -= _dropDownWindowKeyDownHandler;

        _dropDownWindow.KeyPreview = _previousKeyPreview;

        _dropDownWindow = null;
        _dropDownHandlersAttached = false;
    }

    private void DropDownWindowOnMouseDown(object sender, MouseEventArgs e)
    {
        if (!_dropDownHandlersAttached || _dropDownMenu == null || !_dropDownMenu.Visible)
            return;

        if (_dropDownMenu.Bounds.Contains(e.Location))
            return;

        if (_dropDownWindow != null)
        {
            var comboBounds = GetWindowRelativeBounds(this, _dropDownWindow);
            if (comboBounds.Contains(e.Location))
                return;
        }

        _dropDownMenu.BeginClose();
    }

    private void DropDownWindowOnDeactivate(object sender, EventArgs e)
    {
        if (_dropDownMenu != null && _dropDownMenu.Visible)
        {
            _dropDownMenu.BeginClose();
        }
    }

    private void DropDownWindowOnKeyDown(object sender, KeyEventArgs e)
    {
        if (_dropDownMenu == null || !_dropDownMenu.Visible)
            return;

        if (e.KeyCode == Keys.Escape)
        {
            _dropDownMenu.BeginClose();
            e.Handled = true;
        }
    }

    private static Rectangle GetWindowRelativeBounds(UIElementBase element, UIWindow window)
    {
        var screenLocation = element.PointToScreen(Point.Empty);
        var windowPoint = window.PointToClient(screenLocation);
        return new Rectangle(windowPoint, element.Size);
    }

    private static float Lerp(float start, float end, float progress) => start + (end - start) * progress;

    private string GetItemText(object item)
    {
        if (item == null) return string.Empty;

        if (!string.IsNullOrEmpty(_displayMember) && item is IList list)
        {
            var prop = item.GetType().GetProperty(_displayMember);
            if (prop != null)
                return prop.GetValue(item)?.ToString() ?? string.Empty;
        }

        return item.ToString();
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var size = base.GetPreferredSize(proposedSize);
        size.Height = 23;
        return size;
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        Height = 23;
    }

    public void BeginUpdate() { /* Not implemented */ }
    public void EndUpdate() { /* Not implemented */ }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DetachDropDownWindowHandlers();
            _dropDownMenu?.Dispose();
        }
        base.Dispose(disposing);
    }

    private static void RestartAnimation(AnimationEngine engine, AnimationDirection direction)
    {
        if (engine == null)
            return;

        engine.SetDirection(direction);

        var startProgress = direction switch
        {
            AnimationDirection.Out or AnimationDirection.InOutOut or AnimationDirection.InOutRepeatingOut => 1.0,
            _ => 0.0
        };

        engine.SetProgress(startProgress);
        engine.StartNewAnimation(direction);
    }

    private void RestartDropDownAnimation(AnimationDirection direction)
    {
        RestartAnimation(_dropDownAnimation, direction);
    }
}
