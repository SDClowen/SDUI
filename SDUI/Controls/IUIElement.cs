using SDUI.Collections;
using SkiaSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

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

        IUIElement FocusedElement { get; set; }
        
        ElementCollection Controls { get; }
        object Tag { get; set; }
        DockStyle Dock { get; set; }
        int ZOrder { get; set; }
        int TabIndex { get; set; }
        bool TabStop { get; set; }

        void Render(SKCanvas canvas);
        void Invalidate();
        void Refresh();
        void PerformLayout();
        void SuspendLayout();
        void ResumeLayout();
        void ResumeLayout(bool performLayout);
        void UpdateZOrder();
        void BringToFront();
        void OnCreateControl();
        UIWindowBase GetParentWindow();
        void EnsureLoadedRecursively();
        void EnsureUnloadedRecursively();
    }
}