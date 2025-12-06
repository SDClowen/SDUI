using System;
using SDUI.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI
{
    internal static class LayoutEngine
    {
        internal static void Perform(UIElementBase control, Rectangle clientArea, ref Rectangle remainingArea)
        {
            if (!control.Visible)
                return;

            // Child margin ile gerçek yerleşim alanını daralt
            var margin = control.Margin;
            var available = new Rectangle(
                remainingArea.Left + margin.Left,
                remainingArea.Top + margin.Top,
                Math.Max(0, remainingArea.Width - margin.Horizontal),
                Math.Max(0, remainingArea.Height - margin.Vertical));

            switch (control.Dock)
            {
                case DockStyle.Top:
                    control.Size = new Size(available.Width, control.Size.Height);
                    control.Location = new Point(available.Left, available.Top);
                    remainingArea = new Rectangle(
                        remainingArea.Left,
                        remainingArea.Top + control.Size.Height + margin.Vertical,
                        remainingArea.Width,
                        remainingArea.Height - control.Size.Height - margin.Vertical
                    );
                    break;

                case DockStyle.Bottom:
                    control.Size = new Size(available.Width, control.Size.Height);
                    control.Location = new Point(available.Left, available.Bottom - control.Size.Height);
                    remainingArea = new Rectangle(
                        remainingArea.Left,
                        remainingArea.Top,
                        remainingArea.Width,
                        remainingArea.Height - control.Size.Height - margin.Vertical
                    );
                    break;

                case DockStyle.Left:
                    control.Size = new Size(control.Size.Width, available.Height);
                    control.Location = new Point(available.Left, available.Top);
                    remainingArea = new Rectangle(
                        remainingArea.Left + control.Size.Width + margin.Horizontal,
                        remainingArea.Top,
                        remainingArea.Width - control.Size.Width - margin.Horizontal,
                        remainingArea.Height
                    );
                    break;

                case DockStyle.Right:
                    control.Size = new Size(control.Size.Width, available.Height);
                    control.Location = new Point(available.Right - control.Size.Width, available.Top);
                    remainingArea = new Rectangle(
                        remainingArea.Left,
                        remainingArea.Top,
                        remainingArea.Width - control.Size.Width - margin.Horizontal,
                        remainingArea.Height
                    );
                    break;

                case DockStyle.Fill:
                    control.Location = new Point(available.Left, available.Top);
                    control.Size = new Size(available.Width, available.Height);
                    remainingArea = new Rectangle(remainingArea.Left, remainingArea.Top, 0, 0);
                    break;
            }

            if (control.Dock != DockStyle.None)
                return;

            // Anchors (sol üst referans + sağ/alt mesafeleri koru)
            var anchor = control.Anchor;
            var location = control.Location;
            var size = control.Size;

            int left = location.X;
            int top = location.Y;
            int right = clientArea.Width - (location.X + size.Width);
            int bottom = clientArea.Height - (location.Y + size.Height);

            if ((anchor & AnchorStyles.Left) != 0 && (anchor & AnchorStyles.Right) != 0)
            {
                size.Width = Math.Max(0, clientArea.Width - left - right);
            }
            else if ((anchor & AnchorStyles.Right) != 0)
            {
                location.X = Math.Max(0, clientArea.Width - right - size.Width);
            }

            if ((anchor & AnchorStyles.Top) != 0 && (anchor & AnchorStyles.Bottom) != 0)
            {
                size.Height = Math.Max(0, clientArea.Height - top - bottom);
            }
            else if ((anchor & AnchorStyles.Bottom) != 0)
            {
                location.Y = Math.Max(0, clientArea.Height - bottom - size.Height);
            }

            control.Location = location;
            control.Size = size;
        }
    }
}
