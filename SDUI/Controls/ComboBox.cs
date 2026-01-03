using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
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
    private const int DropDownVerticalPadding = 6;
    private const int DefaultMinVisibleItems = 4;
    private const int MinDropDownItemHeight = 32;

    // Dropdown rendering enforces a minimum item height for a modern look.
    // Keep dropdown sizing consistent with what the dropdown actually draws.
    private int EffectiveDropDownItemHeight => Math.Max(MinDropDownItemHeight, _itemHeight);

    #region Inner Classes

    private class DropDownPanel : UIElementBase
    {
        private readonly ComboBox _owner;
        private int _hoveredIndex = -1;
        private int _selectedIndex = -1;
        private ScrollBar _scrollBar;
        private int _scrollOffset = 0;
        private int _visibleItemCount;
        private readonly AnimationManager _openAnimation = new AnimationManager(singular: true)
        {
            Increment = 0.13,
            AnimationType = AnimationType.EaseOut,
            InterruptAnimation = true
        };
        private bool _isClosing;
        private bool _openingUpwards;

        private SKPaint? _layerPaint;
        private SKPaint? _bgPaint;
        private SKPaint? _highlightPaint;
        private SKShader? _highlightShader;
        private int _highlightShaderHeight;
        private SKPaint? _borderPaint;
        private SKPaint? _textPaint;
        private SKPaint? _selectionPaint;
        private SKPaint? _hoverPaint;
        private SKPath? _clipPath;

        private SKFont? _cachedFont;
        private Font? _cachedFontSource;
        private int _cachedFontDpi;

        private readonly SKPaint?[] _shadowPaints = new SKPaint?[4];
        private readonly SKMaskFilter?[] _shadowMaskFilters = new SKMaskFilter?[4];

        // Per-item hover animasyonlar�
        private readonly Dictionary<int, AnimationManager> _itemHoverAnims = new();

        // Windows 11 WinUI3 benzeri modern bo�luklar
        private const int VERTICAL_PADDING = DropDownVerticalPadding;
        private const int ITEM_MARGIN = 10; // Dropdown kenar�ndan item arkaplan kenar�na kadar margin
        private const float CORNER_RADIUS = 10f;
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

        private SKFont GetCachedFont()
        {
            int dpi = DeviceDpi > 0 ? DeviceDpi : 96;
            var font = _owner.Font;

            if (_cachedFont == null || !ReferenceEquals(_cachedFontSource, font) || _cachedFontDpi != dpi)
            {
                _cachedFont?.Dispose();
                _cachedFont = new SKFont
                {
                    Size = font.Size.PtToPx(this),
                    Typeface = SDUI.Helpers.FontManager.GetSKTypeface(font),
                    Subpixel = true,
                    Edging = SKFontEdging.SubpixelAntialias
                };
                _cachedFontSource = font;
                _cachedFontDpi = dpi;
            }

            return _cachedFont;
        }

        private void EnsureHighlightShader(int height)
        {
            if (_highlightShader != null && _highlightShaderHeight == height)
                return;

            _highlightShader?.Dispose();
            _highlightShader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, height * 0.15f),
                new[] { SKColors.White.WithAlpha(12), SKColors.Transparent },
                null,
                SKShaderTileMode.Clamp);
            _highlightShaderHeight = height;
        }

        private void EnsureShadowResources()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_shadowPaints[i] != null)
                    continue;

                float blurRadius = 6 + i * 4;
                _shadowMaskFilters[i] = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius);
                _shadowPaints[i] = new SKPaint
                {
                    IsAntialias = true,
                    MaskFilter = _shadowMaskFilters[i]
                };
            }
        }

        private int ItemHeight => Math.Max(32, _owner.ItemHeight);

        private void ScrollBar_ValueChanged(object sender, EventArgs e)
        {
            _scrollOffset = _scrollBar.Value;
            Invalidate();
        }

        private AnimationManager EnsureItemAnim(int index)
        {
            if (!_itemHoverAnims.TryGetValue(index, out var ae))
            {
                ae = new AnimationManager(singular: true)
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
            _openAnimation.SetProgress(0); // Her a��l��ta 0'dan ba�la
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

            _layerPaint ??= new SKPaint { IsAntialias = true };
            _layerPaint.Color = SKColors.White.WithAlpha((byte)(255 * openProgress));
            canvas.SaveLayer(_layerPaint);
                
                // Subtle fade-in animasyonu
                float translateY = (_openingUpwards ? 1f - openProgress : openProgress - 1f) * 8f;
                canvas.Translate(0, translateY);

                var mainRect = new SKRect(0, 0, Width, Height);
                var mainRoundRect = new SKRoundRect(mainRect, CORNER_RADIUS);
                
                // Multi-layer modern shadow (ContextMenuStrip gibi)
                canvas.Save();
                EnsureShadowResources();
                for (int i = 0; i < 4; i++)
                {
                    float offsetY = 2 + i * 2;
                    byte shadowAlpha = (byte)((25 - i * 5) * openProgress);

                    var shadowPaint = _shadowPaints[i]!;
                    shadowPaint.Color = SKColors.Black.WithAlpha(shadowAlpha);

                    canvas.Save();
                    canvas.Translate(0, offsetY);
                    canvas.DrawRoundRect(mainRoundRect, shadowPaint);
                    canvas.Restore();
                }
                canvas.Restore();
                
                // High-quality solid background
                var surfaceColor = ColorScheme.BackColor.ToSKColor().InterpolateColor(SKColors.White, 0.06f);

                _bgPaint ??= new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    FilterQuality = SKFilterQuality.High
                };
                _bgPaint.Color = surfaceColor;
                canvas.DrawRoundRect(mainRoundRect, _bgPaint);

                // Clip path

                _clipPath ??= new SKPath();
                _clipPath.Reset();
                _clipPath.AddRoundRect(mainRoundRect);
                canvas.Save();
                canvas.ClipPath(_clipPath, antialias: true);

                // Minimal �st highlight

                EnsureHighlightShader(Height);
                _highlightPaint ??= new SKPaint { IsAntialias = true };
                _highlightPaint.Shader = _highlightShader;
                canvas.DrawRect(mainRect, _highlightPaint);

                // High-quality border

                _borderPaint ??= new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1f,
                    FilterQuality = SKFilterQuality.High
                };
                _borderPaint.Color = ColorScheme.BorderColor.Alpha(100).ToSKColor();
                var borderRect = new SKRoundRect(
                    new SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f),
                    CORNER_RADIUS - 0.5f);
                canvas.DrawRoundRect(borderRect, _borderPaint);

                // High-quality text paint

                var font = GetCachedFont();
                _textPaint ??= new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                    Style = SKPaintStyle.Fill
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
                        _selectionPaint ??= new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High,
                            Style = SKPaintStyle.Fill
                        };
                        _selectionPaint.Color = ColorScheme.AccentColor.Alpha(70).ToSKColor();
                        var selRect = new SKRoundRect(itemRect, itemRadius);
                        canvas.DrawRoundRect(selRect, _selectionPaint);
                    }
                    // High-quality hover effect
                    else if (hProg > 0.001f)
                    {
                        byte hoverAlpha = (byte)(30 + 45 * hProg);
                        _hoverPaint ??= new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High,
                            Style = SKPaintStyle.Fill
                        };
                        _hoverPaint.Color = ColorScheme.AccentColor.Alpha(hoverAlpha).ToSKColor();
                        var hoverRect = new SKRoundRect(itemRect, itemRadius);
                        canvas.DrawRoundRect(hoverRect, _hoverPaint);
                    }

                    // High-quality text rendering
                    string text = _owner.GetItemText(_owner.Items[i]);
                    _textPaint.Color = ColorScheme.ForeColor.ToSKColor();

                    float baseTextY = currentY + ItemHeight / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;
                    float textX = ITEM_MARGIN + 8;
                    
                    TextRenderingHelper.DrawText(canvas, text, textX, baseTextY, font, _textPaint);

                    currentY += ItemHeight;
                }

                canvas.Restore(); // clipPath restore
                canvas.Restore(); // layerPaint restore
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cachedFont?.Dispose();
                _cachedFont = null;
                _cachedFontSource = null;

                _layerPaint?.Dispose();
                _bgPaint?.Dispose();
                _highlightPaint?.Dispose();
                _highlightShader?.Dispose();
                _borderPaint?.Dispose();
                _textPaint?.Dispose();
                _selectionPaint?.Dispose();
                _hoverPaint?.Dispose();
                _clipPath?.Dispose();

                for (int i = 0; i < _shadowPaints.Length; i++)
                {
                    _shadowPaints[i]?.Dispose();
                    _shadowPaints[i] = null;
                    _shadowMaskFilters[i]?.Dispose();
                    _shadowMaskFilters[i] = null;
                }
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// ComboBox ��eleri koleksiyonu
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
                // DropDown stilinde kullan�c� listeden bir se�im yap�nca metni g�ncelle
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

    private int _dropDownHeight = 0;
    [Browsable(true)]
    public int DropDownHeight
    {
        get => _dropDownHeight;
        set => _dropDownHeight = value;
    }

    private bool _autoDropDownHeight = true;
    [Browsable(true)]
    public bool AutoDropDownHeight
    {
        get => _autoDropDownHeight;
        set => _autoDropDownHeight = value;
    }

    private int _minDropDownItems = DefaultMinVisibleItems;
    [Browsable(true)]
    public int MinDropDownItems
    {
        get => _minDropDownItems;
        set => _minDropDownItems = Math.Max(1, value);
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
            // DropDownList'te text d�zenlenemez
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
    private readonly AnimationManager _hoverAnimation;
    private readonly AnimationManager _pressAnimation;
    private readonly AnimationManager _arrowAnimation;
    private UIWindow _parentWindow;
    private MouseEventHandler _windowMouseDownHandler;
    private EventHandler _windowDeactivateHandler;
    private KeyEventHandler _windowKeyDownHandler;
    private bool _handlersAttached;
    private bool _ignoreNextClick;

    private SKPaint? _paintShadow;
    private SKMaskFilter? _shadowMaskFilter;
    private float _shadowMaskBlur;
    private SKPaint? _paintBase;
    private SKPaint? _paintGlow;
    private SKPaint? _paintHighlight;
    private SKShader? _highlightShader;
    private int _highlightShaderHeight;
    private SKPaint? _paintBorder;
    private SKPaint? _paintText;
    private SKPaint? _paintChevronBg;
    private SKPaint? _paintChevronStroke;
    private SKPath? _chevronPath;
    private SKFont? _defaultSkFont;
    private Font? _defaultSkFontSource;
    private int _defaultSkFontDpi;

    #endregion

    #region Constructor

    public ComboBox()
    {
        MinimumSize = new Size(50, 28);
        Size = new Size(120, 28);

        _hoverAnimation = new AnimationManager(singular: true)
        {
            Increment = 0.15,
            AnimationType = AnimationType.EaseInOut
        };
        _hoverAnimation.OnAnimationProgress += _ => Invalidate();

        _pressAnimation = new AnimationManager(singular: true)
        {
            Increment = 0.2,
            AnimationType = AnimationType.EaseInOut
        };
        _pressAnimation.OnAnimationProgress += _ => Invalidate();

        _arrowAnimation = new AnimationManager(singular: true)
        {
            Increment = 0.12,
            AnimationType = AnimationType.EaseInOut
        };
        _arrowAnimation.OnAnimationProgress += _ => Invalidate();
        // Ba�lang��ta ok a�a�� bakmal� (0 rotation)
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

            // DropDownStyle'a g�re davran��
            if (DropDownStyle == ComboBoxStyle.Simple)
            {
                // Simple modda dropdown a��lmaz
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
        using var font = new SKFont
        {
            Size = Font.Size.PtToPx(this),
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
        };
        float maxText = 0f;
        for (int i = 0; i < Items.Count; i++)
        {
            var txt = GetItemText(Items[i]) ?? string.Empty;
            var bounds = new SKRect();
            font.MeasureText(txt, out bounds);
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
        int padding = DropDownVerticalPadding * 2;
        int itemHeight = EffectiveDropDownItemHeight;
        int totalItemsHeight = Items.Count * itemHeight + padding;
        int minVisibleItems = Math.Min(Math.Max(_minDropDownItems, 1), Math.Max(Items.Count, 1));
        int minHeight = Math.Max(itemHeight + padding, minVisibleItems * itemHeight + padding);

        int spaceBelow = _parentWindow.Height - comboLocation.Y - Height - MARGIN;
        int spaceAbove = comboLocation.Y - MARGIN;
        int availableSpace = Math.Max(spaceBelow, spaceAbove);

        int dropdownHeight;
        if (_autoDropDownHeight)
        {
            dropdownHeight = Math.Min(totalItemsHeight, availableSpace);
            dropdownHeight = Math.Max(minHeight, dropdownHeight);
        }
        else
        {
            int bestVisibleItems = Math.Min(MaxDropDownItems, Items.Count);
            int preferredHeight = Math.Min(totalItemsHeight, bestVisibleItems * itemHeight + padding);
            int target = _dropDownHeight > 0 ? _dropDownHeight : preferredHeight;
            dropdownHeight = Math.Max(minHeight, Math.Min(target, totalItemsHeight));
        }

        dropdownHeight = Math.Min(dropdownHeight, client.Height - MARGIN * 2);
        int dropdownWidth = _dropDownWidth == 0 ? MeasureDropDownAutoWidth() : DropDownWidth;

        // Smart positioning with flip support (ContextMenuStrip gibi)
        bool canOpenDown = dropdownHeight <= spaceBelow;
        bool canOpenUp = dropdownHeight <= spaceAbove;

        Point dropdownLocation;
        if (canOpenDown)
        {
            dropdownHeight = Math.Min(dropdownHeight, Math.Max(minHeight, spaceBelow));
            dropdownLocation = new Point(comboLocation.X, comboLocation.Y + Height + 2);
        }
        else if (canOpenUp)
        {
            dropdownHeight = Math.Min(dropdownHeight, Math.Max(minHeight, spaceAbove));
            dropdownLocation = new Point(comboLocation.X, comboLocation.Y - dropdownHeight - 2);
        }
        else
        {
            // Neither fits perfectly - pick the side with more space
            if (spaceBelow >= spaceAbove)
            {
                dropdownLocation = new Point(comboLocation.X, comboLocation.Y + Height + 2);
                dropdownHeight = Math.Max(minHeight, Math.Min(dropdownHeight, spaceBelow));
            }
            else
            {
                dropdownHeight = Math.Max(minHeight, Math.Min(dropdownHeight, spaceAbove));
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

        // ComboBox'�n kendi alan� i�inde mi kontrol et
        var comboBounds = new Rectangle(GetLocationRelativeToWindow(), Size);
        System.Diagnostics.Debug.WriteLine($"OnWindowMouseDown: ComboBox bounds=({comboBounds.X},{comboBounds.Y},{comboBounds.Width},{comboBounds.Height})");
        if (comboBounds.Contains(e.Location))
        {
            System.Diagnostics.Debug.WriteLine("OnWindowMouseDown: Click inside ComboBox, not closing");
            return;
        }

        // Dropdown panel i�inde mi kontrol et
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

    protected override void InvalidateFontCache()
    {
        base.InvalidateFontCache();
        _defaultSkFont?.Dispose();
        _defaultSkFont = null;
        _defaultSkFontSource = null;
        _defaultSkFontDpi = 0;
    }

    #region Rendering

    private SKFont GetDefaultSkFont()
    {
        int dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        if (_defaultSkFont == null || !ReferenceEquals(_defaultSkFontSource, Font) || _defaultSkFontDpi != dpi)
        {
            _defaultSkFont?.Dispose();
            _defaultSkFont = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
                Subpixel = true
            };
            _defaultSkFontSource = Font;
            _defaultSkFontDpi = dpi;
        }
        return _defaultSkFont;
    }

    private void EnsureHighlightShader(int height)
    {
        if (_highlightShader != null && _highlightShaderHeight == height)
            return;

        _highlightShader?.Dispose();
        _highlightShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, height * 0.4f),
            new[] { SKColors.White.WithAlpha(10), SKColors.Transparent },
            null,
            SKShaderTileMode.Clamp);
        _highlightShaderHeight = height;
    }

    private void EnsureShadowMaskFilter(float blur)
    {
        if (_shadowMaskFilter != null && Math.Abs(_shadowMaskBlur - blur) < 0.001f)
            return;

        _shadowMaskFilter?.Dispose();
        _shadowMaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur);
        _shadowMaskBlur = blur;
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var rect = new SKRect(0, 0, Width, Height);

        float hoverProgress = (float)_hoverAnimation.GetProgress();
        float pressProgress = (float)_pressAnimation.GetProgress();

        var accentColor = ColorScheme.AccentColor.ToSKColor();
        var baseColor = ColorScheme.BackColor.ToSKColor();

        // Arka plan rengi hesapla (daha yumu�ak blend)
        float blendFactor = Math.Clamp(hoverProgress * 0.15f + pressProgress * 0.1f + (DroppedDown ? 0.2f : 0f), 0f, 0.4f);
        var backgroundColor = baseColor.InterpolateColor(accentColor, blendFactor);

        // Modern g�lge efekti (hover ile dinamik - daha subtle)
        if (ShadowDepth > 0)
        {
            float shadowAlpha = 12 + (hoverProgress * 8) + (pressProgress * 5);
            _paintShadow ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            EnsureShadowMaskFilter(ShadowDepth * 0.5f);
            _paintShadow.MaskFilter = _shadowMaskFilter;
            _paintShadow.Color = SKColors.Black.WithAlpha((byte)shadowAlpha);
            canvas.Save();
            canvas.Translate(0, ShadowDepth * 0.3f);
            canvas.DrawRoundRect(rect, _radius, _radius, _paintShadow);
            canvas.Restore();
        }

        // Base arka plan
        _paintBase ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _paintBase.Color = backgroundColor;
        canvas.DrawRoundRect(rect, _radius, _radius, _paintBase);

        // Hover/press glow overlay
        if (hoverProgress > 0 || pressProgress > 0)
        {
            byte glowAlpha = (byte)((hoverProgress * 15 + pressProgress * 10));
            _paintGlow ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _paintGlow.Color = accentColor.WithAlpha(glowAlpha);
            canvas.DrawRoundRect(rect, _radius, _radius, _paintGlow);
        }

        // �stte ince ayd�nl�k highlight (acrylic effect)
        EnsureHighlightShader(Height);
        _paintHighlight ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _paintHighlight.Shader = _highlightShader;
        canvas.DrawRoundRect(rect, _radius, _radius, _paintHighlight);

        // Modern kenarl�k (ince ve zarif)
        float borderAlpha = 0.4f + (hoverProgress * 0.2f) + (pressProgress * 0.1f);
        _paintBorder ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
        _paintBorder.Color = ColorScheme.BorderColor.ToSKColor()
            .InterpolateColor(accentColor, blendFactor * 0.4f)
            .ToColor()
            .Alpha((byte)(255 * borderAlpha))
            .ToSKColor();
        var borderRect = rect;
        borderRect.Inflate(-0.5f, -0.5f);
        canvas.DrawRoundRect(borderRect, _radius - 0.5f, _radius - 0.5f, _paintBorder);
        
        var textColor = ColorScheme.ForeColor.ToSKColor();
        var displayText = Text;

        var font = GetDefaultSkFont();
        _paintText ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _paintText.Color = textColor;

        var textBounds = new SKRect();
        font.MeasureText(displayText, out textBounds);
        float textY = Height / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;
        float textX = 16;

        TextRenderingHelper.DrawText(canvas, displayText, textX, textY, font, _paintText);

        // Modern Chevron ikonu - DropDown ve DropDownList stillerinde g�ster
        if (DropDownStyle != ComboBoxStyle.Simple)
        {
            float chevronSize = 10;
            float chevronX = Width - 22;
            float chevronY = Height / 2f;

            // Arrow rotation animasyonu: 0� kapal�, 180� a��k
            float arrowProgress = (float)_arrowAnimation.GetProgress();
            float rotation = 180f * arrowProgress;

            // Chevron rengi (hover ve press durumuna g�re)
            float chevronBlend = Math.Clamp(hoverProgress * 0.35f + pressProgress * 0.5f, 0f, 1f);
            var chevronColor = ColorScheme.ForeColor.ToSKColor().InterpolateColor(accentColor, chevronBlend);

            // Chevron hover background circle (animasyonlu)
            if (hoverProgress > 0.05f)
            {
                float circleRadius = 13f * (float)Math.Clamp(hoverProgress, 0, 1);
                _paintChevronBg ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
                _paintChevronBg.Color = accentColor.WithAlpha((byte)(hoverProgress * 18));
                canvas.DrawCircle(chevronX, chevronY, circleRadius, _paintChevronBg);
            }

            // Rotasyon i�in canvas'� kaydet
            canvas.Save();
            canvas.Translate(chevronX, chevronY);
            canvas.RotateDegrees(rotation);

            // Modern chevron (V �ekli) - ince ve zarif
            _paintChevronStroke ??= new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };
            _paintChevronStroke.Color = chevronColor;

            _chevronPath ??= new SKPath();
            _chevronPath.Reset();
            float halfSize = chevronSize * 0.45f;
            _chevronPath.MoveTo(-halfSize, -halfSize * 0.5f);
            _chevronPath.LineTo(0, halfSize * 0.5f);
            _chevronPath.LineTo(halfSize, -halfSize * 0.5f);
            canvas.DrawPath(_chevronPath, _paintChevronStroke);

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
        Height = 28; // Sabit y�kseklik
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DetachWindowHandlers();
            _dropDownPanel?.Dispose();

            _defaultSkFont?.Dispose();
            _defaultSkFont = null;
            _defaultSkFontSource = null;

            _paintShadow?.Dispose();
            _paintShadow = null;
            _shadowMaskFilter?.Dispose();
            _shadowMaskFilter = null;

            _paintBase?.Dispose();
            _paintBase = null;
            _paintGlow?.Dispose();
            _paintGlow = null;
            _paintHighlight?.Dispose();
            _paintHighlight = null;
            _highlightShader?.Dispose();
            _highlightShader = null;
            _paintBorder?.Dispose();
            _paintBorder = null;
            _paintText?.Dispose();
            _paintText = null;
            _paintChevronBg?.Dispose();
            _paintChevronBg = null;
            _paintChevronStroke?.Dispose();
            _paintChevronStroke = null;
            _chevronPath?.Dispose();
            _chevronPath = null;
        }
        base.Dispose(disposing);
    }

    #endregion
}
