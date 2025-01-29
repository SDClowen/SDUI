using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public enum ScrollOrientation
    {
        Vertical,
        Horizontal
    }

    public class ScrollBar : UIElementBase
    {
        private int _value;
        private int _minimum;
        private int _maximum = 100;
        private int _largeChange = 10;
        private int _smallChange = 1;
        private bool _isDragging;
        private Point _dragStartPoint;
        private int _dragStartValue;
        private bool _isHovered;
        private bool _isThumbHovered;
        private bool _isThumbPressed;
        private Rectangle _thumbRect;
        private Rectangle _trackRect;
        private ScrollOrientation _orientation = ScrollOrientation.Vertical;

        public event EventHandler ValueChanged;
        public event EventHandler Scroll;

        [DefaultValue(ScrollOrientation.Vertical)]
        public ScrollOrientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value) return;
                _orientation = value;
                Size = IsVertical ? new Size(12, 100) : new Size(100, 12);
                UpdateThumbRect();
                Invalidate();
            }
        }

        public bool IsVertical => Orientation == ScrollOrientation.Vertical;

        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                value = Math.Max(Minimum, Math.Min(Maximum, value));
                if (_value == value) return;
                _value = value;
                OnValueChanged(EventArgs.Empty);
                UpdateThumbRect();
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

        public ScrollBar()
        {
            Size = IsVertical ? new Size(12, 100) : new Size(100, 12);
            BackColor = Color.FromArgb(40, 40, 40);
            Cursor = Cursors.Default;
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

            if (IsVertical)
            {
                thumbPos = (int)((float)(Value - Minimum) / (Maximum - Minimum) * (Height - thumbLength));
                _thumbRect = new Rectangle(0, thumbPos, Width, thumbLength);
                _trackRect = new Rectangle(0, 0, Width, Height);
            }
            else
            {
                thumbPos = (int)((float)(Value - Minimum) / (Maximum - Minimum) * (Width - thumbLength));
                _thumbRect = new Rectangle(thumbPos, 0, thumbLength, Height);
                _trackRect = new Rectangle(0, 0, Width, Height);
            }
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            base.OnPaint(e);

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            // Track çizimi
            using (var paint = new SKPaint
            {
                Color = new SKColor((byte)(BackColor.R), (byte)(BackColor.G), (byte)(BackColor.B), (byte)(BackColor.A)),
                IsAntialias = true
            })
            {
                // Track arkaplanı
                canvas.DrawRoundRect(
                    new SKRoundRect(new SKRect(0, 0, Width, Height), 6),
                    paint
                );
            }

            if (_thumbRect.IsEmpty) return;

            // Thumb çizimi
            using (var paint = new SKPaint
            {
                Color = _isThumbPressed ?
                    new SKColor(100, 100, 100) :
                    _isThumbHovered ?
                        new SKColor(90, 90, 90) :
                        new SKColor(80, 80, 80),
                IsAntialias = true
            })
            {
                canvas.DrawRoundRect(
                    new SKRoundRect(
                        new SKRect(
                            _thumbRect.X,
                            _thumbRect.Y,
                            _thumbRect.Right,
                            _thumbRect.Bottom
                        ),
                        4
                    ),
                    paint
                );
            }
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
                    // Track'e tıklandığında thumb'ı o noktaya taşı
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
                Invalidate();
            }
        }

        internal override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool oldThumbHovered = _isThumbHovered;
            _isThumbHovered = _thumbRect.Contains(e.Location);

            if (oldThumbHovered != _isThumbHovered)
                Invalidate();

            if (_isDragging)
            {
                int delta = IsVertical ? e.Y - _dragStartPoint.Y : e.X - _dragStartPoint.X;
                int trackLength = IsVertical ? Height - _thumbRect.Height : Width - _thumbRect.Width;
                float valuePerPixel = (float)(Maximum - Minimum) / trackLength;
                int newValue = _dragStartValue + (int)(delta * valuePerPixel);

                Value = Math.Max(Minimum, Math.Min(Maximum, newValue));
                OnScroll(EventArgs.Empty);
            }
        }

        internal override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                _isThumbPressed = false;
                Invalidate();
            }
        }

        internal override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int scrollLines = SystemInformation.MouseWheelScrollLines;
            int delta = (e.Delta / 120) * scrollLines * SmallChange;
            Value -= delta;
            OnScroll(EventArgs.Empty);
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected virtual void OnScroll(EventArgs e)
        {
            Scroll?.Invoke(this, e);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return IsVertical ? new Size(12, 100) : new Size(100, 12);
        }
    }
}