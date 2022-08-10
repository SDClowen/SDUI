using SDUI.Extensions;
using SDUI.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class ComboBox : System.Windows.Forms.ComboBox
    {
        private int _radius = 5;
        public int Radius
        {
            get => _radius;
            set
            {
                return;

                _radius = value;

                Invalidate();
            }
        }

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

        public ComboBox()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor, true
            );

            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            var index = e.Index;
            if (index < 0 || index >= Items.Count)
                return;

            var foreColor = ColorScheme.ForeColor;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                foreColor = Color.White;
                e.DrawBackground();
            }
            else
                e.Graphics.FillRectangle(new SolidBrush(ColorScheme.BackColor), e.Bounds);

            TextRenderer.DrawText(e.Graphics, GetItemText(Items[index]), e.Font, e.Bounds, foreColor, TextFormatFlags.Left);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            SuspendLayout();
            Update();
            ResumeLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!Focused)
            {
                SelectionLength = 0;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            ButtonRenderer.DrawParentBackground(graphics, ClientRectangle, this);

            var rectf = ClientRectangle.ToRectangleF();

            if (ColorScheme.DrawDebugBorders)
            {
                var redPen = new Pen(Color.Red, 1);
                redPen.Alignment = PenAlignment.Outset;
                graphics.DrawRectangle(redPen, 0, 0, rectf.Width - 1, rectf.Height - 1);
            }

            var inflate = _shadowDepth / 4f;
            rectf.Inflate(-inflate, -inflate);

            var textRectangle = new Rectangle(3, 0, Width - 18, Height);

            var backColor = ColorScheme.BackColor;
            var borderColor = ColorScheme.ForeColor.Alpha(60);

            using (var path = rectf.Radius(_radius))
            {
                e.Graphics.FillPath(new SolidBrush(backColor), path);

                var borderPen = new Pen(borderColor);
                var _extendBoxRect = new RectangleF(rectf.Width - 24f, 0, 16, rectf.Height -  4 + _shadowDepth);

                var symbolPen = new Pen(ColorScheme.ForeColor);
                graphics.DrawLine(symbolPen,
                        _extendBoxRect.Left + _extendBoxRect.Width / 2 - 5 - 1,
                        _extendBoxRect.Top + _extendBoxRect.Height / 2 - 2,
                        _extendBoxRect.Left + _extendBoxRect.Width / 2 - 1,
                        _extendBoxRect.Top + _extendBoxRect.Height / 2 + 3);

                graphics.DrawLine(symbolPen,
                    _extendBoxRect.Left + _extendBoxRect.Width / 2 + 5 - 1,
                    _extendBoxRect.Top + _extendBoxRect.Height / 2 - 2,
                    _extendBoxRect.Left + _extendBoxRect.Width / 2 - 1,
                    _extendBoxRect.Top + _extendBoxRect.Height / 2 + 3);

                ControlPaintHelper.DrawShadow(graphics, rectf, _shadowDepth, _radius);
                e.Graphics.DrawPath(borderPen, path);

                var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl;
                TextRenderer.DrawText(graphics, Text, Font, textRectangle, ColorScheme.ForeColor, flags);
            }
        }
    }
}
