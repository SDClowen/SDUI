using SDUI.Renderers;
using System;

namespace SDUI.Controls;

public class ContextMenuStrip : System.Windows.Forms.ContextMenuStrip
{
    public ContextMenuStrip()
    {
        Renderer = new MenuRenderer();
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }
}