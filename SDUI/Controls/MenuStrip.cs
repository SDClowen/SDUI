using SDUI.Renderers;
using System;

namespace SDUI.Controls;

public class MenuStrip : System.Windows.Forms.MenuStrip
{
    public MenuStrip()
    {
        Renderer = new MenuRenderer();
        ImageScalingSize = new System.Drawing.Size(20, 20);
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }
}
