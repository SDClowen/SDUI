using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SDUI.Controls;

public class NumUpDown : NumericUpDown
{
    public new BorderStyle BorderStyle
    {
        get => BorderStyle.FixedSingle;
        set => base.BorderStyle = value;
    }

    public NumUpDown()
    {
        var renderer = new UpDownButtonRenderer(Controls[0]);
    }

    private Color GetColor()
    {
        var color = ColorScheme.BackColor;
        if (color.IsDark())
            return ControlPaint.Light(color, .2f);
        else
            return ControlPaint.Dark(color, -.4f);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (BackColor != GetColor())
        {
            BackColor = GetColor();
            ForeColor = ColorScheme.ForeColor;
        }

        using (var pen = new Pen(ColorScheme.BorderColor, 1))
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

            e.Graphics.DrawRectangle(pen,
                ClientRectangle.Left, ClientRectangle.Top,
                ClientRectangle.Width - 1, ClientRectangle.Height - 1);
        }
    }

    private class UpDownButtonRenderer : NativeWindow
    {
        [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "BeginPaint", CharSet = CharSet.Auto)]
        private static extern IntPtr IntBeginPaint(IntPtr hWnd, [In, Out] ref PAINTSTRUCT lpPaint);
        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public int rcPaint_left;
            public int rcPaint_top;
            public int rcPaint_right;
            public int rcPaint_bottom;
            public bool fRestore;
            public bool fIncUpdate;
            public int reserved1;
            public int reserved2;
            public int reserved3;
            public int reserved4;
            public int reserved5;
            public int reserved6;
            public int reserved7;
            public int reserved8;
        }
        [DllImport("user32.dll", ExactSpelling = true, EntryPoint = "EndPaint", CharSet = CharSet.Auto)]
        private static extern bool IntEndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

        Control updown;
        public UpDownButtonRenderer(Control c)
        {
            this.updown = c;
            if (updown.IsHandleCreated)
                this.AssignHandle(updown.Handle);
            else
                updown.HandleCreated += (s, e) => this.AssignHandle(updown.Handle);
        }
        private Point[] GetDownArrow(Rectangle r)
        {
            var middle = new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
            return new Point[]
            {
                new Point(middle.X - 3, middle.Y - 2),
                new Point(middle.X + 4, middle.Y - 2),
                new Point(middle.X, middle.Y + 2)
            };
        }
        private Point[] GetUpArrow(Rectangle r)
        {
            var middle = new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
            return new Point[]
            {
                new Point(middle.X - 4, middle.Y + 2),
                new Point(middle.X + 4, middle.Y + 2),
                new Point(middle.X, middle.Y - 3)
            };
        }
        protected override void WndProc(ref Message m)
        {
            var parent = (NumUpDown)updown.Parent;
            if (m.Msg == 0xF /*WM_PAINT*/ && parent.BorderStyle == BorderStyle.FixedSingle)
            {
                var s = new PAINTSTRUCT();
                IntBeginPaint(updown.Handle, ref s);
                using (var g = Graphics.FromHdc(s.hdc))
                {
                    var enabled = updown.Enabled;
                    using (var backBrush = new SolidBrush(parent.BackColor))
                    {
                        g.FillRectangle(backBrush, updown.ClientRectangle);
                    }

                    var r1 = new Rectangle(0, 0, updown.Width, updown.Height / 2);
                    var r2 = new Rectangle(0, updown.Height / 2, updown.Width, updown.Height / 2 + 1);
                    var p = updown.PointToClient(MousePosition);

                    if (enabled && updown.ClientRectangle.Contains(p))
                    {
                        using (var b = new SolidBrush(ColorScheme.BorderColor))
                        {
                            if (r1.Contains(p))
                                g.FillRectangle(b, r1);
                            else
                                g.FillRectangle(b, r2);
                        }
                    }

                    using (var pen = new Pen(ColorScheme.BorderColor))
                    {
                        g.DrawLines(pen,
                            new[] { new Point(0, 0), new Point(0, updown.Height),
                                        new Point(0, updown.Height / 2), new Point(updown.Width, updown.Height / 2)
                            });
                    }

                    var brush = new SolidBrush(ColorScheme.ForeColor);
                    g.FillPolygon(brush, GetUpArrow(r1));
                    g.FillPolygon(brush, GetDownArrow(r2));
                }

                m.Result = (IntPtr)1;
                base.WndProc(ref m);
                IntEndPaint(updown.Handle, ref s);
            }
            else if (m.Msg == 0x0014/*WM_ERASEBKGND*/)
            {
                using (var g = Graphics.FromHdcInternal(m.WParam))
                    g.FillRectangle(new SolidBrush(ColorScheme.BackColor), updown.ClientRectangle);
                m.Result = (IntPtr)1;
            }
            else
                base.WndProc(ref m);
        }
    }
}
