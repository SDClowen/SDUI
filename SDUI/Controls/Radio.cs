using SDUI.Extensions;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    [DefaultEvent("CheckedChanged")]
    public class Radio : Label
    {
        public event EventHandler CheckedChanged;

        private bool isHovered = false;
        private bool isPressed = false;
        private bool isFocused = false;

        private bool isChecked = false;

        public bool Checked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                InvalidateControls();
                CheckedChanged?.Invoke(this, EventArgs.Empty);

                Invalidate();
            }
        }

        private void InvalidateControls()
        {
            if (!IsHandleCreated || !isChecked)
                return;

            foreach (Control C in Parent.Controls)
            {
                if (!object.ReferenceEquals(C, this) && C is Radio)
                {
                    ((Radio)C).Checked = false;
                }
            }
        }

        public Radio()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint, true);

            AutoSize = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color borderColor, foreColor;

            if (isHovered && !isPressed && Enabled)
            {
                foreColor = ColorScheme.ForeColor.Alpha(150);
                borderColor = ColorScheme.BorderColor.Alpha(150);
            }
            else if (isHovered && isPressed && Enabled)
            {
                foreColor = ColorScheme.ForeColor.Brightness(-.05f);
                borderColor = ColorScheme.BorderColor.Brightness(-.05f);
            }
            else if (!Enabled)
            {
                foreColor = Color.Gray;
                borderColor = ColorScheme.BorderColor.Alpha(50);
            }
            else
            {
                foreColor = ColorScheme.ForeColor;
                borderColor = ColorScheme.BorderColor;
            }

            var boxRect = new Rectangle(0, Height / 2 - 7, 14, 14);

            using (var path = boxRect.Radius(10))
            {
                if (Checked)
                {
                    using (var brush = new LinearGradientBrush(boxRect, Color.Blue, Color.DarkBlue, 180f))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    TextRenderer.DrawText(e.Graphics, "h", new Font("Marlett", 11), boxRect, Color.White);
                }

                using (var p = new Pen(borderColor))
                {
                    e.Graphics.DrawPath(p, path);
                }
            }

            var textRect = new Rectangle(16, -1, Width - 16, Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
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
                Checked = !Checked;
                Invalidate();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
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

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
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

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics())
            {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, Text, Font, proposedSize, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                preferredSize.Width += 25;
            }

            return preferredSize;
        }

    }
}
