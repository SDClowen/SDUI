using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class Tooltip : UIElementBase
    {
        private string _text = string.Empty;
        private int _showDelay = 500;
        private int _autoPopDelay = 5000;
        private bool _isActive;
        private Timer _showTimer;
        private Timer _hideTimer;
        private readonly Dictionary<UIElementBase, string> _tooltips = new();
        private UIElementBase _currentControl;
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
            set => _showDelay = Math.Max(0, value);
        }

        [DefaultValue(5000)]
        public int AutoPopDelay
        {
            get => _autoPopDelay;
            set => _autoPopDelay = Math.Max(0, value);
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

        public Tooltip()
        {
            BackColor = Color.FromArgb(50, 50, 50);
            ForeColor = Color.White;
            Visible = false;

            _showTimer = new Timer { Interval = ShowDelay };
            _showTimer.Tick += ShowTimer_Tick;

            _hideTimer = new Timer { Interval = AutoPopDelay };
            _hideTimer.Tick += HideTimer_Tick;
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
            if (_isActive)
            {
                UpdatePosition(e.Location);
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

        public void Show()
        {
            if (_currentControl == null) return;

            _isActive = true;
            Visible = true;
            UpdatePosition(Control.MousePosition);
            _hideTimer.Start();
        }

        public void Hide()
        {
            _isActive = false;
            Visible = false;
            _currentControl = null;
            StopHideTimer();
        }

        private void UpdatePosition(Point mousePosition)
        {
            if (_currentControl == null) return;

            var screenPoint = _currentControl.PointToScreen(mousePosition);
            var offset = 15; // İmleçten uzaklık

            Location = new Point(
                screenPoint.X + offset,
                screenPoint.Y + offset
            );
        }

        private void UpdateSize()
        {
            if (string.IsNullOrEmpty(Text))
            {
                Size = Size.Empty;
                return;
            }

            using (var paint = new SKPaint
            {
                TextSize = Font.Size * DpiHelper.GetScaleFactor(),
                Typeface = SKTypeface.FromFamilyName(Font.Name)
            })
            {
                var bounds = new SKRect();
                paint.MeasureText(Text, ref bounds);

                int width = Math.Min(_maxWidth, (int)bounds.Width + (_padding * 2));
                int height = (int)bounds.Height + (_padding * 2);

                if (bounds.Width > _maxWidth)
                {
                    // Çok satırlı metin için yüksekliği ayarla
                    var lines = TextRenderer.MeasureText(
                        Text,
                        Font,
                        new Size(_maxWidth - (_padding * 2), int.MaxValue),
                        TextFormatFlags.WordBreak
                    );
                    height = lines.Height + (_padding * 2);
                }

                Size = new Size(width, height);
            }
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
                using (var paint = new SKPaint
                {
                    Color = new SKColor((byte)(ForeColor.R), (byte)(ForeColor.G), (byte)(ForeColor.B), (byte)(ForeColor.A)),
                    TextSize = Font.Size * DpiHelper.GetScaleFactor(),
                    Typeface = SKTypeface.FromFamilyName(Font.Name),
                    IsAntialias = true
                })
                {
                    var bounds = new SKRect();
                    paint.MeasureText(Text, ref bounds);

                    float x = _padding;
                    float y = _padding - bounds.Top;

                    canvas.DrawText(Text, x, y, paint);
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