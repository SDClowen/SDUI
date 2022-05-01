using SDUI.Extensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class ToggleButton : Control
    {
        public event EventHandler CheckedChanged;

        private bool isHovered = false;
        private bool isPressed = false;
        private bool isFocused = false;

        private bool isChecked = false;
        public bool Checked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public override Color BackColor 
        { 
            get => Color.Transparent; 
            set => base.BackColor = Color.Transparent; 
        }

        public ToggleButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var bitmap = new Bitmap(Width, Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.Clear(BackColor);

                    //var clientRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
                    //graphics.DrawRectangle(Pens.Red, clientRectangle);

                    var boxRectangle = new Rectangle(4, 5, 62, 27);
                    var radius = boxRectangle.Radius(24);

                    if (!Checked)
                    {
                        using (var pen = new Pen(ColorScheme.BorderColor))
                        {
                            graphics.FillPath(new SolidBrush(ColorScheme.ForeColor.Alpha(5)), radius);
                            graphics.DrawPath(pen, radius);

                            graphics.FillEllipse(new SolidBrush(pen.Color), 7, 7, 22, 22);
                            graphics.DrawEllipse(pen, 7, 7, 22, 22);

                            if (isPressed)
                            {
                                for (int i = 0; i < 17; i++)
                                {
                                    graphics.FillEllipse(new SolidBrush(pen.Color), 7, 7, 22 + i, 22);
                                    graphics.DrawEllipse(pen, 7, 7, 22 + i, 22);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var pen = new Pen(ColorScheme.ForeColor))
                        {
                            graphics.DrawPath(pen, radius);

                            graphics.FillEllipse(new SolidBrush(pen.Color), 40, 7, 22, 22);
                            graphics.DrawEllipse(pen, 40, 7, 22, 22);

                            if (isPressed)
                            {
                                for (int i = 0; i < 17; i++)
                                {
                                    graphics.FillEllipse(new SolidBrush(pen.Color), 40 - i, 7, 22 + i, 22);
                                    graphics.DrawEllipse(pen, 40 - i, 7, 22 + i, 22);
                                }
                            }
                        }
                    }
                }

                e.Graphics.DrawImage(bitmap, 0, 0);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            isFocused = true;
            Invalidate();

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            isFocused = false;
            isHovered = false;
            isPressed = false;
            Invalidate();

            base.OnLostFocus(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            isFocused = true;
            Invalidate();

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            isFocused = false;
            isHovered = false;
            isPressed = false;
            Invalidate();

            base.OnLeave(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                isHovered = true;
                isPressed = true;
                Invalidate();
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            isHovered = false;
            isPressed = false;
            Invalidate();

            base.OnKeyUp(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();

            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPressed = true;
                //Checked = !Checked;
                Invalidate();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (isPressed)
                Checked = !Checked;

            isPressed = false;
            Invalidate();

            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();

            base.OnMouseLeave(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Width = 72;
            Height = 38;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics())
            {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, Text, Font, proposedSize, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                preferredSize.Width += 16;
            }

            return preferredSize;
        }
    }
}
