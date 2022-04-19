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

        public Panel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = ClientRectangle;

            using (var path = rect.Radius(Radius))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                rect = new Rectangle(0, 0,
                    rect.Width, Font.Height + 7);

                var color = Color.FromArgb(20, ColorScheme.BorderColor);
                using (var brush = new SolidBrush(color))
                    e.Graphics.FillPath(brush, path);

                using (var pen = new Pen(ColorScheme.BorderColor, 1))
                    e.Graphics.DrawPath(pen, path);
            }
        }
    }
}
