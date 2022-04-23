using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Controls
{
    [DefaultEvent("CheckedChanged")]
    public class Radio : Control
    {
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
                if (CheckedChanged != null)
                {
                    CheckedChanged(this);
                }
                Invalidate();
            }
        }

        public event CheckedChangedEventHandler CheckedChanged;

        public delegate void CheckedChangedEventHandler(object sender);

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

        private int _lockedHeight;
        protected int LockHeight
        {
            get { return _lockedHeight; }
            set
            {
                _lockedHeight = value;
                if (!(LockHeight == 0) && IsHandleCreated)
                    Height = LockHeight;
            }
        }

        public Radio()
        {
            LockHeight = 22;
            Width = 140;

            SetStyle(ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;

            graphics.SmoothingMode = SmoothingMode.HighQuality;

            var color = Color.FromArgb(220, 1, 52, 153).Brightness(ColorScheme.BackColor.Determine().GetBrightness());

            if (isChecked || isHovered)
                graphics.FillEllipse(new SolidBrush(color), new Rectangle(new Point(7, 7), new Size(8, 8)));

            graphics.DrawEllipse(new Pen(color), new Rectangle(new Point(4, 4), new Size(14, 14)));

            var textRect = new Point(23, 11);
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ColorScheme.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
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

    }
}
