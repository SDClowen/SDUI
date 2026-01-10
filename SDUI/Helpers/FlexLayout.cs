using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SDUI.Controls;
using SDUI.Enums;

namespace SDUI.Enums
{
    /// <summary>
    ///     Flex container direction
    /// </summary>
    public enum FlexDirection
    {
        Row,
        RowReverse,
        Column,
        ColumnReverse
    }

    /// <summary>
    ///     Main axis alignment
    /// </summary>
    public enum JustifyContent
    {
        FlexStart,
        FlexEnd,
        Center,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly
    }

    /// <summary>
    ///     Cross axis alignment
    /// </summary>
    public enum AlignItems
    {
        FlexStart,
        FlexEnd,
        Center,
        Stretch
    }

    /// <summary>
    ///     Item wrapping behavior
    /// </summary>
    public enum FlexWrap
    {
        NoWrap,
        Wrap,
        WrapReverse
    }
}

namespace SDUI.Helpers
{
    /// <summary>
    ///     Modern flex layout system for SDUI controls
    /// </summary>
    public class FlexLayout
    {
        public FlexDirection Direction { get; set; } = FlexDirection.Row;
        public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
        public AlignItems AlignItems { get; set; } = AlignItems.FlexStart;
        public FlexWrap Wrap { get; set; } = FlexWrap.NoWrap;
        public int Gap { get; set; } = 0;
        public Padding Padding { get; set; } = new(0);

        /// <summary>
        ///     Perform flex layout on controls
        /// </summary>
        public void PerformLayout(UIElementBase container, Rectangle clientArea)
        {
            var controls = container.Controls
                .OfType<UIElementBase>()
                .Where(c => c.Visible && c.Dock == DockStyle.None)
                .ToList();

            if (controls.Count == 0)
                return;

            // Apply padding
            var contentArea = new Rectangle(
                clientArea.Left + Padding.Left,
                clientArea.Top + Padding.Top,
                clientArea.Width - Padding.Horizontal,
                clientArea.Height - Padding.Vertical
            );

            var isHorizontal = Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse;
            var isReverse = Direction == FlexDirection.RowReverse || Direction == FlexDirection.ColumnReverse;

            if (isReverse)
                controls.Reverse();

            // Calculate layout
            if (Wrap == FlexWrap.NoWrap)
                LayoutSingleLine(controls, contentArea, isHorizontal);
            else
                LayoutMultiLine(controls, contentArea, isHorizontal);
        }

        private void LayoutSingleLine(List<UIElementBase> controls, Rectangle area, bool isHorizontal)
        {
            // Calculate total size
            var totalSize = 0;
            var maxCrossSize = 0;

            foreach (var control in controls)
                if (isHorizontal)
                {
                    totalSize += control.Size.Width;
                    maxCrossSize = Math.Max(maxCrossSize, control.Size.Height);
                }
                else
                {
                    totalSize += control.Size.Height;
                    maxCrossSize = Math.Max(maxCrossSize, control.Size.Width);
                }

            totalSize += Gap * (controls.Count - 1);

            // Calculate main axis positions
            var positions = CalculateMainAxisPositions(
                controls.Count,
                totalSize,
                isHorizontal ? area.Width : area.Height
            );

            // Position controls
            for (var i = 0; i < controls.Count; i++)
            {
                var control = controls[i];
                var mainPos = positions[i];
                var crossPos = CalculateCrossAxisPosition(
                    isHorizontal ? control.Size.Height : control.Size.Width,
                    isHorizontal ? area.Height : area.Width
                );

                if (isHorizontal)
                {
                    control.Location = new Point(area.Left + mainPos, area.Top + crossPos);

                    // Apply stretch
                    if (AlignItems == AlignItems.Stretch)
                        control.Size = new Size(control.Size.Width, area.Height);
                }
                else
                {
                    control.Location = new Point(area.Left + crossPos, area.Top + mainPos);

                    // Apply stretch
                    if (AlignItems == AlignItems.Stretch)
                        control.Size = new Size(area.Width, control.Size.Height);
                }
            }
        }

        private void LayoutMultiLine(List<UIElementBase> controls, Rectangle area, bool isHorizontal)
        {
            // Group controls into lines
            var lines = new List<List<UIElementBase>>();
            var currentLine = new List<UIElementBase>();
            var currentLineSize = 0;
            var maxLineSize = isHorizontal ? area.Width : area.Height;

            foreach (var control in controls)
            {
                var controlSize = isHorizontal ? control.Size.Width : control.Size.Height;
                var requiredSize = currentLineSize + controlSize + (currentLine.Count > 0 ? Gap : 0);

                if (requiredSize > maxLineSize && currentLine.Count > 0)
                {
                    lines.Add(currentLine);
                    currentLine = new List<UIElementBase>();
                    currentLineSize = 0;
                }

                currentLine.Add(control);
                currentLineSize += controlSize + (currentLine.Count > 1 ? Gap : 0);
            }

            if (currentLine.Count > 0)
                lines.Add(currentLine);

            // Position each line
            var crossOffset = 0;
            foreach (var line in lines)
            {
                var lineMainSize = 0;
                var lineCrossSize = 0;

                foreach (var control in line)
                    if (isHorizontal)
                    {
                        lineMainSize += control.Size.Width;
                        lineCrossSize = Math.Max(lineCrossSize, control.Size.Height);
                    }
                    else
                    {
                        lineMainSize += control.Size.Height;
                        lineCrossSize = Math.Max(lineCrossSize, control.Size.Width);
                    }

                lineMainSize += Gap * (line.Count - 1);

                var positions = CalculateMainAxisPositions(line.Count, lineMainSize, maxLineSize);

                for (var i = 0; i < line.Count; i++)
                {
                    var control = line[i];
                    var mainPos = positions[i];
                    var crossPos = CalculateCrossAxisPosition(
                        isHorizontal ? control.Size.Height : control.Size.Width,
                        lineCrossSize
                    );

                    if (isHorizontal)
                        control.Location = new Point(area.Left + mainPos, area.Top + crossOffset + crossPos);
                    else
                        control.Location = new Point(area.Left + crossOffset + crossPos, area.Top + mainPos);
                }

                crossOffset += lineCrossSize + Gap;
            }
        }

        private int[] CalculateMainAxisPositions(int count, int totalSize, int availableSize)
        {
            var positions = new int[count];

            switch (JustifyContent)
            {
                case JustifyContent.FlexStart:
                    var pos = 0;
                    for (var i = 0; i < count; i++)
                    {
                        positions[i] = pos;
                        pos += i < count - 1 ? Gap : 0;
                    }

                    break;

                case JustifyContent.FlexEnd:
                    var offset = availableSize - totalSize;
                    pos = offset;
                    for (var i = 0; i < count; i++)
                    {
                        positions[i] = pos;
                        pos += i < count - 1 ? Gap : 0;
                    }

                    break;

                case JustifyContent.Center:
                    offset = (availableSize - totalSize) / 2;
                    pos = offset;
                    for (var i = 0; i < count; i++)
                    {
                        positions[i] = pos;
                        pos += i < count - 1 ? Gap : 0;
                    }

                    break;

                case JustifyContent.SpaceBetween:
                    if (count == 1)
                    {
                        positions[0] = 0;
                    }
                    else
                    {
                        var spacing = (availableSize - totalSize + Gap * (count - 1)) / (count - 1);
                        pos = 0;
                        for (var i = 0; i < count; i++)
                        {
                            positions[i] = pos;
                            pos += spacing;
                        }
                    }

                    break;

                case JustifyContent.SpaceAround:
                    var space = (availableSize - totalSize + Gap * (count - 1)) / count;
                    pos = space / 2;
                    for (var i = 0; i < count; i++)
                    {
                        positions[i] = pos;
                        pos += space;
                    }

                    break;

                case JustifyContent.SpaceEvenly:
                    space = (availableSize - totalSize + Gap * (count - 1)) / (count + 1);
                    pos = space;
                    for (var i = 0; i < count; i++)
                    {
                        positions[i] = pos;
                        pos += space;
                    }

                    break;
            }

            return positions;
        }

        private int CalculateCrossAxisPosition(int itemSize, int lineSize)
        {
            return AlignItems switch
            {
                AlignItems.FlexStart => 0,
                AlignItems.FlexEnd => lineSize - itemSize,
                AlignItems.Center => (lineSize - itemSize) / 2,
                AlignItems.Stretch => 0,
                _ => 0
            };
        }
    }
}