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

            cp.Style |= WS_MINIMIZEBOX | WS_SYSMENU | WS_SIZEBOX;
            //cp.ClassStyle |= CS_DBLCLKS;

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

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case 0x84:
            {  // Trap WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                pos = this.PointToClient(pos);
                if (pos.X >= this.ClientSize.Width - 5 && pos.Y >= this.ClientSize.Height - 5)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
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

        base.WndProc(ref m);

        //if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)     // drag the form
        //  m.Result = (IntPtr)HTCAPTION;
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