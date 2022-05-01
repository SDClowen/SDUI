using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class ShapeProgressBar : Control
    {
        private long _value;
        public long Value
        {
            get => _value;
            set
            {
                if (value > _maximum)
                    value = _maximum;

                _value = value;
                Invalidate();
            }
        }

        private long _maximum = 100;
        public long Maximum
        {
            get => _maximum;
            set
            {
                if (value < 1)
                    value = 1;

                _maximum = value;
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetStandardSize();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SetStandardSize();
        }

        public ShapeProgressBar()
        {
            Size = new Size(130, 130);
            Font = new Font("Segoe UI", 15);
            MinimumSize = new Size(100, 100);
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        private void SetStandardSize()
        {
            var size = Math.Max(Width, Height);
            Size = new Size(size, size);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var bitmap = new Bitmap(Width, Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.Clear(Color.Transparent);

                    var calc = (int)Math.Round((double)((360.0 / _maximum) * _value));

                    using (var brush = new LinearGradientBrush(ClientRectangle, _gradient[0], _gradient[1], LinearGradientMode.ForwardDiagonal))
                    {
                        using (var pen = new Pen(brush, 14f))
                        {
                            pen.StartCap = LineCap.Round;
                            pen.EndCap = LineCap.Round;
                            graphics.DrawArc(pen, 18, 18, (Width - 35) - 2, (Height - 35) - 2, -90, calc);
                        }
                    }

                    if (_drawHatch)
                    {
                        using (var hatchBrush = new HatchBrush(HatchType, Color.FromArgb(50, _gradient[0]), Color.FromArgb(50, _gradient[1])))
                        {
                            using (var pen = new Pen(hatchBrush, 14f))
                            {
                                pen.StartCap = LineCap.Round;
                                pen.EndCap = LineCap.Round;
                                graphics.DrawArc(pen, 18, 18, (Width - 35) - 2, (Height - 35) - 2, -90, calc);
                            }
                        }
                    }

                    using (var brush = new LinearGradientBrush(ClientRectangle, ColorScheme.BackColor, ColorScheme.BorderColor, LinearGradientMode.Vertical))
                        graphics.FillEllipse(brush, 24, 24, (Width - 48) - 1, (Height - 48) - 1);

                    var percent = (100 / _maximum) * _value;
                    var percentString = percent.ToString();
                    var stringSize = graphics.MeasureString(percentString, Font);

                    using (var textBrush = new SolidBrush(ColorScheme.ForeColor))
                        graphics.DrawString(percentString, Font, textBrush, Width / 2 - stringSize.Width / 2, Height / 2 - stringSize.Height / 2);
                }

                e.Graphics.DrawImage(bitmap, 0, 0);
            }
        }
    }
}
