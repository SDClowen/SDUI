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
                    {
                        // Width is constrained by container; height may be auto-sized.
                        int height = control.Size.Height;
                        if (control.AutoSize)
                        {
                            var pref = control.GetPreferredSize(new Size(available.Width, 0));
                            height = pref.Height;
                            if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                                height = Math.Max(control.Size.Height, height);

                            if (control.MinimumSize.Height > 0) height = Math.Max(height, control.MinimumSize.Height);
                            if (control.MaximumSize.Height > 0) height = Math.Min(height, control.MaximumSize.Height);
                        }

                        control.Size = new Size(available.Width, height);
                        control.Location = new Point(available.Left, available.Top);
                        remainingArea = new Rectangle(
                            remainingArea.Left,
                            remainingArea.Top + control.Size.Height + margin.Vertical,
                            remainingArea.Width,
                            remainingArea.Height - control.Size.Height - margin.Vertical
                        );
                    }
                    break;

                case DockStyle.Bottom:
                    {
                        int height = control.Size.Height;
                        if (control.AutoSize)
                        {
                            var pref = control.GetPreferredSize(new Size(available.Width, 0));
                            height = pref.Height;
                            if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                                height = Math.Max(control.Size.Height, height);

                            if (control.MinimumSize.Height > 0) height = Math.Max(height, control.MinimumSize.Height);
                            if (control.MaximumSize.Height > 0) height = Math.Min(height, control.MaximumSize.Height);
                        }

                        control.Size = new Size(available.Width, height);
                        control.Location = new Point(available.Left, available.Bottom - control.Size.Height);
                        remainingArea = new Rectangle(
                            remainingArea.Left,
                            remainingArea.Top,
                            remainingArea.Width,
                            remainingArea.Height - control.Size.Height - margin.Vertical
                        );
                    }
                    break;

                case DockStyle.Left:
                    {
                        int width = control.Size.Width;
                        if (control.AutoSize)
                        {
                            var pref = control.GetPreferredSize(new Size(0, available.Height));
                            width = pref.Width;
                            if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                                width = Math.Max(control.Size.Width, width);

                            if (control.MinimumSize.Width > 0) width = Math.Max(width, control.MinimumSize.Width);
                            if (control.MaximumSize.Width > 0) width = Math.Min(width, control.MaximumSize.Width);
                        }

                        control.Size = new Size(width, available.Height);
                        control.Location = new Point(available.Left, available.Top);
                        remainingArea = new Rectangle(
                            remainingArea.Left + control.Size.Width + margin.Horizontal,
                            remainingArea.Top,
                            remainingArea.Width - control.Size.Width - margin.Horizontal,
                            remainingArea.Height
                        );
                    }
                    break;

                case DockStyle.Right:
                    {
                        int width = control.Size.Width;
                        if (control.AutoSize)
                        {
                            var pref = control.GetPreferredSize(new Size(0, available.Height));
                            width = pref.Width;
                            if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                                width = Math.Max(control.Size.Width, width);

                            if (control.MinimumSize.Width > 0) width = Math.Max(width, control.MinimumSize.Width);
                            if (control.MaximumSize.Width > 0) width = Math.Min(width, control.MaximumSize.Width);
                        }

                        control.Size = new Size(width, available.Height);
                        control.Location = new Point(available.Right - control.Size.Width, available.Top);
                        remainingArea = new Rectangle(
                            remainingArea.Left,
                            remainingArea.Top,
                            remainingArea.Width - control.Size.Width - margin.Horizontal,
                            remainingArea.Height
                        );
                    }
                    break;

                case DockStyle.Fill:
                    control.Location = new Point(available.Left, available.Top);
                    control.Size = new Size(available.Width, available.Height);
                    remainingArea = new Rectangle(remainingArea.Left, remainingArea.Top, 0, 0);
                    break;
            }

            if (control.Dock != DockStyle.None)
                return;

            // If the control is not docked but AutoSize is enabled, compute preferred size and apply it
            if (control.AutoSize)
            {
                var proposed = control.GetPreferredSize(Size.Empty);
                if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                {
                    proposed.Width = Math.Max(control.Size.Width, proposed.Width);
                    proposed.Height = Math.Max(control.Size.Height, proposed.Height);
                }

                // MinimumSize and MaximumSize enforcement
                if (control.MinimumSize.Width > 0) proposed.Width = Math.Max(proposed.Width, control.MinimumSize.Width);
                if (control.MinimumSize.Height > 0) proposed.Height = Math.Max(proposed.Height, control.MinimumSize.Height);
                if (control.MaximumSize.Width > 0) proposed.Width = Math.Min(proposed.Width, control.MaximumSize.Width);
                if (control.MaximumSize.Height > 0) proposed.Height = Math.Min(proposed.Height, control.MaximumSize.Height);

                if (control.Size != proposed)
                    control.Size = proposed;
            }

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
