using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class ComboBox : System.Windows.Forms.ComboBox
    {
        private int _startIndex = 0;

        public int StartIndex
        {
            get
            {
                return _startIndex;
            }
            set
            {
                _startIndex = value;
                try
                {
                    base.SelectedIndex = value;
                }
                catch
                {
                }
                Invalidate();
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
            if (e.Index < 0)
                return;

            var gradient = new LinearGradientBrush(e.Bounds, Color.Blue, Color.DarkBlue, 90.0F);
            var text = GetItemText(Items[e.Index]);

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(gradient, e.Bounds);
                TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, Color.White, TextFormatFlags.Left);

            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(ColorScheme.BackColor.Brightness(-.16f)), e.Bounds);
                TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, ColorScheme.ForeColor, TextFormatFlags.Left);
            }

            gradient.Dispose();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            SuspendLayout();
            Update();
            ResumeLayout();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!Focused)
            {
                SelectionLength = 0;
            }
        }

        public ComboBox()
        {
            SetStyle((ControlStyles)139286, true);
            SetStyle(ControlStyles.Selectable, false);

            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;

            Size = new Size(135, 26);
            ItemHeight = 17;
            DropDownHeight = 100;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(Parent.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            var textRectangle = new Rectangle(3, 0, Width - 20, Height);

            var determinedColor = ColorScheme.BackColor.Determine();
            var colorBegin = Color.FromArgb(60, determinedColor.Brightness(.5f));
            var colorEnd = Color.FromArgb(30, determinedColor.Brightness(-.5f));
            var gradient = new LinearGradientBrush(ClientRectangle, colorBegin, colorEnd, 90f);

            e.Graphics.SetClip(rectangle);
            e.Graphics.FillRectangle(gradient, ClientRectangle);
            e.Graphics.ResetClip();

            e.Graphics.DrawRectangle(new Pen(ColorScheme.BorderColor), rectangle);

            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl;
            TextRenderer.DrawText(e.Graphics, Text, Font, textRectangle, ColorScheme.ForeColor, flags);

            e.Graphics.DrawString("6", new Font("Marlett", 13, FontStyle.Regular), new SolidBrush(ColorScheme.BorderColor), new Rectangle(3, 0, Width - 4, Height), new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Far
            });

            e.Graphics.DrawLine(new Pen(colorEnd), Width - 24, 4, Width - 24, this.Height - 5);
            gradient.Dispose();
        }
    }
}
