using System;
using System.Drawing;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class CleanForm : Form
{
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

    public CleanForm()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

        //BackColor = Color.FromArgb(0, 0, 0, 0);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
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

        var flag = DWMSBT_TABBEDWINDOW;
        DwmSetWindowAttribute(
            Handle,
            DWMWA_SYSTEMBACKDROP_TYPE,
            ref flag,
            sizeof(int));
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_NCCALCSIZE:
                if (!DesignMode && FormBorderStyle == FormBorderStyle.FixedSingle)
                    return;
                else
                    break;
        }

        base.WndProc(ref m);

        if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)     // drag the form
            m.Result = (IntPtr)HTCAPTION;
    }

    public void ChangeControlsTheme(Control control)
    {
        if (control is RichTextBox || control is ListView || control is ListBox)
        {
            control.BackColor = ColorScheme.BackColor;
            control.ForeColor = ColorScheme.ForeColor;
        }

        Helpers.WindowsHelper.UseImmersiveDarkMode(control.Handle, ColorScheme.BackColor.IsDark());

        foreach (Control subControl in control.Controls)
        {
            ChangeControlsTheme(subControl);
        }
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

            if (BackColor.IsDark())
            {
                margins = new MARGINS()
                {
                    Bottom = -1,
                    Left = -1,
                    Right = -1,
                    Top = -1
                };
            }

            DwmExtendFrameIntoClientArea(this.Handle, ref margins);

        }

        Helpers.WindowsHelper.UseImmersiveDarkMode(Handle, ColorScheme.BackColor.IsDark());
        if (!Helpers.WindowsHelper.TenOrHigher)
            return;


        var flag = DWMSBT_TABBEDWINDOW;
        DwmSetWindowAttribute(
            Handle,
            DWMWA_SYSTEMBACKDROP_TYPE,
            ref flag,
            sizeof(int));
    }
}