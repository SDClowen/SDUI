using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public partial class InfoControl : UserControl
    {
        public InfoControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            Font = new Font("Segoe UI Semibold", 13.37f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var brush = new SolidBrush(Color.FromArgb(33, 150, 243));
            var pen = new Pen(Color.FromArgb(33, 100, 210));
            pen.Width = 4;
            pen.DashCap = DashCap.Triangle;
            pen.DashStyle = DashStyle.DashDotDot;

            e.Graphics.FillRectangle(brush, 0, 0, Width, Height);
            e.Graphics.DrawRectangle(pen, 0, 0, Width, Height);

            var size = TextRenderer.MeasureText(Text, Font);
            size.Width += 100;
            size.Height += 50;
            Size = size;

            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Color.White, flags);

            brush.Dispose();
            pen.Dispose();
        }
    }
}