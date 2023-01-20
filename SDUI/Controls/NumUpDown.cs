using SDUI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class NumUpDown : Control
    {
        private const int LONG_PRESS_TIMER_INTERVAL = 250;

        public event EventHandler ValueChanged;

        private decimal _value;
        private decimal _min;
        private decimal _max;
        private int Xval;
        private bool _isUsingKeyboard;
        private Timer _longPressTimer = new();

        public decimal Value
        {
            get
            {
                return _value;
            }
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
            get
            {
                return _min;
            }
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
            get
            {
                return _max;
            }
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
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
            _min = 0;
            _max = 100;
            Font = new Font("Segoe UI", 9.25f);
            Size = new Size(80, 25);
            MinimumSize = Size;
            DoubleBuffered = true;

            _longPressTimer.Tick += LongPressTimer_Tick;
            _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Xval = e.Location.X;
            Invalidate();

            if (e.X < Width - 50)
            {
                Cursor = Cursors.IBeam;
            }
            else
            {
                Cursor = Cursors.Default;
            }
            if (e.X > Width - 25 && e.X < Width - 10)
            {
                Cursor = Cursors.Default;
            }
            if (e.X > Width - 44 && e.X < Width - 33)
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClickButton()
        {
            if (Xval > Width - 25 && Xval < Width - 10)
            {
                if (_value + 1 <= _max)
                {
                    Value++;
                }
            }
            else
            {
                if (Xval > Width - 44 && Xval < Width - 33)
                {
                    if (_value - 1 >= _min)
                    {
                        Value--;
                    }
                }
                _isUsingKeyboard = !_isUsingKeyboard;
            }
            Focus();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            ClickButton();
            _longPressTimer.Start();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _longPressTimer.Stop();
            _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;
        }

        private void LongPressTimer_Tick(object sender, EventArgs e)
        {
            ClickButton();

            if(_longPressTimer.Interval == LONG_PRESS_TIMER_INTERVAL)
                _longPressTimer.Interval = 50;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
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

        protected override void OnKeyUp(KeyEventArgs e)
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

        protected override void OnMouseWheel(MouseEventArgs e)
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

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            var graphics = e.Graphics;
            ButtonRenderer.DrawParentBackground(e.Graphics, Bounds, this);

            using var borderPen = new Pen(ColorScheme.BorderColor);
            using var backColorBrush = ColorScheme.BackColor.Brush();
            using var foreColorBrush = ColorScheme.ForeColor.Brush();

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var round = ClientRectangle.Radius(8);
            graphics.FillPath(backColorBrush, round);
            graphics.DrawPath(borderPen, round);

            using var plusMinusFont = new Font("Tahoma", 12.75f, FontStyle.Bold);
            graphics.DrawString("+", plusMinusFont, foreColorBrush, Width - 22, 1);
            graphics.DrawLine(borderPen, Width - 25, 1, Width - 25, Height - 2);
            graphics.DrawString("-", plusMinusFont, foreColorBrush, Width - 41, 1);
            graphics.DrawLine(borderPen, Width - 45, 1, Width - 45, Height - 2);

            TextRenderer.DrawText(graphics, Value.ToString(), Font, new Rectangle(1, 0, Width - 1, Height - 1), ColorScheme.ForeColor, TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }
}
