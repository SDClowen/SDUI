using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class ComboBox : UIElementBase
{
    internal int DropDownVerticalPadding => (int)(6 * ScaleFactor);
    private const int DefaultMinVisibleItems = 4;
    internal int MinDropDownItemHeight => (int)(32 * ScaleFactor);

    #region Constructor

    public ComboBox()
    {
        MinimumSize = new Size((int)(50 * ScaleFactor), (int)(28 * ScaleFactor));
        Size = new Size((int)(120 * ScaleFactor), (int)(28 * ScaleFactor));

        _hoverAnimation = new AnimationManager(true)
        {
            Increment = 0.15,
            AnimationType = AnimationType.EaseInOut
        };
        _hoverAnimation.OnAnimationProgress += _ => Invalidate();

        _pressAnimation = new AnimationManager(true)
        {
            Increment = 0.2,
            AnimationType = AnimationType.EaseInOut
        };
        _pressAnimation.OnAnimationProgress += _ => Invalidate();

        _arrowAnimation = new AnimationManager(true)
        {
            Increment = 0.12,
            AnimationType = AnimationType.EaseInOut
        };
        _arrowAnimation.OnAnimationProgress += _ => Invalidate();
        // Ba�lang��ta ok a�a�� bakmal� (0 rotation)
        _arrowAnimation.SetProgress(0);

        _dropDownPanel = CreateDropDownPanel();
    }

    #endregion

    // Dropdown rendering enforces a minimum item height for a modern look.
    // Keep dropdown sizing consistent with what the dropdown actually draws.
    protected virtual int EffectiveDropDownItemHeight => Math.Max(MinDropDownItemHeight, ItemHeight);

    protected override void InvalidateFontCache()
    {
        base.InvalidateFontCache();
        _defaultSkFont?.Dispose();
        _defaultSkFont = null;
        _defaultSkFontSource = null;
        _defaultSkFontDpi = 0;
    }

    protected virtual DropDownPanel CreateDropDownPanel()
    {
        return new DropDownPanel(this);
    }

    #region Inner Classes

    

    /// <summary>
    ///     ComboBox ��eleri koleksiyonu
    /// </summary>
    public class ObjectCollection : IList
    {
        private readonly List<object> _items = new();
        private readonly ComboBox _owner;

        public ObjectCollection(ComboBox owner)
        {
            _owner = owner;
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

        public bool Contains(object value)
        {
            return _items.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            _items.CopyTo((object[])array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return _items.IndexOf(value);
        }

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
    private int RadiusScaled => (int)(_radius * ScaleFactor);
    private float ShadowDepthScaled => ShadowDepth * ScaleFactor;

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
                    _text = GetItemText(_selectedItem);
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

    [Browsable(true)] public string ValueMember { get; set; }

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

    [Browsable(true)] public int MaxDropDownItems { get; set; } = 8;

    [Browsable(true)] public DrawMode DrawMode { get; set; } = DrawMode.Normal;

    [Browsable(true)] public bool FormattingEnabled { get; set; }

    [Browsable(true)] public bool IntegralHeight { get; set; } = true;

    [Browsable(true)] public int ItemHeight { get; set; } = 32;

    [Browsable(true)] public int DropDownHeight { get; set; }

    [Browsable(true)] public bool AutoDropDownHeight { get; set; } = true;

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

    [Browsable(true)] public float ShadowDepth { get; set; } = 4;

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

    private readonly DropDownPanel _dropDownPanel;
    private readonly AnimationManager _hoverAnimation;
    private readonly AnimationManager _pressAnimation;
    internal protected readonly AnimationManager _arrowAnimation;
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

    #region Event Handlers

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

    internal virtual void OnDropDownClosed(EventArgs e)
    {
        DropDownClosed?.Invoke(this, e);
    }

    internal virtual void OnSelectionChangeCommitted(EventArgs e)
    {
        SelectionChangeCommitted?.Invoke(this, e);
    }

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
                // Simple modda dropdown a��lmaz
                return;

            if (DroppedDown)
            {
                CloseDropDown();
            }
            else if (Items.Count > 0)
            {
                // Debug: Console'a yaz
                Debug.WriteLine($"ComboBox OnMouseDown: Items.Count={Items.Count}, Opening dropdown");
                _arrowAnimation.StartNewAnimation(AnimationDirection.In);
                OpenDropDown();
            }
            else
            {
                Debug.WriteLine($"ComboBox OnMouseDown: No items to show (Items.Count={Items.Count})");
            }
        }
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Left) _pressAnimation.StartNewAnimation(AnimationDirection.Out);
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
            Typeface = FontManager.GetSKTypeface(Font)
        };
        var maxText = 0f;
        for (var i = 0; i < Items.Count; i++)
        {
            var txt = GetItemText(Items[i]) ?? string.Empty;
            var bounds = new SKRect();
            font.MeasureText(txt, out bounds);
            maxText = Math.Max(maxText, bounds.Width);
        }

        // padding + potential scrollbar + borders
        var hPadding = 10 + 10 + 8; // left + right + extra inset
        var predictScrollbar = Items.Count > MaxDropDownItems;
        var scrollbar = predictScrollbar ? 14 + 6 : 0;
        var autoWidth = (int)Math.Ceiling(maxText) + hPadding + scrollbar;
        return Math.Max(autoWidth, Width);
    }

    private void OpenDropDown()
    {
        Debug.WriteLine($"OpenDropDown called: Items.Count={Items.Count}, DroppedDown={DroppedDown}");

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

        var comboLocation = GetLocationRelativeToWindow();
        var client = _parentWindow.ClientRectangle;

        const int MARGIN = 8;
        var padding = DropDownVerticalPadding * 2;
        var itemHeight = EffectiveDropDownItemHeight;
        var totalItemsHeight = Items.Count * itemHeight + padding;
        var minVisibleItems = Math.Min(Math.Max(_minDropDownItems, 1), Math.Max(Items.Count, 1));
        var minHeight = Math.Max(itemHeight + padding, minVisibleItems * itemHeight + padding);

        var spaceBelow = _parentWindow.Height - comboLocation.Y - Height - MARGIN;
        var spaceAbove = comboLocation.Y - MARGIN;
        var availableSpace = Math.Max(spaceBelow, spaceAbove);

        int dropdownHeight;
        if (AutoDropDownHeight)
        {
            dropdownHeight = Math.Min(totalItemsHeight, availableSpace);
            dropdownHeight = Math.Max(minHeight, dropdownHeight);
        }
        else
        {
            var bestVisibleItems = Math.Min(MaxDropDownItems, Items.Count);
            var preferredHeight = Math.Min(totalItemsHeight, bestVisibleItems * itemHeight + padding);
            var target = DropDownHeight > 0 ? DropDownHeight : preferredHeight;
            dropdownHeight = Math.Max(minHeight, Math.Min(target, totalItemsHeight));
        }

        dropdownHeight = Math.Min(dropdownHeight, client.Height - MARGIN * 2);
        var dropdownWidth = _dropDownWidth == 0 ? MeasureDropDownAutoWidth() : DropDownWidth;

        // Smart positioning with flip support (ContextMenuStrip gibi)
        var canOpenDown = dropdownHeight <= spaceBelow;
        var canOpenUp = dropdownHeight <= spaceAbove;

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
            var leftPos = dropdownLocation.X - dropdownWidth + Width;
            if (leftPos >= client.Left + MARGIN)
                dropdownLocation.X = leftPos;
            else
                dropdownLocation.X = client.Right - dropdownWidth - MARGIN;
        }

        // Final safety clamp
        dropdownLocation.X = Math.Max(client.Left + MARGIN,
            Math.Min(dropdownLocation.X, client.Right - dropdownWidth - MARGIN));
        dropdownLocation.Y = Math.Max(client.Top + MARGIN,
            Math.Min(dropdownLocation.Y, client.Bottom - dropdownHeight - MARGIN));

        // Clamp size to available space
        dropdownWidth = Math.Min(dropdownWidth, client.Width - MARGIN * 2);
        dropdownHeight = Math.Min(dropdownHeight, client.Height - MARGIN * 2);

        _dropDownPanel.Location = dropdownLocation;
        _dropDownPanel.Width = dropdownWidth;
        _dropDownPanel.Height = dropdownHeight;

        _dropDownPanel.BeginOpen(!canOpenDown && canOpenUp);
        _dropDownPanel.ZOrder = 9999;

        DroppedDown = true;
        _ignoreNextClick = true;

        AttachWindowHandlers();
        _parentWindow.Invalidate();

        Task.Delay(100).ContinueWith(_ =>
        {
            if (_parentWindow?.InvokeRequired == true)
                _parentWindow.Invoke(() => _ignoreNextClick = false);
            else
                _ignoreNextClick = false;
        });
    }

    private void CloseDropDown()
    {
        Debug.WriteLine($"CloseDropDown called: DroppedDown={DroppedDown}");
        if (!DroppedDown)
        {
            Debug.WriteLine("CloseDropDown: Already closed");
            return;
        }

        _arrowAnimation.StartNewAnimation(AnimationDirection.Out);
        _dropDownPanel.BeginClose();
        Debug.WriteLine("CloseDropDown: Dropdown closed");
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

    internal void DetachWindowHandlers()
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
        Debug.WriteLine(
            $"OnWindowMouseDown: Click at ({e.Location.X},{e.Location.Y}), _ignoreNextClick={_ignoreNextClick}");

        if (_ignoreNextClick)
        {
            _ignoreNextClick = false;
            Debug.WriteLine("OnWindowMouseDown: Ignoring click (first click after open)");
            return;
        }

        // ComboBox'�n kendi alan� i�inde mi kontrol et
        var comboBounds = new Rectangle(GetLocationRelativeToWindow(), Size);
        Debug.WriteLine(
            $"OnWindowMouseDown: ComboBox bounds=({comboBounds.X},{comboBounds.Y},{comboBounds.Width},{comboBounds.Height})");
        if (comboBounds.Contains(e.Location))
        {
            Debug.WriteLine("OnWindowMouseDown: Click inside ComboBox, not closing");
            return;
        }

        // Dropdown panel i�inde mi kontrol et
        var panelBounds = new Rectangle(_dropDownPanel.Location, _dropDownPanel.Size);
        Debug.WriteLine(
            $"OnWindowMouseDown: Panel bounds=({panelBounds.X},{panelBounds.Y},{panelBounds.Width},{panelBounds.Height})");
        if (panelBounds.Contains(e.Location))
        {
            Debug.WriteLine("OnWindowMouseDown: Click inside panel, not closing");
            return;
        }

        Debug.WriteLine("OnWindowMouseDown: Click outside, closing dropdown");
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

    internal protected string GetItemText(object item)
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

    private SKFont GetDefaultSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        if (_defaultSkFont == null || !ReferenceEquals(_defaultSkFontSource, Font) || _defaultSkFontDpi != dpi)
        {
            _defaultSkFont?.Dispose();
            _defaultSkFont = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(Font),
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

    public override void OnPaint(SKCanvas canvas)
    {
        var rect = new SKRect(0, 0, Width, Height);

        var hoverProgress = (float)_hoverAnimation.GetProgress();
        var pressProgress = (float)_pressAnimation.GetProgress();

        var accentColor = ColorScheme.AccentColor.ToSKColor();
        var baseColor = ColorScheme.BackColor.ToSKColor();

        // Arka plan rengi hesapla (daha yumuşak blend)
        var blendFactor = Math.Clamp(hoverProgress * 0.15f + pressProgress * 0.1f + (DroppedDown ? 0.2f : 0f), 0f,
            0.4f);
        var backgroundColor = baseColor.InterpolateColor(accentColor, blendFactor);

        // Modern gölge efekti (hover ile dinamik - daha subtle)
        if (ShadowDepth > 0)
        {
            var shadowAlpha = 12 + hoverProgress * 8 + pressProgress * 5;
            _paintShadow ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            EnsureShadowMaskFilter(ShadowDepthScaled * 0.5f);
            _paintShadow.MaskFilter = _shadowMaskFilter;
            _paintShadow.Color = SKColors.Black.WithAlpha((byte)shadowAlpha);
            canvas.Save();
            canvas.Translate(0, ShadowDepthScaled * 0.3f);
            canvas.DrawRoundRect(rect, RadiusScaled, RadiusScaled, _paintShadow);
            canvas.Restore();
        }

        // Base arka plan
        _paintBase ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _paintBase.Color = backgroundColor;
        canvas.DrawRoundRect(rect, RadiusScaled, RadiusScaled, _paintBase);

        // Hover/press glow overlay
        if (hoverProgress > 0 || pressProgress > 0)
        {
            var glowAlpha = (byte)(hoverProgress * 15 + pressProgress * 10);
            _paintGlow ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _paintGlow.Color = accentColor.WithAlpha(glowAlpha);
            canvas.DrawRoundRect(rect, RadiusScaled, RadiusScaled, _paintGlow);
        }

        // �stte ince ayd�nl�k highlight (acrylic effect)
        EnsureHighlightShader(Height);
        _paintHighlight ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _paintHighlight.Shader = _highlightShader;
        canvas.DrawRoundRect(rect, RadiusScaled, RadiusScaled, _paintHighlight);

        // Modern kenarl�k (ince ve zarif)
        var borderAlpha = 0.4f + hoverProgress * 0.2f + pressProgress * 0.1f;
        _paintBorder ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f * ScaleFactor };
        _paintBorder.Color = ColorScheme.BorderColor.ToSKColor()
            .InterpolateColor(accentColor, blendFactor * 0.4f)
            .ToColor()
            .Alpha((byte)(255 * borderAlpha))
            .ToSKColor();
        var borderRect = rect;
        borderRect.Inflate(-0.5f * ScaleFactor, -0.5f * ScaleFactor);
        canvas.DrawRoundRect(borderRect, RadiusScaled - 0.5f * ScaleFactor, RadiusScaled - 0.5f * ScaleFactor, _paintBorder);

        var textColor = ColorScheme.ForeColor.ToSKColor();
        var displayText = Text;

        var font = GetDefaultSkFont();
        _paintText ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _paintText.Color = textColor;

        var textBounds = new SKRect();
        font.MeasureText(displayText, out textBounds);
        var textY = Height / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;
        float textX = 16f * ScaleFactor;

        TextRenderingHelper.DrawText(canvas, displayText, textX, textY, font, _paintText);

        // Modern Chevron ikonu - DropDown ve DropDownList stillerinde g�ster
        if (DropDownStyle != ComboBoxStyle.Simple)
        {
            float chevronSize = 10f * ScaleFactor;
            float chevronX = Width - 22f * ScaleFactor;
            var chevronY = Height / 2f;

            // Arrow rotation animation: 0 closed, 180 opened
            var arrowProgress = (float)_arrowAnimation.GetProgress();
            var rotation = 180f * arrowProgress;

            // Chevron color (status of hover and pressre)
            var chevronBlend = Math.Clamp(hoverProgress * 0.35f + pressProgress * 0.5f, 0f, 1f);
            var chevronColor = ColorScheme.ForeColor.ToSKColor().InterpolateColor(accentColor, chevronBlend);

            // Chevron hover background circle (with animation)
            if (hoverProgress > 0.05f)
            {
                var circleRadius = 13f * ScaleFactor * Math.Clamp(hoverProgress, 0, 1);
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
                StrokeWidth = 1.5f * ScaleFactor,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };
            _paintChevronStroke.Color = chevronColor;

            _chevronPath ??= new SKPath();
            _chevronPath.Reset();
            var halfSize = chevronSize * 0.45f;
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
        return new Size(Math.Max((int)(120 * ScaleFactor), proposedSize.Width), (int)(28 * ScaleFactor));
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        Height = (int)(28 * ScaleFactor); // Sabit y�kseklik
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