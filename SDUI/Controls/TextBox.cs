using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SDUI.Controls;

internal static class NativeTextBoxMenu
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int TrackPopupMenuEx(IntPtr hMenu, uint uFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetFocus();

    private const uint TPM_RETURNCMD = 0x0100;
    private const uint TPM_LEFTALIGN = 0x0000;
    private const uint MF_STRING = 0x0000;
    private const uint MF_SEPARATOR = 0x0800;
    private const uint MF_GRAYED = 0x0001;

    private const uint CMD_UNDO = 1;
    private const uint CMD_CUT = 2;
    private const uint CMD_COPY = 3;
    private const uint CMD_PASTE = 4;
    private const uint CMD_DELETE = 5;
    private const uint CMD_SELECTALL = 6;

    public static void ShowContextMenu(TextBox textBox, Point screenLocation)
    {
        var hMenu = CreatePopupMenu();
        if (hMenu == IntPtr.Zero) return;

        try
        {
            bool hasSelection = textBox.SelectionLength > 0;
            bool canPaste = Clipboard.ContainsText();
            bool isReadOnly = textBox.ReadOnly;

            // Geri Al
            AppendMenu(hMenu, MF_STRING | MF_GRAYED, CMD_UNDO, "&Geri Al");
            AppendMenu(hMenu, MF_SEPARATOR, 0, null);

            // Kes
            uint cutFlags = MF_STRING;
            if (!hasSelection || isReadOnly) cutFlags |= MF_GRAYED;
            AppendMenu(hMenu, cutFlags, CMD_CUT, "Ke&s\tCtrl+X");

            // Kopyala
            uint copyFlags = MF_STRING;
            if (!hasSelection) copyFlags |= MF_GRAYED;
            AppendMenu(hMenu, copyFlags, CMD_COPY, "&Kopyala\tCtrl+C");

            // Yapıştır
            uint pasteFlags = MF_STRING;
            if (!canPaste || isReadOnly) pasteFlags |= MF_GRAYED;
            AppendMenu(hMenu, pasteFlags, CMD_PASTE, "&Yapıştır\tCtrl+V");

            // Sil
            uint deleteFlags = MF_STRING;
            if (!hasSelection || isReadOnly) deleteFlags |= MF_GRAYED;
            AppendMenu(hMenu, deleteFlags, CMD_DELETE, "Si&l\tDel");

            AppendMenu(hMenu, MF_SEPARATOR, 0, null);

            // Tümünü Seç
            AppendMenu(hMenu, MF_STRING, CMD_SELECTALL, "&Tümünü Seç\tCtrl+A");

            // Ana form'un handle'ını al
            var form = textBox.FindForm();
            var hWnd = form?.Handle ?? IntPtr.Zero;
            if (hWnd == IntPtr.Zero) return;

            var cmd = (uint)TrackPopupMenuEx(hMenu, TPM_RETURNCMD | TPM_LEFTALIGN, 
                screenLocation.X, screenLocation.Y, hWnd, IntPtr.Zero);

            if (cmd > 0)
            {
                switch (cmd)
                {
                    case CMD_CUT:
                        textBox.Cut();
                        break;
                    case CMD_COPY:
                        textBox.Copy();
                        break;
                    case CMD_PASTE:
                        textBox.Paste();
                        break;
                    case CMD_DELETE:
                        textBox.DeleteSelection();
                        break;
                    case CMD_SELECTALL:
                        textBox.SelectAll();
                        break;
                }
            }
        }
        finally
        {
            DestroyMenu(hMenu);
        }
    }
}

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
    private Color _borderColor = Color.FromArgb(200, 200, 200);
    private Color _focusedBorderColor = Color.FromArgb(0, 120, 215);
    private float _borderWidth = 1.0f;
    private float _cornerRadius = 2.0f;
    private Timer _cursorBlinkTimer = null!;
    private bool _showCursor;
    private bool _isDragging;
    private HorizontalAlignment _textAlignment = HorizontalAlignment.Left;
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
    private ScrollBar _verticalScrollBar = null!;
    private ScrollBar _horizontalScrollBar = null!;
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
        Size = new Size(200, 32);
        BackColor = Color.White;
        ForeColor = Color.FromArgb(32, 32, 32);
        Cursor = Cursors.IBeam;
        TabStop = true;
        Padding = new Padding(10, 6, 10, 6);

        InitializeCursorBlink();
        InitializeScrollBars();
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
    public new bool AutoScroll
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

    internal void DeleteSelection()
    {
        if (_selectionLength > 0 && !ReadOnly)
        {
            Text = Text.Remove(_selectionStart, _selectionLength);
            _selectionLength = 0;
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
            NativeTextBoxMenu.ShowContextMenu(this, PointToScreen(e.Location));
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

        // 1. Draw Background (flat, no gradient)
        using (var bgPaint = new SKPaint())
        {
            bgPaint.Color = Enabled ? BackColor.ToSKColor() : Color.FromArgb(250, 250, 250).ToSKColor();
            bgPaint.IsAntialias = true;
            canvas.DrawRoundRect(new SKRect(0, 0, bounds.Width, bounds.Height), CornerRadius, CornerRadius, bgPaint);
        }

        // 2. Draw Border (simple, flat)
        using (var borderPaint = new SKPaint())
        {
            var borderColor = Focused ? FocusedBorderColor : BorderColor;
            borderPaint.Color = Enabled ? borderColor.ToSKColor() : Color.FromArgb(220, 220, 220).ToSKColor();
            borderPaint.IsAntialias = true;
            borderPaint.IsStroke = true;
            borderPaint.StrokeWidth = Focused ? 2.0f : BorderWidth;
            
            var inset = borderPaint.StrokeWidth / 2;
            var borderRect = new SKRect(inset, inset, bounds.Width - inset, bounds.Height - inset);
            canvas.DrawRoundRect(borderRect, CornerRadius, CornerRadius, borderPaint);
        }

        // 4. Prepare Font
        using var font = new SKFont
        {
            Size = Font.Size * 96.0f / 72.0f,
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
        };

        // 5. Determine Text to Draw
        var displayText = UseSystemPasswordChar && !string.IsNullOrEmpty(Text)
            ? new string(PasswordChar, Text.Length)
            : Text;
            
        bool isPlaceholder = string.IsNullOrEmpty(displayText) && !string.IsNullOrEmpty(PlaceholderText) && !Focused;
        string textToDraw = isPlaceholder ? PlaceholderText : displayText;
        
        // 6. Calculate Text Position
        var textBounds = new SKRect();
        if (!string.IsNullOrEmpty(textToDraw))
        {
            font.MeasureText(textToDraw, out textBounds);
        }
        
        var metrics = font.Metrics;
        var y = (bounds.Height / 2) - ((metrics.Ascent + metrics.Descent) / 2);
        var x = GetTextX(bounds.Width, textBounds.Width);

        // 7. Draw Text
        if (!string.IsNullOrEmpty(textToDraw) && !IsRich)
        {
            using var textPaint = new SKPaint
            {
                Color = isPlaceholder ? Color.Gray.ToSKColor() : (Enabled ? ForeColor.ToSKColor() : Color.Gray.ToSKColor()),
                IsAntialias = true
            };
            
            TextRenderingHelper.DrawText(canvas, textToDraw, x, y, SKTextAlign.Left, font, textPaint);
        }

        // 8. Draw Selection (if not placeholder and not Rich)
        if (!isPlaceholder && Focused && _selectionLength > 0 && !IsRich)
        {
            var selectedText = displayText.Substring(_selectionStart, _selectionLength);
            var selectedBounds = new SKRect();
            font.MeasureText(selectedText, out selectedBounds);

            var selectionStartX = x;
            if (_selectionStart > 0)
            {
                var preText = displayText.Substring(0, _selectionStart);
                var preBounds = new SKRect();
                font.MeasureText(preText, out preBounds);
                selectionStartX += preBounds.Width;
            }

            using (var selectionPaint = new SKPaint
            {
                Color = SKColors.LightBlue.WithAlpha(128),
                IsAntialias = true
            })
            {
                var selectionTop = y + metrics.Ascent;
                var selectionBottom = y + metrics.Descent;
                
                canvas.DrawRect(
                    new SKRect(selectionStartX, selectionTop,
                        selectionStartX + selectedBounds.Width,
                        selectionBottom),
                    selectionPaint);
            }
        }

        // 9. Draw Cursor
        if (Focused && _showCursor && _selectionLength == 0)
        {
             using var cursorPaint = new SKPaint
             {
                 Color = ForeColor.ToSKColor(),
                 IsAntialias = true,
                 StrokeWidth = _caretWidth
             };
             
             var cursorX = x;
             if (!string.IsNullOrEmpty(displayText))
             {
                 var preText = displayText.Substring(0, Math.Min(_selectionStart, displayText.Length));
                 var preBounds = new SKRect();
                 font.MeasureText(preText, out preBounds);
                 cursorX += preBounds.Width;
             }
             
             var cursorTop = y + metrics.Ascent;
             var cursorBottom = y + metrics.Descent;
             
             canvas.DrawLine(new SKPoint(cursorX, cursorTop), new SKPoint(cursorX, cursorBottom), cursorPaint);
        }
        
        // 10. CharCount
        if (ShowCharCount)
        {
            var countText = MaxLength > 0 ? $"{Text.Length}/{MaxLength}" : Text.Length.ToString();
            using (var countFont = new SKFont
            {
                Size = (Font.Size - 2) * 96.0f / 72.0f,
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font)
            })
            using (var countPaint = new SKPaint
            {
                Color = CharCountColor.ToSKColor(),
                IsAntialias = true
            })
            {
                var countBounds = new SKRect();
                countFont.MeasureText(countText, out countBounds);
                TextRenderingHelper.DrawText(
                    canvas,
                    countText,
                    bounds.Width - countBounds.Width - 5,
                    bounds.Height - 5,
                    countFont,
                    countPaint);
            }
        }

        // 11. Scrollbars
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

        // 12. Rich Text
        if (IsRich && !string.IsNullOrEmpty(Text))
        {
            using (var paint = new SKPaint
            {
                IsAntialias = true
            })
            {
                var lines = GetTextLines(font);
                var richY = Padding.Top - _scrollPosition;
                var lineHeight = font.Size + LineSpacing;

                foreach (var line in lines)
                {
                    if (richY + lineHeight < 0)
                    {
                        richY += lineHeight;
                        continue;
                    }

                    if (richY > bounds.Height)
                        break;

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

                    TextRenderingHelper.DrawText(
                        canvas,
                        line,
                        Padding.Left - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0),
                        richY + font.Size,
                        font,
                        paint);
                    richY += lineHeight;

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