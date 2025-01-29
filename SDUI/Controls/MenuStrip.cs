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
    private Color _menuBackColor = Color.FromArgb(32, 32, 32);
    private Color _menuForeColor = Color.White;
    private Color _hoverBackColor = Color.FromArgb(45, 45, 45);
    private Color _hoverForeColor = Color.FromArgb(0, 120, 215);
    private Color _submenuBackColor = Color.FromArgb(28, 28, 28);
    private Color _submenuBorderColor = Color.FromArgb(64, 64, 64);
    private Color _separatorColor = Color.FromArgb(64, 64, 64);
    private float _itemHeight = 28f;
    private float _itemPadding = 12f;
    private float _submenuArrowSize = 8f;
    private float _iconSize = 16f;
    private float _separatorHeight = 1f;
    private bool _showIcons = true;
    private bool _showHoverEffect = true;
    private bool _showSubmenuArrow = true;
    private bool _roundedCorners = true;
    private float _cornerRadius = 4f;
    private float _submenuCornerRadius = 6f;
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

    public MenuStrip()
    {
        Height = (int)_itemHeight;
        BackColor = _menuBackColor;
        ForeColor = _menuForeColor;
        InitializeAnimationTimer();
    }

    [Browsable(false)]
    public List<MenuItem> Items => _items;

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool Stretch
    {
        get => _stretch;
        set
        {
            if (_stretch == value) return;
            _stretch = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Size), "20, 20")]
    public Size ImageScalingSize
    {
        get => _imageScalingSize;
        set
        {
            if (_imageScalingSize == value) return;
            _imageScalingSize = value;
            _iconSize = Math.Min(value.Width, value.Height);
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool ShowSubmenuArrow
    {
        get => _showSubmenuArrow;
        set
        {
            if (_showSubmenuArrow == value) return;
            _showSubmenuArrow = value;
            Invalidate();
        }
    }

    private void InitializeAnimationTimer()
    {
        _animationTimer = new Timer { Interval = 16 }; // ~60 FPS
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

    [Category("Appearance")]
    public Color MenuBackColor
    {
        get => _menuBackColor;
        set
        {
            if (_menuBackColor == value) return;
            _menuBackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color MenuForeColor
    {
        get => _menuForeColor;
        set
        {
            if (_menuForeColor == value) return;
            _menuForeColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color HoverBackColor
    {
        get => _hoverBackColor;
        set
        {
            if (_hoverBackColor == value) return;
            _hoverBackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color HoverForeColor
    {
        get => _hoverForeColor;
        set
        {
            if (_hoverForeColor == value) return;
            _hoverForeColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SubmenuBackColor
    {
        get => _submenuBackColor;
        set
        {
            if (_submenuBackColor == value) return;
            _submenuBackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SubmenuBorderColor
    {
        get => _submenuBorderColor;
        set
        {
            if (_submenuBorderColor == value) return;
            _submenuBorderColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SeparatorColor
    {
        get => _separatorColor;
        set
        {
            if (_separatorColor == value) return;
            _separatorColor = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    public float ItemHeight
    {
        get => _itemHeight;
        set
        {
            if (_itemHeight == value) return;
            _itemHeight = value;
            Height = (int)value;
            Invalidate();
        }
    }

    [Category("Layout")]
    public float ItemPadding
    {
        get => _itemPadding;
        set
        {
            if (_itemPadding == value) return;
            _itemPadding = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public bool ShowIcons
    {
        get => _showIcons;
        set
        {
            if (_showIcons == value) return;
            _showIcons = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    public bool ShowHoverEffect
    {
        get => _showHoverEffect;
        set
        {
            if (_showHoverEffect == value) return;
            _showHoverEffect = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public bool RoundedCorners
    {
        get => _roundedCorners;
        set
        {
            if (_roundedCorners == value) return;
            _roundedCorners = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SeparatorBackColor
    {
        get => _separatorBackColor;
        set
        {
            if (_separatorBackColor == value) return;
            _separatorBackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SeparatorForeColor
    {
        get => _separatorForeColor;
        set
        {
            if (_separatorForeColor == value) return;
            _separatorForeColor = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    public float SeparatorMargin
    {
        get => _separatorMargin;
        set
        {
            if (_separatorMargin == value) return;
            _separatorMargin = value;
            Invalidate();
        }
    }

    public void AddItem(MenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
        item.Parent = this;
        Invalidate();
    }

    public void RemoveItem(MenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (_items.Remove(item))
        {
            item.Parent = null;
            Invalidate();
        }
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);

        var canvas = e.Surface.Canvas;
        var bounds = ClientRectangle;

        // Ana menü arka planı
        using (var paint = new SKPaint
        {
            Color = MenuBackColor.ToSKColor(),
            IsAntialias = true
        })
        {
            if (RoundedCorners)
            {
                canvas.DrawRoundRect(new SKRect(0, 0, bounds.Width, bounds.Height), _cornerRadius, _cornerRadius, paint);
            }
            else
            {
                canvas.DrawRect(new SKRect(0, 0, bounds.Width, bounds.Height), paint);
            }
        }

        // Menü öğelerini çiz
        float x = ItemPadding;
        float availableWidth = bounds.Width - (ItemPadding * 2);
        float totalItemWidth = 0;
        float[] itemWidths = new float[_items.Count];

        // Önce tüm öğelerin genişliklerini hesapla
        for (int i = 0; i < _items.Count; i++)
        {
            itemWidths[i] = MeasureItemWidth(_items[i]);
            totalItemWidth += itemWidths[i];
            if (i < _items.Count - 1)
                totalItemWidth += ItemPadding;
        }

        // Stretch modunda ise boşlukları eşit dağıt
        float extraPadding = 0;
        if (Stretch && _items.Count > 0 && totalItemWidth < availableWidth)
        {
            extraPadding = (availableWidth - totalItemWidth) / (_items.Count - 1);
        }

        // Öğeleri çiz
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var itemWidth = itemWidths[i];
            var itemBounds = new SKRect(x, 0, x + itemWidth, ItemHeight);
            DrawMenuItem(canvas, item, itemBounds);
            x += itemWidth + ItemPadding + (i < _items.Count - 1 ? extraPadding : 0);
        }

        // Alt menüleri çiz
        if (_openedItem != null)
        {
            DrawSubmenu(canvas, _openedItem);
        }
    }

    private void DrawMenuItem(SKCanvas canvas, MenuItem item, SKRect bounds)
    {
        var isHovered = item == _hoveredItem;
        var isOpened = item == _openedItem;

        // Hover efekti
        if ((isHovered || isOpened) && ShowHoverEffect)
        {
            using var paint = new SKPaint
            {
                Color = HoverBackColor.ToSKColor(),
                IsAntialias = true
            };

            if (RoundedCorners)
            {
                canvas.DrawRoundRect(bounds, _cornerRadius, _cornerRadius, paint);
            }
            else
            {
                canvas.DrawRect(bounds, paint);
            }
        }

        float textX = bounds.Left;

        // İkon
        if (ShowIcons && item.Icon != null)
        {
            var iconY = (ItemHeight - _iconSize) / 2;
            using (var image = SKImage.FromBitmap(item.Icon.ToSKBitmap()))
            {
                canvas.DrawImage(image, new SKRect(bounds.Left, iconY, bounds.Left + _iconSize, iconY + _iconSize));
            }
            textX += _iconSize + 4;
        }

        // Metin
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
            canvas.DrawText(item.Text, textX, textY, paint);
        }

        // Alt menü oku
        if (ShowSubmenuArrow && item.HasDropDown)
        {
            using var paint = new SKPaint
            {
                Color = (isHovered || isOpened ? HoverForeColor : MenuForeColor).ToSKColor(),
                IsAntialias = true
            };

            var arrowX = bounds.Right - _submenuArrowSize - 4;
            var arrowY = bounds.MidY;
            var path = new SKPath();
            path.MoveTo(arrowX, arrowY - _submenuArrowSize / 2);
            path.LineTo(arrowX + _submenuArrowSize, arrowY);
            path.LineTo(arrowX, arrowY + _submenuArrowSize / 2);
            path.Close();

            canvas.DrawPath(path, paint);
        }
    }

    private void DrawSubmenu(SKCanvas canvas, MenuItem parentItem)
    {
        if (!parentItem.HasDropDown) return;

        var parentBounds = GetItemBounds(parentItem);
        var submenuWidth = MeasureSubmenuWidth(parentItem);
        var submenuHeight = MeasureSubmenuHeight(parentItem);

        var x = parentBounds.Left;
        var y = parentBounds.Bottom + _submenuOffset;

        // Gölge efekti
        using (var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(50),
            ImageFilter = SKImageFilter.CreateBlur(_submenuShadowBlur, _submenuShadowBlur),
            IsAntialias = true
        })
        {
            if (RoundedCorners)
            {
                canvas.DrawRoundRect(
                    new SKRect(x, y, x + submenuWidth, y + submenuHeight),
                    _submenuCornerRadius, _submenuCornerRadius,
                    shadowPaint);
            }
            else
            {
                canvas.DrawRect(
                    new SKRect(x, y, x + submenuWidth, y + submenuHeight),
                    shadowPaint);
            }
        }

        // Alt menü arka planı
        using (var paint = new SKPaint
        {
            Color = SubmenuBackColor.ToSKColor(),
            IsAntialias = true
        })
        {
            if (RoundedCorners)
            {
                canvas.DrawRoundRect(
                    new SKRect(x, y, x + submenuWidth, y + submenuHeight),
                    _submenuCornerRadius, _submenuCornerRadius,
                    paint);
            }
            else
            {
                canvas.DrawRect(
                    new SKRect(x, y, x + submenuWidth, y + submenuHeight),
                    paint);
            }
        }

        // Alt menü kenarlığı
        using (var paint = new SKPaint
        {
            Color = SubmenuBorderColor.ToSKColor(),
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 1
        })
        {
            if (RoundedCorners)
            {
                canvas.DrawRoundRect(
                    new SKRect(x, y, x + submenuWidth, y + submenuHeight),
                    _submenuCornerRadius, _submenuCornerRadius,
                    paint);
            }
            else
            {
                canvas.DrawRect(
                    new SKRect(x, y, x + submenuWidth, y + submenuHeight),
                    paint);
            }
        }

        // Alt menü öğelerini çiz
        float currentY = y + _itemPadding;
        foreach (var item in parentItem.DropDownItems)
        {
            if (item.IsSeparator)
            {
                DrawSeparator(canvas, x, currentY, submenuWidth);
                currentY += _separatorHeight + _itemPadding;
            }
            else
            {
                var itemBounds = new SKRect(x + _itemPadding, currentY,
                    x + submenuWidth - _itemPadding, currentY + ItemHeight);
                DrawMenuItem(canvas, item, itemBounds);
                currentY += ItemHeight + _itemPadding;
            }
        }
    }

    private void DrawSeparator(SKCanvas canvas, float x, float y, float width)
    {
        var margin = _separatorMargin;
        var height = _separatorHeight;

        // Arka plan
        using (var paint = new SKPaint
        {
            Color = _separatorBackColor.ToSKColor(),
            IsAntialias = true
        })
        {
            canvas.DrawRect(
                new SKRect(x + margin, y, x + width - margin, y + height),
                paint);
        }

        // Ön plan çizgisi
        using (var paint = new SKPaint
        {
            Color = _separatorForeColor.ToSKColor(),
            IsAntialias = true,
            StrokeWidth = 1,
            IsStroke = true
        })
        {
            canvas.DrawLine(
                x + margin,
                y + height / 2,
                x + width - margin,
                y + height / 2,
                paint);
        }
    }

    protected float MeasureItemWidth(MenuItem item)
    {
        if (item is MenuItemSeparator)
            return 20f; // Sabit ayraç genişliği

        using var paint = new SKPaint
        {
            TextSize = Font.Size.PtToPx(this),
            Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name)
        };

        var textBounds = new SKRect();
        paint.MeasureText(item.Text, ref textBounds);
        float width = textBounds.Width;

        if (ShowIcons && item.Icon != null)
        {
            width += _iconSize + 4;
        }

        if (ShowSubmenuArrow && item.HasDropDown)
        {
            width += _submenuArrowSize + 8;
        }

        return width;
    }

    private float MeasureSubmenuWidth(MenuItem parentItem)
    {
        float maxWidth = 0;
        foreach (var item in parentItem.DropDownItems)
        {
            if (!item.IsSeparator)
            {
                maxWidth = Math.Max(maxWidth, MeasureItemWidth(item));
            }
        }
        return maxWidth + (_itemPadding * 2);
    }

    private float MeasureSubmenuHeight(MenuItem parentItem)
    {
        float height = _itemPadding;
        foreach (var item in parentItem.DropDownItems)
        {
            if (item.IsSeparator)
            {
                height += _separatorHeight + _itemPadding;
            }
            else
            {
                height += ItemHeight + _itemPadding;
            }
        }
        return height;
    }

    private SKRect GetItemBounds(MenuItem item)
    {
        float x = ItemPadding;
        foreach (var menuItem in _items)
        {
            var width = MeasureItemWidth(menuItem);
            if (menuItem == item)
            {
                return new SKRect(x, 0, x + width, ItemHeight);
            }
            x += width + ItemPadding;
        }
        return SKRect.Empty;
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var oldHovered = _hoveredItem;
        _hoveredItem = GetItemAtPoint(e.Location);

        if (oldHovered != _hoveredItem)
        {
            if (_hoveredItem?.HasDropDown == true && _openedItem != _hoveredItem)
            {
                OpenSubmenu(_hoveredItem);
            }
            Invalidate();
        }
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Left)
        {
            var item = GetItemAtPoint(e.Location);
            if (item != null)
            {
                if (item.HasDropDown)
                {
                    if (_openedItem == item)
                    {
                        CloseSubmenu();
                    }
                    else
                    {
                        OpenSubmenu(item);
                    }
                }
                else
                {
                    item.OnClick();
                    CloseSubmenu();
                }
            }
            else
            {
                CloseSubmenu();
            }
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoveredItem = null;
        Invalidate();
    }

    private MenuItem GetItemAtPoint(Point point)
    {
        float x = ItemPadding;
        foreach (var item in _items)
        {
            var width = MeasureItemWidth(item);
            if (point.X >= x && point.X <= x + width && point.Y <= ItemHeight)
            {
                return item;
            }
            x += width + ItemPadding;
        }
        return null;
    }

    private void OpenSubmenu(MenuItem item)
    {
        if (_openedItem == item) return;

        _openedItem = item;
        _isAnimating = true;
        _animationProgress = 0f;
        _animationTimer.Start();
        Invalidate();
    }

    private void CloseSubmenu()
    {
        if (_openedItem == null) return;

        _openedItem = null;
        _isAnimating = false;
        _animationTimer.Stop();
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}