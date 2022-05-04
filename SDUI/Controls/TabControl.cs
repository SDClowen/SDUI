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
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            UpdateStyles();
        }

        public bool HideTabArea { get; set; }

        private Padding _border = new Padding(1);
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
            var bounds = SelectedTab.Bounds;

            //Draw a border around TabPage
            bounds.Inflate(3, 3);

            var brush = new SolidBrush(ColorScheme.BackColor.Alpha(200));

            e.Graphics.FillRectangle(brush, bounds);

            ControlPaint.DrawBorder(e.Graphics, bounds,
                                  ColorScheme.BorderColor, _border.Left, ButtonBorderStyle.Solid,
                                  ColorScheme.BorderColor, _border.Top, ButtonBorderStyle.Solid,
                                  ColorScheme.BorderColor, _border.Right, ButtonBorderStyle.Solid,
                                  ColorScheme.BorderColor, _border.Bottom, ButtonBorderStyle.Solid);

            for (int index = 0; index <= TabCount - 1; index++)
                if (index != SelectedIndex)
                    DrawTab(index, e.Graphics);

            DrawTab(SelectedIndex, e.Graphics);

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
                    brush.Color = ColorScheme.ForeColor.Alpha(20);
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

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !DesignMode && HideTabArea) 
                m.Result = (IntPtr)1;
            else
                base.WndProc(ref m);
        }
    }
}
