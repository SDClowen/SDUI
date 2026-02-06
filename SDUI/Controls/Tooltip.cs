using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Windows.Forms;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class ToolTip : UIElementBase
{
    private readonly SKColor _borderColor = new SKColor(100, 100, 100);
    private readonly int _cornerRadius = 6;
    private readonly int _maxWidth = 300;
    private readonly int _padding = 8;
    private readonly int _shadowBlur = 10;
    private readonly int _shadowOffsetY = 3;
    private readonly float _shadowOpacity = 0.2f;
    private readonly Dictionary<UIElementBase, string> _tooltips = new();
    private int _autoPopDelay = 5000;
    private SKPaint? _bgPaint;
    private SKPaint? _borderPaint;
    private UIElementBase _currentControl;
    private UIWindow _currentWindow;

    // Cached Skia resources (avoid per-frame allocations)
    private SKFont? _defaultSkFont;
    private int _defaultSkFontDpi;
    private Font? _defaultSkFontSource;
    private bool _handlersAttached;
    private Timer _hideTimer;
    private bool _isActive;
    private bool _isBalloon = true;

    private SKImageFilter? _shadowImageFilter;
    private int _shadowImageFilterBlur;

    private SKPaint? _shadowPaint;
    private int _showDelay = 500;
    private Timer _showTimer;
    private string _text = string.Empty;
    private SKPaint? _textPaint;
    private EventHandler _windowDeactivateHandler;
    private KeyEventHandler _windowKeyDownHandler;
    private MouseEventHandler _windowMouseDownHandler;

    public ToolTip()
    {
        BackColor = new SKColor(50, 50, 50);
        ForeColor = SKColor.White;
        Visible = false;
        AutoSize = false;
        TabStop = false;

        _showTimer = new Timer { Interval = ShowDelay };
        _showTimer.Tick += ShowTimer_Tick;

        _hideTimer = new Timer { Interval = AutomaticDelay };
        _hideTimer.Tick += HideTimer_Tick;
    }

    public ToolTip(IContainer container) : this()
    {
    }

    [DefaultValue("")]
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            UpdateSize();
            Invalidate();
        }
    }

    [DefaultValue(500)]
    public int ShowDelay
    {
        get => _showDelay;
        set
        {
            _showDelay = Math.Max(0, value);
            if (_showTimer != null)
                _showTimer.Interval = _showDelay;
        }
    }

    [DefaultValue(5000)]
    public int AutomaticDelay
    {
        get => _autoPopDelay;
        set
        {
            _autoPopDelay = Math.Max(0, value);
            if (_hideTimer != null)
                _hideTimer.Interval = _autoPopDelay;
        }
    }

    [DefaultValue(true)]
    public bool IsBalloon
    {
        get => _isBalloon;
        set
        {
            if (_isBalloon == value) return;
            _isBalloon = value;
            Invalidate();
        }
    }

    public void SetToolTip(UIElementBase control, string text)
    {
        if (control == null) return;

        if (string.IsNullOrEmpty(text))
        {
            _tooltips.Remove(control);
            UnsubscribeFromControl(control);
        }
        else
        {
            _tooltips[control] = text;
            SubscribeToControl(control);
        }
    }

    private void SubscribeToControl(UIElementBase control)
    {
        control.MouseEnter += Control_MouseEnter;
        control.MouseLeave += Control_MouseLeave;
        control.MouseMove += Control_MouseMove;
    }

    private void UnsubscribeFromControl(UIElementBase control)
    {
        control.MouseEnter -= Control_MouseEnter;
        control.MouseLeave -= Control_MouseLeave;
        control.MouseMove -= Control_MouseMove;
    }

    private void Control_MouseEnter(object sender, EventArgs e)
    {
        if (sender is UIElementBase control && _tooltips.TryGetValue(control, out var text))
        {
            _currentControl = control;
            Text = text;
            StartShowTimer();
        }
    }

    private void Control_MouseLeave(object sender, EventArgs e)
    {
        StopShowTimer();
        Hide();
    }

    private void Control_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isActive && sender is UIElementBase control)
        {
            var screenPoint = control.PointToScreen(e.Location);
            UpdatePosition(screenPoint);
        }
    }

    private void ShowTimer_Tick(object sender, EventArgs e)
    {
        _showTimer.Stop();
        Show();
    }

    private void HideTimer_Tick(object sender, EventArgs e)
    {
        _hideTimer.Stop();
        Hide();
    }

    private void StartShowTimer()
    {
        StopShowTimer();
        StopHideTimer();
        _showTimer.Start();
    }

    private void StopShowTimer()
    {
        _showTimer.Stop();
    }

    private void StopHideTimer()
    {
        _hideTimer.Stop();
    }

    public void Show(UIElementBase control, string text)
    {
        if (control == null) return;

        _currentControl = control;
        Text = text;
        Show();
    }

    public void Show()
    {
        if (_currentControl == null) return;

        var window = _currentControl.ParentWindow as UIWindow ?? _currentControl.FindForm() as UIWindow;
        if (window == null)
            return;

        EnsureAddedToWindow(window);

        _isActive = true;
        Visible = true;
        UpdatePosition(Cursor.Position);
        AttachWindowHandlers(window);
        _hideTimer.Start();
    }

    public void Hide()
    {
        _isActive = false;
        Visible = false;
        _currentControl = null;
        StopHideTimer();
        DetachWindowHandlers();
    }

    private void UpdatePosition(SKPoint screenPosition)
    {
        if (_currentControl == null)
            return;

        var window = _currentWindow ??
                     _currentControl.ParentWindow as UIWindow ?? _currentControl.FindForm() as UIWindow;
        if (window == null)
            return;

        var offset = 15;
        var windowPoint = window.PointToClient(screenPosition);
        var target = new SKPoint(windowPoint.X + offset, windowPoint.Y + offset);

        target.X = Math.Max(0, Math.Min(target.X, window.Width - Width));
        target.Y = Math.Max(0, Math.Min(target.Y, window.Height - Height));

        Location = target;
        BringToFront();
    }

    private void EnsureAddedToWindow(UIWindow window)
    {
        if (window == null)
            return;

        if (Parent is UIWindow existing && existing == window)
        {
            _currentWindow = window;
            return;
        }

        Parent?.Controls.Remove(this);

        if (!window.Controls.Contains(this)) window.Controls.Add(this);

        _currentWindow = window;
    }

    private void AttachWindowHandlers(UIWindow window)
    {
        if (window == null)
            return;

        if (_handlersAttached && _currentWindow == window)
            return;

        DetachWindowHandlers();

        _currentWindow = window;

        _windowMouseDownHandler ??= Window_MouseDown;
        _windowDeactivateHandler ??= Window_Deactivate;
        _windowKeyDownHandler ??= Window_KeyDown;

        window.MouseDown += _windowMouseDownHandler;
        window.Deactivate += _windowDeactivateHandler;
        window.KeyDown += _windowKeyDownHandler;

        _handlersAttached = true;
    }

    private void DetachWindowHandlers()
    {
        if (!_handlersAttached || _currentWindow == null)
            return;

        if (_windowMouseDownHandler != null)
            _currentWindow.MouseDown -= _windowMouseDownHandler;
        if (_windowDeactivateHandler != null)
            _currentWindow.Deactivate -= _windowDeactivateHandler;
        if (_windowKeyDownHandler != null)
            _currentWindow.KeyDown -= _windowKeyDownHandler;

        _handlersAttached = false;
        _currentWindow = null;
    }

    private void Window_MouseDown(object sender, MouseEventArgs e)
    {
        if (!_isActive || _currentWindow == null)
            return;

        if (Bounds.Contains(e.Location))
            return;

        if (_currentControl != null)
        {
            var controlBounds = GetWindowRelativeBounds(_currentControl, _currentWindow);
            if (controlBounds.Contains(e.Location))
                return;
        }

        Hide();
    }

    private void Window_Deactivate(object sender, EventArgs e)
    {
        if (_isActive)
            Hide();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (!_isActive)
            return;

        if (e.KeyCode == Keys.Escape)
        {
            Hide();
            e.Handled = true;
        }
    }

    private static SkiaSharp.SKRect GetWindowRelativeBounds(UIElementBase element, UIWindow window)
    {
        var screenLocation = element.PointToScreen(SKPoint.Empty);
        var windowPoint = window.PointToClient(screenLocation);
        return new Rectangle(windowPoint, element.Size);
    }

    private (List<string> Lines, float MaxWidth) WrapTextIntoLines(string text, SKFont font, float maxWidth)
    {
        List<string> lines = new();
        var maxLineWidth = 0f;

        foreach (var paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                lines.Add(string.Empty);
                continue;
            }

            var words = paragraph.Split(' ');
            var currentLine = string.Empty;

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                    continue;

                var candidate = string.IsNullOrEmpty(currentLine)
                    ? word
                    : $"{currentLine} {word}";

                var candidateWidth = font.MeasureText(candidate);

                if (candidateWidth > maxWidth && !string.IsNullOrEmpty(currentLine))
                {
                    maxLineWidth = Math.Max(maxLineWidth, font.MeasureText(currentLine));
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else if (candidateWidth > maxWidth)
                {
                    foreach (var wrapped in BreakLongWord(word, font, maxWidth))
                    {
                        maxLineWidth = Math.Max(maxLineWidth, font.MeasureText(wrapped));
                        lines.Add(wrapped);
                    }

                    currentLine = string.Empty;
                }
                else
                {
                    currentLine = candidate;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                maxLineWidth = Math.Max(maxLineWidth, font.MeasureText(currentLine));
                lines.Add(currentLine);
            }
        }

        if (lines.Count == 0)
        {
            lines.Add(text);
            maxLineWidth = Math.Max(maxLineWidth, font.MeasureText(text));
        }

        return (lines, Math.Max(maxLineWidth, 0));
    }

    private static IEnumerable<string> BreakLongWord(string word, SKFont font, float maxWidth)
    {
        List<string> segments = new();
        var current = string.Empty;

        foreach (var c in word)
        {
            var candidate = current + c;
            if (font.MeasureText(candidate) > maxWidth && current.Length > 0)
            {
                segments.Add(current);
                current = c.ToString();
            }
            else
            {
                current = candidate;
            }
        }

        if (!string.IsNullOrEmpty(current))
            segments.Add(current);

        if (segments.Count == 0)
            segments.Add(word);

        return segments;
    }

    private void UpdateSize()
    {
        if (string.IsNullOrEmpty(Text))
        {
            Size = Size.Empty;
            return;
        }

        var font = GetDefaultSkFont();

        float maxContentWidth = Math.Max(1, _maxWidth - _padding * 2);
        var (lines, maxLineWidth) = WrapTextIntoLines(Text, font, maxContentWidth);

        var metrics = font.Metrics;
        var lineHeight = metrics.Descent - metrics.Ascent;
        var lineCount = Math.Max(1, lines.Count);

        var width = (int)Math.Ceiling(Math.Min(_maxWidth, maxLineWidth + _padding * 2));
        var height = (int)Math.Ceiling(lineHeight * lineCount + _padding * 2);

        Size = new SKSize(Math.Max(width, _padding * 2), Math.Max(height, _padding * 2));
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        EnsureSkiaCaches();

        // Gölge çizimi
        if (_shadowOpacity > 0)
        {
            EnsureShadowFilter();
            _shadowPaint!.Color = SKColors.Black.WithAlpha((byte)(_shadowOpacity * 255));
            _shadowPaint.ImageFilter = _shadowImageFilter;

            var shadowRect = IsBalloon
                ? new SKRoundRect(
                    new SkiaSharp.SKRect(_shadowBlur, _shadowBlur + _shadowOffsetY, Width - _shadowBlur, Height - _shadowBlur),
                    _cornerRadius)
                : new SKRoundRect(
                    new SkiaSharp.SKRect(_shadowBlur, _shadowBlur + _shadowOffsetY, Width - _shadowBlur, Height - _shadowBlur),
                    0);

            canvas.DrawRoundRect(shadowRect, _shadowPaint);
        }

        // Arkaplan çizimi
        _bgPaint!.Color = BackColor;
        var rect = IsBalloon
            ? new SKRoundRect(new SkiaSharp.SKRect(0, 0, Width, Height), _cornerRadius)
            : new SKRoundRect(new SkiaSharp.SKRect(0, 0, Width, Height), 0);

        canvas.DrawRoundRect(rect, _bgPaint);

        // Kenarlık çizimi
        _borderPaint!.Color = _borderColor;
        var borderRect = IsBalloon
            ? new SKRoundRect(new SkiaSharp.SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f), _cornerRadius)
            : new SKRoundRect(new SkiaSharp.SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f), 0);

        canvas.DrawRoundRect(borderRect, _borderPaint);

        // Metin çizimi
        if (!string.IsNullOrEmpty(Text))
        {
            var font = GetDefaultSkFont();
            _textPaint!.Color = ForeColor;

            var metrics = font.Metrics;
            var lineHeight = metrics.Descent - metrics.Ascent;
            var (lines, _) = WrapTextIntoLines(Text, font, Math.Max(1, Width - _padding * 2));
            float x = _padding;
            var y = _padding - metrics.Ascent;

            foreach (var line in lines)
            {
                TextRenderingHelper.DrawText(canvas, line, x, y, font, _textPaint);
                y += lineHeight;
            }
        }
    }

    private void EnsureSkiaCaches()
    {
        _shadowPaint ??= new SKPaint { IsAntialias = true };
        _bgPaint ??= new SKPaint { IsAntialias = true };
        _borderPaint ??= new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 1 };
        _textPaint ??= new SKPaint { IsAntialias = true };
    }

    private void EnsureShadowFilter()
    {
        if (_shadowImageFilter != null && _shadowImageFilterBlur == _shadowBlur)
            return;

        _shadowImageFilter?.Dispose();
        _shadowImageFilter = SKImageFilter.CreateBlur(_shadowBlur, _shadowBlur);
        _shadowImageFilterBlur = _shadowBlur;
    }

    private SKFont GetDefaultSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        if (_defaultSkFont == null || !ReferenceEquals(_defaultSkFontSource, Font) || _defaultSkFontDpi != dpi)
        {
            _defaultSkFont?.Dispose();
            _defaultSkFont = new SKFont
            {
                Size = Font.Size.Topx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true
            };
            _defaultSkFontSource = Font;
            _defaultSkFontDpi = dpi;
        }

        return _defaultSkFont;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Ensure we don't keep controls/windows alive via event handlers.
            Hide();

            foreach (var pair in _tooltips)
                if (pair.Key != null)
                    UnsubscribeFromControl(pair.Key);
            _tooltips.Clear();

            DetachWindowHandlers();

            if (_showTimer != null)
            {
                _showTimer.Stop();
                _showTimer.Tick -= ShowTimer_Tick;
                _showTimer.Dispose();
                _showTimer = null;
            }

            if (_hideTimer != null)
            {
                _hideTimer.Stop();
                _hideTimer.Tick -= HideTimer_Tick;
                _hideTimer.Dispose();
                _hideTimer = null;
            }

            _defaultSkFont?.Dispose();
            _defaultSkFont = null;

            _shadowPaint?.Dispose();
            _shadowPaint = null;
            _bgPaint?.Dispose();
            _bgPaint = null;
            _borderPaint?.Dispose();
            _borderPaint = null;
            _textPaint?.Dispose();
            _textPaint = null;

            _shadowImageFilter?.Dispose();
            _shadowImageFilter = null;
        }

        base.Dispose(disposing);
    }
}