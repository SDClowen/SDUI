using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDUI.Extensions;

namespace SDUI.Controls
{
    public class Panel : System.Windows.Forms.Panel
    {
        private int _radius = 12;
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

        public Panel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            this.DoubleBuffered = true;
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);

            BackColor = ColorScheme.BackColor;
            ForeColor = ColorScheme.ForeColor;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //e.Graphics.Clear(BackColor);

            var rect = ClientRectangle;

            var color = Color.FromArgb(15, ColorScheme.BackColor.Determine());

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

            using (var pen = new Pen(ColorScheme.BorderColor, 1))
                e.Graphics.DrawRectangle(pen, rect);
        }
    }
}
