using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

/// <summary>
/// Summary description for TabControl.
/// </summary>
[ToolboxBitmap(typeof(System.Windows.Forms.TabControl))] //,
                                                         //Designer(typeof(Designers.SDTabControlDesigner))]
public class TabControl : System.Windows.Forms.TabControl
{
    public TabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

        UpdateStyles();
    }

    public bool HideTabArea { get; set; }

    private Padding _border = new(1);
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
        GroupBoxRenderer.DrawParentBackground(e.Graphics, ClientRectangle, this);

        if (TabCount <= 0 || SelectedTab == null)
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
            //if (index != SelectedIndex)
            DrawTab(index, e.Graphics);

        //DrawTab(SelectedIndex, e.Graphics);

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
            //tabRect.Inflate(0, -2);

            if (tabpage.BackColor != ColorScheme.BackColor)
                tabpage.BackColor = ColorScheme.BackColor;

            if (index != SelectedIndex)
            {
                //graphics.FillRectangle(brush, tabRect);
            }
            else
            {
                brush.Color = ColorScheme.BorderColor;//.Alpha(20);

                using var path = InitializeTabPath(tabRect);
                graphics.FillPath(brush, path);
            }

            /*
            ControlPaint.DrawBorder(graphics, tabRect,
                              ColorScheme.BorderColor, Convert.ToInt32(index == 0), ButtonBorderStyle.Solid,
                              ColorScheme.BorderColor, 1, ButtonBorderStyle.Solid,
                              ColorScheme.BorderColor, 1, ButtonBorderStyle.Solid,
                              Color.Transparent, 0, ButtonBorderStyle.None);
            */
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

    private GraphicsPath InitializeTabPath(Rectangle tabBounds)
    {
        var path = new GraphicsPath();

        var spread = (int)Math.Floor((decimal)tabBounds.Width * 2 / 3);
        var eigth = (int)Math.Floor((decimal)tabBounds.Width * 1 / 8);
        var sixth = (int)Math.Floor((decimal)tabBounds.Width * 1 / 6);
        var quarter = (int)Math.Floor((decimal)tabBounds.Width * 1 / 4);

        path.AddCurve(new Point[] {  new Point(tabBounds.X, tabBounds.Bottom)
                                          ,new Point(tabBounds.X + sixth, tabBounds.Bottom - eigth)
                                          ,new Point(tabBounds.X + spread - quarter, tabBounds.Y + eigth)
                                          ,new Point(tabBounds.X + spread, tabBounds.Y)});

        path.AddLine(tabBounds.X + spread, tabBounds.Y, tabBounds.Right - spread, tabBounds.Y);

        path.AddCurve(new Point[] {  new Point(tabBounds.Right - spread, tabBounds.Y)
                                          ,new Point(tabBounds.Right - spread + quarter, tabBounds.Y + eigth)
                                          ,new Point(tabBounds.Right - sixth, tabBounds.Bottom - eigth)
                                          ,new Point(tabBounds.Right, tabBounds.Bottom)});

        path.CloseFigure();

        return path;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x1328 && !DesignMode && HideTabArea)
            m.Result = (IntPtr)1;
        else
            base.WndProc(ref m);
    }
}
