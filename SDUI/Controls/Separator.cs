using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class Separator : Control
    {
        public Separator()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            this.Size = new Size(120, 10);
        }

        private bool _isVertical = false;
        public bool IsVertical
        {
            get => _isVertical;
            set
            {
                _isVertical = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(ColorScheme.BackColor);

            using (var pen = new Pen(ColorScheme.BorderColor))
            {
                if (_isVertical)
                {
                    var x = Width / 2;
                    e.Graphics.DrawLine(pen, x, 0, x, Height);
                }
                else
                {
                    var y = Height / 2;
                    e.Graphics.DrawLine(pen, 0, y, Width, y);
                }
            }
        }
    }
}
