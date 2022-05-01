using SDUI.Extensions;
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

        private int _radius = 2;
        public int Radius
        {
            get => _radius;
            set
            {
                _radius = value;

                Invalidate();
            }
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
            var graphics = e.Graphics;
            graphics.Clear(ColorScheme.BackColor);
            var clientRectangle = ClientRectangle;

            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var color = ColorScheme.ForeColor;

            Brush gradient = null;
            switch (_mouseState)
            {
                case 0:
                    gradient = new SolidBrush(Color == Color.Transparent ? color.Alpha(30) : Color);
                    break;

                case 1:

                    gradient = new SolidBrush(Color == Color.Transparent ? color.Alpha(40) : Enabled ? Color.Alpha(220) : Color);
                    break;

                case 2:
                    gradient = new SolidBrush(Color == Color.Transparent ? color.Alpha(50) : Enabled ? Color.Alpha(200) : Color);
                    break;
            }

            var outerPen = new Pen(Color == Color.Transparent ? ColorScheme.BorderColor : Color.Determine().Alpha(95));

            using (var path = clientRectangle.Radius(_radius))
            {
                graphics.FillPath(gradient, path);
                gradient.Dispose();

                graphics.DrawPath(outerPen, path);
                outerPen.Dispose();
            }

            var foreColor = Color == Color.Transparent ? ColorScheme.ForeColor : ForeColor;
            if (!Enabled)
            {
                foreColor = Color.Gray;
            }

            var textRectangle = new Rectangle(0, 1, Width - 1, Height - 1);
            TextRenderer.DrawText(graphics, Text, Font, textRectangle, foreColor, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
    }
}
