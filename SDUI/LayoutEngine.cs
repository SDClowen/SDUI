using System;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Controls;

namespace SDUI;

internal static class LayoutEngine
{
    /// <summary>
    ///     Measures a child element with the given available size.
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
    ///     Arranges a child element in the given final rectangle.
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

        // Apply margin to get actual available space for this control
        var margin = control.Margin;
        var available = new Rectangle(
            remainingArea.X + margin.Left,
            remainingArea.Y + margin.Top,
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

                // Arrange at the top of available area
                var arrangeRect = new Rectangle(available.X, available.Y, available.Width, measuredSize.Height);
                control.Arrange(arrangeRect);

                // Update remaining area - shrink from top by control height + full margin
                var consumedHeight = measuredSize.Height + margin.Top + margin.Bottom;
                remainingArea = new Rectangle(
                    remainingArea.X,
                    remainingArea.Y + consumedHeight,
                    remainingArea.Width,
                    Math.Max(0, remainingArea.Height - consumedHeight)
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

                // Arrange at the bottom of remaining area
                var consumedHeight = measuredSize.Height + margin.Top + margin.Bottom;
                var arrangeRect = new Rectangle(
                    available.X,
                    remainingArea.Y + remainingArea.Height - measuredSize.Height - margin.Bottom,
                    available.Width,
                    measuredSize.Height);
                control.Arrange(arrangeRect);

                // Update remaining area - shrink from bottom
                remainingArea = new Rectangle(
                    remainingArea.X,
                    remainingArea.Y,
                    remainingArea.Width,
                    Math.Max(0, remainingArea.Height - consumedHeight)
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

                // Arrange at the left of remaining area
                var arrangeRect = new Rectangle(available.X, available.Y, measuredSize.Width, available.Height);
                control.Arrange(arrangeRect);

                // Update remaining area - shrink from left
                var consumedWidth = measuredSize.Width + margin.Left + margin.Right;
                remainingArea = new Rectangle(
                    remainingArea.X + consumedWidth,
                    remainingArea.Y,
                    Math.Max(0, remainingArea.Width - consumedWidth),
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

                // Arrange at the right of remaining area
                var consumedWidth = measuredSize.Width + margin.Left + margin.Right;
                var arrangeRect = new Rectangle(
                    remainingArea.X + remainingArea.Width - measuredSize.Width - margin.Right,
                    available.Y,
                    measuredSize.Width,
                    available.Height);
                control.Arrange(arrangeRect);

                // Update remaining area - shrink from right
                remainingArea = new Rectangle(
                    remainingArea.X,
                    remainingArea.Y,
                    Math.Max(0, remainingArea.Width - consumedWidth),
                    remainingArea.Height
                );
            }
                break;

            case DockStyle.Fill:
                // Arrange to fill the entire available area
                control.Arrange(new Rectangle(available.X, available.Y, available.Width, available.Height));
                remainingArea = new Rectangle(remainingArea.X, remainingArea.Y, 0, 0);
                break;
        }

        if (control.Dock != DockStyle.None)
            return;

        // Non-docked controls: Measure if AutoSize is enabled
        var finalSize = control.Size;
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

        // WinForms anchor behavior: distances are relative to DisplayRectangle (parent's client area minus padding)
        // clientArea IS the DisplayRectangle, but we need its dimensions for anchor calculations
        var displayWidth = clientArea.Width;
        var displayHeight = clientArea.Height;

        // Calculate initial distances from edges of DisplayRectangle
        var left = location.X;
        var top = location.Y;
        var right = displayWidth - (location.X + finalSize.Width);
        var bottom = displayHeight - (location.Y + finalSize.Height);

        // Adjust position and size based on anchors
        // Default anchor is Top,Left - control stays at fixed position
        if ((anchor & AnchorStyles.Left) != 0 && (anchor & AnchorStyles.Right) != 0)
            // Anchored to both left and right: stretch horizontally, maintain distances
            finalSize.Width = Math.Max(0, displayWidth - left - right);
        else if ((anchor & AnchorStyles.Right) != 0 && (anchor & AnchorStyles.Left) == 0)
            // Anchored to right only: maintain right distance, adjust X position
            location.X = Math.Max(0, displayWidth - right - finalSize.Width);
        // else: Left only or no horizontal anchor - keep location.X as is

        if ((anchor & AnchorStyles.Top) != 0 && (anchor & AnchorStyles.Bottom) != 0)
            // Anchored to both top and bottom: stretch vertically, maintain distances
            finalSize.Height = Math.Max(0, displayHeight - top - bottom);
        else if ((anchor & AnchorStyles.Bottom) != 0 && (anchor & AnchorStyles.Top) == 0)
            // Anchored to bottom only: maintain bottom distance, adjust Y position
            location.Y = Math.Max(0, displayHeight - bottom - finalSize.Height);
        // else: Top only or no vertical anchor - keep location.Y as is

        // Arrange the control at its final position
        // Location is already in parent's coordinate system (relative to parent's top-left corner)
        control.Arrange(new Rectangle(
            location.X,
            location.Y,
            Math.Max(0, finalSize.Width),
            Math.Max(0, finalSize.Height)));
    }
}