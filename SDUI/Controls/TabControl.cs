using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    /// <summary>
    /// Summary description for TabControl.
    /// </summary>
    public class TabControl : System.Windows.Forms.TabControl
    {
        public TabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(ColorScheme.BackColor);

            if (TabCount <= 0)
                return;

            //Draw a custom background for Transparent TabPages
            var r = SelectedTab.Bounds;

            //Draw a border around TabPage
            r.Inflate(3, 3);

            var brush = new SolidBrush(Color.FromArgb(200, ColorScheme.BackColor));
            var pen = new Pen(ColorScheme.BorderColor);

            e.Graphics.FillRectangle(brush, r);
            e.Graphics.DrawRectangle(pen, r);


            for (int index = 0; index <= TabCount - 1; index++)
                if (index != SelectedIndex)
                    DrawTab(index, e.Graphics);

            DrawTab(SelectedIndex, e.Graphics);

            pen.Dispose();
            brush.Dispose();
        }

        private void DrawTab(int index, Graphics graphics)
        {
            var tabpage = TabPages[index];
            var brush = new SolidBrush(ColorScheme.BackColor);
            var pen = new Pen(ColorScheme.BorderColor);

            var r = GetTabRect(index);

            tabpage.BackColor = ColorScheme.BackColor;
            brush.Color = tabpage.BackColor;

            if (index != SelectedIndex)
            {
                graphics.FillRectangle(brush, r);
                graphics.DrawRectangle(pen, r);
            }
            else
            {
                graphics.FillRectangle(pen.Brush, r);
                graphics.DrawRectangle(pen, r);
            }

            brush.Color = ColorScheme.BackColor.Determine();

            //Set up rotation for left and right aligned tabs
            if (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right)
            {
                float RotateAngle = 90;
                if (Alignment == TabAlignment.Left) RotateAngle = 270;
                PointF cp = new PointF(r.Left + (r.Width >> 1), r.Top + (r.Height >> 1));
                graphics.TranslateTransform(cp.X, cp.Y);
                graphics.RotateTransform(RotateAngle);
                r = new Rectangle(-(r.Height >> 1), -(r.Width >> 1), r.Height, r.Width);
            }

            //Draw the Tab Text
            if (tabpage.Enabled)
                TextRenderer.DrawText(graphics, tabpage.Text, Font, r, brush.Color, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            else
            {
                var stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                ControlPaint.DrawStringDisabled(graphics, tabpage.Text, Font, tabpage.BackColor, (RectangleF)r, stringFormat);
            }

            graphics.ResetTransform();

            brush.Dispose();
            pen.Dispose();
        }
    }
}
