using SDUI.Animation;
using SDUI.Extensions;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public enum ValueDivisor
    {
        By1 = 1,
        By10 = 10,
        By100 = 100,
        By1000 = 1000
    }

    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public enum ThumbShape
    {
        Circle,
        Square,
        Triangle,
        Diamond
    }

    public enum TrackStyle
    {
        Simple,
        Rounded,
        Groove,
        Glass
    }

    public class TrackBar : UIElementBase
    {
        #region Variables

        private bool _isDragging;
        private bool _isHovered;
        private Point _mouseLocation;
        private readonly AnimationEngine _thumbHoverAnimation;
        private readonly AnimationEngine _thumbPressAnimation;
        private readonly AnimationEngine _trackHoverAnimation;
        private readonly AnimationEngine _valueAnimation;
        private readonly Tooltip _tooltip;

        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private int _animatedValue = 0;
        private int _smallChange = 1;
        private int _largeChange = 10;
        private bool _drawValueString = false;
        private bool _jumpToMouse = false;
        private bool _showTooltip = true;
        private bool _showTicks = false;
        private int _tickFrequency = 10;
        private ValueDivisor _dividedValue = ValueDivisor.By1;
        private Size _thumbSize = new(16, 16);
        private Orientation _orientation = Orientation.Horizontal;
        private ThumbShape _thumbShape = ThumbShape.Circle;
        private Color _trackColor;
        private Color _thumbColor;
        private Color _tickColor;
        private TrackStyle _trackStyle = TrackStyle.Simple;
        private Color _trackGradientStart = Color.Empty;
        private Color _trackGradientEnd = Color.Empty;
        private Color _thumbGradientStart = Color.Empty;
        private Color _thumbGradientEnd = Color.Empty;
        private string _valueFormat = "{0}";
        private Size _customThumbSize = Size.Empty;

        #endregion

        #region Properties

        [Category("Behavior")]
        public int Minimum
        {
            get => _minimum;
            set
            {
                if (value >= _maximum)
                    value = _maximum - 10;

                if (_value < value)
                    _value = value;

                _minimum = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (value <= _minimum)
                    value = _minimum + 10;

                if (_value > value)
                    _value = value;

                _maximum = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;

                var oldValue = _value;
                if (value < _minimum)
                    _value = _minimum;
                else if (value > _maximum)
                    _value = _maximum;
                else
                    _value = value;

                if (_showTooltip && _isDragging)
                    UpdateTooltip();

                // Değer değişim animasyonu
                _animatedValue = oldValue;
                _valueAnimation.StartNewAnimation(AnimationDirection.In);

                OnValueChanged();
                Invalidate();
            }
        }

        [Category("Behavior")]
        public int SmallChange
        {
            get => _smallChange;
            set => _smallChange = Math.Max(1, value);
        }

        [Category("Behavior")]
        public int LargeChange
        {
            get => _largeChange;
            set => _largeChange = Math.Max(1, value);
        }

        [Category("Behavior")]
        public ValueDivisor ValueDivision
        {
            get => _dividedValue;
            set
            {
                _dividedValue = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public bool JumpToMouse
        {
            get => _jumpToMouse;
            set => _jumpToMouse = value;
        }

        [Category("Appearance")]
        public bool DrawValueString
        {
            get => _drawValueString;
            set
            {
                _drawValueString = value;
                UpdateControlSize();
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value) return;
                _orientation = value;
                UpdateControlSize();
                Invalidate();
            }
        }

        [Category("Behavior")]
        public bool ShowTooltip
        {
            get => _showTooltip;
            set => _showTooltip = value;
        }

        [Category("Appearance")]
        public ThumbShape ThumbStyle
        {
            get => _thumbShape;
            set
            {
                if (_thumbShape == value) return;
                _thumbShape = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public bool ShowTicks
        {
            get => _showTicks;
            set
            {
                if (_showTicks == value) return;
                _showTicks = value;
                UpdateControlSize();
                Invalidate();
            }
        }

        [Category("Appearance")]
        public int TickFrequency
        {
            get => _tickFrequency;
            set
            {
                if (value < 1) value = 1;
                if (_tickFrequency == value) return;
                _tickFrequency = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color TrackColor
        {
            get => _trackColor;
            set
            {
                if (_trackColor == value) return;
                _trackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color ThumbColor
        {
            get => _thumbColor;
            set
            {
                if (_thumbColor == value) return;
                _thumbColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color TickColor
        {
            get => _tickColor;
            set
            {
                if (_tickColor == value) return;
                _tickColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public TrackStyle TrackStyle
        {
            get => _trackStyle;
            set
            {
                if (_trackStyle == value) return;
                _trackStyle = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color TrackGradientStart
        {
            get => _trackGradientStart;
            set
            {
                if (_trackGradientStart == value) return;
                _trackGradientStart = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color TrackGradientEnd
        {
            get => _trackGradientEnd;
            set
            {
                if (_trackGradientEnd == value) return;
                _trackGradientEnd = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color ThumbGradientStart
        {
            get => _thumbGradientStart;
            set
            {
                if (_thumbGradientStart == value) return;
                _thumbGradientStart = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color ThumbGradientEnd
        {
            get => _thumbGradientEnd;
            set
            {
                if (_thumbGradientEnd == value) return;
                _thumbGradientEnd = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Size CustomThumbSize
        {
            get => _customThumbSize;
            set
            {
                if (_customThumbSize == value) return;
                _customThumbSize = value;
                if (!value.IsEmpty)
                    _thumbSize = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public string ValueFormat
        {
            get => _valueFormat;
            set
            {
                if (_valueFormat == value) return;
                _valueFormat = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        public float ValueToSet
        {
            get => _value / (int)_dividedValue;
            set => Value = (int)(value * (int)_dividedValue);
        }

        public event EventHandler ValueChanged;

        #endregion

        public TrackBar()
        {
            Size = new Size(200, 22);
            MinimumSize = new Size(50, 22);

            _thumbHoverAnimation = new AnimationEngine
            {
                Increment = 0.08f,
                AnimationType = AnimationType.Linear
            };
            _thumbPressAnimation = new AnimationEngine
            {
                Increment = 0.15f,
                AnimationType = AnimationType.Linear
            };
            _trackHoverAnimation = new AnimationEngine
            {
                Increment = 0.06f,
                AnimationType = AnimationType.Linear
            };
            _valueAnimation = new AnimationEngine
            {
                Increment = 0.12f,
                AnimationType = AnimationType.EaseInOut
            };

            _thumbHoverAnimation.OnAnimationProgress += (s) => Invalidate();
            _thumbPressAnimation.OnAnimationProgress += (s) => Invalidate();
            _trackHoverAnimation.OnAnimationProgress += (s) => Invalidate();
            _valueAnimation.OnAnimationProgress += (s) =>
            {
                int pVal = (int)s;
                _animatedValue = (int)(_value * pVal + _animatedValue * (1 - pVal));
                Invalidate();
            };

            _tooltip = new();

            // Varsayılan renkler
            _trackColor = ColorScheme.BackColor;
            _thumbColor = ColorScheme.AccentColor;
            _tickColor = ColorScheme.ForeColor;
        }

        private void UpdateControlSize()
        {
            if (_orientation == Orientation.Horizontal)
            {
                Height = (_drawValueString ? 35 : 22) + (_showTicks ? 15 : 0);
                MinimumSize = new Size(50, Height);
            }
            else
            {
                Width = (_drawValueString ? 35 : 22) + (_showTicks ? 15 : 0);
                MinimumSize = new Size(Width, 50);
            }
        }

        private void UpdateTooltip()
        {
            _tooltip.SetToolTip(this, ValueToSet.ToString());
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        internal override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Left when _orientation == Orientation.Horizontal:
                case Keys.Down when _orientation == Orientation.Vertical:
                    Value -= _smallChange;
                    break;

                case Keys.Right when _orientation == Orientation.Horizontal:
                case Keys.Up when _orientation == Orientation.Vertical:
                    Value += _smallChange;
                    break;

                case Keys.PageDown:
                    Value -= _largeChange;
                    break;

                case Keys.PageUp:
                    Value += _largeChange;
                    break;

                case Keys.Home:
                    Value = _minimum;
                    break;

                case Keys.End:
                    Value = _maximum;
                    break;
            }
        }

        internal override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Value += Math.Sign(e.Delta) * _smallChange;
        }

        internal override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            var thumbRect = GetThumbRect();
            _isDragging = thumbRect.Contains(e.Location);

            if (_jumpToMouse || _isDragging)
            {
                var trackRect = GetTrackRect();
                var position = _orientation == Orientation.Horizontal ?
                    e.X - trackRect.X :
                    e.Y - trackRect.Y;
                var length = _orientation == Orientation.Horizontal ?
                    trackRect.Width :
                    trackRect.Height;
                var percentage = Math.Clamp(position / length, 0, 1);
                Value = (int)(_minimum + (_maximum - _minimum) * percentage);
            }

            if (_isDragging)
            {
                _thumbPressAnimation.StartNewAnimation(AnimationDirection.In);
                if (_showTooltip)
                    UpdateTooltip();
            }

            Focus();
        }

        internal override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mouseLocation = e.Location;

            if (_isDragging)
            {
                var trackRect = GetTrackRect();
                var position = _orientation == Orientation.Horizontal ?
                    e.X - trackRect.X :
                    e.Y - trackRect.Y;
                var length = _orientation == Orientation.Horizontal ?
                    trackRect.Width :
                    trackRect.Height;
                var percentage = Math.Clamp(position / length, 0, 1);
                Value = (int)(_minimum + (_maximum - _minimum) * percentage);
            }
            else
            {
                var thumbRect = GetThumbRect();
                var trackRect = GetTrackRect();
                var wasHovered = _isHovered;
                _isHovered = thumbRect.Contains(e.Location) || trackRect.Contains(e.Location);

                if (wasHovered != _isHovered)
                {
                    _thumbHoverAnimation.StartNewAnimation(_isHovered ? AnimationDirection.In : AnimationDirection.Out);
                    _trackHoverAnimation.StartNewAnimation(_isHovered ? AnimationDirection.In : AnimationDirection.Out);
                }
            }

            Invalidate();
        }

        internal override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_isDragging)
            {
                _isDragging = false;
                _thumbPressAnimation.StartNewAnimation(AnimationDirection.Out);
                if (_showTooltip)
                    _tooltip.Hide();
            }
        }

        internal override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _thumbHoverAnimation.StartNewAnimation(AnimationDirection.Out);
            _trackHoverAnimation.StartNewAnimation(AnimationDirection.Out);
        }

        private RectangleF GetTrackRect()
        {
            if (_orientation == Orientation.Horizontal)
                return new RectangleF(10 * ScaleFactor, Height / 2f - ScaleFactor, Width - 20 * ScaleFactor, 2 * ScaleFactor);
            else
                return new RectangleF(Width / 2f - ScaleFactor, 10 * ScaleFactor, 2 * ScaleFactor, Height - 20 * ScaleFactor);
        }

        private RectangleF GetThumbRect()
        {
            var trackRect = GetTrackRect();
            var percentage = (_value - _minimum) / (float)(_maximum - _minimum);

            if (_orientation == Orientation.Horizontal)
            {
                var thumbX = trackRect.X + trackRect.Width * percentage - _thumbSize.Width / 2f;
                var thumbY = Height / 2f - _thumbSize.Height / 2f;
                return new RectangleF(thumbX, thumbY, _thumbSize.Width, _thumbSize.Height);
            }
            else
            {
                var thumbX = Width / 2f - _thumbSize.Width / 2f;
                var thumbY = trackRect.Y + trackRect.Height * (1 - percentage) - _thumbSize.Height / 2f;
                return new RectangleF(thumbX, thumbY, _thumbSize.Width, _thumbSize.Height);
            }
        }

        private void DrawThumb(SKCanvas canvas, RectangleF thumbRect, SKPaint paint)
        {
            var centerX = thumbRect.X;
            var centerY = thumbRect.Y;
            var halfWidth = thumbRect.Width / 2;

            switch (_thumbShape)
            {
                case ThumbShape.Circle:
                    canvas.DrawCircle(centerX, centerY, halfWidth, paint);
                    break;

                case ThumbShape.Square:
                    canvas.DrawRect(
                        new SKRect(
                            centerX - halfWidth,
                            centerY - halfWidth,
                            centerX + halfWidth,
                            centerY + halfWidth
                        ),
                        paint
                    );
                    break;

                case ThumbShape.Triangle:
                    using (var path = new SKPath())
                    {
                        if (_orientation == Orientation.Horizontal)
                        {
                            path.MoveTo(centerX, centerY - halfWidth);
                            path.LineTo(centerX + halfWidth, centerY + halfWidth);
                            path.LineTo(centerX - halfWidth, centerY + halfWidth);
                        }
                        else
                        {
                            path.MoveTo(centerX - halfWidth, centerY);
                            path.LineTo(centerX + halfWidth, centerY - halfWidth);
                            path.LineTo(centerX + halfWidth, centerY + halfWidth);
                        }
                        path.Close();
                        canvas.DrawPath(path, paint);
                    }
                    break;

                case ThumbShape.Diamond:
                    using (var path = new SKPath())
                    {
                        path.MoveTo(centerX, centerY - halfWidth);
                        path.LineTo(centerX + halfWidth, centerY);
                        path.LineTo(centerX, centerY + halfWidth);
                        path.LineTo(centerX - halfWidth, centerY);
                        path.Close();
                        canvas.DrawPath(path, paint);
                    }
                    break;
            }
        }

        private void DrawTicks(SKCanvas canvas, RectangleF trackRect)
        {
            if (!_showTicks) return;

            using var paint = new SKPaint
            {
                Color = (_tickColor == Color.Empty ? ColorScheme.ForeColor : _tickColor)
                    .Alpha(150).ToSKColor(),
                StrokeWidth = 1 * ScaleFactor,
                IsAntialias = true
            };

            var tickCount = (_maximum - _minimum) / _tickFrequency;
            var isHorizontal = _orientation == Orientation.Horizontal;

            for (int i = 0; i <= tickCount; i++)
            {
                var position = i / (float)tickCount;
                float x, y;

                if (isHorizontal)
                {
                    x = trackRect.Left + trackRect.Width * position;
                    y = trackRect.Bottom + 3 * ScaleFactor;
                    canvas.DrawLine(x, y, x, y + 5 * ScaleFactor, paint);
                }
                else
                {
                    x = trackRect.Right + 3 * ScaleFactor;
                    y = trackRect.Top + trackRect.Height * (1 - position);
                    canvas.DrawLine(x, y, x + 5 * ScaleFactor, y, paint);
                }

                // Tick değerini yaz
                if (i % 2 == 0) // Her ikinci tick'te değer göster
                {
                    var value = _minimum + i * _tickFrequency;
                    var text = (value / (float)_dividedValue).ToString();

                    using var textPaint = new SKPaint
                    {
                        Color = paint.Color,
                        TextSize = 8f.PtToPx(this),
                        TextAlign = isHorizontal ? SKTextAlign.Center : SKTextAlign.Left,
                        IsAntialias = true,
                        Typeface = SKTypeface.FromFamilyName("Segoe UI")
                    };

                    if (isHorizontal)
                        canvas.DrawText(text, x, y + 15 * ScaleFactor, textPaint);
                    else
                        canvas.DrawText(text, x + 8 * ScaleFactor, y + 4 * ScaleFactor, textPaint);
                }
            }
        }

        private void DrawTrack(SKCanvas canvas, RectangleF trackRect, SKPaint paint)
        {
            switch (_trackStyle)
            {
                case TrackStyle.Simple:
                    canvas.DrawRoundRect(trackRect.ToSKRect(), 1 * ScaleFactor, 1 * ScaleFactor, paint);
                    break;

                case TrackStyle.Rounded:
                    canvas.DrawRoundRect(trackRect.ToSKRect(), trackRect.Height / 2, trackRect.Height / 2, paint);
                    break;

                case TrackStyle.Groove:
                    var oldMaskFilter = paint.MaskFilter;
                    paint.MaskFilter = null;

                    paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1f * ScaleFactor);
                    canvas.DrawRoundRect(trackRect.ToSKRect(), 2 * ScaleFactor, 2 * ScaleFactor, paint);
                    paint.MaskFilter = oldMaskFilter;
                    oldMaskFilter?.Dispose();

                    break;

                case TrackStyle.Glass:
                    using (var path = new SKPath())
                    {
                        path.AddRoundRect(trackRect.ToSKRect(), 2 * ScaleFactor, 2 * ScaleFactor);

                        // Ana track
                        canvas.DrawPath(path, paint);

                        // Parlama efekti
                        using var shimmerPaint = new SKPaint
                        {
                            Style = SKPaintStyle.Fill,
                            Color = SKColors.White.WithAlpha(40),
                            IsAntialias = true,
                            ImageFilter = SKImageFilter.CreateBlur(2 * ScaleFactor, 2 * ScaleFactor)
                        };

                        var shimmerRect = trackRect;
                        shimmerRect.Height /= 2;
                        using var shimmerPath = new SKPath();
                        shimmerPath.AddRoundRect(shimmerRect.ToSKRect(), 2 * ScaleFactor, 2 * ScaleFactor);
                        canvas.DrawPath(shimmerPath, shimmerPaint);
                    }
                    break;
            }
        }

        private SKPaint CreateGradientPaint(RectangleF rect, Color startColor, Color endColor, Color defaultColor, byte alpha = 255)
        {
            var paint = new SKPaint { IsAntialias = true };

            if (startColor != Color.Empty && endColor != Color.Empty)
            {
                paint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(rect.Left, rect.Top),
                    new SKPoint(rect.Right, rect.Bottom),
                    new[]
                    {
                        startColor.ToSKColor().WithAlpha(alpha),
                        endColor.ToSKColor().WithAlpha(alpha)
                    },
                    null,
                    SKShaderTileMode.Clamp
                );
            }
            else
            {
                paint.Color = defaultColor.ToSKColor().WithAlpha(alpha);
            }

            return paint;
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            base.OnPaint(e);
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var trackRect = GetTrackRect();
            var thumbRect = GetThumbRect();

            // Track arka planı
            using (var paint = CreateGradientPaint(
                trackRect,
                _trackGradientStart,
                _trackGradientEnd,
                _trackColor == Color.Empty ? ColorScheme.BackColor : _trackColor,
                40))
            {
                DrawTrack(canvas, trackRect, paint);
            }

            // Doldurulmuş track kısmı
            using (var paint = CreateGradientPaint(
                trackRect,
                _trackGradientStart,
                _trackGradientEnd,
                _thumbColor == Color.Empty ? ColorScheme.AccentColor : _thumbColor,
                (byte)(150 + _trackHoverAnimation.GetProgress() * 105)))
            {
                var filledRect = _orientation == Orientation.Horizontal ?
                    new SKRect(
                        trackRect.Left,
                        trackRect.Top,
                        thumbRect.Left + thumbRect.Width / 2,
                        trackRect.Bottom
                    ) :
                    new SKRect(
                        trackRect.Left,
                        thumbRect.Top + thumbRect.Height / 2,
                        trackRect.Right,
                        trackRect.Bottom
                    );

                DrawTrack(canvas, filledRect.ToDrawingRect(), paint);
            }

            // Tick'leri çiz
            DrawTicks(canvas, trackRect);

            // Thumb çizimi
            var thumbHoverProgress = _thumbHoverAnimation.GetProgress();
            var thumbPressProgress = _thumbPressAnimation.GetProgress();
            var thumbAlpha = 180 + (byte)(thumbHoverProgress * 75);

            // Thumb gölgesi
            if (thumbHoverProgress > 0 || thumbPressProgress > 0)
            {
                using var shadowPaint = new SKPaint
                {
                    Color = SKColors.Black.WithAlpha(20),
                    ImageFilter = SKImageFilter.CreateDropShadow(
                        0,
                        1 * ScaleFactor,
                        2 * ScaleFactor,
                        2 * ScaleFactor,
                        SKColors.Black.WithAlpha(30)
                    ),
                    IsAntialias = true
                };

                DrawThumb(canvas, thumbRect, shadowPaint);
            }

            // Thumb arka planı
            using (var paint = CreateGradientPaint(
                thumbRect,
                _thumbGradientStart,
                _thumbGradientEnd,
                ColorScheme.BackColor))
            {
                DrawThumb(canvas, thumbRect, paint);
            }

            // Thumb kenarlığı
            using (var paint = new SKPaint
            {
                Color = (_thumbColor == Color.Empty ? ColorScheme.AccentColor : _thumbColor)
                    .Alpha(thumbAlpha)
                    .ToSKColor(),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f * ScaleFactor,
                IsAntialias = true
            })
            {
                DrawThumb(canvas, thumbRect, paint);
            }

            // Değer metni
            if (_drawValueString)
            {
                using var paint = new SKPaint
                {
                    Color = ColorScheme.ForeColor.ToSKColor(),
                    TextSize = 9f.PtToPx(this),
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Segoe UI")
                };

                var formattedValue = string.Format(_valueFormat, ValueToSet);

                if (_orientation == Orientation.Horizontal)
                {
                    canvas.DrawText(
                        formattedValue,
                        5 * ScaleFactor,
                        Height - 8 * ScaleFactor,
                        paint
                    );
                }
                else
                {
                    canvas.Save();
                    canvas.RotateDegrees(90);
                    canvas.DrawText(
                        formattedValue,
                        5 * ScaleFactor,
                        -Width + 8 * ScaleFactor,
                        paint
                    );
                    canvas.Restore();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tooltip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
