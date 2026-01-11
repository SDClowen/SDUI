using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public abstract partial class UIElementBase
{
    protected void PerformDefaultLayout(UIElementBase control, Rectangle clientArea, ref Rectangle remainingArea)
    {
        var dock = control.Dock;
        
        // Handle Dock first (WinForms priority)
        if (dock != DockStyle.None)
        {
            var newBounds = Rectangle.Empty;
            
            switch (dock)
            {
                case DockStyle.Top:
                    newBounds = new Rectangle(
                        remainingArea.X,
                        remainingArea.Y,
                        remainingArea.Width,
                        control.Height);
                    remainingArea.Y += control.Height;
                    remainingArea.Height -= control.Height;
                    break;
                    
                case DockStyle.Bottom:
                    newBounds = new Rectangle(
                        remainingArea.X,
                        remainingArea.Bottom - control.Height,
                        remainingArea.Width,
                        control.Height);
                    remainingArea.Height -= control.Height;
                    break;
                    
                case DockStyle.Left:
                    newBounds = new Rectangle(
                        remainingArea.X,
                        remainingArea.Y,
                        control.Width,
                        remainingArea.Height);
                    remainingArea.X += control.Width;
                    remainingArea.Width -= control.Width;
                    break;
                    
                case DockStyle.Right:
                    newBounds = new Rectangle(
                        remainingArea.Right - control.Width,
                        remainingArea.Y,
                        control.Width,
                        remainingArea.Height);
                    remainingArea.Width -= control.Width;
                    break;
                    
                case DockStyle.Fill:
                    newBounds = remainingArea;
                    break;
            }
            
            if (control.Bounds != newBounds)
                control.Bounds = newBounds;
        }
        // Handle Anchor if no Dock
        else if (control.Anchor != AnchorStyles.None)
        {
            var anchor = control.Anchor;
            var x = control.Location.X;
            var y = control.Location.Y;
            var width = control.Width;
            var height = control.Height;
            
            // Left anchor
            if ((anchor & AnchorStyles.Left) == AnchorStyles.Left)
            {
                // X stays the same
            }
            else if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
            {
                // Move with right edge
                x = clientArea.Right - (clientArea.Width - control.Location.X - control.Width) - control.Width;
            }
            
            // Top anchor
            if ((anchor & AnchorStyles.Top) == AnchorStyles.Top)
            {
                // Y stays the same
            }
            else if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
            {
                // Move with bottom edge
                y = clientArea.Bottom - (clientArea.Height - control.Location.Y - control.Height) - control.Height;
            }
            
            // Width resize
            if ((anchor & AnchorStyles.Left) == AnchorStyles.Left && 
                (anchor & AnchorStyles.Right) == AnchorStyles.Right)
            {
                width = clientArea.Width - control.Location.X - (clientArea.Width - control.Location.X - control.Width);
            }
            
            // Height resize
            if ((anchor & AnchorStyles.Top) == AnchorStyles.Top && 
                (anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
            {
                height = clientArea.Height - control.Location.Y - (clientArea.Height - control.Location.Y - control.Height);
            }
            
            var newBounds = new Rectangle(x, y, width, height);
            if (control.Bounds != newBounds)
                control.Bounds = newBounds;
        }
    }
}
