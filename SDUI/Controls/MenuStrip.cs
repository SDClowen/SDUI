using SDUI.Renderers;

namespace SDUI.Controls
{
    public class MenuStrip : System.Windows.Forms.MenuStrip
    {
        public MenuStrip()
        {
            Renderer = new MenuRenderer();
        }
    }
}
