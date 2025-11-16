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
    #region Inner Classes

    private class DropDownPanel : UIElementBase
    {
        private readonly ComboBox _owner;
        private int _hoveredIndex = -1;
        private int _selectedIndex = -1;
        private ScrollBar _scrollBar;
        private int _scrollOffset = 0;
        private int _visibleItemCount;
        private readonly AnimationEngine _openAnimation = new AnimationEngine(singular: true)
        {
            Increment = 0.13,
            AnimationType = AnimationType.EaseOut,
            InterruptAnimation = true
        };
        private bool _isClosing;
        private bool _openingUpwards;

        // Per-item hover animasyonları
        private readonly Dictionary<int, AnimationEngine> _itemHoverAnims = new();

        // Windows 11 WinUI3 benzeri modern boşluklar
        private const int VERTICAL_PADDING = 4;
        private const int ITEM_MARGIN = 6; // Dropdown kenarından item arkaplan kenarına kadar margin
        private const float CORNER_RADIUS = 8f;
        private const int SCROLL_BAR_WIDTH = 14;

        public DropDownPanel(ComboBox owner)
        {
            _owner = owner;
            BackColor = Color.Transparent;
            Visible = false;
            TabStop = false;

            _scrollBar = new ScrollBar
            {
                Orientation = SDUI.Orientation.Vertical,
                Visible = false,
                Minimum = 0,
                Maximum = 0,
                Value = 0,
                SmallChange = 1,
                LargeChange = 3,
                Thickness = 6,
                Radius = 6,
                AutoHide = true
            };
            _scrollBar.ValueChanged += ScrollBar_ValueChanged;
            Controls.Add(_scrollBar);

            _openAnimation.OnAnimationProgress += _ =>
            {
                Invalidate();
                var p = _openAnimation.GetProgress();
                if (_isClosing && p <= 0.001)
                    Hide();
            };
        }

        private int ItemHeight => Math.Max(32, _owner.ItemHeight);

        private void ScrollBar_ValueChanged(object sender, EventArgs e)
        {
            _scrollOffset = _scrollBar.Value;
            Invalidate();
        }

        private AnimationEngine EnsureItemAnim(int index)
        {
            if (!_itemHoverAnims.TryGetValue(index, out var ae))
            {
                ae = new AnimationEngine(singular: true)
                {
                    Increment = 0.18,
                    AnimationType = AnimationType.EaseInOut,
                    InterruptAnimation = true
                };
                ae.OnAnimationProgress += _ => Invalidate();
                _itemHoverAnims[index] = ae;
            }
            return ae;
        }

        public void ShowItems()
        {
            _hoveredIndex = -1;
            _selectedIndex = _owner.SelectedIndex;

            int totalItems = _owner.Items.Count;
            int maxVisibleItems = Math.Max(1, (Height - 2 * VERTICAL_PADDING) / ItemHeight);
            _visibleItemCount = Math.Min(totalItems, maxVisibleItems);

            bool needsScrollBar = totalItems > maxVisibleItems;
            _scrollBar.Visible = needsScrollBar;

            if (needsScrollBar)
            {
                _scrollBar.Location = new Point(Width - SCROLL_BAR_WIDTH - 2, VERTICAL_PADDING);
                _scrollBar.Size = new Size(SCROLL_BAR_WIDTH, Height - 2 * VERTICAL_PADDING);
                _scrollBar.Maximum = Math.Max(0, totalItems - maxVisibleItems);
                _scrollBar.LargeChange = maxVisibleItems;
            }

            _scrollOffset = 0;
            _scrollBar.Value = 0;

            Invalidate();
        }

        public void BeginOpen(bool openUpwards)
        {
            _openingUpwards = openUpwards;
            _isClosing = false;
            ShowItems();
            Visible = true;
            _openAnimation.SetProgress(0); // Her açılışta 0'dan başla
            _openAnimation.StartNewAnimation(AnimationDirection.In);
        }

        public void BeginClose()
        {
            if (!Visible) return;
            _isClosing = true;
            _openAnimation.StartNewAnimation(AnimationDirection.Out);
        }

        internal override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int newIndex = GetItemIndexAtPoint(e.Location);
            if (newIndex != _hoveredIndex)
            {
                if (_hoveredIndex >= 0)
                    EnsureItemAnim(_hoveredIndex).StartNewAnimation(AnimationDirection.Out);

                _hoveredIndex = newIndex;
                if (_hoveredIndex >= 0)
                    EnsureItemAnim(_hoveredIndex).StartNewAnimation(AnimationDirection.In);

                Invalidate();
            }
        }

        internal override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredIndex >= 0)
                EnsureItemAnim(_hoveredIndex).StartNewAnimation(AnimationDirection.Out);
            _hoveredIndex = -1;
            Invalidate();
        }

        internal override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!_scrollBar.Visible) return;
            int delta = e.Delta > 0 ? -1 : 1;
            int newValue = _scrollBar.Value + delta;
            newValue = Math.Max(_scrollBar.Minimum, Math.Min(_scrollBar.Maximum, newValue));
            _scrollBar.Value = newValue;
        }

        internal override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                int itemIndex = GetItemIndexAtPoint(e.Location);
                if (itemIndex >= 0 && itemIndex < _owner.Items.Count)
                {
                    _owner.SelectedIndex = itemIndex;
                    _owner.OnSelectionChangeCommitted(EventArgs.Empty);
                    Hide();
                }
            }
        }

        public new void Hide()
        {
            Visible = false;
            Width = 0;
            Height = 0;
            _owner.DroppedDown = false;
            _owner._arrowAnimation.SetProgress(0);
            _owner.OnDropDownClosed(EventArgs.Empty);
            _owner.DetachWindowHandlers();
            Parent?.Controls.Remove(this);
        }

        private int GetItemIndexAtPoint(Point point)
        {
            if (point.Y < VERTICAL_PADDING || point.Y > Height - VERTICAL_PADDING)
                return -1;

            if (_scrollBar.Visible && point.X >= _scrollBar.Bounds.Left)
                return -1;

            int relativeY = point.Y - VERTICAL_PADDING;
            int itemIndex = (relativeY / ItemHeight) + _scrollOffset;

            return (itemIndex >= 0 && itemIndex < _owner.Items.Count) ? itemIndex : -1;
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (Width <= 0 || Height <= 0 || _owner.Items.Count == 0)
                return;

            float openProgress = (float)_openAnimation.GetProgress();
            if (!Visible) openProgress = 0;

            using (var layerPaint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(255 * openProgress)) })
            {
                canvas.SaveLayer(layerPaint);
                
                // Subtle fade-in animasyonu
                float translateY = (_openingUpwards ? 1f - openProgress : openProgress - 1f) * 8f;
                canvas.Translate(0, translateY);

                var mainRect = new SKRect(0, 0, Width, Height);
                var mainRoundRect = new SKRoundRect(mainRect, CORNER_RADIUS);
                
                // Multi-layer modern shadow (ContextMenuStrip gibi)
                canvas.Save();
                for (int i = 0; i < 4; i++)
                {
                    float offsetY = 2 + i * 2;
                    float blurRadius = 6 + i * 4;
                    byte shadowAlpha = (byte)((25 - i * 5) * openProgress);

                    using var shadowPaint = new SKPaint
                    {
                        Color = SKColors.Black.WithAlpha(shadowAlpha),
                        MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius),
                        IsAntialias = true
                    };

                    canvas.Save();
                    canvas.Translate(0, offsetY);
                    canvas.DrawRoundRect(mainRoundRect, shadowPaint);
                    canvas.Restore();
                }
                canvas.Restore();
                
                // High-quality solid background
                using (var bgPaint = new SKPaint 
                { 
                    Color = ColorScheme.BackColor.ToSKColor(),
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High
                })
                {
                    canvas.DrawRoundRect(mainRoundRect, bgPaint);
                }

                // Clip path
                using (var clipPath = new SKPath())
                {
                    clipPath.AddRoundRect(mainRoundRect);
                    canvas.Save();
                    canvas.ClipPath(clipPath, antialias: true);
                }

                // Minimal üst highlight
                using (var highlightPaint = new SKPaint
                {
                    Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0), 
                        new SKPoint(0, Height * 0.15f),
                        new[] 
                        { 
                            SKColors.White.WithAlpha(12), 
                            SKColors.Transparent 
                        },
                        null,
                        SKShaderTileMode.Clamp),
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(mainRect, highlightPaint);
                }

                // High-quality border
                using (var borderPaint = new SKPaint 
                { 
                    Color = ColorScheme.BorderColor.Alpha(100).ToSKColor(),
                    IsAntialias = true, 
                    Style = SKPaintStyle.Stroke, 
                    StrokeWidth = 1f,
                    FilterQuality = SKFilterQuality.High
                })
                {
                    var borderRect = new SKRoundRect(
                        new SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f), 
                        CORNER_RADIUS - 0.5f);
                    canvas.DrawRoundRect(borderRect, borderPaint);
                }

                // High-quality text paint
                using var textPaint = new SKPaint
                {
                    TextSize = _owner.Font.Size.PtToPx(this),
                    Typeface = SKTypeface.FromFamilyName(_owner.Font.FontFamily.Name, SKFontStyle.Normal),
                    IsAntialias = true,
                    SubpixelText = true,
                    LcdRenderText = true,
                    FilterQuality = SKFilterQuality.High
                };

                float contentRightInset = (_scrollBar.Visible ? SCROLL_BAR_WIDTH + 6 : 0);
                float currentY = VERTICAL_PADDING;
                int startIndex = _scrollOffset;
                int endIndex = Math.Min(_owner.Items.Count, startIndex + _visibleItemCount);

                for (int i = startIndex; i < endIndex && currentY < Height - VERTICAL_PADDING; i++)
                {
                    // Item rect with proper margins from dropdown edges
                    float itemLeftEdge = ITEM_MARGIN;
                    float itemRightEdge = Width - ITEM_MARGIN - contentRightInset;
                    var itemRect = new SKRect(
                        itemLeftEdge, 
                        currentY, 
                        itemRightEdge, 
                        currentY + ItemHeight);

                    var hoverAE = EnsureItemAnim(i);
                    float hProg = (float)hoverAE.GetProgress();
                    
                    bool isSelected = (i == _selectedIndex);
                    float itemRadius = 4f;

                    // High-quality selection background
                    if (isSelected)
                    {
                        using var selPaint = new SKPaint
                        {
                            Color = ColorScheme.AccentColor.Alpha(45).ToSKColor(),
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High
                        };
                        var selRect = new SKRoundRect(itemRect, itemRadius);
                        canvas.DrawRoundRect(selRect, selPaint);
                    }
                    // High-quality hover effect
                    else if (hProg > 0.001f)
                    {
                        byte hoverAlpha = (byte)(25 + 35 * hProg);
                        using var hoverPaint = new SKPaint 
                        { 
                            Color = ColorScheme.AccentColor.Alpha(hoverAlpha).ToSKColor(),
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High
                        };
                        var hoverRect = new SKRoundRect(itemRect, itemRadius);
                        canvas.DrawRoundRect(hoverRect, hoverPaint);
                    }

                    // High-quality text rendering
                    string text = _owner.GetItemText(_owner.Items[i]);
                    textPaint.Color = ColorScheme.ForeColor.ToSKColor();

                    float baseTextY = currentY + ItemHeight / 2f + textPaint.FontMetrics.XHeight / 2f;
                    float textX = ITEM_MARGIN + 8;
                    
                    canvas.DrawText(text, textX, baseTextY, textPaint);

                    currentY += ItemHeight;
                }

                canvas.Restore(); // clipPath restore
                canvas.Restore(); // layerPaint restore
            }
        }
    }

    /// <summary>
    /// ComboBox öğeleri koleksiyonu
    /// </summary>
    public class ObjectCollection : IList
    {
        private readonly List<object> _items = new();
        private readonly ComboBox _owner;

        public ObjectCollection(ComboBox owner) => _owner = owner;

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
                _items.Add(item);
            _owner.Invalidate();
        }
    }

    #endregion

    #region Properties

    private int _radius = 6;
    [Browsable(true)]
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            Invalidate();
        }
    }

    private ObjectCollection _items;
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObjectCollection Items => _items ??= new ObjectCollection(this);

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
            if (_selectedIndex != value && value >= -1 && value < Items.Count)
            {
                _selectedIndex = value;
                _selectedItem = value >= 0 ? Items[value] : null;
                // DropDown stilinde kullanıcı listeden bir seçim yapınca metni güncelle
                if (_dropDownStyle == ComboBoxStyle.DropDown && _selectedItem != null)
                {
                    _text = GetItemText(_selectedItem);
                }
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

    private int _dropDownWidth;
    [Browsable(true)]
    public int DropDownWidth
    {
        get => _dropDownWidth == 0 ? Width : _dropDownWidth;
        set => _dropDownWidth = value;
    }

    private int _maxDropDownItems = 8;
    [Browsable(true)]
    public int MaxDropDownItems
    {
        get => _maxDropDownItems;
        set => _maxDropDownItems = value;
    }

    private DrawMode _drawMode = DrawMode.Normal;
    [Browsable(true)]
    public DrawMode DrawMode
    {
        get => _drawMode;
        set => _drawMode = value;
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

    private int _itemHeight = 32;
    [Browsable(true)]
    public int ItemHeight
    {
        get => _itemHeight;
        set => _itemHeight = value;
    }

    private int _dropDownHeight = 150;
    [Browsable(true)]
    public int DropDownHeight
    {
        get => _dropDownHeight;
        set => _dropDownHeight = value;
    }

    private ComboBoxStyle _dropDownStyle = ComboBoxStyle.DropDown;
    [Browsable(true)]
    public ComboBoxStyle DropDownStyle
    {
        get => _dropDownStyle;
        set
        {
            if (_dropDownStyle == value) return;
            _dropDownStyle = value;
            Invalidate();
        }
    }

    private float _shadowDepth = 4;
    [Browsable(true)]
    public float ShadowDepth
    {
        get => _shadowDepth;
        set => _shadowDepth = value;
    }

    private string _text = "";
    [Browsable(true)]
    public override string Text
    {
        get => DropDownStyle == ComboBoxStyle.DropDown ? _text :
               _selectedItem != null ? GetItemText(_selectedItem) : "";
        set
        {
            if (DropDownStyle == ComboBoxStyle.DropDown)
            {
                _text = value;
                Invalidate();
            }
            // DropDownList'te text düzenlenemez
        }
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

    #endregion

    #region Private Fields

    private DropDownPanel _dropDownPanel;
    private readonly AnimationEngine _hoverAnimation;
    private readonly AnimationEngine _pressAnimation;
    private readonly AnimationEngine _arrowAnimation;
    private UIWindow _parentWindow;
    private MouseEventHandler _windowMouseDownHandler;
    private EventHandler _windowDeactivateHandler;
    private KeyEventHandler _windowKeyDownHandler;
    private bool _handlersAttached;
    private bool _ignoreNextClick;

    #endregion

    #region Constructor

    public ComboBox()
    {
        MinimumSize = new Size(50, 28);
        Size = new Size(120, 28);

        _hoverAnimation = new AnimationEngine(singular: true)
        {
            Increment = 0.15,
            AnimationType = AnimationType.EaseInOut
        };
        _hoverAnimation.OnAnimationProgress += _ => Invalidate();

        _pressAnimation = new AnimationEngine(singular: true)
        {
            Increment = 0.2,
            AnimationType = AnimationType.EaseInOut
        };
        _pressAnimation.OnAnimationProgress += _ => Invalidate();

        _arrowAnimation = new AnimationEngine(singular: true)
        {
            Increment = 0.12,
            AnimationType = AnimationType.EaseInOut
        };
        _arrowAnimation.OnAnimationProgress += _ => Invalidate();
        // Başlangıçta ok aşağı bakmalı (0 rotation)
        _arrowAnimation.SetProgress(0);

        _dropDownPanel = new DropDownPanel(this);
    }

    #endregion

    #region Event Handlers

    protected virtual void OnSelectedIndexChanged(EventArgs e) => SelectedIndexChanged?.Invoke(this, e);
    protected virtual void OnSelectedItemChanged(EventArgs e) => SelectedItemChanged?.Invoke(this, e);
    protected virtual void OnDropDown(EventArgs e) => DropDown?.Invoke(this, e);
    protected virtual void OnDropDownClosed(EventArgs e) => DropDownClosed?.Invoke(this, e);
    protected virtual void OnSelectionChangeCommitted(EventArgs e) => SelectionChangeCommitted?.Invoke(this, e);

    internal override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hoverAnimation.StartNewAnimation(AnimationDirection.In);
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoverAnimation.StartNewAnimation(AnimationDirection.Out);
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Left)
        {
            _pressAnimation.StartNewAnimation(AnimationDirection.In);

            // DropDownStyle'a göre davranış
            if (DropDownStyle == ComboBoxStyle.Simple)
            {
                // Simple modda dropdown açılmaz
                return;
            }

            if (DroppedDown)
            {
                CloseDropDown();
            }
            else if (Items.Count > 0)
            {
                // Debug: Console'a yaz
                System.Diagnostics.Debug.WriteLine($"ComboBox OnMouseDown: Items.Count={Items.Count}, Opening dropdown");
                _arrowAnimation.StartNewAnimation(AnimationDirection.In);
                OpenDropDown();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ComboBox OnMouseDown: No items to show (Items.Count={Items.Count})");
            }
        }
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Left)
        {
            _pressAnimation.StartNewAnimation(AnimationDirection.Out);
        }
    }

    #endregion

    #region Private Methods

    // helper to measure dropdown auto width based on items
    private int MeasureDropDownAutoWidth()
    {
        if (Items.Count == 0) return Width;
        using var paint = new SKPaint
        {
            TextSize = Font.Size.PtToPx(this),
            Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
            IsAntialias = true
        };
        float maxText = 0f;
        for (int i = 0; i < Items.Count; i++)
        {
            var txt = GetItemText(Items[i]) ?? string.Empty;
            var bounds = new SKRect();
            paint.MeasureText(txt, ref bounds);
            maxText = Math.Max(maxText, bounds.Width);
        }
        // padding + potential scrollbar + borders
        int hPadding = 10 + 10 + 8; // left + right + extra inset
        bool predictScrollbar = Items.Count > MaxDropDownItems;
        int scrollbar = predictScrollbar ? 14 + 6 : 0;
        int autoWidth = (int)Math.Ceiling(maxText) + hPadding + scrollbar;
        return Math.Max(autoWidth, Width);
    }

    private void OpenDropDown()
    {
        System.Diagnostics.Debug.WriteLine($"OpenDropDown called: Items.Count={Items.Count}, DroppedDown={DroppedDown}");

        if (Items.Count == 0 || DroppedDown)
            return;

        _parentWindow = ParentWindow as UIWindow ?? Parent as UIWindow ?? FindForm() as UIWindow;
        if (_parentWindow == null)
            return;

        if (_dropDownPanel.Parent != _parentWindow)
        {
            _dropDownPanel.Parent?.Controls.Remove(_dropDownPanel);
            _parentWindow.Controls.Add(_dropDownPanel);
        }

        Point comboLocation = GetLocationRelativeToWindow();
        var client = _parentWindow.ClientRectangle;

        const int MARGIN = 8;
        int totalItemsHeight = Items.Count * ItemHeight + (2 * 4); // VERTICAL_PADDING
        int preferredHeight = Math.Min(totalItemsHeight, MaxDropDownItems * ItemHeight + (2 * 4));

        int maxAvailableHeight = _parentWindow.Height - comboLocation.Y - Height - MARGIN;
        int actualHeight = Math.Min(preferredHeight, maxAvailableHeight);
        actualHeight = Math.Max(actualHeight, ItemHeight + (2 * 4));

        int dropdownHeight = Math.Min(DropDownHeight, actualHeight);
        int dropdownWidth = _dropDownWidth == 0 ? MeasureDropDownAutoWidth() : DropDownWidth;

        // Smart positioning with flip support (ContextMenuStrip gibi)
        bool canOpenDown = comboLocation.Y + Height + dropdownHeight <= client.Bottom - MARGIN;
        bool canOpenUp = comboLocation.Y - dropdownHeight >= client.Top + MARGIN;

        Point dropdownLocation;
        if (canOpenDown)
        {
            dropdownLocation = new Point(comboLocation.X, comboLocation.Y + Height + 2);
        }
        else if (canOpenUp)
        {
            dropdownLocation = new Point(comboLocation.X, comboLocation.Y - dropdownHeight - 2);
        }
        else
        {
            // Neither fits perfectly - pick the side with more space
            int spaceBelow = client.Bottom - (comboLocation.Y + Height);
            int spaceAbove = comboLocation.Y;
            
            if (spaceBelow > spaceAbove)
            {
                dropdownLocation = new Point(comboLocation.X, comboLocation.Y + Height + 2);
                dropdownHeight = Math.Max(ItemHeight + 8, spaceBelow - MARGIN);
            }
            else
            {
                dropdownHeight = Math.Max(ItemHeight + 8, spaceAbove - MARGIN);
                dropdownLocation = new Point(comboLocation.X, comboLocation.Y - dropdownHeight - 2);
            }
        }

        // Horizontal bounds check with flip
        if (dropdownLocation.X + dropdownWidth > client.Right - MARGIN)
        {
            int leftPos = dropdownLocation.X - dropdownWidth + Width;
            if (leftPos >= client.Left + MARGIN)
                dropdownLocation.X = leftPos;
            else
                dropdownLocation.X = client.Right - dropdownWidth - MARGIN;
        }

        // Final safety clamp
        dropdownLocation.X = Math.Max(client.Left + MARGIN, Math.Min(dropdownLocation.X, client.Right - dropdownWidth - MARGIN));
        dropdownLocation.Y = Math.Max(client.Top + MARGIN, Math.Min(dropdownLocation.Y, client.Bottom - dropdownHeight - MARGIN));
        
        // Clamp size to available space
        dropdownWidth = Math.Min(dropdownWidth, client.Width - MARGIN * 2);
        dropdownHeight = Math.Min(dropdownHeight, client.Height - MARGIN * 2);

        _dropDownPanel.Location = dropdownLocation;
        _dropDownPanel.Width = dropdownWidth;
        _dropDownPanel.Height = dropdownHeight;

        _dropDownPanel.BeginOpen(openUpwards: !canOpenDown && canOpenUp);
        _dropDownPanel.ZOrder = 9999;

        DroppedDown = true;
        _ignoreNextClick = true;

        AttachWindowHandlers();
        _parentWindow.Invalidate();

        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
        {
            if (_parentWindow?.InvokeRequired == true) 
                _parentWindow.Invoke(() => _ignoreNextClick = false);
            else 
                _ignoreNextClick = false;
        });
    }

    private void CloseDropDown()
    {
        System.Diagnostics.Debug.WriteLine($"CloseDropDown called: DroppedDown={DroppedDown}");
        if (!DroppedDown)
        {
            System.Diagnostics.Debug.WriteLine("CloseDropDown: Already closed");
            return;
        }

        _arrowAnimation.StartNewAnimation(AnimationDirection.Out);
        _dropDownPanel.BeginClose();
        System.Diagnostics.Debug.WriteLine("CloseDropDown: Dropdown closed");
    }

    private Point GetLocationRelativeToWindow()
    {
        int x = 0, y = 0;
        UIElementBase current = this;

        while (current != null && current.Parent != _parentWindow)
        {
            x += current.Location.X;
            y += current.Location.Y;
            current = current.Parent as UIElementBase;
        }

        if (current != null && current.Parent == _parentWindow)
        {
            x += current.Location.X;
            y += current.Location.Y;
        }

        return new Point(x, y);
    }

    private void AttachWindowHandlers()
    {
        if (_handlersAttached)
            return;

        _windowMouseDownHandler ??= OnWindowMouseDown;
        _windowDeactivateHandler ??= OnWindowDeactivate;
        _windowKeyDownHandler ??= OnWindowKeyDown;

        _parentWindow.MouseDown += _windowMouseDownHandler;
        _parentWindow.Deactivate += _windowDeactivateHandler;
        _parentWindow.KeyDown += _windowKeyDownHandler;

        _handlersAttached = true;
    }

    private void DetachWindowHandlers()
    {
        if (!_handlersAttached || _parentWindow == null)
            return;

        _parentWindow.MouseDown -= _windowMouseDownHandler;
        _parentWindow.Deactivate -= _windowDeactivateHandler;
        _parentWindow.KeyDown -= _windowKeyDownHandler;

        _handlersAttached = false;
    }

    private void OnWindowMouseDown(object sender, MouseEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnWindowMouseDown: Click at ({e.Location.X},{e.Location.Y}), _ignoreNextClick={_ignoreNextClick}");

        if (_ignoreNextClick)
        {
            _ignoreNextClick = false;
            System.Diagnostics.Debug.WriteLine("OnWindowMouseDown: Ignoring click (first click after open)");
            return;
        }

        // ComboBox'ın kendi alanı içinde mi kontrol et
        var comboBounds = new Rectangle(GetLocationRelativeToWindow(), Size);
        System.Diagnostics.Debug.WriteLine($"OnWindowMouseDown: ComboBox bounds=({comboBounds.X},{comboBounds.Y},{comboBounds.Width},{comboBounds.Height})");
        if (comboBounds.Contains(e.Location))
        {
            System.Diagnostics.Debug.WriteLine("OnWindowMouseDown: Click inside ComboBox, not closing");
            return;
        }

        // Dropdown panel içinde mi kontrol et
        var panelBounds = new Rectangle(_dropDownPanel.Location, _dropDownPanel.Size);
        System.Diagnostics.Debug.WriteLine($"OnWindowMouseDown: Panel bounds=({panelBounds.X},{panelBounds.Y},{panelBounds.Width},{panelBounds.Height})");
        if (panelBounds.Contains(e.Location))
        {
            System.Diagnostics.Debug.WriteLine("OnWindowMouseDown: Click inside panel, not closing");
            return;
        }

        System.Diagnostics.Debug.WriteLine("OnWindowMouseDown: Click outside, closing dropdown");
        CloseDropDown();
    }

    private void OnWindowDeactivate(object sender, EventArgs e)
    {
        CloseDropDown();
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            CloseDropDown();
            e.Handled = true;
        }
    }

    private string GetItemText(object item)
    {
        if (item == null)
            return string.Empty;

        if (!string.IsNullOrEmpty(_displayMember))
        {
            var prop = item.GetType().GetProperty(_displayMember);
            if (prop != null)
                return prop.GetValue(item)?.ToString() ?? string.Empty;
        }

        return item.ToString();
    }

    #endregion

    #region Rendering

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var rect = new SKRect(0, 0, Width, Height);

        float hoverProgress = (float)_hoverAnimation.GetProgress();
        float pressProgress = (float)_pressAnimation.GetProgress();

        var accentColor = ColorScheme.AccentColor.ToSKColor();
        var baseColor = ColorScheme.BackColor.ToSKColor();

        // Arka plan rengi hesapla (daha yumuşak blend)
        float blendFactor = Math.Clamp(hoverProgress * 0.15f + pressProgress * 0.1f + (DroppedDown ? 0.2f : 0f), 0f, 0.4f);
        var backgroundColor = baseColor.InterpolateColor(accentColor, blendFactor);

        // Modern gölge efekti (hover ile dinamik - daha subtle)
        if (ShadowDepth > 0)
        {
            float shadowAlpha = 12 + (hoverProgress * 8) + (pressProgress * 5);
            using (var shadowPaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha((byte)shadowAlpha),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, ShadowDepth * 0.5f),
                IsAntialias = true
            })
            {
                canvas.Save();
                canvas.Translate(0, ShadowDepth * 0.3f);
                canvas.DrawRoundRect(rect, _radius, _radius, shadowPaint);
                canvas.Restore();
            }
        }

        // Base arka plan
        using (var basePaint = new SKPaint
        {
            Color = backgroundColor,
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, basePaint);
        }

        // Hover/press glow overlay
        if (hoverProgress > 0 || pressProgress > 0)
        {
            byte glowAlpha = (byte)((hoverProgress * 15 + pressProgress * 10));
            using var glowPaint = new SKPaint
            {
                Color = accentColor.WithAlpha(glowAlpha),
                IsAntialias = true
            };
            canvas.DrawRoundRect(rect, _radius, _radius, glowPaint);
        }

        // Üstte ince aydınlık highlight (acrylic effect)
        using (var highlightPaint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, Height * 0.4f),
                new[]
                {
                    SKColors.White.WithAlpha(10),
                    SKColors.Transparent
                },
                null,
                SKShaderTileMode.Clamp),
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, highlightPaint);
        }

        // Modern kenarlık (ince ve zarif)
        float borderAlpha = 0.4f + (hoverProgress * 0.2f) + (pressProgress * 0.1f);
        using (var borderPaint = new SKPaint
        {
            Color = ColorScheme.BorderColor.ToSKColor().InterpolateColor(accentColor, blendFactor * 0.4f).ToColor().Alpha((byte)(255 * borderAlpha)).ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f
        })
        {
            rect.Inflate(-0.5f, -0.5f);
            canvas.DrawRoundRect(rect, _radius - 0.5f, _radius - 0.5f, borderPaint);
        }

        // Metin çizimi
        string displayText = Text;
        if (string.IsNullOrEmpty(displayText))
        {
            displayText = "Seçiniz...";
        }
        var textColor = ColorScheme.ForeColor.ToSKColor();

        using var textPaint = new SKPaint
        {
            Color = textColor,
            TextSize = Font.Size.PtToPx(this),
            Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
            IsAntialias = true,
            SubpixelText = true
        };

        var textBounds = new SKRect();
        textPaint.MeasureText(displayText, ref textBounds);
        float textY = Height / 2f + textPaint.FontMetrics.XHeight / 2f;
        float textX = 16;

        canvas.DrawText(displayText, textX, textY, textPaint);

        // Modern Chevron ikonu - DropDown ve DropDownList stillerinde göster
        if (DropDownStyle != ComboBoxStyle.Simple)
        {
            float chevronSize = 10;
            float chevronX = Width - 22;
            float chevronY = Height / 2f;

            // Arrow rotation animasyonu: 0° kapalı, 180° açık
            float arrowProgress = (float)_arrowAnimation.GetProgress();
            float rotation = 180f * arrowProgress;

            // Chevron rengi (hover ve press durumuna göre)
            float chevronBlend = Math.Clamp(hoverProgress * 0.35f + pressProgress * 0.5f, 0f, 1f);
            var chevronColor = ColorScheme.ForeColor.ToSKColor().InterpolateColor(accentColor, chevronBlend);

            // Chevron hover background circle (animasyonlu)
            if (hoverProgress > 0.05f)
            {
                float circleRadius = 13f * (float)Math.Clamp(hoverProgress, 0, 1);
                using var bgPaint = new SKPaint
                {
                    Color = accentColor.WithAlpha((byte)(hoverProgress * 18)),
                    IsAntialias = true
                };
                canvas.DrawCircle(chevronX, chevronY, circleRadius, bgPaint);
            }

            // Rotasyon için canvas'ı kaydet
            canvas.Save();
            canvas.Translate(chevronX, chevronY);
            canvas.RotateDegrees(rotation);

            // Modern chevron (V şekli) - ince ve zarif
            using (var chevronPaint = new SKPaint
            {
                Color = chevronColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            })
            {
                float halfSize = chevronSize * 0.45f;
                using var chevronPath = new SKPath();
                chevronPath.MoveTo(-halfSize, -halfSize * 0.5f);
                chevronPath.LineTo(0, halfSize * 0.5f);
                chevronPath.LineTo(halfSize, -halfSize * 0.5f);
                canvas.DrawPath(chevronPath, chevronPaint);
            }

            canvas.Restore();
        }
    }

    #endregion

    #region Overrides

    public override Size GetPreferredSize(Size proposedSize)
    {
        return new Size(Math.Max(120, proposedSize.Width), 28);
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        Height = 28; // Sabit yükseklik
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DetachWindowHandlers();
            _dropDownPanel?.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}
