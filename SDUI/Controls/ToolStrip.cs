using SDUI.Renderers;
using System;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ToolStrip : System.Windows.Forms.ToolStrip
{
    public ToolStrip()
    {
        Renderer = new MenuRenderer();
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.ResizeRedraw |
                  ControlStyles.UserPaint, true);
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }
}