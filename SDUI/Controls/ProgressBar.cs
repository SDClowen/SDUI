using SDUI.Extensions;
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

        private int _radius = 0;
        public int Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
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

        public override Color BackColor 
        { 
            get => Color.Transparent; 
            set => base.BackColor = Color.Transparent; 
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
            var intValue = (int)((1.0 * _value / _maximum) * Width);
            var percent = Math.Round((100.0 * Value) / Maximum, _percentIndices);

            using (var bitmap = new Bitmap(Width, Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.Clear(BackColor);

                    var linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, (intValue <= 0 ? intValue - 1 : 1), Height - 1), _gradient[0], _gradient[1], 90);
                    var hatchBrush = new HatchBrush(HatchType, Color.FromArgb(50, _gradient[0]), Color.FromArgb(50, _gradient[1]));

                    if (Radius > 0)
                    {
                        var backPath = new Rectangle(0, 0, Width, Height).Radius(_radius);
                        var valuePath = new Rectangle(0, 0, intValue, Height).Radius(_radius);

                        graphics.FillPath(new SolidBrush(ColorScheme.BorderColor), backPath);
                        graphics.FillPath(linearGradientBrush, valuePath);
                        backPath.Dispose();
                        valuePath.Dispose();

                        graphics.FillPath(hatchBrush, new Rectangle(0, 0, intValue - 1, Height - 2).Radius(_radius));
                        graphics.DrawPath(new Pen(Color.FromArgb(40, Parent.BackColor.Determine())), new Rectangle(0, 0, Width - 1, Height - 1).Radius(_radius));
                    }
                    else
                    {

                        graphics.FillRectangle(new SolidBrush(ColorScheme.BorderColor), 0, 0, Width, Height);
                        graphics.FillRectangle(linearGradientBrush, 0, 0, intValue, Height);
                        graphics.FillRectangle(linearGradientBrush, 0, 0, intValue - 1, Height - 2);

                        if (_drawHatch)
                            graphics.FillRectangle(hatchBrush, 0, 0, intValue, Height);

                        graphics.DrawRectangle(new Pen(ColorScheme.BorderColor), 0, 0, Width - 1, Height - 1);
                    }
                }

                e.Graphics.DrawImage(bitmap, 0, 0);



                if (ShowValue)
                {
                    e.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                    var shadowColor = ColorScheme.ForeColor.Determine();
                    var textColor = ColorScheme.ForeColor;
                    Text = _showAsPercent ? $"{percent}%" : $"{_value} / {_maximum}";

                    if (percent > 50)
                    {
                        textColor = Color.White;
                        shadowColor = Color.Black;
                    }

                    // draw shadow
                    var shadowBrush = new SolidBrush(shadowColor);
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
}
