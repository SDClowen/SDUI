using SDUI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class UIWindowBase : Form
{
    protected bool enableFullDraggable;
    private int dwmMargin = 1;
    private bool right = false;
    private Point location;


    public int DwmMargin
    {
        get => dwmMargin;
        set => dwmMargin = value;
    }

    /// <summary>
    /// Get DPI
    /// </summary>
    public float DPI => DeviceDpi / 96.0f;

    /// <summary>
    /// Has aero enabled by windows <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _aeroEnabled
    {
        get
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return enabled == 1;
            }

            return false;
        }
    }

    public UIWindowBase()
    {
        //BackColor = Color.FromArgb(0, 0, 0, 0);

        SetStyle(ControlStyles.UserPaint, true);
        UpdateStyles();
        ResizeRedraw = true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (msg.Msg == 256 | msg.Msg == 260)
            if (keyData == Keys.Escape)
                Close();

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        right = e.Button == MouseButtons.Right;
        if (right)
        {
            location = e.Location;
            Invalidate();
        }

        if (!enableFullDraggable)
            return;

        if (e.Button == MouseButtons.Left)
            DragForm(Handle);
    }

    protected void DragForm(IntPtr handle)
    {
        ReleaseCapture();
        SendMessage(handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            if (!_aeroEnabled)
                cp.ClassStyle |= CS_DROPSHADOW;

            cp.ClassStyle |= CS_DBLCLKS;

            var style = (uint)cp.Style;
            //style &= ~(uint)SetWindowLongFlags.WS_CAPTION;
            style &= ~(uint)SetWindowLongFlags.WS_SYSMENU;
            style &= ~(uint)SetWindowLongFlags.WS_THICKFRAME;
            style &= ~(uint)SetWindowLongFlags.WS_MINIMIZE;
            style &= ~(uint)SetWindowLongFlags.WS_MINIMIZEBOX;
            style &= ~(uint)SetWindowLongFlags.WS_MAXIMIZE;
            style &= ~(uint)SetWindowLongFlags.WS_MAXIMIZEBOX;
            style |= (uint)SetWindowLongFlags.WS_TILED;
            cp.Style = (int)style;

            return cp;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (DesignMode)
            return;

        // Otherwise, it will not be applied.
        if (StartPosition == FormStartPosition.CenterScreen)
            CenterToScreen();

        if (BackColor != ColorScheme.BackColor)
            BackColor = ColorScheme.BackColor;
    }

    private const int htLeft = 10;
    private const int htRight = 11;
    private const int htTop = 12;
    private const int htTopLeft = 13;
    private const int htTopRight = 14;
    private const int htBottom = 15;
    private const int htBottomLeft = 16;
    private const int htBottomRight = 17;
    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case 0x84:
                {  
                    if (WindowState != FormWindowState.Maximized)
                    {
                        int gripDist = 10;
                        //int x = (int)(m.LParam.ToInt64() & 0xFFFF);
                        //int x = Cursor.Position.X;
                        // int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);
                        //Console.WriteLine(x);
                        Point pt = PointToClient(Cursor.Position);
                        //Console.WriteLine(pt);
                        Size clientSize = ClientSize;
                        ///allow resize on the lower right corner
                        if (pt.X >= clientSize.Width - gripDist && pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htBottomLeft : htBottomRight);
                            return;
                        }
                        ///allow resize on the lower left corner
                        if (pt.X <= gripDist && pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htBottomRight : htBottomLeft);
                            return;
                        }
                        ///allow resize on the upper right corner
                        if (pt.X <= gripDist && pt.Y <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htTopRight : htTopLeft);
                            return;
                        }
                        ///allow resize on the upper left corner
                        if (pt.X >= clientSize.Width - gripDist && pt.Y <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htTopLeft : htTopRight);
                            return;
                        }
                        ///allow resize on the top border
                        if (pt.Y <= 2 && clientSize.Height >= 2)
                        {
                            m.Result = (IntPtr)htTop;
                            return;
                        }
                        ///allow resize on the bottom border
                        if (pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htBottom;
                            return;
                        }
                        ///allow resize on the left border
                        if (pt.X <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htLeft;
                            return;
                        }
                        ///allow resize on the right border
                        if (pt.X >= clientSize.Width - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htRight;
                            return;
                        }
                    }


                    break;
                }
            case WM_NCCALCSIZE:
                if (FormBorderStyle != FormBorderStyle.None && m.WParam.ToInt32() == 1)
                {
                    m.Result = new IntPtr(0xF0);
                    return;
                }
                else
                    break;
        }


        if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)     // drag the form
            m.Result = (IntPtr)HTCAPTION;

        base.WndProc(ref m);
    }
    public void ChangeControlsTheme(Control control)
    {
        var isDark = ColorScheme.BackColor.IsDark();
        if (control is ListView)
        {
            if (WindowsHelper.IsModern)
            {
                SendMessage(control.Handle, 0x0127, (1 << 16) | (1 & 0xffff), 0);

                AllowDarkModeForWindow(control.Handle, true);
            }
        }

        if (control is RichTextBox || control is ListBox)
        {
            control.BackColor = ColorScheme.BackColor;
            control.ForeColor = ColorScheme.ForeColor;
        }

        WindowsHelper.UseImmersiveDarkMode(control.Handle, isDark);

        foreach (Control subControl in control.Controls)
        {
            ChangeControlsTheme(subControl);
        }
    }
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (DesignMode)
            return;

        WindowsHelper.ApplyRoundCorner(this.Handle);
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (DesignMode)
            return;
        ChangeControlsTheme(this);
        ForeColor = ColorScheme.ForeColor;
        if (_aeroEnabled)
        {
            var v = 2;

            DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, ref v, 4);
            var margins = new MARGINS()
            {
                Bottom = dwmMargin,
                Left = dwmMargin,
                Right = dwmMargin,
                Top = dwmMargin
            };

            DwmExtendFrameIntoClientArea(this.Handle, ref margins);

        }

        WindowsHelper.UseImmersiveDarkMode(Handle, ColorScheme.BackColor.IsDark());
        if (!WindowsHelper.IsModern)
            return;

        //EnableAcrylic(this, Color.FromArgb(0,0,0,0));

        var flag = DWMSBT_TABBEDWINDOW;
        DwmSetWindowAttribute(
            Handle,
            DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
            ref flag,
            sizeof(int));
    }
}