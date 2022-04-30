using SDUI.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class ToggleButton : Control
    {
        public delegate void ToggledChangedEventHandler();
        private ToggledChangedEventHandler ToggledChangedEvent;

        public event ToggledChangedEventHandler ToggledChanged
        {
            add
            {
                ToggledChangedEvent = (ToggledChangedEventHandler)System.Delegate.Combine(ToggledChangedEvent, value);
            }
            remove
            {
                ToggledChangedEvent = (ToggledChangedEventHandler)System.Delegate.Remove(ToggledChangedEvent, value);
            }
        }

        private bool _toggled;

        public bool Toggled
        {
            get
            {
                return _toggled;
            }
            set
            {
                _toggled = value;
                Invalidate();
                if (ToggledChangedEvent != null)
                    ToggledChangedEvent();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Width = 72;
            Height = 38;
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Toggled = !Toggled;
            Focus();
        }

        public ToggleButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }


        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            var clientRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);

            var graphics = e.Graphics;

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Parent.BackColor);

            //graphics.DrawRectangle(Pens.Red, clientRectangle);

            var boxRectangle = new Rectangle(4, 5, 62, 27);
            var radius = boxRectangle.Radius(24);

            if (!_toggled)
            {
                using (var pen = new Pen(ColorScheme.BorderColor))
                {
                    graphics.FillPath(new SolidBrush(ColorScheme.ForeColor.Alpha(5)), radius);
                    graphics.DrawPath(pen, radius);

                    graphics.FillEllipse(new SolidBrush(pen.Color), 7, 7, 22, 22);
                    graphics.DrawEllipse(pen, 7, 7, 22, 22);
                }
            }
            else
            {
                using (var pen = new Pen(ColorScheme.ForeColor))
                {
                    graphics.DrawPath(pen, radius);
                    graphics.FillEllipse(new SolidBrush(pen.Color), 40, 7, 22, 22);
                    graphics.DrawEllipse(pen, 40, 7, 22, 22);
                }
            }

            
            

        }
    }
}
