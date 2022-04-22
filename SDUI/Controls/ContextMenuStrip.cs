using SDUI.Renderers;

namespace SDUI.Controls
{
    public class ContextMenuStrip : System.Windows.Forms.ContextMenuStrip
    {
        public ContextMenuStrip()
        {
            Renderer = new MenuRenderer();
        }
    }
}
