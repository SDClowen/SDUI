using SDUI.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI
{
    internal static class LayoutEngine
    {
        internal static void Perform(UIElementBase control, Rectangle clientArea, Padding clientPadding)
        {
            if (!control.Visible)
                return;

            var remainingArea = clientArea;
            switch (control.Dock)
            {
                case DockStyle.Top:
                    control.Size = new Size(remainingArea.Width, control.Size.Height);
                    control.Location = new Point(remainingArea.Left, remainingArea.Top);
                    remainingArea = new Rectangle(
                        remainingArea.Left,
                        remainingArea.Top + control.Size.Height,
                        remainingArea.Width,
                        remainingArea.Height - control.Size.Height
                    );
                    break;

                case DockStyle.Bottom:
                    control.Size = new Size(remainingArea.Width, control.Size.Height);
                    control.Location = new Point(remainingArea.Left, remainingArea.Bottom - control.Size.Height);
                    remainingArea = new Rectangle(
                        remainingArea.Left,
                        remainingArea.Top,
                        remainingArea.Width,
                        remainingArea.Height - control.Size.Height
                    );
                    break;

                case DockStyle.Left:
                    control.Size = new Size(control.Size.Width, remainingArea.Height);
                    control.Location = new Point(remainingArea.Left, remainingArea.Top);
                    remainingArea = new Rectangle(
                        remainingArea.Left + control.Size.Width,
                        remainingArea.Top,
                        remainingArea.Width - control.Size.Width,
                        remainingArea.Height
                    );
                    break;

                case DockStyle.Right:
                    control.Size = new Size(control.Size.Width, remainingArea.Height);
                    control.Location = new Point(remainingArea.Right - control.Size.Width, remainingArea.Top);
                    remainingArea = new Rectangle(
                        remainingArea.Left,
                        remainingArea.Top,
                        remainingArea.Width - control.Size.Width,
                        remainingArea.Height
                    );
                    break;

                case DockStyle.Fill:
                    control.Location = new Point(remainingArea.Left, remainingArea.Top);
                    control.Size = new Size(remainingArea.Width, remainingArea.Height);
                    break;
            }

            if (control.Dock != DockStyle.None)
                return;

            // Anchors

            var anchor = control.Anchor;
            var location = control.Location;
            var size = control.Size;

            // Yatay konumlandırma
            if ((anchor & AnchorStyles.Left) != 0 && (anchor & AnchorStyles.Right) != 0)
            {
                size.Width = clientArea.Width - (location.X + (clientArea.Width - (location.X + size.Width)));
            }
            else if ((anchor & AnchorStyles.Right) != 0)
            {
                location.X = clientArea.Width - (clientArea.Width - location.X);
            }

            // Dikey konumlandırma
            if ((anchor & AnchorStyles.Top) != 0 && (anchor & AnchorStyles.Bottom) != 0)
            {
                size.Height = clientArea.Height - (location.Y + (clientArea.Height - (location.Y + size.Height)));
            }
            else if ((anchor & AnchorStyles.Bottom) != 0)
            {
                location.Y = clientArea.Height - (clientArea.Height - location.Y);
            }

            control.Location = location;
            control.Size = size;
        }
    }
}
