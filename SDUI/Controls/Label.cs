using System.Windows.Forms;

namespace SDUI.Controls
{
    public class Label : Control
    {
        public Label()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        }

        private string _text;
        public new string Text
        {
            get => _text;
            set
            {
                _text = value;
                var size = TextRenderer.MeasureText(value, Font);
                Size = size;

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ColorScheme.ForeColor);
        }
    }
}
