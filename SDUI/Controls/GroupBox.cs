using SDUI.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class GroupBox : System.Windows.Forms.GroupBox
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

        public GroupBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(3, 10, 3, 3);
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

                var clip = e.Graphics.ClipBounds;
                e.Graphics.SetClip(rect);
                e.Graphics.DrawLine(new Pen(color), 0, rect.Height - 1, rect.Width, rect.Height - 1);
                e.Graphics.FillPath(new SolidBrush(color), path);
                
                TextRenderer.DrawText(e.Graphics, Text, Font, rect, ColorScheme.ForeColor);
                e.Graphics.SetClip(clip);

                using (var pen = new Pen(ColorScheme.BorderColor, 1))
                    e.Graphics.DrawPath(pen, path);
            }
        }
    }
}
