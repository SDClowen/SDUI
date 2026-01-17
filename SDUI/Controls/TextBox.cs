using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

internal static class NativeTextBoxMenu
{
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
    private const uint CMD_BOLD = 7;
    private const uint CMD_ITALIC = 8;

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

    public static void ShowContextMenu(TextBox textBox, Point screenLocation)
    {
        var hMenu = CreatePopupMenu();
        if (hMenu == IntPtr.Zero) return;

        try
        {
            // Ensure the textbox is focused before opening the menu (selection visibility / caret state).
            if (!textBox.Focused)
                textBox.Focus();

            var hasSelection = textBox.SelectionLength > 0;
            var canPaste = Clipboard.ContainsText();
            var isReadOnly = textBox.ReadOnly;

            // Geri Al
            AppendMenu(hMenu, MF_STRING | MF_GRAYED, CMD_UNDO, "&Geri Al");
            AppendMenu(hMenu, MF_SEPARATOR, 0, null);

            // Kes
            var cutFlags = MF_STRING;
            if (!hasSelection || isReadOnly) cutFlags |= MF_GRAYED;
            AppendMenu(hMenu, cutFlags, CMD_CUT, "Ke&s\tCtrl+X");

            // Kopyala
            var copyFlags = MF_STRING;
            if (!hasSelection) copyFlags |= MF_GRAYED;
            AppendMenu(hMenu, copyFlags, CMD_COPY, "&Kopyala\tCtrl+C");

            // Yapıştır
            var pasteFlags = MF_STRING;
            if (!canPaste || isReadOnly) pasteFlags |= MF_GRAYED;
            AppendMenu(hMenu, pasteFlags, CMD_PASTE, "&Yapıştır\tCtrl+V");

            // Sil
            var deleteFlags = MF_STRING;
            if (!hasSelection || isReadOnly) deleteFlags |= MF_GRAYED;
            AppendMenu(hMenu, deleteFlags, CMD_DELETE, "Si&l\tDel");

            // Rich text formatting (only when supported)
            if (textBox.IsRich)
            {
                var boldFlags = MF_STRING;
                if (!hasSelection || isReadOnly) boldFlags |= MF_GRAYED;
                AppendMenu(hMenu, boldFlags, CMD_BOLD, "&Kalın\tCtrl+B");

                var italicFlags = MF_STRING;
                if (!hasSelection || isReadOnly) italicFlags |= MF_GRAYED;
                AppendMenu(hMenu, italicFlags, CMD_ITALIC, "&İtalik\tCtrl+I");

                AppendMenu(hMenu, MF_SEPARATOR, 0, null);
            }

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
                        // TrackPopupMenuEx can steal focus via the host window; restore focus and redraw.
                        if (!textBox.Focused)
                            textBox.Focus();
                        textBox.SelectAll();
                        textBox.Invalidate();
                        break;
                    case CMD_BOLD:
                        if (textBox.IsRich && !textBox.ReadOnly) textBox.ToggleBoldSelection();
                        break;
                    case CMD_ITALIC:
                        if (textBox.IsRich && !textBox.ReadOnly) textBox.ToggleItalicSelection();
                        break;
                }

                // Keep UI state consistent after a menu command.
                if (!textBox.Focused)
                    textBox.Focus();
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

    public RichTextBoxScrollBars ScrollBars
    {
        get => _verticalScrollBar.Visible && _horizontalScrollBar.Visible
            ? RichTextBoxScrollBars.Both
            : _verticalScrollBar.Visible
                ? RichTextBoxScrollBars.Vertical
                : _horizontalScrollBar.Visible
                    ? RichTextBoxScrollBars.Horizontal
                    : RichTextBoxScrollBars.None;
        set
        {
            switch (value)
            {
                case RichTextBoxScrollBars.Both:
                    _verticalScrollBar.Visible = true;
                    _horizontalScrollBar.Visible = true;
                    break;
                case RichTextBoxScrollBars.Vertical:
                    _verticalScrollBar.Visible = true;
                    _horizontalScrollBar.Visible = false;
                    break;
                case RichTextBoxScrollBars.Horizontal:
                    _verticalScrollBar.Visible = false;
                    _horizontalScrollBar.Visible = true;
                    break;
                case RichTextBoxScrollBars.None:
                    _verticalScrollBar.Visible = false;
                    _horizontalScrollBar.Visible = false;
                    break;
            }

            Invalidate();
        }
    }


    /// <summary>
    ///     Select a range of text inside the RichTextBox.
    /// </summary>
    /// <param name="start">Zero-based start index.</param>
    /// <param name="length">Number of characters to select.</param>
    public void Select(int start, int length)
    {
        if (start < 0) start = 0;
        if (length < 0) length = 0;

        if (start > Text.Length)
            start = Text.Length;

        if (start + length > Text.Length)
            length = Math.Max(0, Text.Length - start);

        // Use base selection properties
        SelectionStart = start;
        SelectionLength = length;

        // Make selection visible
        if (AutoScroll)
            ScrollToCaret();

        // Optionally focus so selection is visible when possible
        if (!Focused)
            Focus();

        Invalidate();
    }
}

public class TextBox : UIElementBase
{
    private readonly int _caretWidth = 1;
    private int CaretWidthScaled => (int)(_caretWidth * ScaleFactor);
    private readonly List<TextStyle> _styles = new();
    private readonly List<TextStyleSpan> _styleSpans = new();
    private bool _acceptsReturn;
    private bool _acceptsTab;
    private bool _autoHeight;
    private bool _autoScroll = true;

    // Cached Skia resources (avoid per-frame allocations)
    private SKPaint? _bgPaint;
    private SKFont? _boldSkFont;
    private int _boldSkFontDpi;
    private Font? _boldSkFontSource;

    private Font? _boldSystemFont;
    private Font? _boldSystemFontSource;
    private Color _borderColor = ColorScheme.BorderColor;
    private SKPaint? _borderPaint;
    private float _borderWidth = 1.0f;
    private float BorderWidthScaled => _borderWidth * ScaleFactor;
    private Color _charCountColor = Color.Gray;
    private SKPaint? _charCountPaint;

    private SKFont? _charCountSkFont;
    private float _charCountSkFontBaseSize;
    private int _charCountSkFontDpi;
    private Font? _charCountSkFontSource;
    private float _cornerRadius = 4.0f;
    private float CornerRadiusScaled => _cornerRadius * ScaleFactor;
    private Timer _cursorBlinkTimer = null!;
    private SKPaint? _cursorPaint;

    private SKFont? _defaultSkFont;
    private int _defaultSkFontDpi;
    private Font? _defaultSkFontSource;

    // Focus animation
    private AnimationManager _focusAnimation = null!;
    private Color _focusedBorderColor = ColorScheme.AccentColor;
    private SKPaint? _focusGlowPaint;
    private bool _hideSelection;
    internal ScrollBar _horizontalScrollBar = null!;
    private bool _isDragging;
    private bool _isDraggingScrollBar;
    private bool _isMultiLine;
    private bool _isRich;
    private bool _isScrollBarHovered;
    private SKFont? _italicSkFont;
    private int _italicSkFontDpi;
    private Font? _italicSkFontSource;

    private Font? _italicSystemFont;
    private Font? _italicSystemFontSource;
    private int _lineSpacing = 1;
    private int _maxLength = 32767;
    private float _maxScroll;
    private bool _passFocusShow;
    private char _passwordChar = '●';
    private string _placeholderText = string.Empty;
    private float? _preferredCaretX;
    private int _radius = 2;
    private bool _readOnly;
    private SKPaint? _richTextPaint;
    private Color _scrollBarColor = Color.FromArgb(150, 150, 150);
    private Color _scrollBarHoverColor = Color.FromArgb(120, 120, 120);
    private SKPaint? _scrollbarPaint;
    private float _scrollPosition;
    private float _scrollSpeed = 1.0f;
    private int _selectionAnchor = -1;
    private Color _selectionBackColor = Color.Empty;
    private Color _selectionColor = ColorScheme.AccentColor.Alpha(90);
    private int _selectionLength;
    private SKPaint? _selectionPaint;
    private int _selectionStart;
    private bool _showCharCount;
    private bool _showCursor;
    private bool _showScrollbar;

    private string _text = string.Empty;
    private HorizontalAlignment _textAlignment = HorizontalAlignment.Left;
    private SKPaint? _textPaint;
    private Color _themeBorderColorSnapshot;
    private Color _themeFocusedBorderColorSnapshot;

    // Theme-driven defaults (only update if user hasn't overridden)
    private Color _themeForeColorSnapshot;
    private Color _themeSelectionColorSnapshot;
    private bool _useSystemPasswordChar;
    internal ScrollBar _verticalScrollBar = null!;
    private bool _wordWrap = true;
    public BorderStyle BorderStyle = BorderStyle.Fixed3D; // Not Implemented

    public TextBox()
    {
        MinimumSize = new Size((int)(50 * ScaleFactor), (int)(28 * ScaleFactor));
        Size = new Size((int)(120 * ScaleFactor), (int)(28 * ScaleFactor));
        BackColor = Color.Transparent; // Transparent olmalı ki UIElementBase canvas.Clear ile beyaz çizmesin
        ForeColor = ColorScheme.ForeColor;
        BorderColor = ColorScheme.BorderColor;
        FocusedBorderColor = ColorScheme.AccentColor;
        Cursor = Cursors.IBeam;
        TabStop = true;
        Padding = new Padding(10, 6, 10, 6);

        InitializeCursorBlink();
        InitializeFocusAnimation();
        InitializeThemeBinding();
        InitializeScrollBars();

        // sensible default: when multiline is used, enable scrollbars
        if (MultiLine && !_showScrollbar)
            _showScrollbar = true;

        NeedsRedraw = true;
        Invalidate();
        Debug.WriteLine("TextBox.Constructor: SDUI Custom TextBox created!");
    }

    public int SelectionStart
    {
        get => _selectionStart;
        set
        {
            _selectionStart = value;
            Invalidate();
        }
    }

    public int SelectionLength
    {
        get => _selectionLength;
        set
        {
            _selectionLength = value;
            Invalidate();
        }
    }

    public int TextLength => Text.Length;
    public int LineCount => Text.Split('\n').Length;

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool AcceptsReturn
    {
        get => _acceptsReturn;
        set
        {
            if (_acceptsReturn == value) return;
            _acceptsReturn = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    public bool AcceptsTab
    {
        get => _acceptsTab;
        set
        {
            if (_acceptsTab == value) return;
            _acceptsTab = value;
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
            _placeholderText = value ?? string.Empty;
            Invalidate();
        }
    }

    /// <summary>
    ///     Returns the text broken into lines. Normalizes CRLF to LF so callers can assume '\n' separators.
    /// </summary>
    public string[] Lines
    {
        get
        {
            if (string.IsNullOrEmpty(Text)) return Array.Empty<string>();
            return Text.Replace("\r\n", "\n").Split('\n');
        }
    }

    public Font SelectionFont { get; set; } = new("Segoe UI", 9.75f);

    [Category("Appearance")]
    [DefaultValue("")]
    public override string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;

            if (_maxLength > 0 && value != null && value.Length > _maxLength) value = value.Substring(0, _maxLength);

            _text = value ?? string.Empty;
            OnTextChanged(EventArgs.Empty);
            UpdateScrollBars();
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
                Height = (int)(32 * ScaleFactor);
            }
            else
            {
                // When enabling multiline, enable scrollbars by default and allow auto-height adjustments
                if (!_showScrollbar)
                    _showScrollbar = true;
            }

            UpdateScrollBars();
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
            if (UseSystemPasswordChar) Invalidate();
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
            if (_maxLength > 0 && Text.Length > _maxLength) Text = Text.Substring(0, _maxLength);
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
    [DefaultValue(6.0f)]
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

    [Category("Appearance")]
    public Color SelectionBackColor
    {
        get => _selectionBackColor;
        set
        {
            if (_selectionBackColor == value) return;
            _selectionBackColor = value;
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
            if (value && MultiLine) UpdateAutoHeight();
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
            if (MultiLine) UpdateAutoHeight();
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
            if (MultiLine) UpdateAutoHeight();
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
            UpdateScrollBars();
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
            if (value) ScrollToCaret();
        }
    }

    [Category("Behavior")]
    [DefaultValue(1.0f)]
    public float ScrollSpeed
    {
        get => _scrollSpeed;
        set => _scrollSpeed = Math.Max(0.1f, Math.Min(10.0f, value));
    }

    /// <summary>
    ///     Returns the zero-based index of the first character in the specified line.
    ///     Returns -1 if the lineIndex is out of range.
    /// </summary>
    public int GetFirstCharIndexFromLine(int lineIndex)
    {
        if (lineIndex < 0) return -1;
        var lines = Lines;
        if (lineIndex >= lines.Length) return -1;
        var index = 0;
        for (var i = 0; i < lineIndex; i++)
            // each line ended with a single '\n' after normalization
            index += lines[i].Length + 1;
        return Math.Min(index, Text.Length);
    }

    private void InitializeThemeBinding()
    {
        _themeForeColorSnapshot = ColorScheme.ForeColor;
        _themeBorderColorSnapshot = ColorScheme.BorderColor;
        _themeFocusedBorderColorSnapshot = ColorScheme.AccentColor;
        _themeSelectionColorSnapshot = ColorScheme.AccentColor.Alpha(90);

        // Ensure our defaults are aligned with the current theme
        if (ForeColor == default)
            ForeColor = _themeForeColorSnapshot;
        if (BorderColor == default)
            BorderColor = _themeBorderColorSnapshot;
        if (FocusedBorderColor == default)
            FocusedBorderColor = _themeFocusedBorderColorSnapshot;

        ColorScheme.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        var newFore = ColorScheme.ForeColor;
        if (ForeColor == _themeForeColorSnapshot)
            ForeColor = newFore;
        _themeForeColorSnapshot = newFore;

        var newBorder = ColorScheme.BorderColor;
        if (BorderColor == _themeBorderColorSnapshot)
            BorderColor = newBorder;
        _themeBorderColorSnapshot = newBorder;

        var newFocusedBorder = ColorScheme.AccentColor;
        if (FocusedBorderColor == _themeFocusedBorderColorSnapshot)
            FocusedBorderColor = newFocusedBorder;
        _themeFocusedBorderColorSnapshot = newFocusedBorder;

        var newSelection = ColorScheme.AccentColor.Alpha(90);
        if (SelectionColor == _themeSelectionColorSnapshot)
            SelectionColor = newSelection;
        _themeSelectionColorSnapshot = newSelection;

        Invalidate();
    }

    private void InitializeFocusAnimation()
    {
        _focusAnimation = new AnimationManager()
        {
            // Keep this snappy; AnimationManager computes duration from Increment.
            Increment = 0.2,
            AnimationType = AnimationType.EaseOut,
            InterruptAnimation = true
        };
        _focusAnimation.OnAnimationProgress += _ => Invalidate();
        _focusAnimation.SetProgress(Focused ? 1.0 : 0.0);
    }

    private void InitializeCursorBlink()
    {
        var blinkTime = SystemInformation.CaretBlinkTime;
        // Eğer blink time -1 ise (blink disabled), default 530ms kullan
        if (blinkTime <= 0)
            blinkTime = 530;

        _cursorBlinkTimer = new Timer
        {
            Interval = blinkTime,
            Enabled = false
        };
        _cursorBlinkTimer.Tick += CursorBlinkTimer_Tick;
    }

    private void CursorBlinkTimer_Tick(object sender, EventArgs e)
    {
        if (!Focused)
            return;

        _showCursor = !_showCursor;
        Invalidate();
    }

    private void InitializeScrollBars()
    {
        _verticalScrollBar = new ScrollBar
        {
            Orientation = Orientation.Vertical,
            Visible = false,
            Width = 12,
            TabStop = false,
            CanSelect = false
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
            Height = 12,
            TabStop = false,
            CanSelect = false
        };
        _horizontalScrollBar.ValueChanged += (s, e) => Invalidate();

        Controls.Add(_verticalScrollBar);
        Controls.Add(_horizontalScrollBar);
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
        if (_selectionLength > 0) Clipboard.SetText(Text.Substring(_selectionStart, _selectionLength));
    }

    public void Paste()
    {
        if (!ReadOnly && Clipboard.ContainsText())
        {
            var clipText = Clipboard.GetText();
            if (!MultiLine) clipText = clipText.Replace("\r\n", "").Replace("\n", "");

            if (_selectionLength > 0) DeleteSelection();

            if (MaxLength > 0)
            {
                var remainingLength = MaxLength - Text.Length + _selectionLength;
                if (clipText.Length > remainingLength) clipText = clipText.Substring(0, remainingLength);
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

        _focusAnimation?.StartNewAnimation(AnimationDirection.In);

        if (UseSystemPasswordChar && PassFocusShow)
        {
            _useSystemPasswordChar = false;
            Invalidate();
        }

        _showCursor = true;
        _cursorBlinkTimer?.Start();
        Invalidate();
    }

    internal override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);

        _focusAnimation?.StartNewAnimation(AnimationDirection.Out);

        if (UseSystemPasswordChar && PassFocusShow)
        {
            _useSystemPasswordChar = true;
            Invalidate();
        }

        _cursorBlinkTimer?.Stop();
        _showCursor = false;
        Invalidate();
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        _isDragging = false;

        if (e.Button == MouseButtons.Right)
        {
            if (!Focused)
                Focus();

            // Prefer the standard SDUI ContextMenuStrip if assigned (walk up the parent chain).
            var point = PointToScreen(e.Location);
            UIElementBase? current = this;
            while (current != null)
            {
                if (current.ContextMenuStrip != null)
                {
                    current.ContextMenuStrip.Show(this, point);
                    return;
                }

                current = current.Parent as UIElementBase;
            }

            // Fallback to SDUI ContextMenuStrip when no custom menu is assigned.
            var menu = new ContextMenuStrip();
            var hasSelection = _selectionLength > 0;
            var canPaste = Clipboard.ContainsText();
            var isReadOnly = ReadOnly;

            var miCut = new MenuItem("Kes") { Enabled = hasSelection && !isReadOnly };
            miCut.Click += (_, _) => Cut();
            menu.Items.Add(miCut);

            var miCopy = new MenuItem("Kopyala") { Enabled = hasSelection };
            miCopy.Click += (_, _) => Copy();
            menu.Items.Add(miCopy);

            var miPaste = new MenuItem("Yapıştır") { Enabled = canPaste && !isReadOnly };
            miPaste.Click += (_, _) => Paste();
            menu.Items.Add(miPaste);

            var miDelete = new MenuItem("Sil") { Enabled = hasSelection && !isReadOnly };
            miDelete.Click += (_, _) =>
            {
                if (_selectionLength > 0) DeleteSelection();
            };
            menu.Items.Add(miDelete);

            menu.Items.Add(new MenuItem { IsSeparator = true });

            var miSelectAll = new MenuItem("Tümünü Seç") { Enabled = Text.Length > 0 };
            miSelectAll.Click += (_, _) => SelectAll();
            menu.Items.Add(miSelectAll);

            if (IsRich)
            {
                menu.Items.Add(new MenuItem { IsSeparator = true });
                var miBold = new MenuItem("Kalın") { Enabled = _selectionLength > 0 && !isReadOnly };
                miBold.Click += (_, _) => ToggleBoldSelection();
                menu.Items.Add(miBold);
                var miItalic = new MenuItem("İtalik") { Enabled = _selectionLength > 0 && !isReadOnly };
                miItalic.Click += (_, _) => ToggleItalicSelection();
                menu.Items.Add(miItalic);
            }

            menu.Show(this, point);
            return;
        }

        base.OnMouseUp(e);
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (e.Button == MouseButtons.Left && _isDragging) UpdateSelectionFromMousePosition(e.Location, true);

        if (ShowScrollbar && MultiLine)
        {
            var scrollBarBounds = new Rectangle(
                Width - 12,
                2,
                8,
                Height - 4);

            var wasHovered = _isScrollBarHovered;
            _isScrollBarHovered = scrollBarBounds.Contains(e.Location);

            if (wasHovered != _isScrollBarHovered) Invalidate();

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
        if (e.Button == MouseButtons.Right)
        {
            if (!Focused)
                Focus();
            _isDragging = false;
            _isDraggingScrollBar = false;
            return;
        }

        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Left)
        {
            // Always ensure control gets focus on click and reaffirm visuals.
            Focus();
            _focusAnimation?.StartNewAnimation(AnimationDirection.In);
            _isDragging = true;

            var extend = ModifierKeys.HasFlag(Keys.Shift);
            UpdateSelectionFromMousePosition(e.Location, extend);

            if (!extend)
            {
                // Normal click - set anchor to caret
                _selectionAnchor = _selectionStart;
                _selectionLength = 0;
            }
            else
            {
                // Ensure anchor exists
                if (_selectionAnchor < 0) _selectionAnchor = 0;
                var anchor = _selectionAnchor;
                var start = Math.Min(anchor, _selectionStart);
                _selectionLength = Math.Abs(_selectionStart - anchor);
                _selectionStart = start;
            }
        }

        if (ShowScrollbar && MultiLine && _isScrollBarHovered) _isDraggingScrollBar = true;
    }

    internal override void OnMouseDoubleClick(MouseEventArgs e)
    {
        // Basic expected TextBox behavior: double-click selects all.
        if (e.Button == MouseButtons.Left)
        {
            if (!Focused)
                Focus();

            SelectAll();
            return;
        }

        base.OnMouseDoubleClick(e);
    }

    internal override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Control)
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
        else
            switch (e.KeyCode)
            {
                case Keys.Space:
                    if (!ReadOnly && !e.Alt && !e.Control)
                    {
                        if (_selectionLength > 0)
                            DeleteSelection();

                        if (MaxLength > 0 && Text.Length + 1 > MaxLength)
                        {
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            break;
                        }

                        Text = Text.Insert(_selectionStart, " ");
                        _selectionStart++;
                        _selectionLength = 0;
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }

                    break;

                case Keys.Left when _selectionStart > 0:
                    SetCaret(_selectionStart - 1, e.Shift);
                    e.Handled = true;
                    break;

                case Keys.Right when _selectionStart < Text.Length:
                    SetCaret(_selectionStart + 1, e.Shift);
                    e.Handled = true;
                    break;

                case Keys.Home:
                    SetCaret(0, e.Shift);
                    e.Handled = true;
                    break;

                case Keys.End:
                    SetCaret(Text.Length, e.Shift);
                    e.Handled = true;
                    break;

                case Keys.Enter:
                    // If the textbox is multiline and accepts return, insert newline
                    if (MultiLine && AcceptsReturn && !ReadOnly)
                    {
                        if (_selectionLength > 0) DeleteSelection();

                        if (MaxLength > 0 && Text.Length + 1 > MaxLength)
                        {
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            break;
                        }

                        Text = Text.Insert(_selectionStart, "\n");
                        _selectionStart++;
                        _selectionLength = 0;
                        UpdateScrollBars();
                        if (AutoScroll) ScrollToCaret();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }

                    break;

                case Keys.Up:
                    if (MultiLine)
                    {
                        var font = GetDefaultSkFont();
                        var lines = GetTextLinesWithIndices(font);
                        var caret = _selectionStart + _selectionLength;
                        // find current line
                        var lineIndex = 0;
                        for (var i = 0; i < lines.Count; i++)
                            if (caret >= lines[i].StartIndex && caret <= lines[i].StartIndex + lines[i].Line.Length)
                            {
                                lineIndex = i;
                                break;
                            }

                        var curLine = lines[lineIndex];
                        var localPos = caret - curLine.StartIndex;
                        var tmp = new SKRect();
                        font.MeasureText(
                            curLine.Line.Substring(0, Math.Max(0, Math.Min(curLine.Line.Length, localPos))), out tmp);
                        _preferredCaretX = tmp.Width;

                        if (lineIndex > 0)
                        {
                            var target = lines[lineIndex - 1];
                            var bestIdx = 0;
                            var bestDiff = float.MaxValue;
                            for (var i = 0; i <= target.Line.Length; i++)
                            {
                                font.MeasureText(target.Line.Substring(0, i), out tmp);
                                var diff = Math.Abs(tmp.Width - (_preferredCaretX ?? 0));
                                if (diff < bestDiff)
                                {
                                    bestDiff = diff;
                                    bestIdx = i;
                                }
                            }

                            var dest = target.StartIndex + bestIdx;
                            SetCaret(dest, e.Shift);
                        }

                        e.Handled = true;
                    }

                    break;

                case Keys.Down:
                    if (MultiLine)
                    {
                        var font = GetDefaultSkFont();
                        var lines = GetTextLinesWithIndices(font);
                        var caret = _selectionStart + _selectionLength;
                        // find current line
                        var lineIndex = 0;
                        for (var i = 0; i < lines.Count; i++)
                            if (caret >= lines[i].StartIndex && caret <= lines[i].StartIndex + lines[i].Line.Length)
                            {
                                lineIndex = i;
                                break;
                            }

                        var curLine = lines[lineIndex];
                        var localPos = caret - curLine.StartIndex;
                        var tmp = new SKRect();
                        font.MeasureText(
                            curLine.Line.Substring(0, Math.Max(0, Math.Min(curLine.Line.Length, localPos))), out tmp);
                        _preferredCaretX = tmp.Width;

                        if (lineIndex < lines.Count - 1)
                        {
                            var target = lines[lineIndex + 1];
                            var bestIdx = 0;
                            var bestDiff = float.MaxValue;
                            for (var i = 0; i <= target.Line.Length; i++)
                            {
                                font.MeasureText(target.Line.Substring(0, i), out tmp);
                                var diff = Math.Abs(tmp.Width - (_preferredCaretX ?? 0));
                                if (diff < bestDiff)
                                {
                                    bestDiff = diff;
                                    bestIdx = i;
                                }
                            }

                            var dest = target.StartIndex + bestIdx;
                            SetCaret(dest, e.Shift);
                        }

                        e.Handled = true;
                    }

                    break;
                case Keys.Delete:
                    if (!ReadOnly)
                    {
                        if (_selectionLength > 0)
                            DeleteSelection();
                        else if (_selectionStart < Text.Length) Text = Text.Remove(_selectionStart, 1);
                    }

                    e.Handled = true;
                    break;
            }
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        var bounds = ClientRectangle;

        EnsureSkiaCaches();
        var font = GetDefaultSkFont();

        var focusProgress = (float)(_focusAnimation?.GetProgress() ?? (Focused ? 1.0 : 0.0));

        // 1. Draw Background with theme-aware surface color
        var surfaceColor = ResolveSurfaceColor(Enabled);
        _bgPaint!.Color = surfaceColor.ToSKColor();
        var backgroundRect = new SKRect(0, 0, bounds.Width, bounds.Height);
        canvas.DrawRoundRect(backgroundRect, CornerRadiusScaled, CornerRadiusScaled, _bgPaint);

        if (focusProgress > 0.001f)
        {
            _focusGlowPaint!.Color = FocusedBorderColor.Alpha((int)Math.Round(35 * focusProgress)).ToSKColor();

            var glowRect = new SKRect(0, 0, bounds.Width, bounds.Height);
            canvas.DrawRoundRect(glowRect, CornerRadiusScaled, CornerRadiusScaled, _focusGlowPaint);
        }

        // 2. Draw Border (simple, flat)
        var borderColor = BorderColor.ToSKColor().InterpolateColor(FocusedBorderColor.ToSKColor(), focusProgress)
            .ToColor();
        if (!Enabled) borderColor = borderColor.Brightness(ColorScheme.BackColor.IsDark() ? -0.05f : -0.1f);

        _borderPaint!.Color = borderColor.ToSKColor();
        var focusedStroke = Math.Max(2.0f * ScaleFactor, BorderWidthScaled + 0.5f * ScaleFactor);
        _borderPaint.StrokeWidth = BorderWidthScaled + (focusedStroke - BorderWidthScaled) * focusProgress;

        var inset = _borderPaint.StrokeWidth / 2;
        var borderRect = new SKRect(inset, inset, bounds.Width - inset, bounds.Height - inset);
        canvas.DrawRoundRect(borderRect, CornerRadiusScaled, CornerRadiusScaled, _borderPaint);

        // 5. Determine Text to Draw
        var displayText = UseSystemPasswordChar && !string.IsNullOrEmpty(Text)
            ? new string(PasswordChar, Text.Length)
            : Text;

        var isPlaceholder = string.IsNullOrEmpty(displayText) && !string.IsNullOrEmpty(PlaceholderText) && !Focused;
        var textToDraw = isPlaceholder ? PlaceholderText : displayText;

        var metrics = font.Metrics;

        // Draw single-line or multi-line text and selection/caret
        var activeTextColor = (Enabled ? ForeColor : ForeColor.Alpha(120)).ToSKColor();
        var placeholderColor = ColorScheme.ForeColor.Alpha(140).ToSKColor();

        if (!IsRich && !MultiLine)
        {
            // Single-line behavior (existing)
            var textBounds = new SKRect();
            if (!string.IsNullOrEmpty(textToDraw)) font.MeasureText(textToDraw, out textBounds);

            // Center vertically based on CapHeight for better visual alignment than full Ascent/Descent
            var capHeight = metrics.CapHeight > 0 ? metrics.CapHeight : -metrics.Ascent * 0.7f;
            var y = bounds.Height / 2f + capHeight / 2f; 
            
            // Account for horizontal scroll offset when computing X origin
            var hOffset = _horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0;
            var x = GetTextX(bounds.Width, textBounds.Width) - hOffset;

            if (!string.IsNullOrEmpty(textToDraw))
            {
                _textPaint!.Color = isPlaceholder ? placeholderColor : activeTextColor;
                TextRenderingHelper.DrawText(canvas, textToDraw, x, y, SKTextAlign.Left, font, _textPaint);
            }

            // Selection (single-line)
            if (!isPlaceholder && Focused && _selectionLength > 0)
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

                _selectionPaint!.Color =
                    (SelectionBackColor != Color.Empty ? SelectionBackColor : SelectionColor).ToSKColor();
                var selectionTop = y + metrics.Ascent;
                var selectionBottom = y + metrics.Descent;

                canvas.DrawRect(new SKRect(selectionStartX, selectionTop,
                    selectionStartX + selectedBounds.Width,
                    selectionBottom), _selectionPaint);
            }

            // Cursor (single-line)
            if (Focused && _showCursor && _selectionLength == 0)
            {
                _cursorPaint!.Color = ForeColor.ToSKColor();
                _cursorPaint.StrokeWidth = CaretWidthScaled;

                var cursorX = GetTextX(bounds.Width, textBounds.Width) - hOffset;
                if (!string.IsNullOrEmpty(displayText))
                {
                    var preText = displayText.Substring(0, Math.Min(_selectionStart, displayText.Length));
                    var preBounds = new SKRect();
                    font.MeasureText(preText, out preBounds);
                    cursorX += preBounds.Width;
                }

                var cursorTop = bounds.Height / 2 - (metrics.Ascent + metrics.Descent) / 2 + metrics.Ascent;
                var cursorBottom = bounds.Height / 2 - (metrics.Ascent + metrics.Descent) / 2 + metrics.Descent;

                // Ensure caret is not drawn off the left edge (can happen when scrollbar value > text origin)
                cursorX = Math.Max(cursorX, Padding.Left - hOffset);

                canvas.DrawLine(new SKPoint(cursorX, cursorTop), new SKPoint(cursorX, cursorBottom), _cursorPaint);
            }
        }
        else if (!IsRich && MultiLine)
        {
            // Multi-line rendering (non-rich)
            var lines = GetTextLines(font);
            var lineHeight = font.Size + LineSpacing;
            var richY = Padding.Top - _scrollPosition;
            var xOffset = Padding.Left - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0);

            _textPaint!.Color = isPlaceholder ? placeholderColor : activeTextColor;

            // Draw lines
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                // Use -Ascent to properly place baseline from the top
                var drawY = richY + i * lineHeight - metrics.Ascent;
                if (drawY + metrics.Descent < 0)
                    continue;
                if (drawY + metrics.Ascent > bounds.Height)
                    break;

                TextRenderingHelper.DrawText(canvas, line, xOffset, drawY, font, _textPaint);
            }

            // Selection
            if (!isPlaceholder && Focused && _selectionLength > 0)
            {
                var selStart = _selectionStart;
                var selEnd = _selectionStart + _selectionLength;
                var linesWithIdx = GetTextLinesWithIndices(font);
                foreach (var item in linesWithIdx)
                {
                    var lineText = item.Line;
                    var lineStart = item.StartIndex;
                    var lineEnd = lineStart + lineText.Length;

                    var overlapStart = Math.Max(selStart, lineStart);
                    var overlapEnd = Math.Min(selEnd, lineEnd);

                    if (overlapStart < overlapEnd)
                    {
                        var localStart = overlapStart - lineStart;
                        var localLen = overlapEnd - overlapStart;

                        var preText = lineText.Substring(0, localStart);
                        var selText = lineText.Substring(localStart, localLen);

                        var preBounds = new SKRect();
                        var selBounds = new SKRect();
                        font.MeasureText(preText, out preBounds);
                        font.MeasureText(selText, out selBounds);

                        var top = Padding.Top - _scrollPosition + linesWithIdx.IndexOf(item) * lineHeight +
                                  metrics.Ascent;
                        var bottom = top + (metrics.Descent - metrics.Ascent);

                        _selectionPaint!.Color =
                            (SelectionBackColor != Color.Empty ? SelectionBackColor : SelectionColor).ToSKColor();
                        canvas.DrawRect(
                            new SKRect(xOffset + preBounds.Width, top, xOffset + preBounds.Width + selBounds.Width,
                                top + (metrics.Descent - metrics.Ascent)), _selectionPaint);
                    }
                }
            }

            // Cursor
            if (Focused && _showCursor && _selectionLength == 0)
            {
                var caretIndex = Math.Min(_selectionStart, Text.Length);
                var linesWithIdx = GetTextLinesWithIndices(font);
                var caretLineIdx = 0;
                for (var i = 0; i < linesWithIdx.Count; i++)
                {
                    var s = linesWithIdx[i];
                    if (caretIndex >= s.StartIndex && caretIndex <= s.StartIndex + s.Line.Length)
                    {
                        caretLineIdx = i;
                        break;
                    }
                }

                var caretLine = linesWithIdx[caretLineIdx];
                var localPos = caretIndex - caretLine.StartIndex;
                var preText = caretLine.Line.Substring(0, localPos);
                var preBounds = new SKRect();
                font.MeasureText(preText, out preBounds);

                var cursorX = xOffset + preBounds.Width;
                var cursorTop = Padding.Top - _scrollPosition + caretLineIdx * (font.Size + LineSpacing) +
                                metrics.Ascent;
                var cursorBottom = cursorTop + (metrics.Descent - metrics.Ascent);

                _cursorPaint!.Color = ForeColor.ToSKColor();
                _cursorPaint.StrokeWidth = CaretWidthScaled;
                canvas.DrawLine(new SKPoint(cursorX, cursorTop), new SKPoint(cursorX, cursorBottom), _cursorPaint);
            }
        }

        // 10. CharCount
        if (ShowCharCount)
        {
            var countText = MaxLength > 0 ? $"{Text.Length}/{MaxLength}" : Text.Length.ToString();
            var countFont = GetCharCountSkFont();
            _charCountPaint!.Color = CharCountColor.ToSKColor();
            var countBounds = new SKRect();
            countFont.MeasureText(countText, out countBounds);
            TextRenderingHelper.DrawText(
                canvas,
                countText,
                bounds.Width - countBounds.Width - 5,
                bounds.Height - 5,
                countFont,
                _charCountPaint);
        }

        // 11. Scrollbars
        if (ShowScrollbar && MultiLine)
        {
            var scrollBarBounds = new SKRect(
                bounds.Width - 12,
                2,
                bounds.Width - 4,
                bounds.Height - 4);

            _scrollbarPaint!.Color = (_isScrollBarHovered ? ScrollBarHoverColor : ScrollBarColor).ToSKColor();
            canvas.DrawRoundRect(scrollBarBounds, 4, 4, _scrollbarPaint);
        }

        // 12. Rich Text (span-based)
        if (IsRich && !string.IsNullOrEmpty(Text))
        {
            var lines = GetTextLinesWithIndices(font);
            var richY = Padding.Top - _scrollPosition;
            var lineHeight = font.Size + LineSpacing;

            for (var idx = 0; idx < lines.Count; idx++)
            {
                var item = lines[idx];
                var line = item.Line;
                var lineStart = item.StartIndex;

                // Compute baseline (top-aligned): padding + line index * lineHeight - ascent (positive shift), adjusted by scroll
                var top = Padding.Top - _scrollPosition + idx * lineHeight - metrics.Ascent;
                var lineTop = top + metrics.Ascent;

                // Visibility checks
                if (lineTop + lineHeight < 0)
                    // line is above viewport
                    continue;

                if (lineTop > bounds.Height)
                    break;

                // Draw selection for this line (if any)
                if (Focused && _selectionLength > 0)
                {
                    var selStart = _selectionStart;
                    var selEnd = _selectionStart + _selectionLength;

                    var overlapStart = Math.Max(selStart, lineStart);
                    var overlapEnd = Math.Min(selEnd, lineStart + line.Length);

                    if (overlapStart < overlapEnd)
                    {
                        var preText = line.Substring(0, overlapStart - lineStart);
                        var selText = line.Substring(overlapStart - lineStart, overlapEnd - overlapStart);

                        var preBounds = new SKRect();
                        var selBounds = new SKRect();
                        font.MeasureText(preText, out preBounds);
                        font.MeasureText(selText, out selBounds);

                        _selectionPaint!.Color =
                            (SelectionBackColor != Color.Empty ? SelectionBackColor : SelectionColor).ToSKColor();
                        canvas.DrawRect(
                            new SKRect(
                                Padding.Left - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0) +
                                preBounds.Width, top,
                                Padding.Left - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0) +
                                preBounds.Width + selBounds.Width, top + (metrics.Descent - metrics.Ascent)),
                            _selectionPaint);
                    }
                }

                // Draw text in runs according to spans
                var pos = 0;
                while (pos < line.Length)
                {
                    var globalPos = lineStart + pos;
                    var span = _styleSpans.FirstOrDefault(s => s.Start <= globalPos && s.Start + s.Length > globalPos);
                    int segLen;
                    TextStyle? segStyle = null;

                    if (span != null)
                    {
                        segStyle = span.Style;
                        segLen = Math.Min(line.Length - pos, span.Start + span.Length - globalPos);
                    }
                    else
                    {
                        var nextSpanStart = _styleSpans
                            .Where(s => s.Start > globalPos && s.Start < lineStart + line.Length).Select(s => s.Start)
                            .DefaultIfEmpty(lineStart + line.Length).Min();
                        segLen = nextSpanStart - globalPos;
                    }

                    var segText = line.Substring(pos, segLen);
                    SKFont segFont;
                    if (segStyle != null && segStyle.IsBold)
                        segFont = GetBoldSkFont();
                    else if (segStyle != null && segStyle.IsItalic)
                        segFont = GetItalicSkFont();
                    else
                        segFont = font;
                    _richTextPaint!.Color = segStyle != null ? segStyle.Color.ToSKColor() : ForeColor.ToSKColor();

                    var xPos = Padding.Left - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0);
                    var preBounds = new SKRect();
                    font.MeasureText(line.Substring(0, pos), out preBounds);

                    TextRenderingHelper.DrawText(
                        canvas,
                        segText,
                        xPos + preBounds.Width,
                        top,
                        segFont,
                        _richTextPaint);

                    pos += segLen;
                }

                richY += lineHeight;
            }

            // reset rich paint color
            _richTextPaint!.Color = ForeColor.ToSKColor();
        }
    }

    private void EnsureSkiaCaches()
    {
        _bgPaint ??= new SKPaint { IsAntialias = true };
        _focusGlowPaint ??= new SKPaint { IsAntialias = true };

        _borderPaint ??= new SKPaint
        {
            IsAntialias = true,
            IsStroke = true
        };

        _textPaint ??= new SKPaint { IsAntialias = true };
        _selectionPaint ??= new SKPaint { IsAntialias = true };
        _cursorPaint ??= new SKPaint { IsAntialias = true };
        _charCountPaint ??= new SKPaint { IsAntialias = true };
        _scrollbarPaint ??= new SKPaint { IsAntialias = true };
        _richTextPaint ??= new SKPaint { IsAntialias = true };
    }

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
                Hinting = SKFontHinting.Full,
                Edging = SKFontEdging.SubpixelAntialias,
                Subpixel = true
            };
            _defaultSkFontSource = Font;
            _defaultSkFontDpi = dpi;
        }

        return _defaultSkFont;
    }

    private SKFont GetCharCountSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        var baseSize = Math.Max(1f, Font.Size - 2f);
        if (_charCountSkFont == null || !ReferenceEquals(_charCountSkFontSource, Font) || _charCountSkFontDpi != dpi ||
            Math.Abs(_charCountSkFontBaseSize - baseSize) > 0.001f)
        {
            _charCountSkFont?.Dispose();
            _charCountSkFont = new SKFont
            {
                Size = baseSize.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true
            };
            _charCountSkFontSource = Font;
            _charCountSkFontDpi = dpi;
            _charCountSkFontBaseSize = baseSize;
        }

        return _charCountSkFont;
    }

    private SKFont GetBoldSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;

        if (_boldSystemFont == null || !ReferenceEquals(_boldSystemFontSource, Font))
        {
            _boldSystemFont?.Dispose();
            _boldSystemFont = new Font(Font, FontStyle.Bold);
            _boldSystemFontSource = Font;
        }

        if (_boldSkFont == null || !ReferenceEquals(_boldSkFontSource, Font) || _boldSkFontDpi != dpi)
        {
            _boldSkFont?.Dispose();
            _boldSkFont = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(_boldSystemFont),
                Hinting = SKFontHinting.Full,
                Edging = SKFontEdging.SubpixelAntialias,
                Subpixel = true
            };
            _boldSkFontSource = Font;
            _boldSkFontDpi = dpi;
        }

        return _boldSkFont;
    }

    private SKFont GetItalicSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;

        if (_italicSystemFont == null || !ReferenceEquals(_italicSystemFontSource, Font))
        {
            _italicSystemFont?.Dispose();
            _italicSystemFont = new Font(Font, FontStyle.Italic);
            _italicSystemFontSource = Font;
        }

        if (_italicSkFont == null || !ReferenceEquals(_italicSkFontSource, Font) || _italicSkFontDpi != dpi)
        {
            _italicSkFont?.Dispose();
            _italicSkFont = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(_italicSystemFont),
                Hinting = SKFontHinting.Full,
                Edging = SKFontEdging.SubpixelAntialias,
                Subpixel = true
            };
            _italicSkFontSource = Font;
            _italicSkFontDpi = dpi;
        }

        return _italicSkFont;
    }

    private Color ResolveSurfaceColor(bool enabled)
    {
        var baseColor = BackColor == Color.Transparent ? GetThemedSurfaceColor() : BackColor;
        if (enabled)
            return baseColor;

        var adjustment = ColorScheme.BackColor.IsDark() ? -0.04f : -0.08f;
        return baseColor.Brightness(adjustment);
    }

    private Color GetThemedSurfaceColor()
    {
        // Use the shared theme primitives so the TextBox background tracks theme transitions.
        return ColorScheme.Surface;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_cursorBlinkTimer != null)
            {
                _cursorBlinkTimer.Stop();
                _cursorBlinkTimer.Tick -= CursorBlinkTimer_Tick;
                _cursorBlinkTimer.Dispose();
            }

            _focusAnimation?.Dispose();
            ColorScheme.ThemeChanged -= OnThemeChanged;

            _bgPaint?.Dispose();
            _bgPaint = null;
            _focusGlowPaint?.Dispose();
            _focusGlowPaint = null;
            _borderPaint?.Dispose();
            _borderPaint = null;
            _textPaint?.Dispose();
            _textPaint = null;
            _selectionPaint?.Dispose();
            _selectionPaint = null;
            _cursorPaint?.Dispose();
            _cursorPaint = null;
            _charCountPaint?.Dispose();
            _charCountPaint = null;
            _scrollbarPaint?.Dispose();
            _scrollbarPaint = null;
            _richTextPaint?.Dispose();
            _richTextPaint = null;

            _defaultSkFont?.Dispose();
            _defaultSkFont = null;
            _charCountSkFont?.Dispose();
            _charCountSkFont = null;
            _boldSkFont?.Dispose();
            _boldSkFont = null;
            _boldSystemFont?.Dispose();
            _boldSystemFont = null;
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

        // Backspace should delete selection too (SelectAll -> Backspace)
        if (e.KeyChar == '\b')
        {
            if (_selectionLength > 0)
            {
                DeleteSelection();
            }
            else if (_selectionStart > 0)
            {
                Text = Text.Remove(_selectionStart - 1, 1);
                _selectionStart--;
            }

            _selectionLength = 0;
            e.Handled = true;
            base.OnKeyPress(e);
            return;
        }

        // Honor AcceptsTab / AcceptsReturn at the character level
        if (e.KeyChar == '\t' && !AcceptsTab)
        {
            e.Handled = true;
            base.OnKeyPress(e);
            return;
        }

        if ((e.KeyChar == '\r' || e.KeyChar == '\n') && (!AcceptsReturn || !MultiLine))
        {
            e.Handled = true;
            base.OnKeyPress(e);
            return;
        }

        if (e.KeyChar == '\r')
            e.KeyChar = '\n';

        var isInsertable = !char.IsControl(e.KeyChar)
                           || (e.KeyChar == '\t' && AcceptsTab)
                           || (e.KeyChar == '\n' && AcceptsReturn && MultiLine);

        if (isInsertable)
        {
            // Replace selection before inserting a character
            if (_selectionLength > 0)
                DeleteSelection();

            if (MaxLength > 0 && Text.Length + 1 > MaxLength)
            {
                e.Handled = true;
                return;
            }

            var newText = Text.Insert(_selectionStart, e.KeyChar.ToString());
            Text = newText;
            _selectionStart++;
            _selectionLength = 0;
            e.Handled = true;

            // When inserting a newline ensure scrollbars are updated and caret visible
            if (e.KeyChar == '\n')
            {
                UpdateScrollBars();
                if (AutoScroll)
                    ScrollToCaret();
            }
        }

        base.OnKeyPress(e);
    }

    private void UpdateSelectionFromMousePosition(Point location, bool extend)
    {
        var font = GetDefaultSkFont();
        var linesWithIdx = GetTextLinesWithIndices(font);
        var lineHeight = font.Size + LineSpacing;
        var y = location.Y - Padding.Top + (int)_scrollPosition;
        var lineIndex = Math.Max(0, Math.Min(linesWithIdx.Count - 1, (int)(y / lineHeight)));

        if (linesWithIdx.Count == 0) return;

        var line = linesWithIdx[lineIndex];

        // Determine X alignment offset for this line
        // NOTE: Currently GetTextX only supports single-line logic (bounds vs text width).
        // For MultiLine/Rich, we generally assume Left alignment or would need per-line alignment support.
        // But for SingleLine, we MUST match OnPaint's alignment.
        float xOffset;
        if (!IsRich && !MultiLine)
        {
            var textBounds = new SKRect();
            font.MeasureText(Text, out textBounds);
            // Match OnPaint: x = GetTextX(...) - hOffset
            xOffset = GetTextX(ClientRectangle.Width, textBounds.Width) - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0);
        }
        else
        {
            // Match OnPaint MultiLine/Rich: just Padding.Left - hOffset
            xOffset = Padding.Left - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Value : 0);
        }
        
        // Relative X click position
        var clickX = location.X - xOffset;

        var charIndexInLine = 0;
        for (var i = 0; i <= line.Line.Length; i++)
        {
            var textPart = line.Line.Substring(0, i);
            var bounds = new SKRect();
            font.MeasureText(textPart, out bounds);

            // If clickX is within this character's bounds (or close enough)
            if (bounds.Width >= clickX || i == line.Line.Length)
            {
                // Simple hit test: if we are closer to the previous char end, pick previous? 
                // For now, simple geometric cut-off like original code
                charIndexInLine = i;
                break;
            }
        }
        
        var caretIndex = line.StartIndex + charIndexInLine;

        if (!extend)
        {
            _selectionAnchor = caretIndex;
            _selectionStart = caretIndex;
            _selectionLength = 0;
        }
        else
        {
            if (_selectionAnchor < 0)
                _selectionAnchor = _selectionStart;

            var start = Math.Min(_selectionAnchor, caretIndex);
            _selectionStart = start;
            _selectionLength = Math.Abs(caretIndex - _selectionAnchor);
        }

        Invalidate();
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
            HorizontalAlignment.Center => Padding.Left + (boundsWidth - Padding.Horizontal - textWidth) / 2,
            HorizontalAlignment.Right => boundsWidth - Padding.Right - textWidth,
            _ => Padding.Left
        };
    }

    private void SetCaret(int caretIndex, bool extend)
    {
        caretIndex = Math.Max(0, Math.Min(Text.Length, caretIndex));
        if (!extend)
        {
            _selectionAnchor = caretIndex;
            _selectionStart = caretIndex;
            _selectionLength = 0;
        }
        else
        {
            if (_selectionAnchor < 0)
                _selectionAnchor = _selectionStart;

            var start = Math.Min(_selectionAnchor, caretIndex);
            _selectionStart = start;
            _selectionLength = Math.Abs(caretIndex - _selectionAnchor);
        }

        Invalidate();
    }

    private void UpdateAutoHeight()
    {
        if (!AutoHeight || !MultiLine) return;

        var font = GetDefaultSkFont();
        var lines = GetTextLines(font);
        var lineHeight = font.Size + LineSpacing;
        var newHeight = (int)(lines.Count * lineHeight) + Padding.Vertical + 4;

        if (Height != newHeight) Height = newHeight;
    }

    private List<string> GetTextLines(SKFont font)
    {
        // Simple wrapper that returns only the line text; uses the richer indexed implementation.
        var indexed = GetTextLinesWithIndices(font);
        var lines = new List<string>(indexed.Count);
        foreach (var it in indexed)
            lines.Add(it.Line);
        return lines;
    }

    private List<(string Line, int StartIndex)> GetTextLinesWithIndices(SKFont font)
    {
        var result = new List<(string, int)>();
        if (string.IsNullOrEmpty(Text))
        {
            result.Add((string.Empty, 0));
            return result;
        }

        var availableWidth = Math.Max(10, Width - Padding.Horizontal - (ShowScrollbar ? 20 : 0));
        var pos = 0;
        var text = Text;

        while (pos < text.Length)
        {
            var nl = text.IndexOf('\n', pos);
            var paragraphLength = nl >= 0 ? nl - pos : text.Length - pos;
            var paragraph = text.Substring(pos, paragraphLength);

            if (!WordWrap || paragraph.Length == 0)
            {
                // Add paragraph as-is
                result.Add((paragraph, pos));
            }
            else
            {
                var pIdx = 0;
                while (pIdx < paragraph.Length)
                {
                    var start = pIdx;
                    var lastFit = start;
                    var sb = new StringBuilder();
                    for (; pIdx < paragraph.Length; pIdx++)
                    {
                        sb.Append(paragraph[pIdx]);
                        var bounds = new SKRect();
                        font.MeasureText(sb.ToString(), out bounds);
                        if (bounds.Width > availableWidth)
                        {
                            // If nothing fits (very narrow), force at least one char
                            if (sb.Length == 1)
                            {
                                lastFit = start + 1;
                                pIdx = start + 1;
                            }
                            else
                            {
                                lastFit = pIdx - 1;
                            }

                            break;
                        }

                        lastFit = pIdx + 1;
                    }

                    if (lastFit <= start)
                        lastFit = start + 1;

                    var line = paragraph.Substring(start, lastFit - start);
                    result.Add((line, pos + start));

                    if (pIdx >= paragraph.Length)
                        break;
                }
            }

            pos = nl >= 0 ? nl + 1 : text.Length;

            // If paragraph ended with an explicit newline and it was empty, preserve it
            if (nl >= 0 && paragraph.Length == 0) result.Add((string.Empty, pos - 1));
        }

        return result;
    }

    internal override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (MultiLine && _verticalScrollBar.Visible)
        {
            var delta = e.Delta / 120f * _verticalScrollBar.SmallChange * ScrollSpeed;
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
        if (_verticalScrollBar.Visible) _verticalScrollBar.Value = _verticalScrollBar.Maximum;
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

    public void ApplyStyleToSelection(TextStyle style)
    {
        if (_selectionLength <= 0) return;
        _styleSpans.Add(new TextStyleSpan(_selectionStart, _selectionLength, style));
        Invalidate();
    }

    public void RemoveStylesInSelection()
    {
        if (_selectionLength <= 0) return;
        var selStart = _selectionStart;
        var selEnd = _selectionStart + _selectionLength;
        _styleSpans.RemoveAll(s => s.Start < selEnd && s.Start + s.Length > selStart);
        Invalidate();
    }

    public void ToggleBoldSelection()
    {
        if (_selectionLength <= 0) return;
        var selStart = _selectionStart;
        var selEnd = _selectionStart + _selectionLength;
        var hasBold = _styleSpans.Any(s => s.Start < selEnd && s.Start + s.Length > selStart && s.Style.IsBold);
        if (hasBold)
        {
            _styleSpans.RemoveAll(s => s.Start < selEnd && s.Start + s.Length > selStart && s.Style.IsBold);
        }
        else
        {
            var style = new TextStyle(string.Empty, ForeColor, true);
            _styleSpans.Add(new TextStyleSpan(selStart, _selectionLength, style));
        }

        Invalidate();
    }

    public void ToggleItalicSelection()
    {
        if (_selectionLength <= 0) return;
        var selStart = _selectionStart;
        var selEnd = _selectionStart + _selectionLength;
        var hasItalic = _styleSpans.Any(s => s.Start < selEnd && s.Start + s.Length > selStart && s.Style.IsItalic);
        if (hasItalic)
        {
            _styleSpans.RemoveAll(s => s.Start < selEnd && s.Start + s.Length > selStart && s.Style.IsItalic);
        }
        else
        {
            var style = new TextStyle(string.Empty, ForeColor, false, true);
            _styleSpans.Add(new TextStyleSpan(selStart, _selectionLength, style));
        }

        Invalidate();
    }

    public void SetSelectionColor(Color color)
    {
        if (_selectionLength <= 0) return;
        var selStart = _selectionStart;
        var selEnd = _selectionStart + _selectionLength;
        _styleSpans.RemoveAll(s => s.Start < selEnd && s.Start + s.Length > selStart && s.Style.Color != color);
        var style = new TextStyle(string.Empty, color);
        _styleSpans.Add(new TextStyleSpan(selStart, _selectionLength, style));
        Invalidate();
    }

    public void Clear()
    {
        Text = string.Empty;
        _styleSpans.Clear();
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

        var font = GetDefaultSkFont();
        var lines = GetTextLines(font);
        var totalHeight = lines.Count * (font.Size + LineSpacing);
        var maxWidth = 0f;

        foreach (var line in lines)
        {
            var tbounds = new SKRect();
            font.MeasureText(line, out tbounds);
            maxWidth = Math.Max(maxWidth, tbounds.Width);
        }

        _maxScroll = Math.Max(0f, totalHeight - Height);
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

        _verticalScrollBar.Visible = showVertical && ShowScrollbar;
        _horizontalScrollBar.Visible = showHorizontal && ShowScrollbar;
    }
}

public class TextStyle
{
    public TextStyle(string pattern, Color color, bool isBold = false, bool isItalic = false)
    {
        Pattern = pattern;
        Color = color;
        IsBold = isBold;
        IsItalic = isItalic;
    }

    public string Pattern { get; set; }
    public Color Color { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
}

public class TextStyleSpan
{
    public TextStyleSpan(int start, int length, TextStyle style)
    {
        Start = start;
        Length = length;
        Style = style;
    }

    public int Start { get; set; }
    public int Length { get; set; }
    public TextStyle Style { get; set; }
}