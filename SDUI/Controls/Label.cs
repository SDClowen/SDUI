using System;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class Label : Control
    {
        public Label()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            var size = TextRenderer.MeasureText(Text, Font);
            Size = size;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ColorScheme.ForeColor);
        }
    }
}
