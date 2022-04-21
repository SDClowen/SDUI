using System;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Extensions;

namespace SDUI.Controls
{
    /// <summary>
    /// Summary description for TabControl.
    /// </summary>
    public class TabControl : System.Windows.Forms.TabControl
    {
        public TabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
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
            try
            {
                var tabpage = TabPages[index];
                var brush = new SolidBrush(ColorScheme.BackColor);
                var pen = new Pen(ColorScheme.BorderColor);
               
                var tabRect = GetTabRect(index);
                
                if(tabpage.BackColor != ColorScheme.BackColor)
                    tabpage.BackColor = ColorScheme.BackColor;

                if (index != SelectedIndex)
                {
                    //graphics.FillRectangle(brush, tabRect);
                }
                else
                {
                    brush.Color = Color.FromArgb(20, ColorScheme.ForeColor);
                    graphics.FillRectangle(brush, tabRect);
                }

                ControlPaint.DrawBorder(graphics, tabRect,
                                  ColorScheme.BorderColor, Convert.ToInt32(index == 0), ButtonBorderStyle.Solid,
                                  ColorScheme.BorderColor, 1, ButtonBorderStyle.Solid,
                                  ColorScheme.BorderColor, 1, ButtonBorderStyle.Solid,
                                  Color.Transparent, 0, ButtonBorderStyle.None);

                //Set up rotation for left and right aligned tabs
                if (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right)
                {
                    var angle = 90f;
                    if (Alignment == TabAlignment.Left) 
                        angle = 270;

                    var point = new PointF(tabRect.Left + (tabRect.Width >> 1), tabRect.Top + (tabRect.Height >> 1));
                    graphics.TranslateTransform(point.X, point.Y);
                    graphics.RotateTransform(angle);
                    tabRect = new Rectangle(-(tabRect.Height >> 1), -(tabRect.Width >> 1), tabRect.Height, tabRect.Width);
                }

                //Draw the Tab Text
                //if (tabpage.Enabled)
                    TextRenderer.DrawText(graphics, tabpage.Text, Font, tabRect, ColorScheme.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                /*else
                {
                    var stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    ControlPaint.DrawStringDisabled(graphics, tabpage.Text, Font, tabpage.BackColor, (RectangleF)tabRect, stringFormat);
                }*/

                graphics.ResetTransform();

                brush.Dispose();
                pen.Dispose();
            }
            catch
            {
            }
        }
    }
}
