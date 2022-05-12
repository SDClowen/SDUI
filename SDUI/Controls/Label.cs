using System;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class Label : System.Windows.Forms.Label
    {
        public Label()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if(ForeColor != ColorScheme.ForeColor)
                ForeColor = ColorScheme.ForeColor;

            base.OnPaint(e);
        }
    }
}
