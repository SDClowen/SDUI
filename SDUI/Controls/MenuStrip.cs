using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class MenuStrip : UIElementBase
{
    private readonly List<MenuItem> _items = new();
    private MenuItem _hoveredItem;
    private MenuItem _openedItem;
    private ContextMenuStrip _activeDropDown;
    private MenuItem _activeDropDownOwner;
    private Color _menuBackColor = Color.FromArgb(45, 45, 45);
    private Color _menuForeColor = Color.White;
    private Color _hoverBackColor = Color.FromArgb(62, 62, 62);
    private Color _hoverForeColor = Color.FromArgb(0, 122, 204);
    private Color _submenuBackColor = Color.FromArgb(37, 37, 37);
    private Color _submenuBorderColor = Color.FromArgb(80, 80, 80);
    private Color _separatorColor = Color.FromArgb(80, 80, 80);
    private float _itemHeight = 28f;
    private float _itemPadding = 12f;
    private float _submenuArrowSize = 8f;
    private float _iconSize = 16f;
    private float _separatorHeight = 1f;
    private bool _showIcons = true;
    private bool _showHoverEffect = true;
    private bool _showSubmenuArrow = true;
    private bool _roundedCorners = true;
    private float _cornerRadius = 6f;
    private float _submenuCornerRadius = 8f;
    private float _submenuShadowBlur = 10f;
    private float _submenuOffset = 2f;
    private int _submenuAnimationDuration = 150;
    private bool _isAnimating;
    private float _animationProgress;
    private Timer _animationTimer;
    private bool _stretch = true;
    private Size _imageScalingSize = new Size(20, 20);
    private Color _separatorBackColor = Color.FromArgb(50, 50, 50);
    private Color _separatorForeColor = Color.FromArgb(100, 100, 100);
    private float _separatorMargin = 4f;
    private SDUI.Orientation _orientation = SDUI.Orientation.Horizontal;

    public MenuStrip()
    {
        Height = (int)_itemHeight;
        BackColor = ColorScheme.BackColor; // tema uyumu
        ForeColor = ColorScheme.ForeColor;
        InitializeAnimationTimer();
    }

    [Browsable(false)]
    public List<MenuItem> Items => _items;

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool Stretch
    {
        get => _stretch;
        set { if (_stretch == value) return; _stretch = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DefaultValue(SDUI.Orientation.Horizontal)]
    public SDUI.Orientation Orientation
    {
        get => _orientation;
        set { if (_orientation == value) return; _orientation = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Size), "20, 20")]
    public Size ImageScalingSize
    {
        get => _imageScalingSize;
        set { if (_imageScalingSize == value) return; _imageScalingSize = value; _iconSize = Math.Min(value.Width, value.Height); Invalidate(); }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool ShowSubmenuArrow
    {
        get => _showSubmenuArrow;
        set { if (_showSubmenuArrow == value) return; _showSubmenuArrow = value; Invalidate(); }
    }

    private void InitializeAnimationTimer()
    {
        _animationTimer = new Timer { Interval = 16 };
        _animationTimer.Tick += (s, e) =>
        {
            if (!_isAnimating) return;
            _animationProgress = Math.Min(1f, _animationProgress + (16f / _submenuAnimationDuration));
            if (_animationProgress >= 1f)
            {
                _isAnimating = false;
                _animationTimer.Stop();
            }
            Invalidate();
        };
    }

    [Category("Appearance")] public Color MenuBackColor { get => _menuBackColor; set { if (_menuBackColor == value) return; _menuBackColor = value; Invalidate(); } }
    [Category("Appearance")] public Color MenuForeColor { get => _menuForeColor; set { if (_menuForeColor == value) return; _menuForeColor = value; Invalidate(); } }
    [Category("Appearance")] public Color HoverBackColor { get => _hoverBackColor; set { if (_hoverBackColor == value) return; _hoverBackColor = value; Invalidate(); } }
    [Category("Appearance")] public Color HoverForeColor { get => _hoverForeColor; set { if (_hoverForeColor == value) return; _hoverForeColor = value; Invalidate(); } }
    [Category("Appearance")] public Color SubmenuBackColor { get => _submenuBackColor; set { if (_submenuBackColor == value) return; _submenuBackColor = value; Invalidate(); } }
    [Category("Appearance")] public Color SubmenuBorderColor { get => _submenuBorderColor; set { if (_submenuBorderColor == value) return; _submenuBorderColor = value; Invalidate(); } }
    [Category("Appearance")] public Color SeparatorColor { get => _separatorColor; set { if (_separatorColor == value) return; _separatorColor = value; Invalidate(); } }
    [Category("Layout")] public float ItemHeight { get => _itemHeight; set { if (_itemHeight == value) return; _itemHeight = value; Height = (int)value; Invalidate(); } }
    [Category("Layout")] public float ItemPadding { get => _itemPadding; set { if (_itemPadding == value) return; _itemPadding = value; Invalidate(); } }
    [Category("Appearance")] public bool ShowIcons { get => _showIcons; set { if (_showIcons == value) return; _showIcons = value; Invalidate(); } }
    [Category("Behavior")] public bool ShowHoverEffect { get => _showHoverEffect; set { if (_showHoverEffect == value) return; _showHoverEffect = value; Invalidate(); } }
    [Category("Appearance")] public bool RoundedCorners { get => _roundedCorners; set { if (_roundedCorners == value) return; _roundedCorners = value; Invalidate(); } }
    [Category("Appearance")] public Color SeparatorBackColor { get => _separatorBackColor; set { if (_separatorBackColor == value) return; _separatorBackColor = value; Invalidate(); } }
    [Category("Appearance")] public Color SeparatorForeColor { get => _separatorForeColor; set { if (_separatorForeColor == value) return; _separatorForeColor = value; Invalidate(); } }
    [Category("Layout")] public float SeparatorMargin { get => _separatorMargin; set { if (_separatorMargin == value) return; _separatorMargin = value; Invalidate(); } }

    public void AddItem(MenuItem item) { if (item == null) throw new ArgumentNullException(nameof(item)); _items.Add(item); item.Parent = this; Invalidate(); }
    public void RemoveItem(MenuItem item) { if (item == null) throw new ArgumentNullException(nameof(item)); if (_items.Remove(item)) { item.Parent = null; Invalidate(); } }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        var bounds = ClientRectangle;

        // Background
        using (var paint = new SKPaint { Color = MenuBackColor.ToSKColor(), IsAntialias = true })
        {
            canvas.DrawRect(new SKRect(0, 0, bounds.Width, bounds.Height), paint);
        }

        if (Orientation == SDUI.Orientation.Horizontal)
        {
            float x = ItemPadding;
            float availableWidth = bounds.Width - (ItemPadding * 2);
            float totalItemWidth = 0;
            float[] itemWidths = new float[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                itemWidths[i] = MeasureItemWidth(_items[i]);
                totalItemWidth += itemWidths[i];
                if (i < _items.Count - 1) totalItemWidth += ItemPadding;
            }
            float extraPadding = 0;
            if (Stretch && _items.Count > 1 && totalItemWidth < availableWidth)
                extraPadding = (availableWidth - totalItemWidth) / (_items.Count - 1);

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var itemWidth = itemWidths[i];
                var itemBounds = new SKRect(x, 0, x + itemWidth, ItemHeight);
                DrawMenuItem(canvas, item, itemBounds);
                x += itemWidth + ItemPadding + (i < _items.Count - 1 ? extraPadding : 0);
            }
        }
        else
        {
            float y = ItemPadding;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var itemWidth = bounds.Width - (ItemPadding * 2);
                var itemBounds = new SKRect(ItemPadding, y, ItemPadding + itemWidth, y + ItemHeight);
                DrawMenuItem(canvas, item, itemBounds);
                y += ItemHeight + ItemPadding;
            }
        }

        if (_activeDropDown == null || !_activeDropDown.IsOpen)
            _activeDropDownOwner = null;
    }

    private void DrawMenuItem(SKCanvas canvas, MenuItem item, SKRect bounds)
    {
        var isHovered = item == _hoveredItem;
        var isOpened = item == _openedItem;

        if ((isHovered || isOpened) && ShowHoverEffect)
        {
            using var bgPaint = new SKPaint { Color = HoverBackColor.ToSKColor(), IsAntialias = true };
            canvas.DrawRect(bounds, bgPaint);
        }

        float textX = bounds.Left + 8;
        if (ShowIcons && item.Icon != null)
        {
            var iconY = bounds.Top + (ItemHeight - _iconSize) / 2;
            using (var image = SKImage.FromBitmap(item.Icon.ToSKBitmap()))
                canvas.DrawImage(image, new SKRect(bounds.Left + 8, iconY, bounds.Left + 8 + _iconSize, iconY + _iconSize));
            textX += _iconSize + 8;
        }

        using (var paint = new SKPaint
        {
            Color = (isHovered || isOpened ? HoverForeColor : MenuForeColor).ToSKColor(),
            TextSize = Font.Size.PtToPx(this),
            Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
            IsAntialias = true
        })
        {
            var textBounds = new SKRect();
            paint.MeasureText(item.Text, ref textBounds);
            var textY = bounds.MidY + (textBounds.Height / 2);
            if (item.Text.Contains("&")) canvas.DrawTextWithMnemonic(item.Text, paint, textX, textY);
            else canvas.DrawText(item.Text, textX, textY, paint);
        }

        if (ShowSubmenuArrow && item.HasDropDown)
        {
            using var paint = new SKPaint { Color = (isHovered || isOpened ? HoverForeColor : MenuForeColor).ToSKColor(), IsAntialias = true };
            var arrowX = bounds.Right - _submenuArrowSize - 6;
            var arrowY = bounds.MidY;
            var path = new SKPath();
            path.MoveTo(arrowX, arrowY - _submenuArrowSize / 2);
            path.LineTo(arrowX + _submenuArrowSize, arrowY);
            path.LineTo(arrowX, arrowY + _submenuArrowSize / 2);
            path.Close();
            canvas.DrawPath(path, paint);
        }
    }

    private List<RectangleF> ComputeItemRects()
    {
        var rects = new List<RectangleF>(_items.Count);
        var bounds = ClientRectangle;
        if (Orientation == SDUI.Orientation.Horizontal)
        {
            float x = ItemPadding;
            float availableWidth = bounds.Width - (ItemPadding * 2);
            float totalItemWidth = 0;
            float[] itemWidths = new float[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                itemWidths[i] = MeasureItemWidth(_items[i]);
                totalItemWidth += itemWidths[i];
                if (i < _items.Count - 1) totalItemWidth += ItemPadding;
            }
            float extraPadding = 0;
            if (Stretch && _items.Count > 1 && totalItemWidth < availableWidth)
                extraPadding = (availableWidth - totalItemWidth) / (_items.Count - 1);

            for (int i = 0; i < _items.Count; i++)
            {
                float w = itemWidths[i];
                rects.Add(new RectangleF(x, 0, w, ItemHeight));
                x += w + ItemPadding + (i < _items.Count - 1 ? extraPadding : 0);
            }
        }
        else
        {
            float y = ItemPadding;
            float w = bounds.Width - (ItemPadding * 2);
            for (int i = 0; i < _items.Count; i++)
            {
                rects.Add(new RectangleF(ItemPadding, y, w, ItemHeight));
                y += ItemHeight + ItemPadding;
            }
        }
        return rects;
    }

    private SKRect GetItemBounds(MenuItem item)
    {
        var rects = ComputeItemRects();
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] == item)
            {
                var r = rects[i];
                return new SKRect(r.Left, r.Top, r.Right, r.Bottom);
            }
        }
        return SKRect.Empty;
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var rects = ComputeItemRects();
        MenuItem hovered = null;
        for (int i = 0; i < _items.Count; i++)
        {
            if (rects[i].Contains(e.Location)) { hovered = _items[i]; break; }
        }
        var oldHovered = _hoveredItem;
        _hoveredItem = hovered;
        if (oldHovered != _hoveredItem)
        {
            if (_hoveredItem?.HasDropDown == true && _openedItem != _hoveredItem)
                OpenSubmenu(_hoveredItem);
            Invalidate();
        }
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            var rects = ComputeItemRects();
            for (int i = 0; i < _items.Count; i++)
            {
                if (rects[i].Contains(e.Location))
                {
                    var item = _items[i];
                    if (item.HasDropDown)
                    {
                        if (_openedItem == item) CloseSubmenu(); else OpenSubmenu(item);
                    }
                    else
                    {
                        item.OnClick();
                        CloseSubmenu();
                    }
                    return;
                }
            }
            CloseSubmenu();
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoveredItem = null;
        Invalidate();
    }

    private void OpenSubmenu(MenuItem item)
    {
        if (!item.HasDropDown) { CloseSubmenu(); return; }
        if (_activeDropDownOwner == item && _activeDropDown != null && _activeDropDown.IsOpen)
        { CloseSubmenu(); return; }
        CloseSubmenu();
        EnsureDropDownHost();
        _activeDropDown.Items.Clear();
        foreach (var child in item.DropDownItems) _activeDropDown.AddItem(CloneMenuItem(child));
        SyncDropDownAppearance();

        var itemBounds = GetItemBounds(item);
        Point screenPoint;
        if (this is ContextMenuStrip)
        {
            var rightTop = new Point((int)itemBounds.Right, (int)itemBounds.Top);
            screenPoint = PointToScreen(rightTop);
        }
        else
        {
            var leftBottom = new Point((int)itemBounds.Left, (int)itemBounds.Bottom);
            screenPoint = PointToScreen(leftBottom);
        }
        _activeDropDownOwner = item;
        _openedItem = item;
        _activeDropDown.Show(this, screenPoint);
        _isAnimating = true;
        _animationProgress = 0f;
        _animationTimer.Start();
        Invalidate();
    }

    private void CloseSubmenu()
    {
        if (_activeDropDown != null && _activeDropDown.IsOpen) _activeDropDown.Hide();
        _openedItem = null;
        _activeDropDownOwner = null;
        _isAnimating = false;
        _animationTimer.Stop();
        Invalidate();
    }

    private void EnsureDropDownHost()
    {
        if (_activeDropDown != null) return;
        _activeDropDown = new ContextMenuStrip { AutoClose = true, RoundedCorners = true, Dock = DockStyle.None };
        _activeDropDown.Opening += (_, _) => SyncDropDownAppearance();
        _activeDropDown.Closing += (_, _) => { _openedItem = null; _activeDropDownOwner = null; Invalidate(); };
    }

    private void SyncDropDownAppearance()
    {
        if (_activeDropDown == null) return;
        _activeDropDown.MenuBackColor = SubmenuBackColor;
        _activeDropDown.MenuForeColor = MenuForeColor;
        _activeDropDown.SubmenuBackColor = SubmenuBackColor;
        _activeDropDown.SeparatorColor = SeparatorColor;
        _activeDropDown.RoundedCorners = RoundedCorners;
        _activeDropDown.ItemPadding = ItemPadding;
        _activeDropDown.Orientation = SDUI.Orientation.Vertical;
    }

    private MenuItem CloneMenuItem(MenuItem source)
    {
        if (source is MenuItemSeparator separator)
        {
            var cloneSeparator = new MenuItemSeparator { Height = separator.Height, Margin = separator.Margin, LineColor = separator.LineColor, ShadowColor = separator.ShadowColor };
            return cloneSeparator;
        }
        var clone = new MenuItem
        {
            Text = source.Text,
            Icon = source.Icon,
            Image = source.Image,
            ShortcutKeys = source.ShortcutKeys,
            ShowSubmenuArrow = source.ShowSubmenuArrow,
            ForeColor = source.ForeColor,
            BackColor = source.BackColor,
            Enabled = source.Enabled,
            Visible = source.Visible,
            Font = source.Font,
            AutoSize = source.AutoSize,
            Padding = source.Padding,
            Tag = source.Tag,
            Checked = source.Checked
        };
        foreach (var child in source.DropDownItems) clone.AddDropDownItem(CloneMenuItem(child));
        clone.Click += (_, _) => { source.OnClick(); _activeDropDown?.Hide(); };
        return clone;
    }

    protected float MeasureItemWidth(MenuItem item)
    {
        if (item is MenuItemSeparator) return 20f;
        using var paint = new SKPaint { TextSize = Font.Size.PtToPx(this), Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name) };
        var textBounds = new SKRect();
        paint.MeasureText(item.Text, ref textBounds);
        float width = textBounds.Width;
        if (ShowIcons && item.Icon != null) width += _iconSize + 8;
        if (ShowSubmenuArrow && item.HasDropDown) width += _submenuArrowSize + 12;
        return width + 16; // padding
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer?.Dispose();
            _activeDropDown?.Dispose();
        }
        base.Dispose(disposing);
    }
}