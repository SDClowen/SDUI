﻿using SDUI.Extensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class CheckBox : Control
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

        public CheckBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //e.Graphics.Clear(Parent.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color borderColor, foreColor;

            if (isHovered && !isPressed && Enabled)
            {
                foreColor = ColorScheme.ForeColor.Brightness(.05f);
                borderColor = ColorScheme.BorderColor.Brightness(.05f);
            }
            else if (isHovered && isPressed && Enabled)
            {
                foreColor = ColorScheme.ForeColor.Brightness(-.05f);
                borderColor = ColorScheme.BorderColor.Brightness(-.05f);
            }
            else if (!Enabled)
            {
                foreColor = ColorScheme.ForeColor.Brightness(-1f);
                borderColor = ColorScheme.BorderColor.Brightness(-1f);
            }
            else
            {
                foreColor = ColorScheme.ForeColor;
                borderColor = ColorScheme.BorderColor;
            }

            var boxRect = new Rectangle(0, Height / 2 - 6, 14, 14);

            using (var path = boxRect.Radius(4))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                if (Checked)
                {
                    using (var brush = new LinearGradientBrush(boxRect, Color.Blue, Color.DarkBlue, 90f))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    var points = new PointF[3]
                    {
                        new PointF(1, 12),
                        new PointF(4, 15),
                        new PointF(11, 9)
                    };

                    e.Graphics.DrawLines(new Pen(Color.White, 1), points);
                }

                using (var p = new Pen(borderColor))
                {
                    e.Graphics.DrawPath(p, path);
                }
            }

            var textRect = new Rectangle(16, 0, Width - 16, Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

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