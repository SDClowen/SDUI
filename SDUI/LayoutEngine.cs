using System;
using SDUI.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI
{
    internal static class LayoutEngine
    {
        /// <summary>
        /// Measures a child element with the given available size.
        /// </summary>
        internal static Size MeasureChild(UIElementBase child, Size availableSize)
        {
            if (!child.Visible)
                return Size.Empty;

            // Account for margin
            var margin = child.Margin;
            var constrainedSize = new Size(
                Math.Max(0, availableSize.Width - margin.Horizontal),
                Math.Max(0, availableSize.Height - margin.Vertical)
            );

            return child.Measure(constrainedSize);
        }

        /// <summary>
        /// Arranges a child element in the given final rectangle.
        /// </summary>
        internal static void ArrangeChild(UIElementBase child, Rectangle finalRect)
        {
            if (!child.Visible)
                return;

            // Apply margin to get actual bounds
            var margin = child.Margin;
            var arrangeRect = new Rectangle(
                finalRect.X + margin.Left,
                finalRect.Y + margin.Top,
                Math.Max(0, finalRect.Width - margin.Horizontal),
                Math.Max(0, finalRect.Height - margin.Vertical)
            );

            child.Arrange(arrangeRect);
        }

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
                        // Measure: Width is constrained by container; height may be auto-sized.
                        Size measuredSize;
                        if (control.AutoSize)
                        {
                            measuredSize = control.Measure(new Size(available.Width, 0));
                            if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                                measuredSize.Height = Math.Max(control.Size.Height, measuredSize.Height);
                        }
                        else
                        {
                            measuredSize = new Size(available.Width, control.Size.Height);
                        }

                        // Arrange at the top
                        var arrangeRect = new Rectangle(available.Left, available.Top, available.Width, measuredSize.Height);
                        control.Arrange(arrangeRect);

                        // Update remaining area
                        remainingArea = new Rectangle(
                            remainingArea.Left,
                            remainingArea.Top + measuredSize.Height + margin.Vertical,
                            remainingArea.Width,
                            remainingArea.Height - measuredSize.Height - margin.Vertical
                        );
                    }
                    break;

                case DockStyle.Bottom:
                    {
                        // Measure: Width is constrained by container
                        Size measuredSize;
                        if (control.AutoSize)
                        {
                            measuredSize = control.Measure(new Size(available.Width, 0));
                            if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                                measuredSize.Height = Math.Max(control.Size.Height, measuredSize.Height);
                        }
                        else
                        {
                            measuredSize = new Size(available.Width, control.Size.Height);
                        }

                        // Arrange at the bottom
                        var arrangeRect = new Rectangle(
                            available.Left,
                            available.Bottom - measuredSize.Height,
                            available.Width,
                            measuredSize.Height);
                        control.Arrange(arrangeRect);

                        // Update remaining area
                        remainingArea = new Rectangle(
                            remainingArea.Left,
                            remainingArea.Top,
                            remainingArea.Width,
                            remainingArea.Height - measuredSize.Height - margin.Vertical
                        );
                    }
                    break;

                case DockStyle.Left:
                    {
                        // Measure: Height is constrained by container
                        Size measuredSize;
                        if (control.AutoSize)
                        {
                            measuredSize = control.Measure(new Size(0, available.Height));
                            if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                                measuredSize.Width = Math.Max(control.Size.Width, measuredSize.Width);
                        }
                        else
                        {
                            measuredSize = new Size(control.Size.Width, available.Height);
                        }

                        // Arrange at the left
                        var arrangeRect = new Rectangle(available.Left, available.Top, measuredSize.Width, available.Height);
                        control.Arrange(arrangeRect);

                        // Update remaining area
                        remainingArea = new Rectangle(
                            remainingArea.Left + measuredSize.Width + margin.Horizontal,
                            remainingArea.Top,
                            remainingArea.Width - measuredSize.Width - margin.Horizontal,
                            remainingArea.Height
                        );
                    }
                    break;

                case DockStyle.Right:
                    {
                        // Measure: Height is constrained by container
                        Size measuredSize;
                        if (control.AutoSize)
                        {
                            measuredSize = control.Measure(new Size(0, available.Height));
                            if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                                measuredSize.Width = Math.Max(control.Size.Width, measuredSize.Width);
                        }
                        else
                        {
                            measuredSize = new Size(control.Size.Width, available.Height);
                        }

                        // Arrange at the right
                        var arrangeRect = new Rectangle(
                            available.Right - measuredSize.Width,
                            available.Top,
                            measuredSize.Width,
                            available.Height);
                        control.Arrange(arrangeRect);

                        // Update remaining area
                        remainingArea = new Rectangle(
                            remainingArea.Left,
                            remainingArea.Top,
                            remainingArea.Width - measuredSize.Width - margin.Horizontal,
                            remainingArea.Height
                        );
                    }
                    break;

                case DockStyle.Fill:
                    // Arrange to fill the entire available area
                    control.Arrange(new Rectangle(available.Left, available.Top, available.Width, available.Height));
                    remainingArea = new Rectangle(remainingArea.Left, remainingArea.Top, 0, 0);
                    break;
            }

            if (control.Dock != DockStyle.None)
                return;

            // Non-docked controls: Measure if AutoSize is enabled
            Size finalSize = control.Size;
            if (control.AutoSize)
            {
                finalSize = control.Measure(Size.Empty);
                if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                {
                    finalSize.Width = Math.Max(control.Size.Width, finalSize.Width);
                    finalSize.Height = Math.Max(control.Size.Height, finalSize.Height);
                }
            }

            // Apply Anchor logic
            var anchor = control.Anchor;
            var location = control.Location;

            // WinForms anchor behavior: distances are relative to clientArea (parent's client area)
            // Calculate initial distances from edges
            int left = location.X;
            int top = location.Y;
            int right = clientArea.Width - (location.X + finalSize.Width);
            int bottom = clientArea.Height - (location.Y + finalSize.Height);

            // Adjust position and size based on anchors
            // Default anchor is Top,Left - control stays at fixed position
            if ((anchor & AnchorStyles.Left) != 0 && (anchor & AnchorStyles.Right) != 0)
            {
                // Anchored to both left and right: stretch horizontally, maintain distances
                finalSize.Width = Math.Max(0, clientArea.Width - left - right);
            }
            else if ((anchor & AnchorStyles.Right) != 0 && (anchor & AnchorStyles.Left) == 0)
            {
                // Anchored to right only: maintain right distance, adjust X position
                location.X = Math.Max(0, clientArea.Width - right - finalSize.Width);
            }
            // else: Left only or no horizontal anchor - keep location.X as is

            if ((anchor & AnchorStyles.Top) != 0 && (anchor & AnchorStyles.Bottom) != 0)
            {
                // Anchored to both top and bottom: stretch vertically, maintain distances
                finalSize.Height = Math.Max(0, clientArea.Height - top - bottom);
            }
            else if ((anchor & AnchorStyles.Bottom) != 0 && (anchor & AnchorStyles.Top) == 0)
            {
                // Anchored to bottom only: maintain bottom distance, adjust Y position
                location.Y = Math.Max(0, clientArea.Height - bottom - finalSize.Height);
            }
            // else: Top only or no vertical anchor - keep location.Y as is

            // Arrange the control at its final position (margin already included in available area calculations)
            control.Arrange(new Rectangle(location.X, location.Y, Math.Max(0, finalSize.Width), Math.Max(0, finalSize.Height)));
        }
    }
}
