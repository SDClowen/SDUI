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
    public System.Windows.Forms.ColumnHeaderStyle HeaderStyle { get; set; } = System.Windows.Forms.ColumnHeaderStyle.Clickable;
    public System.Windows.Forms.View View { get; set; } = System.Windows.Forms.View.Details;
    public System.Windows.Forms.BorderStyle BorderStyle { get; set; } = System.Windows.Forms.BorderStyle.None;
    public bool FullRowSelect { get; set; } = false;
    public bool CheckBoxes { get; set; } = false;
    public bool ShowItemToolTips { get; set; } = false;
    public bool UseCompatibleStateImageBehavior { get; set; } = false;
    internal List<ListViewItem>? _listViewItems = [];
    private readonly Dictionary<int, ListViewItem> _listItemsTable = [];
    public IndexedList<System.Windows.Forms.ColumnHeader> Columns { get; } = [];
    public ListViewItemCollection Items { get; }
    public IndexedList<System.Windows.Forms.ListViewItem> CheckedItems { get; } = [];
    public IndexedList<int> SelectedIndices { get; } = [];
    public IndexedList<System.Windows.Forms.ListViewItem> SelectedItems { get; } = [];
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

    private int _hoverSeparatorIndex = -1;
    private int _dragStartX;
    private int _dragInitialWidth;
    private float _dragStartHOffset;
    private float _dragStartVOffset;

    public ListViewItem SelectedItem 
    { 
        get => _selectedIndex >= 0 && _selectedIndex < (_listViewItems?.Count ?? 0) ? _listViewItems[_selectedIndex] : null; 
        set 
        { 
            if (_listViewItems != null)
            {
                var index = _listViewItems.IndexOf(value); 
                if(index != -1) 
                    _selectedIndex = index;
            }
        } 
    }
    
    public int SelectedIndex 
    { 
        get => _selectedIndex; 
        set 
        { 
            _selectedIndex = value;  
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);  
        } 
    }

    public System.Windows.Forms.ImageList SmallImageList { get; set; }
    public System.Windows.Forms.ImageList LargeImageList { get; set; }
    public System.Windows.Forms.ImageList? GroupImageList { get; internal set; }
    public bool VirtualMode { get; internal set; }

    public event EventHandler SelectedIndexChanged;
    public event System.Windows.Forms.ItemCheckedEventHandler ItemChecked;

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

        // Horizontal elastic (Shift for horizontal scroll)
        int totalColumnsWidth = Columns.Sum(c => c.Width);
        float hMax = Math.Max(0, totalColumnsWidth - Width);

        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
        {
            if (_horizontalScrollOffset < 0)
            {
                _horizontalScrollOffset *= ElasticDecay;
                if (_horizontalScrollOffset > -1) _horizontalScrollOffset = 0;
                needsRefresh = true;
            }
            else if (_horizontalScrollOffset > hMax)
            {
                _horizontalScrollOffset -= (_horizontalScrollOffset - hMax) * (1 - ElasticDecay);
                if (_horizontalScrollOffset < hMax + 1)
                    _horizontalScrollOffset = hMax;
                needsRefresh = true;
            }
        }

        // Vertical elastic
        int contentHeight = GetContentHeight();
        int viewportHeight = Math.Max(0, Height - 30);
        float vMax = Math.Max(0, contentHeight - viewportHeight);

        if (_verticalScrollOffset < 0)
        {
            _verticalScrollOffset *= ElasticDecay;
            if (_verticalScrollOffset > -1) _verticalScrollOffset = 0;
            needsRefresh = true;
        }
        else if (_verticalScrollOffset > vMax)
        {
            _verticalScrollOffset -= (_verticalScrollOffset - vMax) * (1 - ElasticDecay);
            if (_verticalScrollOffset < vMax + 1)
                _verticalScrollOffset = vMax;
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

    // Effective row height based on current font (keeps 9pt readable and scaled)
    private int RowHeight => Math.Max(24, (int)Math.Ceiling(GetSkTextSize(Font) + 12));

    private float GetSkTextSize(Font font)
    {
        // Use current system DPI for point->pixel to keep 9pt consistent across scales.
        // 1pt = 1/72 inch -> pixels = pt * (DPI / 72)
        float dpi = DeviceDpi > 0 ? DeviceDpi : 96f;
        float pt = font.Unit == GraphicsUnit.Point ? font.SizeInPoints : font.Size;
        return (float)(pt * (dpi / 72f));
    }

    private static SKTypeface CreateTypeface(Font font)
    {
        return SKTypeface.FromFamilyName(font.FontFamily.Name,
            font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
    }

    private SKFont CreateFont(Font font)
    {
        var tf = CreateTypeface(font);
        var f = new SKFont(tf, GetSkTextSize(font))
        {
            Hinting = SKFontHinting.Full,
            Edging = SKFontEdging.SubpixelAntialias,
            Subpixel = true
        };
        return f;
    }

    private static void DrawTextCompat(SKCanvas canvas, string text, float x, float y, SKFont font, SKPaint colorSource)
    {
        using var p = new SKPaint
        {
            IsAntialias = true,
            Color = colorSource.Color,
            Style = SKPaintStyle.Fill,
            TextSize = font.Size,
            Typeface = font.Typeface,
            SubpixelText = true
        };
        canvas.DrawText(text, x, y, p);
    }

    // Adjust header drawing to always fill full width
    private void DrawColumns(SKCanvas canvas)
    {
        // Always fill full header width
        using var headerBack = new SKPaint { IsAntialias = true, Color = SKColors.WhiteSmoke, Style = SKPaintStyle.Fill };
        canvas.DrawRect(new SKRect(0, 0, Width, 30), headerBack);

        using var gridPaint = new SKPaint { IsAntialias = true, Color = SKColors.LightGray, Style = SKPaintStyle.Stroke, StrokeWidth = .5f };
        using var headerFont = CreateFont(Font);
        using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.DarkSlateGray, Style = SKPaintStyle.Fill };

        // horizontal culling for columns
        int colIndex = 0;
        float colX = -_horizontalScrollOffset;
        while (colIndex < Columns.Count && colX + Columns[colIndex].Width <= 0)
        {
            colX += Columns[colIndex].Width;
            colIndex++;
        }
        for (; colIndex < Columns.Count && colX < Width; colIndex++)
        {
            var column = Columns[colIndex];
            DrawTextCompat(canvas, column.Text ?? string.Empty, colX + 5, 20, headerFont, textPaint);
            colX += column.Width;
            canvas.DrawLine(colX, 0, colX, Height, gridPaint);
        }

        canvas.DrawLine(0, 30, Width, 30, gridPaint);
    }

    private void DrawGroups(SKCanvas canvas)
    {
        var y = 30 - _verticalScrollOffset;
        using var groupPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        using var groupTextPaint = new SKPaint { IsAntialias = true, Color = SKColors.DarkSlateGray, Style = SKPaintStyle.Fill };
        using var groupFont = CreateFont(Font);

        foreach (ListViewGroup group in Groups)
        {
            if (y + RowHeight >= 0 && y <= Height)
            {
                var rect = new SKRect();
                rect.Location = new SKPoint(5, y + 8);
                rect.Size = new SKSize(16, 16);

                groupPaint.Style = group.CollapsedState == ListViewGroupCollapsedState.Expanded ? SKPaintStyle.Fill : SKPaintStyle.Stroke;
                groupPaint.Color = group.CollapsedState == ListViewGroupCollapsedState.Expanded ? SKColors.DarkSlateGray : SKColors.LightSlateGray;

                canvas.DrawRoundRect(rect, 4, 4, groupPaint);
                DrawTextCompat(canvas, group.Header ?? string.Empty, 25, y + 20, groupFont, groupTextPaint);
            }

            y += RowHeight;

            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
            {
                foreach (ListViewItem item in group.Items)
                {
                    if (y > Height) break;
                    if (y + RowHeight >= 0)
                    {
                        DrawRow(canvas, item, y);
                    }
                    y += RowHeight;
                }
            }

            if (y > Height) break;
        }

        if (y <= Height && _listViewItems != null)
        {
            foreach (var item in _listViewItems)
            {
                if (item._group == null)
                {
                    if (y > Height) break;
                    if (y + RowHeight >= 0)
                    {
                        DrawRow(canvas, item, y);
                    }
                    y += RowHeight;
                }
            }
        }
    }

    private void DrawRow(SKCanvas canvas, ListViewItem row, float y)
    {
        using var backPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var gridPaint = new SKPaint { IsAntialias = true, Color = SKColors.WhiteSmoke, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
        using var rowFont = CreateFont(row.Font ?? Font);
        using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black, Style = SKPaintStyle.Fill };

        backPaint.Color = row.StateSelected ? SKColors.LightBlue : (!row.BackColor.IsEmpty ? ToSKColor(row.BackColor) : SKColors.White);
        var rect = new SKRect(0, y, Width, y + RowHeight);
        canvas.DrawRect(rect, backPaint);

        var x = -_horizontalScrollOffset;
        int i = 0;
        // skip columns left of viewport
        while (i < Columns.Count && x + Columns[i].Width <= 0)
        {
            x += Columns[i].Width;
            i++;
        }

        for (; i < row.SubItems.Count && i < Columns.Count && x < Width; i++)
        {
            var foreColor = !row.ForeColor.IsEmpty ? ToSKColor(row.ForeColor) : SKColors.Black;
            textPaint.Color = foreColor;

            // Vertical centering using font metrics from SKFont
            var fm = rowFont.Metrics;
            float textY = y + (RowHeight - (fm.Descent - fm.Ascent)) / 2f - fm.Ascent;

            DrawTextCompat(canvas, row.SubItems[i].Text ?? string.Empty, x + 5, textY, rowFont, textPaint);
            x += Columns[i].Width;
        }

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

        // Horizontal
        int totalColumnsWidth = Columns.Sum(c => c.Width);
        if (totalColumnsWidth > Width)
        {
            float scrollbarWidth = Width * (Width / (float)totalColumnsWidth);
            float scrollbarX = _horizontalScrollOffset * (Width - scrollbarWidth) / (totalColumnsWidth - Width);
            var rect = new SKRect(scrollbarX + 5, Height - 15, scrollbarX + scrollbarWidth - 5, Height - 5);
            canvas.DrawRoundRect(rect, 4, 4, paint);
        }

        // Vertical
        int contentHeight = GetContentHeight();
        int viewportHeight = Math.Max(0, Height - 30);
        if (contentHeight > viewportHeight && viewportHeight > 0)
        {
            float scrollbarHeight = viewportHeight * (viewportHeight / (float)contentHeight);
            float scrollbarY = _verticalScrollOffset * (viewportHeight - scrollbarHeight) / (contentHeight - viewportHeight);
            var rect = new SKRect(Width - 15, 35 + scrollbarY, Width - 5, 35 + scrollbarY + scrollbarHeight);
            canvas.DrawRoundRect(rect, 8, 8, paint);
        }
    }

    internal override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        
        if (e == null)
            return;

        var delta = e.Delta / 120f;

        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
        {
            int totalColumnsWidth = Columns.Sum(c => c.Width);
            _horizontalScrollOffset = Math.Max(-Width / 4, Math.Min(_horizontalScrollOffset - delta * 30, totalColumnsWidth - Width + Width / 4));
        }
        else
        {
            int contentHeight = GetContentHeight();
            int viewportHeight = Math.Max(0, Height - 30);
            _verticalScrollOffset = Math.Max(-Height / 4, _verticalScrollOffset - delta * 30);
            _verticalScrollOffset = Math.Min(_verticalScrollOffset, Math.Max(0, contentHeight - viewportHeight) + Height / 4);
        }

        if (!_elasticTimer.Enabled)
        {
            _elasticTimer.Start();
        }

        Invalidate();
    }

    private int HitTestHeaderSeparator(int mouseX)
    {
        float xAccum = -_horizontalScrollOffset;
        for (int i = 0; i < Columns.Count; i++)
        {
            float edge = xAccum + Columns[i].Width;
            if (mouseX >= edge - 6 && mouseX <= edge + 6)
                return i;
            xAccum = edge;
        }
        return -1;
    }

    // Update all row height usages
    private int GetContentRows()
    {
        int rows = 0;
        foreach (ListViewGroup group in Groups)
        {
            rows += 1;
            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
            {
                rows += group.Items.Count;
            }
        }
        if (_listViewItems != null)
        {
            rows += _listViewItems.Count(item => item._group == null);
        }
        return rows;
    }

    private int GetContentHeight() => GetContentRows() * RowHeight;

    internal Rectangle GetItemRect(int index, ItemBoundsPortion portion = ItemBoundsPortion.Entire)
    {
        if (_listViewItems == null || index < 0 || index >= _listViewItems.Count)
            return Rectangle.Empty;

        var y = 30f;
        int currentIndex = 0;

        foreach (ListViewGroup group in Groups)
        {
            y += RowHeight;

            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
            {
                foreach (ListViewItem item in group.Items)
                {
                    if (currentIndex == index)
                    {
                        var baseRect = new Rectangle(0, (int)(y - _verticalScrollOffset), Width, RowHeight);
                        return GetItemRectPortion(baseRect, portion);
                    }
                    y += RowHeight;
                    currentIndex++;
                }
            }
        }

        foreach (var item in _listViewItems)
        {
            if (item._group == null)
            {
                if (currentIndex == index)
                {
                    var baseRect = new Rectangle(0, (int)(y - _verticalScrollOffset), Width, RowHeight);
                    return GetItemRectPortion(baseRect, portion);
                }
                y += RowHeight;
                currentIndex++;
            }
        }

        return Rectangle.Empty;
    }

    private Rectangle GetItemRectPortion(Rectangle baseRect, ItemBoundsPortion portion)
    {
        return portion switch
        {
            ItemBoundsPortion.Entire => baseRect,
            ItemBoundsPortion.Icon => new Rectangle(baseRect.X + 5, baseRect.Y + 5, RowHeight - 10, RowHeight - 10),
            ItemBoundsPortion.Label => new Rectangle(baseRect.X + RowHeight, baseRect.Y, baseRect.Width - RowHeight, baseRect.Height),
            _ => throw new ArgumentOutOfRangeException(nameof(portion), portion, "Invalid ItemBoundsPortion value.")
        };
    }

    internal Rectangle GetSubItemRect(int index, int subItemIndex)
    {
        var item = Items.Cast<ListViewItem>().ElementAt(index).SubItems[subItemIndex];
        return item.Bounds;
    }

    internal void UpdateSavedCheckedItems(ListViewItem listViewItem, bool value)
    {
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
    {
    }

    public void InsertItems(int index, ListViewItem[] items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        
        _listViewItems ??= [];
        
        if (index < 0 || index > _listViewItems.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        foreach (var item in items)
        {
            _listViewItems.Insert(index, item);
            item._listView = this;
            index++;
        }
        
        Invalidate();
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
        return item;
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
        if (_listViewItems == null)
            return -1;

        if (displayIndex >= 0 && displayIndex < _listViewItems.Count && _listViewItems[displayIndex] == listViewItem)
            return displayIndex;

        return _listViewItems.IndexOf(listViewItem);
    }

    private static SKColor ToSKColor(Color color)
    {
        if (color.IsEmpty)
        {
            return SKColors.Transparent;
        }
        // SKColor ctor expects RGBA (bytes)
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    // Add missing mouse handlers for interactions
    internal override void OnMouseDown(MouseEventArgs e)
    {
        // Scrollbar hit test
        int contentHeight = GetContentHeight();
        int viewportHeight = Math.Max(0, Height - 30);
        int totalColumnsWidth = Columns.Sum(c => c.Width);

        if (e.X >= Width - 15 && e.X <= Width - 5 && contentHeight > viewportHeight && e.Y >= 30 && e.Y <= Height)
        {
            _isDraggingScrollbar = true;
            _isVerticalScrollbar = true;
            _scrollbarDragStart = e.Location;
            _dragStartVOffset = _verticalScrollOffset;
            return;
        }
        if (e.Y >= Height - 15 && e.Y <= Height && totalColumnsWidth > Width)
        {
            _isDraggingScrollbar = true;
            _isVerticalScrollbar = false;
            _scrollbarDragStart = e.Location;
            _dragStartHOffset = _horizontalScrollOffset;
            return;
        }

        // Column resize only in header area
        if (e.Y <= 30)
        {
            int sep = HitTestHeaderSeparator(e.X);
            if (sep >= 0)
            {
                _isResizingColumn = true;
                _resizingColumnIndex = sep;
                _dragStartX = e.X;
                _dragInitialWidth = Columns[sep].Width;
                return;
            }
        }

        // Group and item selection
        var y = 30 - _verticalScrollOffset;
        foreach (ListViewGroup group in Groups)
        {
            var groupRect = new SKRect(0, y, Width, y + RowHeight);
            if (groupRect.Contains(e.X, e.Y))
            {
                group.CollapsedState = group.CollapsedState == ListViewGroupCollapsedState.Expanded
                    ? ListViewGroupCollapsedState.Collapsed
                    : ListViewGroupCollapsedState.Expanded;
                Invalidate();
                return;
            }

            y += RowHeight;

            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
            {
                foreach (ListViewItem item in group.Items)
                {
                    var itemRect = new SKRect(0, y, Width, y + RowHeight);
                    if (itemRect.Contains(e.X, e.Y))
                    {
                        if (_listViewItems != null)
                        {
                            foreach (var r in _listViewItems)
                            {
                                r.StateSelected = false;
                            }
                        }

                        item.StateSelected = true;
                        _selectedIndex = _listViewItems?.IndexOf(item) ?? -1;
                        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                        return;
                    }
                    y += RowHeight;
                }
            }

            if (y > Height) break;
        }

        if (_listViewItems != null)
        {
            foreach (var item in _listViewItems)
            {
                if (item._group == null)
                {
                    var itemRect = new SKRect(0, y, Width, y + RowHeight);
                    if (itemRect.Contains(e.X, e.Y))
                    {
                        foreach (var r in _listViewItems)
                        {
                            r.StateSelected = false;
                        }

                        item.StateSelected = true;
                        _selectedIndex = _listViewItems.IndexOf(item);
                        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                        return;
                    }
                    y += RowHeight;
                    if (y > Height) break;
                }
            }
        }
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        bool needInvalidate = false;

        if (_isResizingColumn && _resizingColumnIndex >= 0)
        {
            int delta = e.X - _dragStartX;
            int newWidth = Math.Max(30, _dragInitialWidth + delta);
            if (newWidth != Columns[_resizingColumnIndex].Width)
            {
                Columns[_resizingColumnIndex].Width = newWidth;
                needInvalidate = true;
            }
        }
        else
        {
            int newHover = (e.Y <= 30) ? HitTestHeaderSeparator(e.X) : -1;
            if (newHover != _hoverSeparatorIndex)
            {
                _hoverSeparatorIndex = newHover;
                
                // Cursor'u değiştir ve parent window'a bildir
                if (_hoverSeparatorIndex >= 0)
                {
                    Cursor = Cursors.SizeWE;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
                
                // Parent window'a cursor değişikliğini bildir
                if (Parent is UIWindow parentWindow)
                {
                    parentWindow.UpdateCursor(this);
                }
            }
        }

        if (_isDraggingScrollbar)
        {
            int totalColumnsWidth = Columns.Sum(c => c.Width);
            if (_isVerticalScrollbar)
            {
                int contentHeight = GetContentHeight();
                int viewportHeight = Math.Max(0, Height - 30);
                if (contentHeight > viewportHeight)
                {
                    float sh = viewportHeight * (viewportHeight / (float)contentHeight);
                    float track = Math.Max(1f, viewportHeight - sh);
                    int delta = e.Y - _scrollbarDragStart.Y;
                    float contentDelta = delta * (contentHeight - viewportHeight) / track;
                    float newOffset = _dragStartVOffset + contentDelta;
                    float maxOffset = Math.Max(0, contentHeight - viewportHeight);
                    newOffset = Math.Max(-Height / 4f, Math.Min(newOffset, maxOffset + Height / 4f));
                    if (Math.Abs(newOffset - _verticalScrollOffset) > 0.5f)
                    {
                        _verticalScrollOffset = newOffset;
                        needInvalidate = true;
                    }
                }
            }
            else if (totalColumnsWidth > Width)
            {
                float sw = Width * (Width / (float)totalColumnsWidth);
                float track = Math.Max(1f, Width - sw);
                int delta = e.X - _scrollbarDragStart.X;
                float contentDelta = delta * (totalColumnsWidth - Width) / track;
                float newOffset = _dragStartHOffset + contentDelta;
                float maxOffset = totalColumnsWidth - Width;
                newOffset = Math.Max(-Width / 4f, Math.Min(newOffset, maxOffset + Width / 4f));
                if (Math.Abs(newOffset - _horizontalScrollOffset) > 0.5f)
                {
                    _horizontalScrollOffset = newOffset;
                    needInvalidate = true;
                }
            }
        }

        _scrollbarDragStart = e.Location;
        if (needInvalidate) Invalidate();
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        _isResizingColumn = false;
        _isDraggingScrollbar = false;
        
        // Resize bittiğinde cursor'u sıfırla
        if (_hoverSeparatorIndex < 0)
        {
            Cursor = Cursors.Default;
            if (Parent is UIWindow parentWindow)
            {
                parentWindow.UpdateCursor(this);
            }
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoverSeparatorIndex = -1;
        
        if (!_isResizingColumn)
        {
            Cursor = Cursors.Default;
            if (Parent is UIWindow parentWindow)
            {
                parentWindow.UpdateCursor(null);
            }
        }
    }

    internal override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        // Auto-size column when double-clicking separator in header
        var x = -_horizontalScrollOffset;
        for (int i = 0; i < Columns.Count; i++)
        {
            if (Math.Abs(e.X - (x + Columns[i].Width)) < 6 && e.Y <= 30)
            {
                int maxWidth = 30;
                using (var font = CreateFont(Font))
                {
                    for (int r = 0; r < Items.Count; r++)
                    {
                        var item = Items[r] as ListViewItem;
                        if (item != null && i < item.SubItems.Count)
                        {
                            SKRect bounds;
                            var text = item.SubItems[i].Text ?? string.Empty;
                            font.MeasureText(text, out bounds);
                            int textWidth = (int)Math.Ceiling(bounds.Width);
                            maxWidth = Math.Max(maxWidth, textWidth + 16);
                        }
                    }
                }
                Columns[i].Width = maxWidth;
                Invalidate();
                return;
            }
            x += Columns[i].Width;
        }
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        DrawColumns(canvas);
        DrawGroups(canvas);
        DrawScrollBars(canvas);
    }
}