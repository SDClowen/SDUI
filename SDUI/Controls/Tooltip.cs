using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class ToolTip : UIElementBase
    {
        private string _text = string.Empty;
        private int _showDelay = 500;
        private int _autoPopDelay = 5000;
        private bool _isActive;
        private Timer _showTimer;
        private Timer _hideTimer;
        private readonly Dictionary<UIElementBase, string> _tooltips = new();
        private UIElementBase _currentControl;
        private UIWindow _currentWindow;
        private bool _handlersAttached;
        private MouseEventHandler _windowMouseDownHandler;
        private EventHandler _windowDeactivateHandler;
        private KeyEventHandler _windowKeyDownHandler;
        private bool _isBalloon = true;
        private int _cornerRadius = 6;
        private Color _borderColor = Color.FromArgb(100, 100, 100);
        private float _shadowOpacity = 0.2f;
        private int _shadowBlur = 10;
        private int _shadowOffsetY = 3;
        private int _maxWidth = 300;
        private int _padding = 8;

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

        public ToolTip()
        {
            BackColor = Color.FromArgb(50, 50, 50);
            ForeColor = Color.White;
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
            if (sender is UIElementBase control && _tooltips.TryGetValue(control, out string text))
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

            var window = _currentControl.ParentWindow as UIWindow ?? (_currentControl.FindForm() as UIWindow);
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

        private void UpdatePosition(Point screenPosition)
        {
            if (_currentControl == null)
                return;

            var window = _currentWindow ?? _currentControl.ParentWindow as UIWindow ?? (_currentControl.FindForm() as UIWindow);
            if (window == null)
                return;

            var offset = 15;
            var windowPoint = window.PointToClient(screenPosition);
            var target = new Point(windowPoint.X + offset, windowPoint.Y + offset);

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

            if (!window.Controls.Contains(this))
            {
                window.Controls.Add(this);
            }

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

        private static Rectangle GetWindowRelativeBounds(UIElementBase element, UIWindow window)
        {
            var screenLocation = element.PointToScreen(Point.Empty);
            var windowPoint = window.PointToClient(screenLocation);
            return new Rectangle(windowPoint, element.Size);
        }

        private (List<string> Lines, float MaxWidth) WrapTextIntoLines(string text, SKFont font, float maxWidth)
        {
            List<string> lines = new();
            float maxLineWidth = 0f;

            foreach (var paragraph in text.Replace("\r\n", "\n").Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                {
                    lines.Add(string.Empty);
                    continue;
                }

                var words = paragraph.Split(' ');
                string currentLine = string.Empty;

                foreach (var word in words)
                {
                    if (string.IsNullOrWhiteSpace(word))
                        continue;

                    var candidate = string.IsNullOrEmpty(currentLine)
                        ? word
                        : $"{currentLine} {word}";

                    float candidateWidth = font.MeasureText(candidate);

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
            string current = string.Empty;

            foreach (char c in word)
            {
                string candidate = current + c;
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

            using var font = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(Font.Name)
            };

            float maxContentWidth = Math.Max(1, _maxWidth - (_padding * 2));
            var (lines, maxLineWidth) = WrapTextIntoLines(Text, font, maxContentWidth);

            var metrics = font.Metrics;
            float lineHeight = metrics.Descent - metrics.Ascent;
            int lineCount = Math.Max(1, lines.Count);

            int width = (int)Math.Ceiling(Math.Min(_maxWidth, maxLineWidth + (_padding * 2)));
            int height = (int)Math.Ceiling(lineHeight * lineCount + (_padding * 2));

            Size = new Size(Math.Max(width, _padding * 2), Math.Max(height, _padding * 2));
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            base.OnPaint(e);

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            // Gölge çizimi
            if (_shadowOpacity > 0)
            {
                using (var paint = new SKPaint
                {
                    Color = SKColors.Black.WithAlpha((byte)(_shadowOpacity * 255)),
                    ImageFilter = SKImageFilter.CreateBlur(_shadowBlur, _shadowBlur),
                    IsAntialias = true
                })
                {
                    var shadowRect = IsBalloon ?
                        new SKRoundRect(new SKRect(_shadowBlur, _shadowBlur + _shadowOffsetY, Width - _shadowBlur, Height - _shadowBlur), _cornerRadius) :
                        new SKRoundRect(new SKRect(_shadowBlur, _shadowBlur + _shadowOffsetY, Width - _shadowBlur, Height - _shadowBlur), 0);

                    canvas.DrawRoundRect(shadowRect, paint);
                }
            }

            // Arkaplan çizimi
            using (var paint = new SKPaint
            {
                Color = new SKColor((byte)(BackColor.R), (byte)(BackColor.G), (byte)(BackColor.B), (byte)(BackColor.A)),
                IsAntialias = true
            })
            {
                var rect = IsBalloon ?
                    new SKRoundRect(new SKRect(0, 0, Width, Height), _cornerRadius) :
                    new SKRoundRect(new SKRect(0, 0, Width, Height), 0);

                canvas.DrawRoundRect(rect, paint);
            }

            // Kenarlık çizimi
            using (var paint = new SKPaint
            {
                Color = new SKColor((byte)(_borderColor.R), (byte)(_borderColor.G), (byte)(_borderColor.B), (byte)(_borderColor.A)),
                IsAntialias = true,
                IsStroke = true,
                StrokeWidth = 1
            })
            {
                var rect = IsBalloon ?
                    new SKRoundRect(new SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f), _cornerRadius) :
                    new SKRoundRect(new SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f), 0);

                canvas.DrawRoundRect(rect, paint);
            }

            // Metin çizimi
            if (!string.IsNullOrEmpty(Text))
            {
                using (var font = new SKFont
                {
                    Size = Font.Size.PtToPx(this),
                    Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name)
                })
                using (var paint = new SKPaint
                {
                    Color = new SKColor((byte)(ForeColor.R), (byte)(ForeColor.G), (byte)(ForeColor.B), (byte)(ForeColor.A)),
                    IsAntialias = true
                })
                {
                    var metrics = font.Metrics;
                    float lineHeight = metrics.Descent - metrics.Ascent;
                    var (lines, _) = WrapTextIntoLines(Text, font, Math.Max(1, Width - (_padding * 2)));
                    float x = _padding;
                    float y = _padding - metrics.Ascent;

                    foreach (var line in lines)
                    {
                        canvas.DrawText(line, x, y, font, paint);
                        y += lineHeight;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _showTimer?.Dispose();
                _hideTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}