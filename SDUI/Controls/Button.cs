using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class Button : System.Windows.Forms.Button
    {
        /// <summary>
        /// Button raised color
        /// </summary>
        public Color Color { get; set; } = Color.Transparent;

        /// <summary>
        /// Mouse state
        /// </summary>
        private int _mouseState = 0;

        public Button()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _mouseState = 2;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _mouseState = 1;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _mouseState = 1;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _mouseState = 0;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(ColorScheme.BackColor);
            var graphics = e.Graphics;
            var clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            var innerRectangle = new Rectangle(1, 1, Width - 3, Height - 3);

            graphics.SmoothingMode = SmoothingMode.HighQuality;

            var color = ColorScheme.BackColor.Determine();

            Brush gradient = null;
            switch (_mouseState)
            {
                case 0:
                    gradient = new SolidBrush(Color == Color.Transparent ? Color.FromArgb(45, color) : Color);
                    break;

                case 1:
                    gradient = new SolidBrush(Color == Color.Transparent ? Color.FromArgb(60, color) : Color.FromArgb(180, Color));
                    break;

                case 2:
                    gradient = new SolidBrush(Color == Color.Transparent ? Color.FromArgb(80, color) : Color.FromArgb(220, Color));
                    break;
            }

            var outerPen = new Pen(Color == Color.Transparent ? ColorScheme.BorderColor : Color.FromArgb(95, Color.Determine()));

            graphics.FillRectangle(gradient, clientRectangle);
            gradient.Dispose();

            graphics.DrawRectangle(outerPen, clientRectangle);
            outerPen.Dispose();

            var textRectangle = new Rectangle(0, 1, Width - 1, Height - 1);
            TextRenderer.DrawText(graphics, Text, Font, textRectangle, Color == Color.Transparent ? ColorScheme.ForeColor : ForeColor, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
    }
}
