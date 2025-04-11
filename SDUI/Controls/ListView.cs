using SDUI.Collections;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ListView : UIElementBase
{
    public System.Windows.Forms.ColumnHeaderStyle HeaderStyle { get; set; } = System.Windows.Forms.ColumnHeaderStyle.Clickable; // Not implemented

    public System.Windows.Forms.View View { get; set; } = System.Windows.Forms.View.Details;
    public System.Windows.Forms.BorderStyle BorderStyle { get; set; } = System.Windows.Forms.BorderStyle.None;
    public bool FullRowSelect { get; set; } = false;
    public bool CheckBoxes { get; set; } = false;
    public bool ShowItemToolTips { get; set; } = false;
    public bool UseCompatibleStateImageBehavior { get; set; } = false;
    private List<ListViewItem>? _listViewItems = [];
    private readonly Dictionary<int, ListViewItem> _listItemsTable = [];
    public IndexedList<System.Windows.Forms.ColumnHeader> Columns { get; } = [];
    public ListViewItemCollection Items { get; }
    public IndexedList<System.Windows.Forms.ListViewItem> CheckedItems { get; } = []; // Not implemented
    public IndexedList<int> SelectedIndices { get; } = []; // Not implemented
    public IndexedList<System.Windows.Forms.ListViewItem> SelectedItems { get; } = []; // Not implemented
    public SDUI.Collections.ListViewGroupCollection Groups { get; }

    private float _horizontalScrollOffset = 0;
    private float _verticalScrollOffset = 0;
    private bool _isResizingColumn = false;
    private int _resizingColumnIndex = -1;
    private int _columnResizeStartX = 0;
    private bool _isDraggingScrollbar = false;
    private bool _isVerticalScrollbar = false;
    private Point _scrollbarDragStart;

    private readonly int rowBoundsHeight = 30;
    private int maxVisibleRows => Height / rowBoundsHeight;

    private System.Timers.Timer _elasticTimer;
    private const float ElasticDecay = 0.85f;
    private const int ElasticInterval = 15;

    private int _selectedIndex = -1;
    internal bool IsHandleCreated = true;
    public bool LabelEdit;
    internal int VirtualListSize;

    public ListViewItem SelectedItem { get => _selectedIndex == -1 ? null : Items[_selectedIndex]; set { var index = Items.IndexOf(value); if(index != -1) _selectedIndex = index;  } }
    public int SelectedIndex { get => _selectedIndex; set { _selectedIndex = value;  SelectedIndexChanged?.Invoke(this, EventArgs.Empty);  } }

    public System.Windows.Forms.ImageList SmallImageList { get; set; }
    public System.Windows.Forms.ImageList LargeImageList { get; set; }
    public System.Windows.Forms.ImageList? GroupImageList { get; internal set; }
    public bool VirtualMode { get; internal set; }

    public event EventHandler SelectedIndexChanged;
    public event System.Windows.Forms.ItemCheckedEventHandler ItemChecked; // Not implemented

    public ListView()
    {
        _elasticTimer = new(ElasticInterval);
        _elasticTimer.Elapsed += ElasticTimer_Tick;

        Items = new(this);
        Groups = new(this);
    }

    public void SetItemImage(int index, int imageIndex)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        var item = Items[index];
        item.ImageIndex = imageIndex;
        Invalidate();
    }

    public void SetItemText(int index, string text)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        var item = Items[index];
        item.Text = text;
        Invalidate();
    }

    public void SetItemText(int index, int subItemIndex, string text)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        var item = Items[index];
        if (subItemIndex < 0 || subItemIndex >= item.SubItems.Count)
            throw new ArgumentOutOfRangeException(nameof(subItemIndex));
        var subItem = item.SubItems[subItemIndex];
        subItem.Text = text;
        Invalidate();
    }

    private void ElasticTimer_Tick(object sender, ElapsedEventArgs e)
    {
        bool needsRefresh = false;

        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
        {
            // Horizontal pull-back
            if (_horizontalScrollOffset < 0)
            {
                _horizontalScrollOffset *= ElasticDecay;
                if (_horizontalScrollOffset > -1) _horizontalScrollOffset = 0;
                needsRefresh = true;
            }
            else if (_horizontalScrollOffset > Columns.Sum(c => c.Width) - Width)
            {
                _horizontalScrollOffset -= (_horizontalScrollOffset - (Columns.Sum(c => c.Width) - Width)) * (1 - ElasticDecay);
                if (_horizontalScrollOffset < Columns.Sum(c => c.Width) - Width + 1)
                    _horizontalScrollOffset = Columns.Sum(c => c.Width) - Width;
                needsRefresh = true;
            }

        }

        // Vertical pull-back
        if (_verticalScrollOffset < 0)
        {
            _verticalScrollOffset *= ElasticDecay;
            if (_verticalScrollOffset > -1) _verticalScrollOffset = 0;
            needsRefresh = true;
        }
        else if (_verticalScrollOffset > (Items.Count - maxVisibleRows) * rowBoundsHeight)
        {
            _verticalScrollOffset -= (_verticalScrollOffset - (Items.Count - maxVisibleRows) * rowBoundsHeight) * (1 - ElasticDecay);
            if (_verticalScrollOffset < (Items.Count - maxVisibleRows) * rowBoundsHeight + 1)
                _verticalScrollOffset = (Items.Count - maxVisibleRows) * rowBoundsHeight;
            needsRefresh = true;
        }

        if (needsRefresh)
        {
            Invalidate();
        }
        else
        {
            _elasticTimer.Stop();
        }
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        DrawGroups(canvas);
        DrawColumns(canvas);
        DrawScrollBars(canvas);
    }

    private void DrawColumns(SKCanvas canvas)
    {
        var x = -_horizontalScrollOffset;
        using var gridPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.LightGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = .5f
        };

        using var headerPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.WhiteSmoke,
            Style = SKPaintStyle.StrokeAndFill,
            TextSize = 16f,
        };

        var rect = new SKRect(x, 0, Width, 30);
        canvas.DrawRect(rect, headerPaint);

        headerPaint.Color = SKColors.DarkSlateGray;
        foreach (var column in Columns)
        {
            canvas.DrawText(column.Text, x + 5, 20, headerPaint);

            // Draw grid lines for columns
            x += column.Width;
            canvas.DrawLine(x, 0, x, Height, gridPaint);
        }

        // Draw bottom line for the header
        canvas.DrawLine(0, 30, Width, 30, gridPaint);
    }

    private void DrawGroups(SKCanvas canvas)
    {
        var y = 30 - _verticalScrollOffset;
        using var groupPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            TextSize = 16f,
        };

        using var groupTextPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.DarkSlateGray,
            Style = SKPaintStyle.Fill,
            TextSize = 16f,
        };

        foreach (ListViewGroup group in Groups)
        {
            var rect = new SKRect();
            rect.Location = new SKPoint(5, y + 8);
            rect.Size = new SKSize(16, 16);

            groupPaint.Style = group.CollapsedState == ListViewGroupCollapsedState.Expanded ? SKPaintStyle.Fill : SKPaintStyle.Stroke;
            groupPaint.Color = group.CollapsedState == ListViewGroupCollapsedState.Expanded ? SKColors.DarkSlateGray : SKColors.LightSlateGray;

            canvas.DrawRoundRect(rect, 4, 4, groupPaint);
            canvas.DrawText(group.Header, 25, y + 20, groupTextPaint);

            y += rowBoundsHeight;

            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
            {
                foreach (ListViewItem item in group.Items)
                {
                    DrawRow(canvas, item, y);
                    y += rowBoundsHeight;
                }
            }
        }
    }

    private void DrawRow(SKCanvas canvas, ListViewItem row, float y)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextSize = 15,
            IsAutohinted = true,
        };

        using var gridPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.WhiteSmoke,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f
        };

        paint.Color = row.Selected ? SKColors.WhiteSmoke : SKColors.White;
        var rect = new SKRect(0, y, Width, y + rowBoundsHeight);
        canvas.DrawRect(rect, paint);

        // Draw row content (parameters)
        var x = -_horizontalScrollOffset;
        for (int i = 0; i < row.SubItems.Count; i++)
        {
            if (i < Columns.Count)
            {
                paint.Color = SKColors.Black;
                canvas.DrawText(row.SubItems[i].Text, x + 5, y + rowBoundsHeight / 2 + 5, paint);
                x += Columns[i].Width;
            }
        }

        // Draw row grid lines
        canvas.DrawLine(0, y, Width, y, gridPaint);
    }

    private void DrawScrollBars(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.Silver,
            Style = SKPaintStyle.StrokeAndFill
        };

        // Horizontal ScrollBar
        if (Columns.Sum(c => c.Width) > Width)
        {
            float scrollbarWidth = Width * (Width / (float)Columns.Sum(c => c.Width));
            float scrollbarX = _horizontalScrollOffset * (Width - scrollbarWidth) / (Columns.Sum(c => c.Width) - Width);
            var rect = new SKRect(scrollbarX + 5, Height - 15, scrollbarX + scrollbarWidth - 5, Height - 5);
            canvas.DrawRoundRect(rect, 4, 4, paint);
        }

        // Vertical ScrollBar

        var casted = Items.Cast<ListViewItem>();
        if (casted.Sum(r => rowBoundsHeight) > Height - 30)
        {
            float scrollbarHeight = (Height - 30) * ((Height - 30) / (float)casted.Sum(r => rowBoundsHeight));
            float scrollbarY = _verticalScrollOffset * (Height - 30 - scrollbarHeight) / (casted.Sum(r => rowBoundsHeight) - (Height - 30));
            var rect = new SKRect(Width - 5, 35 + scrollbarY, Width - 15, 15 + scrollbarY + scrollbarHeight);
            canvas.DrawRoundRect(rect, 8, 8, paint);
        }
    }

    internal override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        var delta = e.Delta / 120f;

        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
        {
            _horizontalScrollOffset = Math.Max(-Width / 4, Math.Min(_horizontalScrollOffset - delta * 30, Columns.Sum(c => c.Width) - Width + Width / 4));
        }
        else
        {
            _verticalScrollOffset = Math.Max(-Height / 4, _verticalScrollOffset - delta * 30);
            _verticalScrollOffset = Math.Min(_verticalScrollOffset, (Items.Count - maxVisibleRows) * 30 + Height / 4);
        }

        if (!_elasticTimer.Enabled)
        {
            _elasticTimer.Start();
        }

        Invalidate();
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        // Check if clicking on a scrollbar
        if (e.X >= Width - 15 && e.Y >= 30 && e.Y <= Height)
        {
            _isDraggingScrollbar = true;
            _isVerticalScrollbar = true;
            _scrollbarDragStart = e.Location;
            return;
        }
        if (e.Y >= Height - 15 && e.X >= 0 && e.X <= Width)
        {
            _isDraggingScrollbar = true;
            _isVerticalScrollbar = false;
            _scrollbarDragStart = e.Location;
            return;
        }

        var x = -_horizontalScrollOffset;
        for (int i = 0; i < Columns.Count; i++)
        {
            if (Math.Abs(e.X - (x + Columns[i].Width)) < 5)
            {
                _isResizingColumn = true;
                _resizingColumnIndex = i;
                _columnResizeStartX = e.X;
                return;
            }

            x += Columns[i].Width;
        }

        var y = 30 - _verticalScrollOffset;
        foreach (ListViewGroup group in Groups)
        {
            var groupRect = new SKRect(0, y, Width, y + rowBoundsHeight);
            if (groupRect.Contains(e.X, e.Y))
            {
                group.CollapsedState = group.CollapsedState == ListViewGroupCollapsedState.Expanded ? ListViewGroupCollapsedState.Collapsed : ListViewGroupCollapsedState.Expanded;

                Invalidate();
                return;
            }

            y += rowBoundsHeight;

            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
            {
                foreach (ListViewItem item in group.Items)
                {
                    var itemRect = new SKRect(0, y, Width, y + rowBoundsHeight);
                    if (itemRect.Contains(e.X, e.Y))
                    {
                        foreach (ListViewItem r in Items)
                        {
                            r.Selected = false;
                        }

                        item.Selected = true;

                        SelectedIndex = Items.IndexOf(item);

                        Invalidate();
                        return;
                    }
                    y += rowBoundsHeight;
                }
            }
        }
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        _isResizingColumn = false;
        _isDraggingScrollbar = false;
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        if (_isResizingColumn && _resizingColumnIndex >= 0)
        {
            int delta = e.X - _columnResizeStartX;
            Columns[_resizingColumnIndex].Width = Math.Max(30, Columns[_resizingColumnIndex].Width + delta);
            _columnResizeStartX = e.X;

            Invalidate();
        }

        bool isResizingColumn = false;
        for (int i = 0; i < Columns.Count; i++)
        {
            int columnStartX = Columns.Take(i).Sum(p => p.Width);
            int columnEndX = columnStartX + Columns[i].Width;

            if (e.X > columnEndX - 5 && e.X < columnEndX + 5)
            {
                isResizingColumn = true;
                break;
            }
        }

        if (isResizingColumn)
        {
            Cursor = Cursors.SizeWE; // Resize cursor
        }
        else
        {
            Cursor = Cursors.Default; // Default cursor
        }

        if (_isDraggingScrollbar)
        {
            int delta = _isVerticalScrollbar ? e.Y - _scrollbarDragStart.Y : e.X - _scrollbarDragStart.X;
            if (_isVerticalScrollbar)
            {
                _verticalScrollOffset = Math.Max(-Height / 4, Math.Min(_verticalScrollOffset + delta * 3, Items.Cast<ListViewItem>().Sum(r => rowBoundsHeight) - Height + 30 + Height / 4));
            }
            else
            {
                _horizontalScrollOffset = Math.Max(-Width / 4, Math.Min(_horizontalScrollOffset + delta * 3, Columns.Sum(c => c.Width) - Width + Width / 4));
            }
        }
        _scrollbarDragStart = e.Location;
        Invalidate();
    }

    internal override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        var x = -_horizontalScrollOffset;
        for (int i = 0; i < Columns.Count; i++)
        {
            if (Math.Abs(e.X - (x + Columns[i].Width)) < 5)
            {
                // Auto-size column based on the widest content in the column
                int maxWidth = 30; // Minimum column width
                using (var paint = new SKPaint { TextSize = 14, IsAntialias = true })
                {
                    foreach (ListViewItem row in Items)
                    {
                        if (i < row.SubItems.Count)
                        {
                            var textWidth = (int)paint.MeasureText(row.SubItems[i].Text);
                            maxWidth = Math.Max(maxWidth, textWidth + 10); // Add padding
                        }
                    }
                }
                Columns[i].Width = maxWidth;
                Invalidate(); // Redraw the control
                return;
            }
            x += Columns[i].Width;
        }
    }

    public void BeginUpdate()
    {
        _elasticTimer.Stop();
    }

    public void EndUpdate()
    {
        _elasticTimer.Start();
    }

    internal Rectangle GetItemRect(int index, ItemBoundsPortion portion = ItemBoundsPortion.Entire)
    {
        var item = Items.Cast<ListViewItem>().ElementAt(index);
        var baseRect = new Rectangle(0, 30 + (index * rowBoundsHeight) - (int)_verticalScrollOffset, Width, rowBoundsHeight);

        switch (portion)
        {
            case ItemBoundsPortion.Entire:
                return baseRect;

            case ItemBoundsPortion.Icon:
                return new Rectangle(baseRect.X + 5, baseRect.Y + 5, rowBoundsHeight - 10, rowBoundsHeight - 10);

            case ItemBoundsPortion.Label:
                return new Rectangle(baseRect.X + rowBoundsHeight, baseRect.Y, baseRect.Width - rowBoundsHeight, baseRect.Height);

            default:
                throw new ArgumentOutOfRangeException(nameof(portion), portion, "Invalid ItemBoundsPortion value.");
        }
    }

    internal Rectangle GetSubItemRect(int index, int subItemIndex)
    {
        var item = Items.Cast<ListViewItem>().ElementAt(index).SubItems[subItemIndex];
        return item.Bounds;
    }

    internal void UpdateSavedCheckedItems(ListViewItem listViewItem, bool value)
    {
        //throw new NotImplementedException();
    }

    public void InsertGroupInListView(int index, ListViewGroup group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));
        if (Groups.Contains(group))
            return;
        group.ListView = this;
        Groups.Insert(index, group);
    }

    public void RemoveGroupFromListView(ListViewGroup group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));
        if (!Groups.Contains(group))
            return;
        group.ListView = null;
        Groups.Remove(group);
    }

    internal void SetItemIndentCount(int index, int indentCount)
    { }

    public void InsertItems(int index, ListViewItem[] items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        if (index < 0 || index > Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        foreach (var item in items)
        {
            Items.Insert(index, item);
            index++;
        }
    }

    internal ListViewItem GetItemAt(int x, int y)
    {
        var item = Items.Cast<ListViewItem>().FirstOrDefault(i => i.Bounds.Contains(x, y));
        return item;
    }

    internal void GetSubItemAt(int x, int y, out int itemIndex, out int subItemIndex)
    {
        itemIndex = -1;
        subItemIndex = -1;

        for (int i = 0; i < Items.Count; i++)
        {
            var itemBounds = GetItemRect(i);
            if (itemBounds.Contains(x, y))
            {
                itemIndex = i;

                var subItemX = itemBounds.X;
                for (int j = 0; j < Columns.Count; j++)
                {
                    var subItemWidth = Columns[j].Width;
                    if (x >= subItemX && x < subItemX + subItemWidth)
                    {
                        subItemIndex = j;
                        return;
                    }
                    subItemX += subItemWidth;
                }
            }
        }
    }

    internal void EnsureVisible(int index)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        var itemRect = GetItemRect(index);
        var visibleArea = new Rectangle(0, 30, Width, Height - 30);
        if (!visibleArea.Contains(itemRect))
        {
            _verticalScrollOffset = Math.Max(0, itemRect.Y - visibleArea.Height / 2);
            Invalidate();
        }
    }

    public ListViewItem FindNearestItem(SearchDirectionHint directionHint, int x, int y)
    {
        var item = Items.Cast<ListViewItem>().FirstOrDefault(i => i.Bounds.Contains(x, y));
        if (item != null)
        {
            return item;
        }
        // If no item found, return null
        return null;
    }
        

    internal ListViewItem GetItemAt(Point point)
    {
        var item = Items.Cast<ListViewItem>().FirstOrDefault(i => i.Bounds.Contains(point.X, point.Y));
        return item;
    }

    internal ListViewItem GetSubItemAt(Point point)
    {
        var item = Items.Cast<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem>().Any(s => s.Bounds.Contains(point.X, point.Y)));
        return item;
    }

    public int GetDisplayIndex(ListViewItem listViewItem, int displayIndex)
    {
        return Items.IndexOf(listViewItem);
    }
}