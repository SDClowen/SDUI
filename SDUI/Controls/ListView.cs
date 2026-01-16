using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Collections;
using SDUI.Helpers;
using SkiaSharp;
using ListViewGroupCollection = SDUI.Collections.ListViewGroupCollection;
using Timer = System.Timers.Timer;

namespace SDUI.Controls;

public partial class ListView : UIElementBase
{
    private const float ElasticDecay = 0.85f;
    private const int ElasticInterval = 15;

    private readonly Timer _elasticTimer;

    // Group expand/collapse animation için
    private readonly Dictionary<ListViewGroup, AnimationManager> _groupAnimations = new();
    private readonly Dictionary<int, ListViewItem> _listItemsTable = new();

    private readonly HashSet<ListViewGroup>
        _pendingCollapse = new(); // OUT animasyonu bitince gerçekten collapse edilecekler

    private readonly int rowBoundsHeight = 30;
    private SKPath? _chevronPath;
    private int _columnResizeStartX = 0;

    private SKFont? _defaultSkFont;
    private int _defaultSkFontDpi;
    private Font? _defaultSkFontSource;
    private int _dragInitialWidth;
    private float _dragStartHOffset;
    private float _dragStartVOffset;
    private int _dragStartX;

    private float _horizontalScrollOffset;

    private int _hoverSeparatorIndex = -1;
    private bool _isDraggingScrollbar;
    private bool _isResizingColumn;
    private bool _isVerticalScrollbar;
    internal List<ListViewItem>? _listViewItems;
    private SKPaint? _paintChevron;

    // Skia hot-path caches (avoid per-frame allocations)
    private SKPaint? _paintFill;
    private SKPaint? _paintGrid;
    private SKPaint? _paintHeaderBack;
    private SKPaint? _paintScrollBar;
    private SKPaint? _paintText;
    private SKPaint? _paintTextCompat;
    private int _resizingColumnIndex = -1;
    private Point _scrollbarDragStart;

    private int _selectedIndex = -1;
    private float _verticalScrollOffset;
    internal bool IsHandleCreated = true;
    public bool LabelEdit;
    internal int VirtualListSize;

    public ListView()
    {
        _elasticTimer = new Timer(ElasticInterval);
        _elasticTimer.Elapsed += ElasticTimer_Tick;

        Items = new ListViewItemCollection(this);
        Groups = new ListViewGroupCollection(this);
    }

    public ColumnHeaderStyle HeaderStyle { get; set; } = ColumnHeaderStyle.Clickable;
    public View View { get; set; } = View.Details;
    public BorderStyle BorderStyle { get; set; } = BorderStyle.None;
    public bool FullRowSelect { get; set; } = false;
    public bool CheckBoxes { get; set; } = false;
    public bool ShowItemToolTips { get; set; } = false;
    public bool UseCompatibleStateImageBehavior { get; set; } = false;
    public IndexedList<ColumnHeader> Columns { get; } = new();
    public ListViewItemCollection Items { get; }
    public IndexedList<ListViewItem> CheckedItems { get; } = new();
    public IndexedList<int> SelectedIndices { get; } = new();
    public IndexedList<ListViewItem> SelectedItems { get; } = new();
    public ListViewGroupCollection Groups { get; }

    public ImageList SmallImageList { get; set; }
    public ImageList LargeImageList { get; set; }
    public ImageList? GroupImageList { get; internal set; }
    public bool VirtualMode { get; internal set; }
    private int maxVisibleRows => Height / rowBoundsHeight;

    public ListViewItem SelectedItem
    {
        get => _selectedIndex >= 0 && _selectedIndex < (_listViewItems?.Count ?? 0)
            ? _listViewItems[_selectedIndex]
            : null;
        set
        {
            if (_listViewItems != null)
            {
                var index = _listViewItems.IndexOf(value);
                if (index != -1)
                    SelectedIndex = index;
            }
        }
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;

            if (_listViewItems != null)
            {
                foreach (var item in _listViewItems)
                    item.StateSelected = false;

                SelectedItems.Clear();
                SelectedIndices.Clear();

                if (_selectedIndex >= 0 && _selectedIndex < _listViewItems.Count)
                {
                    var item = _listViewItems[_selectedIndex];
                    item.StateSelected = true;
                    SelectedItems.Add(item);
                    SelectedIndices.Add(_selectedIndex);
                }
            }

            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    // Effective row height based on current font (keeps 9pt readable and scaled)
    private int RowHeight => Math.Max(24, (int)Math.Ceiling(GetSkTextSize(Font) + 12));

    public event EventHandler SelectedIndexChanged;
    public event ItemCheckedEventHandler ItemChecked;

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
        var needsRefresh = false;

        // Horizontal elastic (Shift for horizontal scroll)
        var totalColumnsWidth = GetTotalColumnsWidth();
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
        var contentHeight = GetContentHeight();
        var viewportHeight = Math.Max(0, Height - 30);
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
            Invalidate();
        else
            _elasticTimer.Stop();
    }

    private int GetTotalColumnsWidth()
    {
        var total = 0;
        for (var i = 0; i < Columns.Count; i++)
            total += Columns[i].Width;
        return total;
    }

    private float GetSkTextSize(Font font)
    {
        // Use current system DPI for point->pixel to keep 9pt consistent across scales.
        // 1pt = 1/72 inch -> pixels = pt * (DPI / 72)
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96f;
        var pt = font.Unit == GraphicsUnit.Point ? font.SizeInPoints : font.Size;
        return pt * (dpi / 72f);
    }

    private static SKTypeface CreateTypeface(Font font)
    {
        return FontManager.GetSKTypeface(font);
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

    private SKFont GetDefaultSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        if (_defaultSkFont == null || !ReferenceEquals(_defaultSkFontSource, Font) || _defaultSkFontDpi != dpi)
        {
            _defaultSkFont?.Dispose();
            _defaultSkFont = CreateFont(Font);
            _defaultSkFontSource = Font;
            _defaultSkFontDpi = dpi;
        }

        return _defaultSkFont;
    }

    protected override void InvalidateFontCache()
    {
        base.InvalidateFontCache();
        _defaultSkFont?.Dispose();
        _defaultSkFont = null;
        _defaultSkFontSource = null;
        _defaultSkFontDpi = 0;
    }

    private SKPaint GetFillPaint()
    {
        return _paintFill ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
    }

    private SKPaint GetHeaderBackPaint()
    {
        return _paintHeaderBack ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
    }

    private SKPaint GetGridPaint()
    {
        return _paintGrid ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f };
    }

    private SKPaint GetTextPaint()
    {
        return _paintText ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
    }

    private SKPaint GetScrollBarPaint()
    {
        return _paintScrollBar ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.StrokeAndFill };
    }

    private SKPaint GetChevronPaint()
    {
        return _paintChevron ??= new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.75f,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
    }

    private SKPath GetChevronPath()
    {
        return _chevronPath ??= new SKPath();
    }

    private SKPaint GetTextCompatPaint()
    {
        return _paintTextCompat ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
    }

    private void DrawTextCompat(SKCanvas canvas, string text, float x, float y, SKFont font, SKColor color)
    {
        var p = GetTextCompatPaint();
        p.Color = color;
        TextRenderingHelper.DrawText(canvas, text, x, y, SKTextAlign.Left, font, p);
    }

    // Adjust header drawing to always fill full width
    private void DrawColumns(SKCanvas canvas)
    {
        // Always fill full header width
        var headerBack = GetHeaderBackPaint();
        headerBack.Color = ColorScheme.SurfaceContainerHigh.ToSKColor();
        canvas.DrawRect(new SKRect(0, 0, Width, 30), headerBack);

        var gridPaint = GetGridPaint();
        gridPaint.Color = ColorScheme.OutlineVariant.ToSKColor();
        gridPaint.StrokeWidth = 0.5f;
        var headerFont = GetDefaultSkFont();
        var textPaint = GetTextPaint();
        textPaint.Color = ColorScheme.OnSurface.ToSKColor();

        // horizontal culling for columns
        var colIndex = 0;
        var colX = -_horizontalScrollOffset;
        while (colIndex < Columns.Count && colX + Columns[colIndex].Width <= 0)
        {
            colX += Columns[colIndex].Width;
            colIndex++;
        }

        for (; colIndex < Columns.Count && colX < Width; colIndex++)
        {
            var column = Columns[colIndex];
            var fm = headerFont.Metrics;
            var textY = 15f - (fm.Ascent + fm.Descent) / 2f;
            DrawTextCompat(canvas, column.Text ?? string.Empty, colX + 5, textY, headerFont, textPaint.Color);
            colX += column.Width;
            canvas.DrawLine(colX, 0, colX, Height, gridPaint);
        }

        canvas.DrawLine(0, 30, Width, 30, gridPaint);
    }

    private void DrawGroups(SKCanvas canvas)
    {
        const float HEADER_HEIGHT = 30f;
        var y = HEADER_HEIGHT - _verticalScrollOffset;
        foreach (ListViewGroup group in Groups)
        {
            y += RowHeight;
            var isExpanded = group.CollapsedState == ListViewGroupCollapsedState.Expanded;
            var isAnimatingCollapse = _pendingCollapse.Contains(group);
            var progress = _groupAnimations.TryGetValue(group, out var anim) ? anim.GetProgress() :
                isExpanded ? 1.0 : 0.0;
            var drawItems = isExpanded || isAnimatingCollapse;
            if (drawItems && group.Items.Count > 0)
            {
                var itemCount = group.Items.Count;
                var visible = (int)Math.Ceiling(itemCount * progress);
                for (var i = 0; i < visible && i < itemCount; i++)
                {
                    var item = group.Items[i];
                    var slide = (float)(1.0 - progress) * (RowHeight * 0.3f);
                    var itemY = y - slide;
                    if (itemY + RowHeight >= HEADER_HEIGHT && itemY <= Height)
                        // Grup içindeki item'larda ek arka plan overlay'i kullanma,
                        // sadece satır çizimi ve animasyon (slide) kalsın.
                        DrawRow(canvas, item, itemY, true);
                    y += RowHeight;
                    if (y > Height) break;
                }
            }

            if (y > Height) break;
        }

        if (y <= Height && _listViewItems != null)
            foreach (var item in _listViewItems)
                if (item._group == null)
                {
                    if (y > Height) break;
                    if (y + RowHeight >= HEADER_HEIGHT) DrawRow(canvas, item, y);
                    y += RowHeight;
                }
    }

    private void DrawStickyGroupHeaders(SKCanvas canvas)
    {
        const float HEADER_HEIGHT = 30f;
        var font = GetDefaultSkFont();
        var textPaint = GetTextPaint();
        textPaint.Color = ColorScheme.OnSurface.ToSKColor();

        // Header arka planını her zaman tam genişlikte çiz, scrollbar sağda üstüne binsin.
        var contentHeightAll = GetContentHeight();
        var viewportHeight = Math.Max(0, Height - 30);

        var y = HEADER_HEIGHT - _verticalScrollOffset;
        ListViewGroup sticky = null;
        var stickyY = HEADER_HEIGHT;
        foreach (ListViewGroup group in Groups)
        {
            var prog = _groupAnimations.TryGetValue(group, out var anim) ? anim.GetProgress() :
                group.CollapsedState == ListViewGroupCollapsedState.Expanded ? 1.0 : 0.0;
            var animCollapse = _pendingCollapse.Contains(group);
            var itemCount = group.Items.Count;
            var contentH = group.CollapsedState == ListViewGroupCollapsedState.Collapsed && !animCollapse
                ? 0f
                : (float)(itemCount * RowHeight * prog);
            var startY = y;
            var endY = startY + RowHeight + contentH;
            if (sticky == null && startY <= HEADER_HEIGHT && endY > HEADER_HEIGHT)
            {
                sticky = group;
                stickyY = HEADER_HEIGHT;
                var nextIdx = Groups.IndexOf(group) + 1;
                if (nextIdx < Groups.Count)
                {
                    var ng = Groups[nextIdx];
                    var nProg = _groupAnimations.TryGetValue(ng, out var na) ? na.GetProgress() :
                        ng.CollapsedState == ListViewGroupCollapsedState.Expanded ? 1.0 : 0.0;
                    var nAnimCollapse = _pendingCollapse.Contains(ng);
                    var nContent = ng.CollapsedState == ListViewGroupCollapsedState.Collapsed && !nAnimCollapse
                        ? 0f
                        : (float)(ng.Items.Count * RowHeight * nProg);
                    var nStart = endY;
                    if (nStart < HEADER_HEIGHT + RowHeight) stickyY = nStart - RowHeight;
                }
            }

            y = endY;
            if (y > Height) break;
        }

        y = HEADER_HEIGHT - _verticalScrollOffset;
        foreach (ListViewGroup group in Groups)
        {
            var prog = _groupAnimations.TryGetValue(group, out var anim) ? anim.GetProgress() :
                group.CollapsedState == ListViewGroupCollapsedState.Expanded ? 1.0 : 0.0;
            var animCollapse = _pendingCollapse.Contains(group);
            var itemCount = group.Items.Count;
            var contentH = group.CollapsedState == ListViewGroupCollapsedState.Collapsed && !animCollapse
                ? 0f
                : (float)(itemCount * RowHeight * prog);
            var headerY = y;
            if (group == sticky) headerY = stickyY;
            if (headerY >= HEADER_HEIGHT && headerY <= Height)
            {
                var bg = GetFillPaint();
                bg.Color = ColorScheme.SurfaceContainer.ToSKColor();
                canvas.DrawRect(0, headerY, Width, RowHeight, bg);

                var border = GetGridPaint();
                border.Color = ColorScheme.OutlineVariant.ToSKColor();
                border.StrokeWidth = group == sticky ? 1.5f : 1f;
                canvas.DrawLine(0, headerY + RowHeight, Width, headerY + RowHeight, border);

                // Chevron: collapsed => sağ ok (>), expanded => aşağı V
                // Temel path: aşağı bakan V. Collapsed iken -90° döndürülür (sağa bakar).
                var cX = 12f;
                var cY = headerY + RowHeight / 2f;
                var s = 4.5f;
                var rotation = (float)((1.0 - prog) * -90.0f); // prog=1 -> 0° (down V), prog=0 -> -90° (right arrow)
                var chevPaint = GetChevronPaint();
                chevPaint.Color = ColorScheme.OnSurfaceVariant.ToSKColor();
                var path = GetChevronPath();
                path.Reset();
                // Aşağı bakan V
                path.MoveTo(cX - s, cY - s * 0.5f);
                path.LineTo(cX, cY + s * 0.7f);
                path.LineTo(cX + s, cY - s * 0.5f);
                canvas.Save();
                canvas.Translate(cX, cY);
                canvas.RotateDegrees(rotation);
                canvas.Translate(-cX, -cY);
                canvas.DrawPath(path, chevPaint);
                canvas.Restore();

                // Metni dikey ortala (font metrics)
                var fm = font.Metrics;
                var textHeight = fm.Descent - fm.Ascent;
                var baseline = headerY + (RowHeight - textHeight) / 2f - fm.Ascent;
                DrawTextCompat(canvas, group.Header ?? string.Empty, 25, baseline, font, textPaint.Color);
            }

            y += RowHeight + contentH;
            if (y > Height) break;
        }
    }

    private void DrawRow(SKCanvas canvas, ListViewItem row, float y, bool isGroupItem = false)
    {
        // Use the enhanced version with icon support
        DrawRowWithIcon(canvas, row, y, isGroupItem);
    }

    private void DrawScrollBars(SKCanvas canvas)
    {
        var paint = GetScrollBarPaint();
        paint.Color = ColorScheme.OutlineVariant.Alpha(140).ToSKColor();

        // Horizontal (içerik üstüne overlay)
        var totalColumnsWidth = GetTotalColumnsWidth();
        if (totalColumnsWidth > Width)
        {
            var scrollbarWidth = Width * (Width / (float)totalColumnsWidth);
            var scrollbarX = _horizontalScrollOffset * (Width - scrollbarWidth) / (totalColumnsWidth - Width);
            var rect = new SKRect(scrollbarX + 5, Height - 15, scrollbarX + scrollbarWidth - 5, Height - 5);
            canvas.DrawRoundRect(rect, 4, 4, paint);
        }

        // Vertical (header'dan itibaren içerik üstüne overlay)
        var contentHeight = GetContentHeight();
        var viewportHeight = Math.Max(0, Height - 30);
        if (contentHeight > viewportHeight && viewportHeight > 0)
        {
            var scrollbarHeight = viewportHeight * (viewportHeight / (float)contentHeight);
            var scrollbarY = _verticalScrollOffset * (viewportHeight - scrollbarHeight) /
                             (contentHeight - viewportHeight);
            var rect = new SKRect(Width - 15, 30 + scrollbarY, Width - 5, 30 + scrollbarY + scrollbarHeight);
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
            var totalColumnsWidth = GetTotalColumnsWidth();
            _horizontalScrollOffset = Math.Max(-Width / 4,
                Math.Min(_horizontalScrollOffset - delta * 30, totalColumnsWidth - Width + Width / 4));
        }
        else
        {
            var contentHeight = GetContentHeight();
            var viewportHeight = Math.Max(0, Height - 30);
            _verticalScrollOffset = Math.Max(-Height / 4, _verticalScrollOffset - delta * 30);
            _verticalScrollOffset =
                Math.Min(_verticalScrollOffset, Math.Max(0, contentHeight - viewportHeight) + Height / 4);
        }

        if (!_elasticTimer.Enabled) _elasticTimer.Start();

        Invalidate();
    }

    private int HitTestHeaderSeparator(int mouseX)
    {
        var xAccum = -_horizontalScrollOffset;
        for (var i = 0; i < Columns.Count; i++)
        {
            var edge = xAccum + Columns[i].Width;
            if (mouseX >= edge - 6 && mouseX <= edge + 6)
                return i;
            xAccum = edge;
        }

        return -1;
    }

    // Update all row height usages
    private int GetContentRows()
    {
        var rows = 0;
        foreach (ListViewGroup group in Groups)
        {
            rows += 1; // Group header

            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
            {
                // Animasyon progress'ini dikkate al
                var animProgress = 1.0;
                if (_groupAnimations.TryGetValue(group, out var animation)) animProgress = animation.GetProgress();

                // Animasyonlu row count
                rows += (int)Math.Ceiling(group.Items.Count * animProgress);
            }
        }

        if (_listViewItems != null) rows += _listViewItems.Count(item => item._group == null);

        return rows;
    }

    private int GetContentHeight()
    {
        return GetContentRows() * RowHeight;
    }

    internal Rectangle GetItemRect(int index, ItemBoundsPortion portion = ItemBoundsPortion.Entire)
    {
        if (_listViewItems == null || index < 0 || index >= _listViewItems.Count)
            return Rectangle.Empty;

        var y = 30f;
        var currentIndex = 0;

        foreach (ListViewGroup group in Groups)
        {
            y += RowHeight;

            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
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

        foreach (var item in _listViewItems)
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

        return Rectangle.Empty;
    }

    private Rectangle GetItemRectPortion(Rectangle baseRect, ItemBoundsPortion portion)
    {
        return portion switch
        {
            ItemBoundsPortion.Entire => baseRect,
            ItemBoundsPortion.Icon => new Rectangle(baseRect.X + 5, baseRect.Y + 5, RowHeight - 10, RowHeight - 10),
            ItemBoundsPortion.Label => new Rectangle(baseRect.X + RowHeight, baseRect.Y, baseRect.Width - RowHeight,
                baseRect.Height),
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
        if (group is null) throw new ArgumentNullException(nameof(group));
        if (Groups.Contains(group)) return;
        group.ListView = this;
        Groups.Insert(index, group);
        var anim = new AnimationManager()
        {
            Increment = 0.15,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        double lastProgress = -1;
        anim.OnAnimationProgress += sender =>
        {
            var prog = anim.GetProgress();
            if (Math.Abs(prog - lastProgress) > 0.01) // Only invalidate on visible progress change
            {
                lastProgress = prog;
                Invalidate();
            }
        };
        anim.OnAnimationFinished += _ =>
        {
            if (_pendingCollapse.Contains(group))
            {
                group.CollapsedState = ListViewGroupCollapsedState.Collapsed;
                _pendingCollapse.Remove(group);
                Invalidate();
            }
        };
        anim.SetProgress(group.CollapsedState == ListViewGroupCollapsedState.Expanded ? 1.0 : 0.0);
        _groupAnimations[group] = anim;
        Invalidate();
    }

    public void RemoveGroupFromListView(ListViewGroup group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));
        if (!Groups.Contains(group))
            return;
        group.ListView = null;
        Groups.Remove(group);

        // Animation'ı temizle
        if (_groupAnimations.ContainsKey(group))
        {
            _groupAnimations[group]?.Dispose();
            _groupAnimations.Remove(group);
        }
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

        for (var i = 0; i < Items.Count; i++)
        {
            var itemBounds = GetItemRect(i);
            if (itemBounds.Contains(x, y))
            {
                itemIndex = i;

                var subItemX = itemBounds.X;
                for (var j = 0; j < Columns.Count; j++)
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
        var item = Items.Cast<ListViewItem>().FirstOrDefault(i =>
            i.SubItems.OfType<ListViewItem>().Any(s => s.Bounds.Contains(point.X, point.Y)));
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
        if (color.IsEmpty) return SKColors.Transparent;
        // SKColor ctor expects RGBA (bytes)
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    // Add missing mouse handlers for interactions
    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        // 1) Scrollbar hit test
        var contentHeight = GetContentHeight();
        var viewportHeight = Math.Max(0, Height - 30);
        var totalColumnsWidth = GetTotalColumnsWidth();
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

        // 2) Column resize only in header area
        if (e.Y <= 30)
        {
            var sep = HitTestHeaderSeparatorEnhanced(e.X);
            if (sep >= 0)
            {
                _isResizingColumn = true;
                _resizingColumnIndex = sep;
                _dragStartX = e.X;
                _dragInitialWidth = Columns[sep].Width;
                Cursor = Cursors.SizeWE;
                if (Parent is UIWindow pw) pw.UpdateCursor(this);
                return;
            }
        }

        // 3) Group header toggle + item selection (animation-aware)
        var y = 30 - _verticalScrollOffset;
        foreach (ListViewGroup group in Groups)
        {
            // Header rect
            var headerRect = new SKRect(0, y, Width, y + RowHeight);
            if (headerRect.Contains(e.X, e.Y))
            {
                var isExpanded = group.CollapsedState == ListViewGroupCollapsedState.Expanded;
                if (isExpanded)
                {
                    if (_groupAnimations.TryGetValue(group, out var anim))
                    {
                        _pendingCollapse.Add(group);
                        anim.StartNewAnimation(AnimationDirection.Out);
                    }
                }
                else
                {
                    group.CollapsedState = ListViewGroupCollapsedState.Expanded;
                    if (_groupAnimations.TryGetValue(group, out var anim))
                        anim.StartNewAnimation(AnimationDirection.In);
                }

                Invalidate();
                return;
            }

            // advance into items area for this group
            y += RowHeight;

            // Items hit-test
            var expandedOrAnimatingOut = group.CollapsedState == ListViewGroupCollapsedState.Expanded ||
                                         _pendingCollapse.Contains(group);
            if (expandedOrAnimatingOut && group.Items.Count > 0)
            {
                var prog = _groupAnimations.TryGetValue(group, out var anim) ? anim.GetProgress() : 1.0;
                var visible = (int)Math.Ceiling(group.Items.Count * Math.Clamp(prog, 0.0, 1.0));
                for (var i = 0; i < visible && i < group.Items.Count; i++)
                {
                    var item = group.Items[i];
                    var slide = (float)(1.0 - prog) * (RowHeight * 0.3f);
                    var itemY = y - slide;
                    var itemRect = new SKRect(0, itemY, Width, itemY + RowHeight);
                    if (itemRect.Contains(e.X, e.Y))
                    {
                        // Clear previous selection
                        if (_listViewItems != null)
                            foreach (var r in _listViewItems)
                                r.StateSelected = false;

                        SelectedItems.Clear();
                        SelectedIndices.Clear();

                        item.StateSelected = true;
                        SelectedItems.Add(item);
                        _selectedIndex = _listViewItems != null ? _listViewItems.IndexOf(item) : -1;
                        if (_selectedIndex != -1) SelectedIndices.Add(_selectedIndex);

                        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                        return;
                    }

                    y += RowHeight;
                    if (y > Height) break;
                }
            }

            if (y > Height) break;
        }

        // 4) Ungrouped items selection
        if (_listViewItems != null)
            foreach (var item in _listViewItems)
                if (item._group == null)
                {
                    var itemRect = new SKRect(0, y, Width, y + RowHeight);
                    if (itemRect.Contains(e.X, e.Y))
                    {
                        foreach (var r in _listViewItems) r.StateSelected = false;

                        SelectedItems.Clear();
                        SelectedIndices.Clear();

                        item.StateSelected = true;
                        SelectedItems.Add(item);

                        _selectedIndex = _listViewItems.IndexOf(item);
                        if (_selectedIndex != -1) SelectedIndices.Add(_selectedIndex);

                        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                        return;
                    }

                    y += RowHeight;
                    if (y > Height) break;
                }
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_isResizingColumn)
        {
            _isResizingColumn = false;
            _resizingColumnIndex = -1;
            Cursor = Cursors.Default;
            if (ParentWindow is UIWindow pw) pw.UpdateCursor(this);
        }

        if (_isDraggingScrollbar)
        {
            _isDraggingScrollbar = false;
            _isVerticalScrollbar = false;
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_isResizingColumn)
        {
            _hoverSeparatorIndex = -1;
            Cursor = Cursors.Default;
            if (ParentWindow is UIWindow pw) pw.UpdateCursor(this);
        }
    }

    private void EnsureGroupAnimations()
    {
        foreach (ListViewGroup group in Groups)
        {
            if (_groupAnimations.ContainsKey(group))
                continue;
            var anim = new AnimationManager()
            {
                Increment = 0.15,
                AnimationType = AnimationType.EaseInOut,
                InterruptAnimation = true
            };
            double lastProgress = -1;
            anim.OnAnimationProgress += sender =>
            {
                var prog = anim.GetProgress();
                if (Math.Abs(prog - lastProgress) > 0.01)
                {
                    lastProgress = prog;
                    Invalidate();
                }
            };
            anim.OnAnimationFinished += _ =>
            {
                if (_pendingCollapse.Contains(group))
                {
                    group.CollapsedState = ListViewGroupCollapsedState.Collapsed;
                    _pendingCollapse.Remove(group);
                    Invalidate();
                }
            };
            anim.SetProgress(group.CollapsedState == ListViewGroupCollapsedState.Expanded ? 1.0 : 0.0);
            _groupAnimations[group] = anim;
        }
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        // Draw background once; rows only draw overlays when needed.
        var bg = GetFillPaint();
        // Treat Transparent as "no explicit background" for this control so it
        // doesn't visually fall back to the host window's (often white) background.
        var bgColor = BackColor.IsEmpty || BackColor.A == 0 ? ColorScheme.Surface : BackColor;
        bg.Color = ToSKColor(bgColor);
        canvas.DrawRect(0, 0, Width, Height, bg);

        EnsureGroupAnimations();
        DrawGroups(canvas);
        DrawStickyGroupHeaders(canvas); // header'lar içerik üstünde
        DrawColumns(canvas); // kolon header en üstte
        DrawScrollBars(canvas); // scrollbarlar en üstte kalsın
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var needInvalidate = false;

        // Helper: update hover over column separators even while dragging scrollbar
        void UpdateColumnResizeHover()
        {
            if (_isResizingColumn) return; // keep current cursor during resize
            if (e.Y > 30) // header height
            {
                if (_hoverSeparatorIndex != -1)
                {
                    _hoverSeparatorIndex = -1;
                    if (Cursor != Cursors.Default)
                    {
                        Cursor = Cursors.Default;
                        if (ParentWindow is UIWindow pwDef) pwDef.UpdateCursor(this);
                    }
                }

                return;
            }

            var newHover = HitTestHeaderSeparatorEnhanced(e.X);
            if (newHover != _hoverSeparatorIndex)
            {
                _hoverSeparatorIndex = newHover;
                var desired = newHover >= 0 ? Cursors.SizeWE : Cursors.Default;
                if (Cursor != desired)
                {
                    Cursor = desired;
                    if (ParentWindow is UIWindow pw) pw.UpdateCursor(this);
                }
            }
        }

        if (_isDraggingScrollbar)
        {
            var totalColumnsWidth = GetTotalColumnsWidth();
            if (_isVerticalScrollbar)
            {
                var contentHeight = GetContentHeight();
                var viewportHeight = Math.Max(0, Height - 30);
                if (contentHeight > viewportHeight && viewportHeight > 0)
                {
                    var scrollbarHeight = viewportHeight * (viewportHeight / (float)contentHeight);
                    var trackLength = Math.Max(1f, viewportHeight - scrollbarHeight);
                    var deltaY = e.Y - _scrollbarDragStart.Y;
                    float contentRange = Math.Max(0, contentHeight - viewportHeight);
                    var scrollDelta = deltaY / trackLength * contentRange;
                    var newOffset = Math.Max(0, Math.Min(_dragStartVOffset + scrollDelta, contentRange));
                    if (Math.Abs(newOffset - _verticalScrollOffset) > 0.5f)
                    {
                        _verticalScrollOffset = newOffset;
                        needInvalidate = true;
                    }
                }
            }
            else if (totalColumnsWidth > Width)
            {
                var scrollbarWidth = Width * (Width / (float)totalColumnsWidth);
                var trackLength = Math.Max(1f, Width - scrollbarWidth);
                var deltaX = e.X - _scrollbarDragStart.X;
                float contentRange = Math.Max(0, totalColumnsWidth - Width);
                var scrollDelta = deltaX / trackLength * contentRange;
                var newOffset = Math.Max(0, Math.Min(_dragStartHOffset + scrollDelta, contentRange));
                if (Math.Abs(newOffset - _horizontalScrollOffset) > 0.5f)
                {
                    _horizontalScrollOffset = newOffset;
                    needInvalidate = true;
                }
            }

            UpdateColumnResizeHover();
        }
        else if (_isResizingColumn && _resizingColumnIndex >= 0)
        {
            var delta = e.X - _dragStartX;
            var newWidth = Math.Max(30, _dragInitialWidth + delta);
            if (newWidth != Columns[_resizingColumnIndex].Width)
            {
                Columns[_resizingColumnIndex].Width = newWidth;
                needInvalidate = true;
            }

            if (Cursor != Cursors.SizeWE)
            {
                Cursor = Cursors.SizeWE;
                if (ParentWindow is UIWindow pw) pw.UpdateCursor(this);
            }
        }
        else
        {
            UpdateColumnResizeHover();
        }

        if (needInvalidate) Invalidate();
    }

    // Enhanced hit test with smaller padding and respect to horizontal scroll offset
    private int HitTestHeaderSeparatorEnhanced(int mouseX)
    {
        const int padding = 4; // smaller sensitive region
        var xAccum = -_horizontalScrollOffset;
        for (var i = 0; i < Columns.Count; i++)
        {
            var edge = xAccum + Columns[i].Width;
            // Ignore if edge outside viewport entirely
            if (edge >= -padding && edge <= Width + padding)
                if (mouseX >= edge - padding && mouseX <= edge + padding)
                    return i;
            xAccum = edge;
        }

        return -1;
    }

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            base.Dispose(disposing);
            return;
        }

        if (disposing)
        {
            if (_elasticTimer != null)
            {
                _elasticTimer.Stop();
                _elasticTimer.Elapsed -= ElasticTimer_Tick;
                _elasticTimer.Dispose();
            }

            foreach (var anim in _groupAnimations.Values)
                anim?.Dispose();
            _groupAnimations.Clear();
            _pendingCollapse.Clear();

            _defaultSkFont?.Dispose();
            _defaultSkFont = null;
            _defaultSkFontSource = null;

            _paintFill?.Dispose();
            _paintFill = null;
            _paintGrid?.Dispose();
            _paintGrid = null;
            _paintText?.Dispose();
            _paintText = null;
            _paintHeaderBack?.Dispose();
            _paintHeaderBack = null;
            _paintScrollBar?.Dispose();
            _paintScrollBar = null;
            _paintChevron?.Dispose();
            _paintChevron = null;
            _paintTextCompat?.Dispose();
            _paintTextCompat = null;
            _chevronPath?.Dispose();
            _chevronPath = null;
        }

        base.Dispose(disposing);
    }
}