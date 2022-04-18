using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class ProgressBar : Control
    {
        private long _value = 0;
        public long Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Invalidate();
            }
        }

        private long _maximum = 100;
        public long Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                if (value > 0)
                    _maximum = value;
                else
                    _maximum = 1;

                Invalidate();
            }
        }

        private Color _backGradientBegin;
        public Color BackgroundColorGradiendBegin
        {
            get => _backGradientBegin;
            set
            {
                _backGradientBegin = value;
                Invalidate();
            }
        }

        private Color _backGradientEnd;
        public Color BackgroundColorGradiendEnd
        {
            get => _backGradientEnd;
            set
            {
                _backGradientEnd = value;
                Invalidate();
            }
        }

        private bool _showAsPercent = false;
        public bool ShowAsPercent
        {
            get { return _showAsPercent; }
            set
            {
                _showAsPercent = value;
                Invalidate();
            }
        }

        private int _percentIndices = 2;
        public int PercentIndices
        {
            get { return _percentIndices; }
            set
            {
                _percentIndices = value;
                Invalidate();
            }
        }

        private bool _showValue = false;
        public bool ShowValue
        {
            get { return _showValue; }
            set
            {
                _showValue = value;
                Invalidate();
            }
        }

        public ProgressBar()
           : base()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;

            var intValue = (int)((1.0 * _value / _maximum) * Width);
            var percent = Math.Round((100.0 * Value) / Maximum, _percentIndices);

            graphics.Clear(ColorScheme.BackColor);

            graphics.FillRectangle(new SolidBrush(ColorScheme.BorderColor), 0, 0, Width, Height);
            var linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, (intValue <= 0 ? intValue - 1 : 1), Height - 1), BackgroundColorGradiendBegin, BackgroundColorGradiendEnd, 90);

            graphics.FillRectangle(linearGradientBrush, 0, 0, intValue, Height);
            graphics.DrawRectangle(new Pen(ColorScheme.BorderColor), 0, 0, Width - 1, Height - 1);

            if (ShowValue)
            {
                graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                var shadowColor = ColorScheme.ForeColor.Determine();
                var textColor = ColorScheme.ForeColor;
                Text = _showAsPercent ? $"{percent}%" : $"{_value} / {_maximum}";

                if (percent > 50)
                {
                    textColor = textColor.Determine();
                    shadowColor = shadowColor.Determine();
                }

                // draw shadow
                var shadowBrush = new SolidBrush(shadowColor);
                graphics.DrawString(Text, Font, shadowBrush, new Rectangle(1, 1, Width, Height), new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                });

                shadowBrush.Dispose();
                // draw text
                var textBrush = new SolidBrush(textColor);
                graphics.DrawString(Text, Font, textBrush, new Rectangle(0, 0, Width, Height), new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                });
            }
        }
    }
}
