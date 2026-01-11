using System;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Controls;
using SDUI.Layout;

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

    /// <summary>
    ///     Checks if an anchor style has a specific flag set.
    /// </summary>
    private static bool IsAnchored(AnchorStyles anchor, AnchorStyles flag) => (anchor & flag) != 0;

    /// <summary>
    ///     Updates the control's anchor information based on its current bounds and parent display rectangle.
    ///     This is called after a control is positioned to record its distances from parent edges.
    ///     Matches DefaultLayout.cs UpdateAnchorInfo logic (lines 709-801).
    /// </summary>
    internal static void UpdateAnchorInfo(UIElementBase element, Rectangle parentDisplayRect)
    {
        if (element._anchorInfo == null)
            element._anchorInfo = new AnchorInfo();

        var bounds = element.Bounds;
        var anchor = element.Anchor;
        var anchorInfo = element._anchorInfo;

        // Calculate absolute positions relative to parent DisplayRectangle
        anchorInfo.Left = bounds.Left - parentDisplayRect.X;
        anchorInfo.Top = bounds.Top - parentDisplayRect.Y;
        anchorInfo.Right = bounds.Right - parentDisplayRect.X;
        anchorInfo.Bottom = bounds.Bottom - parentDisplayRect.Y;

        int parentWidth = parentDisplayRect.Width;
        int parentHeight = parentDisplayRect.Height;

        // Convert Right/Bottom to distances from parent's right/bottom edge (negative values)
        // This matches DefaultLayout.cs UpdateAnchorInfo logic (lines 747-801)
        if (IsAnchored(anchor, AnchorStyles.Right))
        {
            anchorInfo.Right -= parentWidth;
            if (!IsAnchored(anchor, AnchorStyles.Left))
                anchorInfo.Left -= parentWidth;
        }
        else if (!IsAnchored(anchor, AnchorStyles.Left))
        {
            // Neither left nor right: center horizontally
            int center = parentWidth / 2;
            anchorInfo.Right -= center;
            anchorInfo.Left -= center;
        }

        if (IsAnchored(anchor, AnchorStyles.Bottom))
        {
            anchorInfo.Bottom -= parentHeight;
            if (!IsAnchored(anchor, AnchorStyles.Top))
                anchorInfo.Top -= parentHeight;
        }
        else if (!IsAnchored(anchor, AnchorStyles.Top))
        {
            // Neither top nor bottom: center vertically
            int center = parentHeight / 2;
            anchorInfo.Bottom -= center;
            anchorInfo.Top -= center;
        }
    }

    /// <summary>
    ///     Computes anchored bounds for a control based on stored anchor information and current parent display rectangle.
    ///     This is called during layout to reposition/resize anchored controls when parent size changes.
    ///     Matches DefaultLayout.cs ComputeAnchoredBounds logic (lines 234-272).
    /// </summary>
    private static Rectangle ComputeAnchoredBounds(UIElementBase element, Rectangle parentDisplayRect)
    {
        var anchorInfo = element._anchorInfo;
        if (anchorInfo == null)
        {
            // No anchor info yet - return current bounds
            return element.Bounds;
        }

        var anchor = element.Anchor;
        int left = anchorInfo.Left + parentDisplayRect.X;
        int top = anchorInfo.Top + parentDisplayRect.Y;
        int right = anchorInfo.Right + parentDisplayRect.X;
        int bottom = anchorInfo.Bottom + parentDisplayRect.Y;

        // Adjust for parent size changes - matches DefaultLayout.cs ComputeAnchoredBounds (lines 236-272)
        if (IsAnchored(anchor, AnchorStyles.Right))
        {
            right += parentDisplayRect.Width;
            if (!IsAnchored(anchor, AnchorStyles.Left))
                left += parentDisplayRect.Width;
        }
        else if (!IsAnchored(anchor, AnchorStyles.Left))
        {
            int center = parentDisplayRect.Width / 2;
            right += center;
            left += center;
        }

        if (IsAnchored(anchor, AnchorStyles.Bottom))
        {
            bottom += parentDisplayRect.Height;
            if (!IsAnchored(anchor, AnchorStyles.Top))
                top += parentDisplayRect.Height;
        }
        else if (!IsAnchored(anchor, AnchorStyles.Top))
        {
            int center = parentDisplayRect.Height / 2;
            bottom += center;
            top += center;
        }

        // Ensure positive size
        if (right < left) right = left;
        if (bottom < top) bottom = top;

        return new Rectangle(left, top, right - left, bottom - top);
    }

    internal static void Perform(UIElementBase control, Rectangle clientArea, ref Rectangle remainingArea)
    {
        if (!control.Visible)
            return;

        switch (control.Dock)
        {
            case DockStyle.Top:
            {
                // Measure with width constrained to remaining width
                Size elementSize;
                if (control.AutoSize)
                {
                    elementSize = control.Measure(new Size(remainingArea.Width, 0));
                    if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                        elementSize.Height = Math.Max(control.Size.Height, elementSize.Height);
                }
                else
                {
                    elementSize = new Size(remainingArea.Width, control.Size.Height);
                }

                // Position at top of remaining area
                Rectangle bounds = new(remainingArea.X, remainingArea.Y, elementSize.Width, elementSize.Height);
                control.Arrange(bounds);

                // Update remaining area using actual bounds (control may have adjusted during Arrange)
                remainingArea.Y += control.Bounds.Height;
                remainingArea.Height -= control.Bounds.Height;
                break;
            }

            case DockStyle.Bottom:
            {
                // Measure with width constrained to remaining width
                Size elementSize;
                if (control.AutoSize)
                {
                    elementSize = control.Measure(new Size(remainingArea.Width, 0));
                    if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                        elementSize.Height = Math.Max(control.Size.Height, elementSize.Height);
                }
                else
                {
                    elementSize = new Size(remainingArea.Width, control.Size.Height);
                }

                // Position at bottom of remaining area
                Rectangle bounds = new(remainingArea.X, remainingArea.Bottom - elementSize.Height, elementSize.Width, elementSize.Height);
                control.Arrange(bounds);

                // Update remaining area using actual bounds
                remainingArea.Height -= control.Bounds.Height;
                break;
            }

            case DockStyle.Left:
            {
                // Measure with height constrained to remaining height
                Size elementSize;
                if (control.AutoSize)
                {
                    elementSize = control.Measure(new Size(0, remainingArea.Height));
                    if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                        elementSize.Width = Math.Max(control.Size.Width, elementSize.Width);
                }
                else
                {
                    elementSize = new Size(control.Size.Width, remainingArea.Height);
                }

                // Position at left of remaining area
                Rectangle bounds = new(remainingArea.X, remainingArea.Y, elementSize.Width, elementSize.Height);
                control.Arrange(bounds);

                // Update remaining area using actual bounds
                remainingArea.X += control.Bounds.Width;
                remainingArea.Width -= control.Bounds.Width;
                break;
            }

            case DockStyle.Right:
            {
                // Measure with height constrained to remaining height
                Size elementSize;
                if (control.AutoSize)
                {
                    elementSize = control.Measure(new Size(0, remainingArea.Height));
                    if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                        elementSize.Width = Math.Max(control.Size.Width, elementSize.Width);
                }
                else
                {
                    elementSize = new Size(control.Size.Width, remainingArea.Height);
                }

                // Position at right of remaining area
                Rectangle bounds = new(remainingArea.Right - elementSize.Width, remainingArea.Y, elementSize.Width, elementSize.Height);
                control.Arrange(bounds);

                // Update remaining area using actual bounds
                remainingArea.Width -= control.Bounds.Width;
                break;
            }

            case DockStyle.Fill:
                // Fill entire remaining area
                control.Arrange(remainingArea);
                remainingArea = new Rectangle(remainingArea.X, remainingArea.Y, 0, 0);
                break;
        }

        // Handle non-docked controls with anchors
        if (control.Dock != DockStyle.None)
            return;

        // If we have stored anchor info, use it to compute bounds based on current parent size
        if (control._anchorInfo != null)
        {
            // Parent has been resized - recompute anchored position/size
            Rectangle anchoredBounds = ComputeAnchoredBounds(control, clientArea);
            control.Arrange(anchoredBounds);
        }
        else
        {
            // First time seeing this anchored control - measure if needed, then store anchor info
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

            // Position control at its current location with final size
            control.Arrange(new Rectangle(control.Location, finalSize));
            
            // Now store anchor info based on where we just placed it
            UpdateAnchorInfo(control, clientArea);
        }
    }
}
