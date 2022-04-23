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
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = ClientRectangle;

            var color = Color.FromArgb(15, ColorScheme.ForeColor);

            if (_radius > 0)
            {
                using (var path = rect.Radius(Radius))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    using (var brush = new SolidBrush(color))
                        e.Graphics.FillPath(brush, path);

                    using (var pen = new Pen(ColorScheme.BorderColor, 1))
                        e.Graphics.DrawPath(pen, path);
                }

                return;
            }

            using (var brush = new SolidBrush(color))
                e.Graphics.FillRectangle(brush, rect);

            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                  ColorScheme.BorderColor, _border.Left, ButtonBorderStyle.Solid,
                                  ColorScheme.BorderColor, _border.Top, ButtonBorderStyle.Solid,
                                  ColorScheme.BorderColor, _border.Right, ButtonBorderStyle.Solid,
                                  ColorScheme.BorderColor, _border.Bottom, ButtonBorderStyle.Solid);
        }
    }
}
