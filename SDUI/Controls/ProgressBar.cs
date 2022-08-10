using SDUI.Extensions;
using SDUI.Helpers;
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

        private Color[] _gradient = new Color[2];
        public Color[] Gradient
        {
            get => _gradient;
            set
            {
                _gradient = value;
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

        private int _radius = 4;
        public int Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                return;
                _radius = value <= 0 ? 1 : value;
                Invalidate();
            }
        }

        private bool _drawHatch = false;
        public bool DrawHatch
        {
            get { return _drawHatch; }
            set
            {
                _drawHatch = value;
                Invalidate();
            }
        }

        private HatchStyle _hatchType = HatchStyle.Min;
        public HatchStyle HatchType
        {
            get
            {
                return _hatchType;
            }
            set
            {
                _hatchType = value;
                Invalidate();
            }
        }

        public ProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            ButtonRenderer.DrawParentBackground(graphics, ClientRectangle, this);

            var intValue = ((1.0f * _value / _maximum) * Width);
            var percent = ((100.0f * Value) / Maximum);

            var linearGradientBrush = new LinearGradientBrush(new RectangleF(0, 0, (intValue <= 0 ? intValue - 1 : 1), Height), _gradient[0], _gradient[1], 90);
            var hatchBrush = new HatchBrush(HatchType, Color.FromArgb(50, _gradient[0]), Color.FromArgb(50, _gradient[1]));

            var rect = ClientRectangle.ToRectangleF();

            using (var path = rect.Radius(_radius))
                graphics.FillPath(new SolidBrush(ColorScheme.BorderColor), path);

            var rectValue = new RectangleF(rect.X, rect.Y, intValue, rect.Height - 1);
            using (var path = rectValue.Radius(_radius))
            {
                graphics.FillPath(linearGradientBrush, path);
                graphics.FillPath(hatchBrush, path);
            }

            graphics.DrawPath(new Pen(Color.FromArgb(10, Parent.BackColor.Determine())), new Rectangle(0, 0, Width - 1, Height - 1).Radius(_radius));

            if (ShowValue)
            {
                e.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                var textShadowColor = ColorScheme.ForeColor.Determine();
                var textColor = ColorScheme.ForeColor;

                Text = _showAsPercent ?
                    percent.ToString($"0.{"0".PadRight(_percentIndices, '0')}") + "%" :
                    $"{_value} / {_maximum}";

                if (percent > 50)
                {
                    textColor = Color.White;
                    textShadowColor = Color.Black;
                }

                // draw shadow
                var shadowBrush = new SolidBrush(textShadowColor);
                e.Graphics.DrawString(Text, Font, shadowBrush, new Rectangle(1, 1, Width, Height), new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                });

                shadowBrush.Dispose();
                // draw text
                var textBrush = new SolidBrush(textColor);
                e.Graphics.DrawString(Text, Font, textBrush, new Rectangle(0, 0, Width, Height), new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                });
            }
        }

    }
}
