using System;
using System.ComponentModel;
using System.Drawing;

namespace SDUI.Controls;

// Partial class extension for TopItem support
public partial class ListView
{
    /// <summary>
    /// Gets or sets the first visible item in the control.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ListViewItem TopItem
    {
        get
        {
            if (_listViewItems == null || _listViewItems.Count == 0)
                return null;

            int topItemIndex = GetTopItemIndex();
            if (topItemIndex >= 0 && topItemIndex < _listViewItems.Count)
                return _listViewItems[topItemIndex];
            
            return null;
        }
        set
        {
            if (value == null || _listViewItems == null)
                return;

            int index = _listViewItems.IndexOf(value);
            if (index >= 0)
            {
                _verticalScrollOffset = CalculateVerticalOffsetForItem(index);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets the index of the top visible item based on current scroll offset
    /// </summary>
    private int GetTopItemIndex()
    {
        const float HEADER_HEIGHT = 30f;
        float y = HEADER_HEIGHT - _verticalScrollOffset;
        int currentIndex = 0;

        // Navigate through groups
        foreach (ListViewGroup group in Groups)
        {
            // Skip group header
            y += RowHeight;

            if (group.CollapsedState != System.Windows.Forms.ListViewGroupCollapsedState.Collapsed)
            {
                foreach (ListViewItem item in group.Items)
                {
                    // Check if this item is at or past the top of the viewport
                    if (y >= HEADER_HEIGHT)
                        return currentIndex;
                    
                    y += RowHeight;
                    currentIndex++;
                }
            }
        }

        // Check ungrouped items
        if (_listViewItems != null)
        {
            foreach (var item in _listViewItems)
            {
                if (item._group == null)
                {
                    if (y >= HEADER_HEIGHT)
                        return currentIndex;
                    
                    y += RowHeight;
                    currentIndex++;
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// Calculates the vertical scroll offset needed to place an item at the top of the viewport
    /// </summary>
    private float CalculateVerticalOffsetForItem(int targetIndex)
    {
        const float HEADER_HEIGHT = 30f;
        float offset = 0;
        int currentIndex = 0;

        foreach (ListViewGroup group in Groups)
        {
            offset += RowHeight; // Group header

            if (group.CollapsedState != System.Windows.Forms.ListViewGroupCollapsedState.Collapsed)
            {
                foreach (ListViewItem item in group.Items)
                {
                    if (currentIndex == targetIndex)
                        return offset - HEADER_HEIGHT;
                    
                    offset += RowHeight;
                    currentIndex++;
                }
            }
        }

        if (_listViewItems != null)
        {
            foreach (var item in _listViewItems)
            {
                if (item._group == null)
                {
                    if (currentIndex == targetIndex)
                        return offset - HEADER_HEIGHT;
                    
                    offset += RowHeight;
                    currentIndex++;
                }
            }
        }

        return 0;
    }
}
