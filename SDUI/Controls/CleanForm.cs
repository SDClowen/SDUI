﻿using System;
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

        public void ChangeControlsTheme(Control control)
        {
            if (control is RichTextBox || control is ListView)
            {
                control.BackColor = ColorScheme.BackColor;
                control.ForeColor = ColorScheme.ForeColor;
            }

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
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            ChangeControlsTheme(this);
        }
    }
}
