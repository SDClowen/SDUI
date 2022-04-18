using System;
using System.Drawing;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls
{
    public partial class CleanForm : Form
    {
        /// <summary>
        /// Variables for box shadow
        /// </summary>
        private bool _aeroEnabled;

        public CleanForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
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
                _aeroEnabled = CheckAeroEnabled();

                var cp = base.CreateParams;
                if (!_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:                        // box shadow
                    if (_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        var margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };

                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)     // drag the form
                m.Result = (IntPtr)HTCAPTION;

        }

        /// <summary>
        /// Change ui theme
        /// </summary>
        public void ChangeTheme()
        {
            ChangeControlsTheme(this);
        }

        public void ChangeControlsTheme(Control control)
        {
            /*if (control.Tag?.ToString() == "private" && !(control is Button))
                return;

            if (control is TextBoxBase txt)
            {
                txt.BorderStyle = BorderStyle.FixedSingle;
            }

            if (control is ComboBox comboBox)
            {
                comboBox.FlatStyle = FlatStyle.Flat;
            }

            if (control is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                if (control.Tag?.ToString() != "private")
                {
                    var determinedColor = ColorScheme.BackColor.Determine();

                    var color = Color.FromArgb(20, determinedColor);
                    var borderColor = Color.FromArgb(50, determinedColor);
                    var mouseOverBackColor = Color.FromArgb(50, determinedColor);
                    var mouseDownBackColor = Color.FromArgb(10, determinedColor);

                    btn.FlatAppearance.BorderColor = borderColor;
                    btn.FlatAppearance.MouseOverBackColor = mouseOverBackColor;
                    btn.FlatAppearance.MouseDownBackColor = mouseDownBackColor;
                    control.BackColor = color;
                    control.ForeColor = determinedColor;
                }
                else
                {
                    btn.FlatAppearance.BorderColor = Color.FromArgb(80, btn.BackColor);
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, btn.BackColor);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, btn.BackColor);
                }

                return;
            }
            else
                control.BackColor = ColorScheme.BackColor;

            if (!(control is ProgressBar))
                control.ForeColor = control.BackColor.Determine();

            foreach (Control subControl in control.Controls)
            {
                ChangeControlsTheme(subControl);
            }*/
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            ChangeTheme();
        }
    }
}
