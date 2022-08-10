using SDUI.Extensions;
using SDUI.Helpers;
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

        private float _shadowDepth = 4f;
        public float ShadowDepth
        {
            get => _shadowDepth;
            set
            {
                if (_shadowDepth == value)
                    return;

                _shadowDepth = value;
                Invalidate();
            }
        }

        private int _radius = 5;
        public int Radius
        {
            get => _radius;
            set
            {
                if (_radius == value)
                    return;

                _radius = value;
                Invalidate();
            }
        }

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
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            ButtonRenderer.DrawParentBackground(graphics, ClientRectangle, this);

            var me = TextRenderer.MeasureText(Text, Font);

            var rectf = new RectangleF(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height); 
            
            if (ColorScheme.DrawDebugBorders)
            {
                var redPen = new Pen(Color.Red, 1);
                redPen.Alignment = PenAlignment.Outset;
                graphics.DrawRectangle(redPen, 0, 0, rectf.Width - 1, rectf.Height - 1);
            }

            var inflate = _shadowDepth / 4f;
            rectf.Inflate(-inflate, -inflate);

            var color = ColorScheme.ForeColor;

            Brush brush = null;
            switch (_mouseState)
            {
                case 0:
                    brush = new SolidBrush(Color == Color.Transparent ? color.Alpha(30) : Color);
                    break;

                case 1:

                    brush = new SolidBrush(Color == Color.Transparent ? color.Alpha(40) : Enabled ? Color.Alpha(220) : Color);
                    break;

                case 2:
                    brush = new SolidBrush(Color == Color.Transparent ? color.Alpha(50) : Enabled ? Color.Alpha(200) : Color);
                    break;
            }

            var borderColor = Color == Color.Transparent ? ColorScheme.BorderColor : Color.Determine().Alpha(95);
            var outerPen = new Pen(borderColor);

            using (var path = rectf.Radius(_radius))
            {
                graphics.FillPath(brush, path);
                brush.Dispose();

                ControlPaintHelper.DrawShadow(graphics, rectf, _shadowDepth, _radius);

                graphics.DrawPath(outerPen, path);
                outerPen.Dispose();
            }

            var foreColor = Color == Color.Transparent ? ColorScheme.ForeColor : ForeColor;
            if (!Enabled)
                foreColor = Color.Gray;
            
            TextRenderer.DrawText(graphics, Text, Font, rectf.ToRectangle(), foreColor, TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
    }
}
