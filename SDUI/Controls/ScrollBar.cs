using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Animation;

namespace SDUI.Controls
{
    public class ScrollBar : UIElementBase
    {
        private int _value;
        private int _targetValue; // animasyon hedefi
        private double _animatedValue; // animasyonlu değer
        private int _minimum;
        private int _maximum = 100;
        private int _largeChange = 10;
        private int _smallChange = 1;
        private bool _isDragging;
        private Point _dragStartPoint;
        private int _dragStartValue;
        private bool _isHovered; // track hover
        private bool _isThumbHovered;
        private bool _isThumbPressed;
        private Rectangle _thumbRect;
        private Rectangle _trackRect;
        private Orientation _orientation = Orientation.Vertical;
        private int _thickness = 2; // ince varsayılan

        // Radius & auto-hide
        private int _radius = 6; // default radius daha büyük
        private bool _autoHide = true;
        private int _hideDelay = 1200; // ms
        private bool _useThumbShadow = true;
        private readonly AnimationManager _visibilityAnim;
        private readonly AnimationManager _scrollAnim; // scroll animasyonu
        private readonly Timer _hideTimer;

        // Yeni: animasyon ayarları
        private double _visibilityAnimIncrement = 0.20; // daha hızlı varsayılan
        private AnimationType _visibilityAnimType = AnimationType.EaseInOut;
        private double _scrollAnimIncrement = 0.45; // kaydırma için hızlı varsayılan
        private AnimationType _scrollAnimType = AnimationType.EaseOut;

        private bool _hostHovered; // container hover durumu

        public event EventHandler ValueChanged;
        public event EventHandler Scroll;

        [DefaultValue(4)]
        [Description("Scrollbar kalınlığı (dikeyde genişlik, yatayda yükseklik)")]
        public int Thickness
        {
            get => _thickness;
            set
            {
                value = Math.Max(2, Math.Min(32, value));
                if (_thickness == value) return;
                _thickness = value;
                ApplyOrientationSize();
                UpdateThumbRect();
                Invalidate();
            }
        }

        [DefaultValue(6)]
        [Description("Köşe yuvarlaklık yarıçapı")]
        public int Radius
        {
            get => _radius;
            set
            {
                value = Math.Max(0, Math.Min(64, value));
                if (_radius == value) return;
                _radius = value;
                Invalidate();
            }
        }

        [DefaultValue(true)]
        [Description("Otomatik gizleme")]
        public bool AutoHide
        {
            get => _autoHide;
            set
            {
                if (_autoHide == value) return;
                _autoHide = value;
                if (!_autoHide)
                {
                    _visibilityAnim.SetProgress(1);
                    Invalidate();
                }
                else
                {
                    ShowWithAutoHide();
                }
            }
        }

        [DefaultValue(1200)]
        [Description("Gizleme gecikmesi (ms)")]
        public int HideDelay
        {
            get => _hideDelay;
            set
            {
                _hideDelay = Math.Max(250, Math.Min(10000, value));
                _hideTimer.Interval = _hideDelay;
            }
        }

        [DefaultValue(true)]
        [Description("Thumb gölge efekti")]
        public bool UseThumbShadow
        {
            get => _useThumbShadow;
            set { _useThumbShadow = value; Invalidate(); }
        }

        [DefaultValue(Orientation.Vertical)]
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value) return;
                _orientation = value;
                ApplyOrientationSize();
                UpdateThumbRect();
                Invalidate();
            }
        }

        // Yeni: animasyon ayarlarını dışarı aç
        [Category("Animation")]
        [DefaultValue(0.20)]
        [Description("Görünürlük animasyonu hız (Increment). Daha yüksek değer daha hızlıdır.")]
        public double VisibilityAnimationIncrement
        {
            get => _visibilityAnimIncrement;
            set
            {
                _visibilityAnimIncrement = Math.Clamp(value, 0.01, 1.0);
                _visibilityAnim.Increment = _visibilityAnimIncrement;
            }
        }

        [Category("Animation")]
        [DefaultValue(typeof(AnimationType), "EaseInOut")]
        [Description("Görünürlük animasyonu tipi (Easing)")]
        public AnimationType VisibilityAnimationType
        {
            get => _visibilityAnimType;
            set
            {
                _visibilityAnimType = value;
                _visibilityAnim.AnimationType = _visibilityAnimType;
            }
        }

        [Category("Animation")]
        [DefaultValue(0.45)]
        [Description("Scroll animasyonu hız (Increment). Daha yüksek değer daha hızlıdır.")]
        public double ScrollAnimationIncrement
        {
            get => _scrollAnimIncrement;
            set
            {
                _scrollAnimIncrement = Math.Clamp(value, 0.01, 1.0);
                _scrollAnim.Increment = _scrollAnimIncrement;
            }
        }

        [Category("Animation")]
        [DefaultValue(typeof(AnimationType), "EaseOut")]
        [Description("Scroll animasyonu tipi (Easing)")]
        public AnimationType ScrollAnimationType
        {
            get => _scrollAnimType;
            set
            {
                _scrollAnimType = value;
                _scrollAnim.AnimationType = _scrollAnimType;
            }
        }

        private void ApplyOrientationSize()
        {
            Size = IsVertical ? new Size(_thickness, Math.Max(Height, 100)) : new Size(Math.Max(Width, 100), _thickness);
        }

        public bool IsVertical => Orientation == Orientation.Vertical;

        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                value = Math.Max(Minimum, Math.Min(Maximum, value));
                if (_value == value) return;
                _value = value;
                
                if (_isDragging)
                {
                    _animatedValue = value;
                    _targetValue = value;
                    UpdateThumbRect();
                }
                else
                {
                    _targetValue = value;
                    _scrollAnim.StartNewAnimation(AnimationDirection.In);
                }
                
                OnValueChanged(EventArgs.Empty);
                Invalidate();
            }
        }

        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set
            {
                if (_minimum == value) return;
                _minimum = value;
                if (Value < value) Value = value;
                UpdateThumbRect();
                Invalidate();
            }
        }

        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (_maximum == value) return;
                _maximum = value;
                if (Value > value) Value = value;
                UpdateThumbRect();
                Invalidate();
            }
        }

        [DefaultValue(10)]
        public int LargeChange
        {
            get => _largeChange;
            set
            {
                if (_largeChange == value) return;
                _largeChange = value;
                UpdateThumbRect();
                Invalidate();
            }
        }

        [DefaultValue(1)]
        public int SmallChange
        {
            get => _smallChange;
            set
            {
                if (_smallChange == value) return;
                _smallChange = value;
            }
        }

        [DefaultValue(typeof(Color), "Transparent")]
        [Description("Track rengi override; Transparent ise ColorScheme kullanılır")]
        public Color TrackColor { get; set; } = Color.Transparent;

        [DefaultValue(typeof(Color), "Transparent")]
        [Description("Thumb rengi override; Transparent ise ColorScheme kullanılır")]
        public Color ThumbColor { get; set; } = Color.Transparent;

        public ScrollBar()
        {
            BackColor = Color.Transparent; // ColorScheme ile uyumlu
            Cursor = Cursors.Default;
            ApplyOrientationSize();

            _visibilityAnim = new AnimationManager(singular: true)
            {
                Increment = _visibilityAnimIncrement,
                AnimationType = _visibilityAnimType,
                InterruptAnimation = true
            };
            _visibilityAnim.OnAnimationProgress += (s) => Invalidate();

            _scrollAnim = new AnimationManager(singular: true)
            {
                Increment = _scrollAnimIncrement,
                AnimationType = _scrollAnimType,
                InterruptAnimation = true
            };
            _scrollAnim.OnAnimationProgress += (s) =>
            {
                _animatedValue = _value + (_targetValue - _value) * _scrollAnim.GetProgress();
                UpdateThumbRect();
                Invalidate();
            };

            _hideTimer = new Timer { Interval = _hideDelay };
            _hideTimer.Tick += (s, e) => HideNow();

            _visibilityAnim.SetProgress(_autoHide ? 0 : 1);
            _animatedValue = _value;
            _targetValue = _value;
        }

        private void UpdateThumbRect()
        {
            if (Maximum <= Minimum)
            {
                _thumbRect = Rectangle.Empty;
                return;
            }

            int trackLength = IsVertical ? Height : Width;
            int thumbLength = Math.Max(20, (int)((float)LargeChange / (Maximum - Minimum + LargeChange) * trackLength));
            int thumbPos;

            double currentValue = _animatedValue;

            if (IsVertical)
            {
                thumbPos = (int)((currentValue - Minimum) / (Maximum - Minimum) * (Height - thumbLength));
                _thumbRect = new Rectangle(0, thumbPos, Width, thumbLength);
                _trackRect = new Rectangle(0, 0, Width, Height);
            }
            else
            {
                thumbPos = (int)((currentValue - Minimum) / (Maximum - Minimum) * (Width - thumbLength));
                _thumbRect = new Rectangle(thumbPos, 0, thumbLength, Height);
                _trackRect = new Rectangle(0, 0, Width, Height);
            }
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            base.OnPaint(e);
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            float visibility = _autoHide ? (float)_visibilityAnim.GetProgress() : 1f;
            if (visibility <= 0f)
                return; // tamamen gizli

            var baseTrackColor = TrackColor == Color.Transparent ? ColorScheme.BackColor2 : TrackColor;
            var blendedTrack = baseTrackColor.BlendWith(ColorScheme.ForeColor, 0.18f);
            var trackAlpha = (byte)(50 * visibility);
            var trackSk = blendedTrack.ToSKColor().WithAlpha(trackAlpha);

            using (var trackPaint = new SKPaint
            {
                Color = trackSk,
                IsAntialias = true
            })
            {
                var radius = Math.Max(0, _radius * ScaleFactor);
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), radius), trackPaint);
            }

            if (_thumbRect.IsEmpty) return;

            Color schemeBase = ThumbColor == Color.Transparent ? ColorScheme.BorderColor : ThumbColor;
            if (schemeBase == Color.Transparent)
                schemeBase = ColorScheme.ForeColor;

            Color stateColor;
            if (_isThumbPressed)
                stateColor = schemeBase.BlendWith(ColorScheme.ForeColor, 0.35f);
            else if (_isThumbHovered || _isHovered || _hostHovered)
                stateColor = schemeBase.BlendWith(ColorScheme.ForeColor, 0.25f);
            else
                stateColor = schemeBase.BlendWith(ColorScheme.BackColor, 0.15f);

            var thumbColor = stateColor.ToSKColor().WithAlpha((byte)(220 * Math.Clamp(visibility, 0f, 1f)));

            if (_useThumbShadow && visibility > 0f)
            {
                using var shadowPaint = new SKPaint
                {
                    Color = SKColors.Black.WithAlpha((byte)(30 * visibility)),
                    ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 2, 2, SKColors.Black.WithAlpha((byte)(70 * visibility))),
                    IsAntialias = true
                };
                var r = new SKRect(_thumbRect.X, _thumbRect.Y, _thumbRect.Right, _thumbRect.Bottom);
                var rad = Math.Max(0, _radius * ScaleFactor);
                canvas.DrawRoundRect(new SKRoundRect(r, rad), shadowPaint);
            }

            using (var thumbPaint = new SKPaint
            {
                Color = thumbColor,
                IsAntialias = true
            })
            {
                var r = new SKRect(_thumbRect.X, _thumbRect.Y, _thumbRect.Right, _thumbRect.Bottom);
                var rad = Math.Max(0, _radius * ScaleFactor);
                canvas.DrawRoundRect(new SKRoundRect(r, rad), thumbPaint);
            }

            // Debug
            if (ColorScheme.DrawDebugBorders)
            {
                using var dbg = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
                canvas.DrawRect(0, 0, Width - 1, Height - 1, dbg);
            }
        }

        private void ShowWithAutoHide()
        {
            if (!_autoHide) return;
            _visibilityAnim.StartNewAnimation(AnimationDirection.In);
            _hideTimer.Stop();
            _hideTimer.Interval = _hideDelay;
            _hideTimer.Start();
        }

        private void HideNow()
        {
            if (!_autoHide) return;
            if (_isHovered || _isDragging || _isThumbHovered) return;
            _hideTimer.Stop();
            _visibilityAnim.StartNewAnimation(AnimationDirection.Out);
        }

        internal override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                if (_thumbRect.Contains(e.Location))
                {
                    _isDragging = true;
                    _isThumbPressed = true;
                    _dragStartPoint = e.Location;
                    _dragStartValue = Value;
                }
                else
                {
                    if (IsVertical)
                    {
                        if (e.Y < _thumbRect.Y)
                            Value -= LargeChange;
                        else if (e.Y > _thumbRect.Bottom)
                            Value += LargeChange;
                    }
                    else
                    {
                        if (e.X < _thumbRect.X)
                            Value -= LargeChange;
                        else if (e.X > _thumbRect.Right)
                            Value += LargeChange;
                    }
                }
                ShowWithAutoHide();
                Invalidate();
            }
        }

        internal override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool oldThumbHovered = _isThumbHovered;
            _isThumbHovered = _thumbRect.Contains(e.Location);
            _isHovered = new Rectangle(Point.Empty, Size).Contains(e.Location);
            if (oldThumbHovered != _isThumbHovered)
                Invalidate();

            if (_isDragging)
            {
                int delta = IsVertical ? e.Y - _dragStartPoint.Y : e.X - _dragStartPoint.X;
                int trackLength = IsVertical ? Height - _thumbRect.Height : Width - _thumbRect.Width;
                if (trackLength <= 0) return;
                float valuePerPixel = (float)(Maximum - Minimum) / trackLength;
                int newValue = _dragStartValue + (int)(delta * valuePerPixel);
                Value = Math.Max(Minimum, Math.Min(Maximum, newValue));
                OnScroll(EventArgs.Empty);
            }

            ShowWithAutoHide();
        }

        internal override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                _isThumbPressed = false;
                Invalidate();
                if (_autoHide)
                {
                    _hideTimer.Stop();
                    _hideTimer.Start();
                }
            }
        }

        internal override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int scrollLines = SystemInformation.MouseWheelScrollLines;
            int delta = (e.Delta / 120) * scrollLines * SmallChange;
            Value -= delta;
            OnScroll(EventArgs.Empty);
            ShowWithAutoHide();
        }

        internal override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            ShowWithAutoHide();
        }

        internal override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            if (_autoHide)
            {
                _hideTimer.Stop();
                _hideTimer.Start();
            }
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
            ShowWithAutoHide();
        }

        protected virtual void OnScroll(EventArgs e)
        {
            Scroll?.Invoke(this, e);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return IsVertical ? new Size(_thickness, 100) : new Size(100, _thickness);
        }

        internal void SetHostHover(bool hovered)
        {
            _hostHovered = hovered;
            if (_autoHide)
            {
                if (hovered)
                {
                    _visibilityAnim.StartNewAnimation(AnimationDirection.In);
                    _hideTimer.Stop();
                }
                else
                {
                    _hideTimer.Stop();
                    _hideTimer.Interval = _hideDelay;
                    _hideTimer.Start();
                }
            }
            Invalidate();
        }
    }
}