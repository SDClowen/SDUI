using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SDUI.Controls;

public class RichTextBox : TextBox
{
    public RichTextBox()
    {
        IsRich = true;
        AutoScroll = true;
    }
}

public class TextBox : UIElementBase
{
    public BorderStyle BorderStyle = BorderStyle.Fixed3D; // Not Implemented

    private string _text = string.Empty;
    private string _placeholderText = string.Empty;
    private bool _isMultiLine;
    private bool _useSystemPasswordChar;
    private bool _passFocusShow;
    private char _passwordChar = '●';
    private int _maxLength = 32767;
    private int _selectionStart;
    private int _selectionLength;
    private bool _readOnly;
    private Color _borderColor = Color.FromArgb(171, 173, 179);
    private Color _focusedBorderColor = Color.FromArgb(0, 120, 212);
    private float _borderWidth = 1.0f;
    private float _cornerRadius = 4.0f;
    private Timer _cursorBlinkTimer;
    private bool _showCursor;
    private bool _isDragging;
    private HorizontalAlignment _textAlignment = HorizontalAlignment.Left;
    private ContextMenuStrip _contextMenu;
    private bool _acceptsTab;
    private bool _acceptsReturn;
    private bool _hideSelection;
    private Color _selectionColor = Color.FromArgb(128, 144, 206, 251);
    private int _caretWidth = 1;
    private int _radius = 2;
    private bool _autoHeight;
    private int _lineSpacing = 1;
    private bool _wordWrap = true;
    private bool _showScrollbar;
    private Color _scrollBarColor = Color.FromArgb(150, 150, 150);
    private Color _scrollBarHoverColor = Color.FromArgb(120, 120, 120);
    private bool _isScrollBarHovered;
    private bool _isDraggingScrollBar;
    private float _scrollPosition;
    private float _maxScroll;
    private bool _showCharCount;
    private Color _charCountColor = Color.Gray;
    private bool _isRich;
    private ScrollBar _verticalScrollBar;
    private ScrollBar _horizontalScrollBar;
    private bool _autoScroll = true;
    private List<TextStyle> _styles = new();
    private float _scrollSpeed = 1.0f;

    public int SelectionStart { get => _selectionStart; set { _selectionStart = value; Invalidate(); } }
    public int SelectionLength { get => _selectionLength; set { _selectionLength = value; Invalidate(); } }
    public int TextLength => Text.Length;
    public int LineCount => Text.Split('\n').Length;
    public Font SelectionFont { get; set; } = new Font("Segoe UI", 9.75f);

    public TextBox()
    {
        Size = new Size(100, 23);
        BackColor = Color.White;
        ForeColor = Color.Black;
        Cursor = Cursors.IBeam;
        TabStop = true;

        InitializeContextMenu();
        InitializeCursorBlink();
        InitializeScrollBars();
    }

    private void InitializeContextMenu()
    {
        _contextMenu = new();

        var kesItem = new MenuItem("Kes", null);
        kesItem.Click += (s, e) => Cut();

        var kopyalaItem = new MenuItem("Kopyala", null);
        kopyalaItem.Click += (s, e) => Copy();

        var yapistirItem = new MenuItem("Yapıştır", null);
        yapistirItem.Click += (s, e) => Paste();

        var silItem = new MenuItem("Sil", null);
        silItem.Click += (s, e) => DeleteSelection();

        var tumunuSecItem = new MenuItem("Tümünü Seç", null);
        tumunuSecItem.Click += (s, e) => Cut();

        _contextMenu.Items.AddRange(
        [
            kesItem, kopyalaItem, yapistirItem,
            new MenuItemSeparator(),
            silItem,
            new MenuItemSeparator(),
            tumunuSecItem
        ]);

        _contextMenu.Opening += (s, e) =>
        {
            bool hasSelection = _selectionLength > 0;
            kesItem.Enabled = hasSelection && !ReadOnly;
            kopyalaItem.Enabled = hasSelection;
            silItem.Enabled = hasSelection && !ReadOnly;
            yapistirItem.Enabled = !ReadOnly && Clipboard.ContainsText();
        };
    }

    private void InitializeCursorBlink()
    {
        _cursorBlinkTimer = new Timer
        {
            Interval = SystemInformation.CaretBlinkTime
        };
        _cursorBlinkTimer.Tick += (s, e) =>
        {
            if (Focused)
            {
                _showCursor = !_showCursor;
                Invalidate();
            }
        };
    }

    private void InitializeScrollBars()
    {
        _verticalScrollBar = new ScrollBar
        {
            Orientation = Orientation.Vertical,
            Visible = false,
            Width = 12
        };
        _verticalScrollBar.ValueChanged += (s, e) =>
        {
            _scrollPosition = _verticalScrollBar.Value;
            Invalidate();
        };

        _horizontalScrollBar = new ScrollBar
        {
            Orientation = Orientation.Horizontal,
            Visible = false,
            Height = 12
        };
        _horizontalScrollBar.ValueChanged += (s, e) => Invalidate();

        Controls.Add(_verticalScrollBar);
        Controls.Add(_horizontalScrollBar);
    }

    [Category("Appearance")]
    [DefaultValue("")]
    public override string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;

            if (_maxLength > 0 && value != null && value.Length > _maxLength)
            {
                value = value.Substring(0, _maxLength);
            }

            _text = value ?? string.Empty;
            OnTextChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue("")]
    public string PlaceholderText
    {
        get => _placeholderText;
        set
        {
            if (_placeholderText == value) return;
            _placeholderText = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool MultiLine
    {
        get => _isMultiLine;
        set
        {
            if (_isMultiLine == value) return;
            _isMultiLine = value;
            if (!value)
            {
                Height = 23;
            }
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool UseSystemPasswordChar
    {
        get => _useSystemPasswordChar;
        set
        {
            if (_useSystemPasswordChar == value) return;
            _useSystemPasswordChar = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue('●')]
    public char PasswordChar
    {
        get => _passwordChar;
        set
        {
            if (_passwordChar == value) return;
            _passwordChar = value;
            if (UseSystemPasswordChar)
            {
                Invalidate();
            }
        }
    }

    [Category("Behavior")]
    [DefaultValue(32767)]
    public int MaxLength
    {
        get => _maxLength;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (_maxLength == value) return;
            _maxLength = value;
            if (_maxLength > 0 && Text.Length > _maxLength)
            {
                Text = Text.Substring(0, _maxLength);
            }
        }
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool ReadOnly
    {
        get => _readOnly;
        set
        {
            if (_readOnly == value) return;
            _readOnly = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor == value) return;
            _borderColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color FocusedBorderColor
    {
        get => _focusedBorderColor;
        set
        {
            if (_focusedBorderColor == value) return;
            _focusedBorderColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(1.0f)]
    public float BorderWidth
    {
        get => _borderWidth;
        set
        {
            if (_borderWidth == value) return;
            _borderWidth = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(4.0f)]
    public float CornerRadius
    {
        get => _cornerRadius;
        set
        {
            if (_cornerRadius == value) return;
            _cornerRadius = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool AcceptsTab
    {
        get => _acceptsTab;
        set => _acceptsTab = value;
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool AcceptsReturn
    {
        get => _acceptsReturn;
        set => _acceptsReturn = value;
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool HideSelection
    {
        get => _hideSelection;
        set
        {
            if (_hideSelection == value) return;
            _hideSelection = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(HorizontalAlignment), "Left")]
    public HorizontalAlignment TextAlignment
    {
        get => _textAlignment;
        set
        {
            if (_textAlignment == value) return;
            _textAlignment = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color SelectionColor
    {
        get => _selectionColor;
        set
        {
            if (_selectionColor == value) return;
            _selectionColor = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool PassFocusShow
    {
        get => _passFocusShow;
        set
        {
            if (_passFocusShow == value) return;
            _passFocusShow = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(2)]
    public int Radius
    {
        get => _radius;
        set
        {
            if (_radius == value) return;
            _radius = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool AutoHeight
    {
        get => _autoHeight;
        set
        {
            if (_autoHeight == value) return;
            _autoHeight = value;
            if (value && MultiLine)
            {
                UpdateAutoHeight();
            }
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(1)]
    public int LineSpacing
    {
        get => _lineSpacing;
        set
        {
            if (_lineSpacing == value) return;
            _lineSpacing = value;
            if (MultiLine)
            {
                UpdateAutoHeight();
            }
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool WordWrap
    {
        get => _wordWrap;
        set
        {
            if (_wordWrap == value) return;
            _wordWrap = value;
            if (MultiLine)
            {
                UpdateAutoHeight();
            }
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(false)]
    public bool ShowScrollbar
    {
        get => _showScrollbar;
        set
        {
            if (_showScrollbar == value) return;
            _showScrollbar = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color ScrollBarColor
    {
        get => _scrollBarColor;
        set
        {
            if (_scrollBarColor == value) return;
            _scrollBarColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color ScrollBarHoverColor
    {
        get => _scrollBarHoverColor;
        set
        {
            if (_scrollBarHoverColor == value) return;
            _scrollBarHoverColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(false)]
    public bool ShowCharCount
    {
        get => _showCharCount;
        set
        {
            if (_showCharCount == value) return;
            _showCharCount = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color CharCountColor
    {
        get => _charCountColor;
        set
        {
            if (_charCountColor == value) return;
            _charCountColor = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool IsRich
    {
        get => _isRich;
        set
        {
            if (_isRich == value) return;
            _isRich = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool AutoScroll
    {
        get => _autoScroll;
        set
        {
            if (_autoScroll == value) return;
            _autoScroll = value;
            if (value)
            {
                ScrollToCaret();
            }
        }
    }

    [Category("Behavior")]
    [DefaultValue(1.0f)]
    public float ScrollSpeed
    {
        get => _scrollSpeed;
        set => _scrollSpeed = Math.Max(0.1f, Math.Min(10.0f, value));
    }

    public void Cut()
    {
        if (_selectionLength > 0 && !ReadOnly)
        {
            Copy();
            DeleteSelection();
        }
    }

    public void Copy()
    {
        if (_selectionLength > 0)
        {
            Clipboard.SetText(Text.Substring(_selectionStart, _selectionLength));
        }
    }

    public void Paste()
    {
        if (!ReadOnly && Clipboard.ContainsText())
        {
            var clipText = Clipboard.GetText();
            if (!MultiLine)
            {
                clipText = clipText.Replace("\r\n", "").Replace("\n", "");
            }

            if (_selectionLength > 0)
            {
                DeleteSelection();
            }

            if (MaxLength > 0)
            {
                var remainingLength = MaxLength - Text.Length + _selectionLength;
                if (clipText.Length > remainingLength)
                {
                    clipText = clipText.Substring(0, remainingLength);
                }
            }

            Text = Text.Insert(_selectionStart, clipText);
            _selectionStart += clipText.Length;
            _selectionLength = 0;
        }
    }

    public void SelectAll()
    {
        _selectionStart = 0;
        _selectionLength = Text.Length;
        Invalidate();
    }

    private void DeleteSelection()
    {
        if (_selectionLength > 0 && !ReadOnly)
        {
            Text = Text.Remove(_selectionStart, _selectionLength);
            _selectionLength = 0;
        }
    }

    internal override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        if (UseSystemPasswordChar && PassFocusShow)
        {
            _useSystemPasswordChar = false;
            Invalidate();
        }
        _cursorBlinkTimer.Start();
        _showCursor = true;
        Invalidate();
    }

    internal override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        if (UseSystemPasswordChar && PassFocusShow)
        {
            _useSystemPasswordChar = true;
            Invalidate();
        }
        _cursorBlinkTimer.Stop();
        _showCursor = false;
        Invalidate();
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _isDragging = false;

        if (e.Button == MouseButtons.Right)
        {
            _contextMenu.Show(this, e.Location);
        }
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (e.Button == MouseButtons.Left && _isDragging)
        {
            UpdateSelectionFromMousePosition(e.Location);
        }

        if (ShowScrollbar && MultiLine)
        {
            var scrollBarBounds = new Rectangle(
                Width - 12,
                2,
                8,
                Height - 4);

            bool wasHovered = _isScrollBarHovered;
            _isScrollBarHovered = scrollBarBounds.Contains(e.Location);

            if (wasHovered != _isScrollBarHovered)
            {
                Invalidate();
            }

            if (_isDraggingScrollBar)
            {
                var totalHeight = Height - 4;
                var scrollPercent = Math.Max(0, Math.Min(1, (e.Y - 2f) / totalHeight));
                _scrollPosition = scrollPercent * _maxScroll;
                Invalidate();
            }
        }
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Left)
        {
            Focus();
            _isDragging = true;

            if (ModifierKeys == Keys.Shift)
            {
                // Shift ile tıklama - seçimi genişlet
                var oldStart = _selectionStart;
                UpdateSelectionFromMousePosition(e.Location);
                var newPos = _selectionStart;
                _selectionStart = Math.Min(oldStart, newPos);
                _selectionLength = Math.Abs(newPos - oldStart);
            }
            else
            {
                // Normal tıklama
                UpdateSelectionFromMousePosition(e.Location);
                _selectionLength = 0;
            }
        }

        if (ShowScrollbar && MultiLine && _isScrollBarHovered)
        {
            _isDraggingScrollBar = true;
        }
    }

    internal override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Control)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    SelectAll();
                    e.Handled = true;
                    break;
                case Keys.C:
                    Copy();
                    e.Handled = true;
                    break;
                case Keys.X:
                    Cut();
                    e.Handled = true;
                    break;
                case Keys.V:
                    Paste();
                    e.Handled = true;
                    break;
            }
        }
        else
        {
            switch (e.KeyCode)
            {
                case Keys.Left when _selectionStart > 0:
                    if (e.Shift)
                    {
                        _selectionLength++;
                    }
                    else
                    {
                        _selectionLength = 0;
                    }
                    _selectionStart--;
                    Invalidate();
                    break;

                case Keys.Right when _selectionStart < Text.Length:
                    if (e.Shift)
                    {
                        _selectionLength++;
                    }
                    else
                    {
                        _selectionLength = 0;
                    }
                    _selectionStart++;
                    Invalidate();
                    break;

                case Keys.Home:
                    if (e.Shift)
                    {
                        _selectionLength = _selectionStart;
                    }
                    else
                    {
                        _selectionLength = 0;
                    }
                    _selectionStart = 0;
                    Invalidate();
                    break;

                case Keys.End:
                    if (e.Shift)
                    {
                        _selectionLength = Text.Length - _selectionStart;
                    }
                    else
                    {
                        _selectionLength = 0;
                    }
                    _selectionStart = Text.Length;
                    Invalidate();
                    break;
                case Keys.Delete:
                    if (!ReadOnly)
                    {
                        if (_selectionLength > 0)
                        {
                            DeleteSelection();
                        }
                        else if (_selectionStart < Text.Length)
                        {
                            Text = Text.Remove(_selectionStart, 1);
                        }
                    }
                    e.Handled = true;
                    break;

                case Keys.Tab:
                    if (!AcceptsTab)
                    {
                        return;
                    }
                    goto case Keys.Space;

                case Keys.Return:
                case Keys.LineFeed:
                    if (!AcceptsReturn || !MultiLine)
                    {
                        return;
                    }
                    goto case Keys.Space;

                case Keys.Space:
                    if (!ReadOnly)
                    {
                        var ch = e.KeyCode == Keys.Space ? ' ' :
                                e.KeyCode == Keys.Tab ? '\t' : '\n';

                        if (_selectionLength > 0)
                        {
                            DeleteSelection();
                        }

                        if (MaxLength == 0 || Text.Length < MaxLength)
                        {
                            Text = Text.Insert(_selectionStart, ch.ToString());
                            _selectionStart++;
                        }
                    }
                    e.Handled = true;
                    break;
            }
        }
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);

        var canvas = e.Surface.Canvas;
        var bounds = ClientRectangle;

        // Arka plan çizimi
        using (var paint = new SKPaint
        {
            Color = BackColor.ToSKColor(),
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(
                new SKRect(0, 0, bounds.Width, bounds.Height),
                Radius, Radius, paint);
        }

        // Kenarlık çizimi
        using (var paint = new SKPaint
        {
            Color = Focused ? FocusedBorderColor.ToSKColor() : BorderColor.ToSKColor(),
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = BorderWidth
        })
        {
            canvas.DrawRoundRect(
                new SKRect(BorderWidth / 2, BorderWidth / 2,
                    bounds.Width - BorderWidth / 2,
                    bounds.Height - BorderWidth / 2),
                Radius, Radius, paint);
        }

        // Metin çizimi
        var displayText = UseSystemPasswordChar && !string.IsNullOrEmpty(Text)
            ? new string(PasswordChar, Text.Length)
            : Text;

        if (string.IsNullOrEmpty(displayText) && !string.IsNullOrEmpty(PlaceholderText) && !Focused)
        {
            using (var font = new SKFont
            {
                Size = Font.Size * 96.0f / 72.0f,
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
            })
            using (var paint = new SKPaint
            {
                Color = Color.Gray.ToSKColor(),
                IsAntialias = true
            })
            {
                var textBounds = new SKRect();
                font.MeasureText(PlaceholderText, out textBounds);
                var y = (bounds.Height - textBounds.Height) / 2 - textBounds.Top;
                var x = GetTextX(bounds.Width, textBounds.Width);
                canvas.DrawText(PlaceholderText, x, y, SKTextAlign.Left, font, paint);
            }
        }
        else if (!string.IsNullOrEmpty(displayText))
        {
            using (var font = new SKFont
            {
                Size = Font.Size * 96.0f / 72.0f,
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
            })
            using (var paint = new SKPaint
            {
                Color = Enabled ? ForeColor.ToSKColor() : Color.Gray.ToSKColor(),
                IsAntialias = true
            })
            {
                var textBounds = new SKRect();
                font.MeasureText(displayText, out textBounds);
                var y = (bounds.Height - textBounds.Height) / 2 - textBounds.Top;
                var x = GetTextX(bounds.Width, textBounds.Width);
                canvas.DrawText(displayText, x, y, SKTextAlign.Left, font, paint);

                // Seçim çizimi
                if (Focused && _selectionLength > 0)
                {
                    var selectedText = displayText.Substring(_selectionStart, _selectionLength);
                    var selectedBounds = new SKRect();
                    font.MeasureText(selectedText, out selectedBounds);

                    var startX = x;
                    if (_selectionStart > 0)
                    {
                        var preText = displayText.Substring(0, _selectionStart);
                        var preBounds = new SKRect();
                        font.MeasureText(preText, out preBounds);
                        startX += preBounds.Width;
                    }

                    using (var selectionPaint = new SKPaint
                    {
                        Color = SKColors.LightBlue.WithAlpha(128),
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(
                            new SKRect(startX, 2,
                                startX + selectedBounds.Width,
                                bounds.Height - 2),
                            selectionPaint);
                    }
                }
            }
        }

        // İmleç çizimi
        if (Focused && _showCursor && _selectionLength == 0)
        {
            using var font = new SKFont
            {
                Size = Font.Size * 96.0f / 72.0f,
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
            };
            using var paint = new SKPaint
            {
                Color = ForeColor.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = _caretWidth
            };

            displayText = UseSystemPasswordChar ? new string(PasswordChar, Text.Length) : Text;
            
            var fullBounds = new SKRect();
            font.MeasureText(displayText, out fullBounds);
            var startX = GetTextX(bounds.Width, fullBounds.Width);

            var preText = displayText.Substring(0, _selectionStart);
            var textBounds = new SKRect();
            font.MeasureText(preText, out textBounds);

            var cursorX = startX + textBounds.Width;

            canvas.DrawLine(
                new SKPoint(cursorX, 2),
                new SKPoint(cursorX, bounds.Height - 2),
                paint);
        }

        // Seçim çizimi
        if (_selectionLength > 0 && (!HideSelection || Focused))
        {
            // ... mevcut seçim çizimi ...
        }

        // Karakter sayacı çizimi
        if (ShowCharCount)
        {
            var countText = MaxLength > 0 ? $"{Text.Length}/{MaxLength}" : Text.Length.ToString();
            using (var font = new SKFont
            {
                Size = (Font.Size - 2) * 96.0f / 72.0f,
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
            })
            using (var paint = new SKPaint
            {
                Color = CharCountColor.ToSKColor(),
                IsAntialias = true
            })
            {
                var textBounds = new SKRect();
                font.MeasureText(countText, out textBounds);
                canvas.DrawText(countText,
                    bounds.Width - textBounds.Width - 5,
                    bounds.Height - 5,
                    font, paint);
            }
        }

        // Kaydırma çubuğu çizimi
        if (ShowScrollbar && MultiLine)
        {
            var scrollBarBounds = new SKRect(
                bounds.Width - 12,
                2,
                bounds.Width - 4,
                bounds.Height - 4);

            using (var paint = new SKPaint
            {
                Color = (_isScrollBarHovered ? ScrollBarHoverColor : ScrollBarColor).ToSKColor(),
                IsAntialias = true
            })
            {
                canvas.DrawRoundRect(scrollBarBounds, 4, 4, paint);
            }
        }

        // Rich text çizimi
        if (IsRich && !string.IsNullOrEmpty(Text))
        {
            using (var font = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
            })
            using (var paint = new SKPaint
            {
                IsAntialias = true
            })
            {
                var lines = GetTextLines(font);
                var y = Padding.Top - _scrollPosition;
                var lineHeight = font.Size + LineSpacing;

                foreach (var line in lines)
                {
                    if (y + lineHeight < 0)
                    {
                        y += lineHeight;
                        continue;
                    }

                    if (y > bounds.Height)
                        break;

                    // Stil uygulaması
                    foreach (var style in _styles)
                    {
                        if (line.Contains(style.Pattern))
                        {
                            paint.Color = style.Color.ToSKColor();
                            if (style.IsBold)
                            {
                                using var boldFont = new Font(Font, FontStyle.Bold);
                                font.Typeface = SDUI.Helpers.FontManager.GetSKTypeface(boldFont);
                            }
                        }
                    }

                    canvas.DrawText(line, Padding.Left - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0), y + font.Size, font, paint);
                    y += lineHeight;

                    // Stili sıfırla
                    paint.Color = ForeColor.ToSKColor();
                    font.Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font);
                }
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cursorBlinkTimer?.Dispose();
            _contextMenu?.Dispose();
        }
        base.Dispose(disposing);
    }

    internal override void OnKeyPress(KeyPressEventArgs e)
    {
        if (ReadOnly)
        {
            e.Handled = true;
            return;
        }

        if (!char.IsControl(e.KeyChar))
        {
            if (MaxLength > 0 && Text.Length >= MaxLength)
            {
                e.Handled = true;
                return;
            }

            var newText = Text.Insert(_selectionStart, e.KeyChar.ToString());
            Text = newText;
            _selectionStart++;
            _selectionLength = 0;
            e.Handled = true;
        }
        else if (e.KeyChar == '\b' && _selectionStart > 0) // Backspace
        {
            if (_selectionLength > 0)
            {
                Text = Text.Remove(_selectionStart, _selectionLength);
                _selectionLength = 0;
            }
            else
            {
                Text = Text.Remove(_selectionStart - 1, 1);
                _selectionStart--;
            }
            e.Handled = true;
        }

        base.OnKeyPress(e);
    }

    private void UpdateSelectionFromMousePosition(Point location)
    {
        using (var font = new SKFont
        {
            Size = Font.Size * 96.0f / 72.0f,
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
        })
        {
            var displayText = UseSystemPasswordChar ? new string(PasswordChar, Text.Length) : Text;
            var clickX = location.X - Padding.Left;

            for (int i = 0; i <= displayText.Length; i++)
            {
                var textPart = displayText.Substring(0, i);
                var bounds = new SKRect();
                font.MeasureText(textPart, out bounds);

                if (bounds.Width >= clickX || i == displayText.Length)
                {
                    _selectionStart = i;
                    _selectionLength = 0;
                    Invalidate();
                    break;
                }
            }
        }
    }

    private SKTextAlign GetSkiaTextAlign()
    {
        return TextAlignment switch
        {
            HorizontalAlignment.Left => SKTextAlign.Left,
            HorizontalAlignment.Center => SKTextAlign.Center,
            HorizontalAlignment.Right => SKTextAlign.Right,
            _ => SKTextAlign.Left
        };
    }

    private float GetTextX(float boundsWidth, float textWidth)
    {
        return TextAlignment switch
        {
            HorizontalAlignment.Left => Padding.Left,
            HorizontalAlignment.Center => (boundsWidth - textWidth) / 2,
            HorizontalAlignment.Right => boundsWidth - textWidth - Padding.Right,
            _ => Padding.Left
        };
    }

    private void UpdateAutoHeight()
    {
        if (!AutoHeight || !MultiLine) return;

        using (var font = new SKFont
        {
            Size = Font.Size * 96.0f / 72.0f,
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
        })
        {
            var lines = GetTextLines(font);
            var lineHeight = font.Size + LineSpacing;
            var newHeight = (int)(lines.Count * lineHeight) + Padding.Vertical + 4;

            if (Height != newHeight)
            {
                Height = newHeight;
            }
        }
    }

    private List<string> GetTextLines(SKFont font)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(Text))
        {
            lines.Add(string.Empty);
            return lines;
        }

        var availableWidth = Width - Padding.Horizontal - (ShowScrollbar ? 20 : 0);

        if (!WordWrap)
        {
            lines.AddRange(Text.Split('\n'));
            return lines;
        }

        var words = Text.Split(' ');
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
            var bounds = new SKRect();
            font.MeasureText(testLine, out bounds);

            if (bounds.Width > availableWidth && currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
                currentLine.Append(word);
            }
            else
            {
                if (currentLine.Length > 0)
                    currentLine.Append(' ');
                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        return lines;
    }

    internal override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (MultiLine && _verticalScrollBar.Visible)
        {
            var delta = (e.Delta / 120f) * _verticalScrollBar.SmallChange * ScrollSpeed;
            _verticalScrollBar.Value = Math.Max(_verticalScrollBar.Minimum,
                Math.Min(_verticalScrollBar.Maximum,
                _verticalScrollBar.Value - (int)delta));
        }
    }

    internal override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        UpdateAutoHeight();
    }

    public void ScrollToCaret()
    {
        if (_verticalScrollBar.Visible)
        {
            _verticalScrollBar.Value = _verticalScrollBar.Maximum;
        }
    }

    public void AddStyle(TextStyle style)
    {
        _styles.Add(style);
        Invalidate();
    }

    public void ClearStyles()
    {
        _styles.Clear();
        Invalidate();
    }

    public void Clear()
    {
        Text = string.Empty;
        Invalidate();
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateScrollBars();
    }

    private void UpdateScrollBars()
    {
        if (!MultiLine) return;

        var bounds = ClientRectangle;
        var showVertical = false;
        var showHorizontal = false;

        using (var font = new SKFont
        {
            Size = Font.Size.PtToPx(this),
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
        })
        {
            var lines = GetTextLines(font);
            var totalHeight = lines.Count * (font.Size + LineSpacing);
            var maxWidth = 0f;

            foreach (var line in lines)
            {
                var tbounds = new SKRect();
                font.MeasureText(line, out tbounds);
                maxWidth = Math.Max(maxWidth, bounds.Width);
            }

            showVertical = totalHeight > Height;
            showHorizontal = maxWidth > Width;

            if (showVertical)
            {
                _verticalScrollBar.Location = new Point(Width - 12, 0);
                _verticalScrollBar.Height = showHorizontal ? Height - 12 : Height;
                _verticalScrollBar.Minimum = 0;
                _verticalScrollBar.Maximum = (int)(totalHeight - Height);
                _verticalScrollBar.SmallChange = (int)font.Size;
                _verticalScrollBar.LargeChange = Height;
            }

            if (showHorizontal)
            {
                _horizontalScrollBar.Location = new Point(0, Height - 12);
                _horizontalScrollBar.Width = showVertical ? Width - 12 : Width;
                _horizontalScrollBar.Minimum = 0;
                _horizontalScrollBar.Maximum = (int)(maxWidth - Width);
                _horizontalScrollBar.SmallChange = 10;
                _horizontalScrollBar.LargeChange = Width;
            }
        }

        _verticalScrollBar.Visible = showVertical;
        _horizontalScrollBar.Visible = showHorizontal;
    }
}

public class TextStyle
{
    public string Pattern { get; set; }
    public Color Color { get; set; }
    public bool IsBold { get; set; }

    public TextStyle(string pattern, Color color, bool isBold = false)
    {
        Pattern = pattern;
        Color = color;
        IsBold = isBold;
    }
}