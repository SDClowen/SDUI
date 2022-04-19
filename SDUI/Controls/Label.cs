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
            var flags = TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis;
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ColorScheme.ForeColor, flags);
        }
    }
}
