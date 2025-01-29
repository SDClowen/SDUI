﻿using SDUI.Animation;
using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class NumUpDown : UIElementBase
    {
        private const int LONG_PRESS_TIMER_INTERVAL = 250;

        public event EventHandler ValueChanged;

        private decimal _value;
        private decimal _min;
        private decimal _max;
        private Point _mouseLocation;
        private bool _isUsingKeyboard;
        private Timer _longPressTimer = new();

        private RectangleF _upButtonRect;
        private RectangleF _downButtonRect;

        private const int SIZE = 20;

        private readonly Animation.AnimationEngine upButtonHoverAnimation;
        private readonly Animation.AnimationEngine downButtonHoverAnimation;
        private readonly Animation.AnimationEngine upButtonPressAnimation;
        private readonly Animation.AnimationEngine downButtonPressAnimation;

        private bool _inUpButton, _inDownButton;
        private bool _upButtonPressed, _downButtonPressed;

        public decimal Value
        {
            get => _value;
            set
            {
                if (value <= _max & value >= _min)
                {
                    _value = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                Invalidate();
            }
        }

        public decimal Minimum
        {
            get => _min;
            set
            {
                if (value < _max)
                {
                    _min = value;
                }

                if (_value < _min)
                    Value = _min;

                Invalidate();
            }
        }

        public decimal Maximum
        {
            get => _max;
            set
            {
                if (value > _min)
                    _max = value;

                if (_value > _max)
                    Value = _max;

                Invalidate();
            }
        }

        public NumUpDown()
        {
            _min = 0;
            _max = 100;
            Size = new Size(80, 25);
            MinimumSize = Size;

            upButtonHoverAnimation = new()
            {
                Increment = 0.08f,
                AnimationType = AnimationType.Linear
            };
            downButtonHoverAnimation = new()
            {
                Increment = 0.08f,
                AnimationType = AnimationType.Linear
            };

            upButtonPressAnimation = new()
            {
                Increment = 0.15f,
                AnimationType = AnimationType.Linear
            };
            downButtonPressAnimation = new()
            {
                Increment = 0.15f,
                AnimationType = AnimationType.Linear
            };

            upButtonHoverAnimation.OnAnimationProgress += sender => Invalidate();
            downButtonHoverAnimation.OnAnimationProgress += sender => Invalidate();
            upButtonPressAnimation.OnAnimationProgress += sender => Invalidate();
            downButtonPressAnimation.OnAnimationProgress += sender => Invalidate();

            _longPressTimer.Tick += LongPressTimer_Tick;
            _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;

            UpdateButtonRects();
        }

        private void UpdateButtonRects()
        {
            _upButtonRect = new(Width - SIZE * ScaleFactor, 0, SIZE * ScaleFactor, Height / 2f);
            _downButtonRect = new(Width - SIZE * ScaleFactor, Height / 2f, SIZE * ScaleFactor, Height / 2f);
        }

        internal override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mouseLocation = e.Location;

            bool inUpButton = e.Location.InRect(_upButtonRect);
            bool inDownButton = e.Location.InRect(_downButtonRect);

            if (inUpButton != _inUpButton)
            {
                _inUpButton = inUpButton;
                upButtonHoverAnimation.StartNewAnimation(_inUpButton ? AnimationDirection.In : AnimationDirection.Out);
            }

            if (inDownButton != _inDownButton)
            {
                _inDownButton = inDownButton;
                downButtonHoverAnimation.StartNewAnimation(_inDownButton ? AnimationDirection.In : AnimationDirection.Out);
            }

            Invalidate();
        }

        internal override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _inUpButton = _inDownButton = false;
            upButtonHoverAnimation.StartNewAnimation(AnimationDirection.Out);
            downButtonHoverAnimation.StartNewAnimation(AnimationDirection.Out);
            Invalidate();
        }

        private void ClickButton()
        {
            if (_mouseLocation.InRect(_upButtonRect))
            {
                if (_value + 1 <= _max)
                    Value++;
            }
            else if (_mouseLocation.InRect(_downButtonRect))
            {
                if (_value - 1 >= _min)
                    Value--;
            }
            else
            {
                _isUsingKeyboard = !_isUsingKeyboard;
            }
            Focus();
            Invalidate();
        }

        internal override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_mouseLocation.InRect(_upButtonRect))
            {
                _upButtonPressed = true;
                upButtonPressAnimation.StartNewAnimation(AnimationDirection.In);
            }
            else if (_mouseLocation.InRect(_downButtonRect))
            {
                _downButtonPressed = true;
                downButtonPressAnimation.StartNewAnimation(AnimationDirection.In);
            }

            ClickButton();
            _longPressTimer.Start();
        }

        internal override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_upButtonPressed)
            {
                _upButtonPressed = false;
                upButtonPressAnimation.StartNewAnimation(AnimationDirection.Out);
            }
            if (_downButtonPressed)
            {
                _downButtonPressed = false;
                downButtonPressAnimation.StartNewAnimation(AnimationDirection.Out);
            }

            _longPressTimer.Stop();
            _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;
        }

        private void LongPressTimer_Tick(object sender, EventArgs e)
        {
            ClickButton();

            if (_longPressTimer.Interval == LONG_PRESS_TIMER_INTERVAL)
                _longPressTimer.Interval = 50;
        }

        internal override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            try
            {
                if (_isUsingKeyboard == true && _value < _max)
                    Value = long.Parse(_value.ToString() + e.KeyChar.ToString());
            }
            catch (Exception)
            {
            }
        }

        internal override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Back)
            {
                var tempVal = _value.ToString();
                tempVal = tempVal.Remove(Convert.ToInt32(tempVal.Length - 1));
                if (tempVal.Length == 0)
                    tempVal = "0";

                Value = Convert.ToInt32(tempVal);
            }
            Invalidate();
        }

        internal override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                if (_value + 1 <= _max)
                {
                    Value++;
                }
                Invalidate();
            }
            else
            {
                if (_value - 1 >= _min)
                {
                    Value--;
                }
                Invalidate();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateButtonRects();
        }

        internal override void OnDpiChanged(EventArgs e)
        {
            base.OnDpiChanged(e);
            UpdateButtonRects();
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            base.OnPaint(e);
            e.Surface.Canvas.Clear(SKColors.Transparent);

            var canvas = e.Surface.Canvas;
            var info = e.Info;

            if (info.Width <= 0 || info.Height <= 0)
                return;

            // Arka plan çizimi
            using (var backColorBrush = new SKPaint { Color = ColorScheme.BackColor.Alpha(20).ToSKColor() })
            using (var path = new SKPath())
            {
                path.AddRoundRect(new SKRect(0, 0, Width, Height), 6 * ScaleFactor, 6 * ScaleFactor);
                canvas.DrawPath(path, backColorBrush);
            }

            // Kenarlık çizimi
            using (var borderPaint = new SKPaint
            {
                Color = ColorScheme.BorderColor.Alpha(80).ToSKColor(),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true
            })
            using (var path = new SKPath())
            {
                path.AddRoundRect(new SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f), 6 * ScaleFactor, 6 * ScaleFactor);
                canvas.DrawPath(path, borderPaint);
            }

            // Buton ayraç çizgileri
            using (var linePaint = new SKPaint
            {
                Color = ColorScheme.BorderColor.Alpha(80).ToSKColor(),
                StrokeWidth = 1f,
                IsAntialias = true
            })
            {
                canvas.DrawLine(_upButtonRect.X, 0, _upButtonRect.X, Height, linePaint);
                canvas.DrawLine(_upButtonRect.X, Height / 2f, Width, Height / 2f, linePaint);
            }

            // Buton hover ve press efektleri
            var upHoverProgress = upButtonHoverAnimation.GetProgress();
            var upPressProgress = upButtonPressAnimation.GetProgress();
            if (upHoverProgress > 0 || upPressProgress > 0)
            {
                using var buttonPaint = new SKPaint
                {
                    Color = ColorScheme.ForeColor.ToSKColor()
                        .WithAlpha((byte)(Math.Max(upHoverProgress * 30, upPressProgress * 80))),
                    IsAntialias = true
                };

                using var path = new SKPath();
                var rect = _upButtonRect.ToSKRect();
                path.AddRoundRect(new SKRect(rect.Left + 1, rect.Top + 1, rect.Right - 1, rect.Bottom),
                    6 * ScaleFactor, 0, SKPathDirection.Clockwise);
                canvas.DrawPath(path, buttonPaint);
            }

            var downHoverProgress = downButtonHoverAnimation.GetProgress();
            var downPressProgress = downButtonPressAnimation.GetProgress();
            if (downHoverProgress > 0 || downPressProgress > 0)
            {
                using var buttonPaint = new SKPaint
                {
                    Color = ColorScheme.ForeColor.ToSKColor()
                        .WithAlpha((byte)(Math.Max(downHoverProgress * 30, downPressProgress * 80))),
                    IsAntialias = true
                };

                using var path = new SKPath();
                var rect = _downButtonRect.ToSKRect();
                path.AddRoundRect(new SKRect(rect.Left + 1, rect.Top, rect.Right - 1, rect.Bottom - 1),
                    6 * ScaleFactor, 0, SKPathDirection.Clockwise);
                canvas.DrawPath(path, buttonPaint);
            }

            // Buton simgeleri
            using (var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                TextSize = 9f.PtToPx(this),
                TextAlign = SKTextAlign.Center,
                IsAntialias = true,
                SubpixelText = true,
                Typeface = SKTypeface.FromFamilyName("Segoe UI Symbol")
            })
            {
                var metrics = textPaint.FontMetrics;
                var textHeight = Math.Abs(metrics.Ascent + metrics.Descent);

                // Up button
                var upColor = ColorScheme.ForeColor;
                if (_upButtonPressed)
                    upColor = upColor.Alpha(255);
                else if (_inUpButton)
                    upColor = upColor.Alpha(230);
                else
                    upColor = upColor.Alpha(180);

                textPaint.Color = upColor.ToSKColor();
                canvas.DrawText("▲",
                    _upButtonRect.X + _upButtonRect.Width / 2,
                    _upButtonRect.Height / 2 + textHeight / 3,
                    textPaint);

                // Down button
                var downColor = ColorScheme.ForeColor;
                if (_downButtonPressed)
                    downColor = downColor.Alpha(255);
                else if (_inDownButton)
                    downColor = downColor.Alpha(230);
                else
                    downColor = downColor.Alpha(180);

                textPaint.Color = downColor.ToSKColor();
                canvas.DrawText("▼",
                    _downButtonRect.X + _downButtonRect.Width / 2,
                    _downButtonRect.Height * 1.5f + textHeight / 3,
                    textPaint);
            }

            // Değer metni
            using (var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                TextSize = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName("Segoe UI"),
                TextAlign = SKTextAlign.Left,
                IsAntialias = true,
                SubpixelText = true
            })
            {
                var metrics = textPaint.FontMetrics;
                var textHeight = Math.Abs(metrics.Ascent + metrics.Descent);

                canvas.DrawText(Value.ToString(),
                    10 * ScaleFactor,
                    Height / 2 + textHeight / 3,
                    textPaint);
            }
        }
    }
}
