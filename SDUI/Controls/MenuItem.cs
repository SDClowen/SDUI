using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class MenuItem
{
    private readonly List<MenuItem> _dropDownItems = new();
    private string _text = string.Empty;
    private string _name = string.Empty;
    private Bitmap _icon;
    private Image _image;
    private Color _imageTransparentColor = Color.Empty;
    private Keys _shortcutKeys = Keys.None;
    private bool _isSeparator;
    private Size _size;
    private Color _foreColor = Color.Empty;
    private Color _backColor = Color.Empty;
    private bool _enabled = true;
    private bool _visible = true;
    private Font _font;
    private ContentAlignment _textAlign = ContentAlignment.MiddleLeft;
    private Padding _padding = new(3);
    private bool _autoSize = true;
    private bool _showSubmenuArrow = true;

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            if (_autoSize) UpdateSize();
            Parent?.Invalidate();
        }
    }

    [Category("Design")]
    [DefaultValue("")]
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
        }
    }

    public Bitmap Icon
    {
        get => _icon;
        set
        {
            if (_icon == value) return;
            _icon = value;
            if (_autoSize) UpdateSize();
            Parent?.Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(typeof(Keys), "None")]
    public Keys ShortcutKeys
    {
        get => _shortcutKeys;
        set
        {
            if (_shortcutKeys == value) return;
            _shortcutKeys = value;
            if (_autoSize) UpdateSize();
            Parent?.Invalidate();
        }
    }

    [Category("Appearance")]
    public Image Image
    {
        get => _image;
        set
        {
            if (_image == value) return;
            _image = value;
            if (_autoSize) UpdateSize();
            Parent?.Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "Empty")]
    public Color ImageTransparentColor
    {
        get => _imageTransparentColor;
        set
        {
            if (_imageTransparentColor == value) return;
            _imageTransparentColor = value;
            Parent?.Invalidate();
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
            if (_autoSize) UpdateSize();
            Parent?.Invalidate();
        }
    }

    public bool IsSeparator
    {
        get => _isSeparator;
        set
        {
            if (_isSeparator == value) return;
            _isSeparator = value;
            Parent?.Invalidate();
        }
    }

    [Category("Layout")]
    public Size Size
    {
        get => _size;
        set
        {
            if (_size == value) return;
            _size = value;
            _autoSize = false;
            Parent?.Invalidate();
        }
    }

    [Category("Appearance")]
    public Color ForeColor
    {
        get => _foreColor;
        set
        {
            if (_foreColor == value) return;
            _foreColor = value;
            Parent?.Invalidate();
        }
    }

    [Category("Appearance")]
    public Color BackColor
    {
        get => _backColor;
        set
        {
            if (_backColor == value) return;
            _backColor = value;
            Parent?.Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            Parent?.Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value) return;
            _visible = value;
            Parent?.Invalidate();
        }
    }

    [Category("Appearance")]
    public Font Font
    {
        get => _font ?? Parent?.Font;
        set
        {
            if (_font == value) return;
            _font = value;
            if (_autoSize) UpdateSize();
            Parent?.Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(ContentAlignment.MiddleLeft)]
    public ContentAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            if (_textAlign == value) return;
            _textAlign = value;
            Parent?.Invalidate();
        }
    }

    [Category("Layout")]
    public Padding Padding
    {
        get => _padding;
        set
        {
            if (_padding == value) return;
            _padding = value;
            if (_autoSize) UpdateSize();
            Parent?.Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize == value) return;
            _autoSize = value;
            if (value) UpdateSize();
            Parent?.Invalidate();
        }
    }

    public bool HasDropDown => _dropDownItems.Count > 0;
    public List<MenuItem> DropDownItems => _dropDownItems;
    internal MenuStrip Parent { get; set; }

    public MenuItem(string text = "", Bitmap icon = null)
    {
        _text = text;
        _icon = icon;
        UpdateSize();
    }

    private void UpdateSize()
    {
        if (Parent == null) return;

        using var paint = new SKPaint
        {
            TextSize = (Font ?? Parent.Font).Size.PtToPx(Parent),
            Typeface = SKTypeface.FromFamilyName((Font ?? Parent.Font).FontFamily.Name)
        };

        var textBounds = new SKRect();
        paint.MeasureText(Text, ref textBounds);

        int width = (int)textBounds.Width + Padding.Horizontal;
        int height = (int)textBounds.Height + Padding.Vertical;

        if (Icon != null || Image != null)
        {
            var imageWidth = Icon?.Width ?? Image?.Width ?? 0;
            var imageHeight = Icon?.Height ?? Image?.Height ?? 0;
            width += imageWidth + 4;
            height = Math.Max(height, imageHeight + Padding.Vertical);
        }

        if (HasDropDown && ShowSubmenuArrow)
        {
            width += 12; // Ok işareti için ekstra genişlik
        }

        if (ShortcutKeys != Keys.None)
        {
            var shortcutText = ShortcutKeys.ToString();
            var shortcutBounds = new SKRect();
            paint.MeasureText(shortcutText, ref shortcutBounds);
            width += (int)shortcutBounds.Width + 20; // Kısayol tuşu için ekstra genişlik
        }

        _size = new Size(width, height);
    }

    public static MenuItem CreateSeparator()
    {
        return new MenuItemSeparator();
    }

    public void AddDropDownItem(MenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _dropDownItems.Add(item);
        Parent?.Invalidate();
    }

    public void RemoveDropDownItem(MenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (_dropDownItems.Remove(item))
        {
            Parent?.Invalidate();
        }
    }

    internal void OnClick()
    {
        if (!Enabled) return;
        Click?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler Click;
}

public class MenuItemSeparator : MenuItem
{
    private float _height = 2f;
    private float _margin = 4f;
    private Color _lineColor = Color.FromArgb(100, 100, 100);
    private Color _shadowColor = Color.FromArgb(50, 50, 50);

    public MenuItemSeparator()
    {
        IsSeparator = true;
    }

    [Category("Appearance")]
    public float Height
    {
        get => _height;
        set
        {
            if (_height == value) return;
            _height = value;
            Parent?.Invalidate();
        }
    }

    [Category("Layout")]
    public float Margin
    {
        get => _margin;
        set
        {
            if (_margin == value) return;
            _margin = value;
            Parent?.Invalidate();
        }
    }

    [Category("Appearance")]
    public Color LineColor
    {
        get => _lineColor;
        set
        {
            if (_lineColor == value) return;
            _lineColor = value;
            Parent?.Invalidate();
        }
    }

    [Category("Appearance")]
    public Color ShadowColor
    {
        get => _shadowColor;
        set
        {
            if (_shadowColor == value) return;
            _shadowColor = value;
            Parent?.Invalidate();
        }
    }
}

public static class MenuStripExtensions
{
    public static MenuItem AddMenuItem(this MenuStrip menu, string text, EventHandler onClick = null, Keys shortcut = Keys.None)
    {
        var item = new MenuItem(text);
        if (onClick != null)
            item.Click += onClick;
        menu.AddItem(item);
        return item;
    }

    public static MenuItem AddMenuItem(this MenuItem parent, string text, EventHandler onClick = null, Keys shortcut = Keys.None)
    {
        var item = new MenuItem(text);
        if (onClick != null)
            item.Click += onClick;
        parent.AddDropDownItem(item);
        return item;
    }

    public static MenuItemSeparator AddSeparator(this MenuStrip menu)
    {
        var separator = new MenuItemSeparator();
        menu.AddItem(separator);
        return separator;
    }

    public static MenuItemSeparator AddSeparator(this MenuItem parent)
    {
        var separator = new MenuItemSeparator();
        parent.AddDropDownItem(separator);
        return separator;
    }
} 