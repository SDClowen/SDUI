using SDUI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class UIWindowBase : Form
{
    protected bool enableFullDraggable;

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

            cp.Style |= WS_MINIMIZEBOX;
            cp.ClassStyle |= CS_DBLCLKS;

            return cp;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        //BackColor = ColorScheme.BackColor;

        if (DesignMode)
            return;

        // Otherwise, it will not be applied.
        if (StartPosition == FormStartPosition.CenterScreen)
            CenterToScreen();
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_NCCALCSIZE:
                if (!DesignMode && FormBorderStyle != FormBorderStyle.None)
                    return;
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

                var headerControlHandle = GetHeaderControl(control as ListView);

                AllowDarkModeForWindow(headerControlHandle, true);
                AllowDarkModeForWindow(control.Handle, true);

                SetWindowTheme(headerControlHandle, "ItemsView", null);
                SetWindowTheme(control.Handle, "ItemsView", null);
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

        var attribute = (int)DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
        var preference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
        DwmSetWindowAttribute(this.Handle, attribute, ref preference, sizeof(uint));
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
            DwmSetWindowAttribute(Handle, 2, ref v, 4);
            var margins = new MARGINS()
            {
                Bottom = 1,
                Left = 1,
                Right = 1,
                Top = 1
            };

            DwmExtendFrameIntoClientArea(this.Handle, ref margins);

        }

        WindowsHelper.UseImmersiveDarkMode(Handle, true);
        if (!WindowsHelper.IsModern)
            return;

        //EnableAcrylic(this, Color.FromArgb(0,0,0,0));

        var flag = DWMSBT_TABBEDWINDOW;
        DwmSetWindowAttribute(
            Handle,
            DWMWA_SYSTEMBACKDROP_TYPE,
            ref flag,
            sizeof(int));
    }
}