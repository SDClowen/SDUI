using System;
using System.Drawing;

namespace SDUI.Controls
{
    public interface IUIElement
    {
        Point Location { get; set; }
        Size Size { get; set; }
        bool Visible { get; set; }
        bool Enabled { get; set; }
        IUIElement Parent { get; set; }
        Color BackColor { get; set; }
        Color ForeColor { get; set; }
        Font Font { get; set; }

        void Invalidate();
        void Update();
        void Refresh();

        event EventHandler Click;
        event EventHandler MouseMove;
        event EventHandler MouseDown;
        event EventHandler MouseUp;
        event EventHandler Paint;

        void OnPaint(SKPaintSurfaceEventArgs e);
    }
}