using SDUI.Extensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class Panel : System.Windows.Forms.Panel
    {
        private int _radius = 1;
        public int Radius
        {
            get => _radius;
            set
            {
                _radius = value;

                Invalidate();
            }
        }

        private Padding _border;
        public Padding Border
        {
            get => _border;
            set
            {
                if (_border == value)
                    return;

                _border = value;
                Invalidate();
            }
        }

        private Color _borderColor = Color.Transparent;
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor == value)
                    return;

                _borderColor = value;
                Invalidate();
            }
        }

        public override Color BackColor 
        { 
            get => Color.Transparent; 
            set => base.BackColor = value; 
        }

        public Panel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            UpdateStyles();
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = ClientRectangle;

            var color = ColorScheme.ForeColor.Alpha(15);
            var borderColor = _borderColor == Color.Transparent ? ColorScheme.BorderColor : _borderColor;

            if (_radius > 0)
            {
                using (var path = rect.Radius(Radius))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    using (var brush = new SolidBrush(color))
                        e.Graphics.FillPath(brush, path);

                    using (var pen = new Pen(borderColor, 1))
                        e.Graphics.DrawPath(pen, path);
                }

                return;
            }

            using (var brush = new SolidBrush(color))
                e.Graphics.FillRectangle(brush, rect);

            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                  borderColor, _border.Left, ButtonBorderStyle.Solid,
                                  borderColor, _border.Top, ButtonBorderStyle.Solid,
                                  borderColor, _border.Right, ButtonBorderStyle.Solid,
                                  borderColor, _border.Bottom, ButtonBorderStyle.Solid);
        }
    }
}
