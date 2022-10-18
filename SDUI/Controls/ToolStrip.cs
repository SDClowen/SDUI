using SDUI.Renderers;
using System;

namespace SDUI.Controls;

public class ToolStrip : System.Windows.Forms.ToolStrip
{
    public ToolStrip()
    {
        Renderer = new MenuRenderer();
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }
}