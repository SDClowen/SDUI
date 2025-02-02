using SDUI.Collections;
using System;
using System.Drawing;

namespace SDUI.Controls
{
    public interface IUIElement
    {
        string Name { get; set; }
        string Text { get; set; }  

        Point Location { get; set; }
        Size Size { get; set; }
        bool Visible { get; set; }
        bool Enabled { get; set; }
        IUIElement Parent { get; set; }
        Color BackColor { get; set; }
        Color ForeColor { get; set; }
        Font Font { get; set; }

        UIElementBase FocusedElement { get; set; }
        
        ElementCollection Controls { get; }

        void Invalidate();
        void Refresh();
        void PerformLayout();
        void SuspendLayout();
        void ResumeLayout();
        void ResumeLayout(bool performLayout);
        void UpdateZOrder();
    }
}