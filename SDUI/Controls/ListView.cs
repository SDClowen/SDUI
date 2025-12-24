using SDUI.Animation;
using SDUI.Collections;
using SDUI.Extensions;
using SDUI.Helpers;
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

    public System.Windows.Forms.ImageList SmallImageList { get; set; }
    public System.Windows.Forms.ImageList LargeImageList { get; set; }
    public System.Windows.Forms.ImageList? GroupImageList { get; internal set; }
    public bool VirtualMode { get; internal set; }

    public event EventHandler SelectedIndexChanged;
    public event System.Windows.Forms.ItemCheckedEventHandler ItemChecked;

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
                if (index != -1)
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

    // Group expand/collapse animation için
    private readonly Dictionary<ListViewGroup, Animation.AnimationManager> _groupAnimations = new();
    private readonly HashSet<ListViewGroup> _pendingCollapse = new(); // OUT animasyonu bitince gerçekten collapse edilecekler

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
        return SDUI.Helpers.FontManager.GetSKTypeface(font);
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
            Style = SKPaintStyle.Fill
        };
        TextRenderingHelper.DrawText(canvas, text, x, y, SKTextAlign.Left, font, p);
    }

    // Adjust header drawing to always fill full width
    private void DrawColumns(SKCanvas canvas)
    {
        // Always fill full header width
        using var headerBack = new SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.SurfaceContainerHigh.ToSKColor(),
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(new SKRect(0, 0, Width, 30), headerBack);

        using var gridPaint = new SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.OutlineVariant.ToSKColor(),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = .5f
        };
        using var headerFont = CreateFont(Font);
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.OnSurface.ToSKColor(),
            Style = SKPaintStyle.Fill
        };

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
            var fm = headerFont.Metrics;
            float textY = 15f - (fm.Ascent + fm.Descent) / 2f;
            DrawTextCompat(canvas, column.Text ?? string.Empty, colX + 5, textY, headerFont, textPaint);
            colX += column.Width;
            canvas.DrawLine(colX, 0, colX, Height, gridPaint);
        }

        canvas.DrawLine(0, 30, Width, 30, gridPaint);
    }

    private void DrawGroups(SKCanvas canvas)
    {
        const float HEADER_HEIGHT = 30f;
        float y = HEADER_HEIGHT - _verticalScrollOffset;
        foreach (ListViewGroup group in Groups)
        {
            y += RowHeight;
            bool isExpanded = group.CollapsedState == ListViewGroupCollapsedState.Expanded;
            bool isAnimatingCollapse = _pendingCollapse.Contains(group);
            double progress = _groupAnimations.TryGetValue(group, out var anim) ? anim.GetProgress() : (isExpanded ? 1.0 : 0.0);
            bool drawItems = isExpanded || isAnimatingCollapse;
            if (drawItems && group.Items.Count > 0)
            {
                int itemCount = group.Items.Count;
                int visible = (int)Math.Ceiling(itemCount * progress);
                for (int i = 0; i < visible && i < itemCount; i++)
                {
                    var item = group.Items[i];
                    float slide = (float)(1.0 - progress) * (RowHeight * 0.3f);
                    float itemY = y - slide;
                    if (itemY + RowHeight >= HEADER_HEIGHT && itemY <= Height)
                    {
                        // Grup içindeki item'larda ek arka plan overlay'i kullanma,
                        // sadece satır çizimi ve animasyon (slide) kalsın.
                        DrawRow(canvas, item, itemY, isGroupItem: true);
                    }
                    y += RowHeight;
                    if (y > Height) break;
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
                    if (y + RowHeight >= HEADER_HEIGHT) DrawRow(canvas, item, y);
                    y += RowHeight;
                }
            }
        }
    }

    private void DrawStickyGroupHeaders(SKCanvas canvas)
    {
        const float HEADER_HEIGHT = 30f;
        using var font = CreateFont(Font);
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.OnSurface.ToSKColor(),
            Style = SKPaintStyle.Fill
        };

        // Header arka planını her zaman tam genişlikte çiz, scrollbar sağda üstüne binsin.
        int contentHeightAll = GetContentHeight();
        int viewportHeight = Math.Max(0, Height - 30);

        float y = HEADER_HEIGHT - _verticalScrollOffset;
        ListViewGroup sticky = null; float stickyY = HEADER_HEIGHT;
        foreach (ListViewGroup group in Groups)
        {
            double prog = _groupAnimations.TryGetValue(group, out var anim) ? anim.GetProgress() : (group.CollapsedState == ListViewGroupCollapsedState.Expanded ? 1.0 : 0.0);
            bool animCollapse = _pendingCollapse.Contains(group);
            int itemCount = group.Items.Count;
            float contentH = (group.CollapsedState == ListViewGroupCollapsedState.Collapsed && !animCollapse) ? 0f : (float)(itemCount * RowHeight * prog);
            float startY = y; float endY = startY + RowHeight + contentH;
            if (sticky == null && startY <= HEADER_HEIGHT && endY > HEADER_HEIGHT)
            {
                sticky = group; stickyY = HEADER_HEIGHT;
                int nextIdx = Groups.IndexOf(group) + 1;
                if (nextIdx < Groups.Count)
                {
                    var ng = (ListViewGroup)Groups[nextIdx];
                    double nProg = _groupAnimations.TryGetValue(ng, out var na) ? na.GetProgress() : (ng.CollapsedState == ListViewGroupCollapsedState.Expanded ? 1.0 : 0.0);
                    bool nAnimCollapse = _pendingCollapse.Contains(ng);
                    float nContent = (ng.CollapsedState == ListViewGroupCollapsedState.Collapsed && !nAnimCollapse) ? 0f : (float)(ng.Items.Count * RowHeight * nProg);
                    float nStart = endY;
                    if (nStart < HEADER_HEIGHT + RowHeight) stickyY = nStart - RowHeight;
                }
            }
            y = endY; if (y > Height) break;
        }
        y = HEADER_HEIGHT - _verticalScrollOffset;
        foreach (ListViewGroup group in Groups)
        {
            double prog = _groupAnimations.TryGetValue(group, out var anim) ? anim.GetProgress() : (group.CollapsedState == ListViewGroupCollapsedState.Expanded ? 1.0 : 0.0);
            bool animCollapse = _pendingCollapse.Contains(group);
            int itemCount = group.Items.Count;
            float contentH = (group.CollapsedState == ListViewGroupCollapsedState.Collapsed && !animCollapse) ? 0f : (float)(itemCount * RowHeight * prog);
            float headerY = y; if (group == sticky) headerY = stickyY;
            if (headerY >= HEADER_HEIGHT && headerY <= Height)
            {
                using var bg = new SKPaint
                {
                    IsAntialias = true,
                    Color = ColorScheme.SurfaceContainer.ToSKColor(),
                    Style = SKPaintStyle.Fill
                };
                canvas.DrawRect(0, headerY, Width, RowHeight, bg);
                using var border = new SKPaint
                {
                    IsAntialias = true,
                    Color = ColorScheme.OutlineVariant.ToSKColor(),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = group == sticky ? 1.5f : 1f
                };
                canvas.DrawLine(0, headerY + RowHeight, Width, headerY + RowHeight, border);

                // Chevron: collapsed => sağ ok (>), expanded => aşağı V
                // Temel path: aşağı bakan V. Collapsed iken -90° döndürülür (sağa bakar).
                float cX = 12f; float cY = headerY + RowHeight / 2f; float s = 4.5f;
                float rotation = (float)((1.0 - prog) * -90.0f); // prog=1 -> 0° (down V), prog=0 -> -90° (right arrow)
                using var chevPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = ColorScheme.OnSurfaceVariant.ToSKColor(),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.75f,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round
                };
                using var path = new SKPath();
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
                float textHeight = fm.Descent - fm.Ascent;
                float baseline = headerY + (RowHeight - textHeight) / 2f - fm.Ascent;
                DrawTextCompat(canvas, group.Header ?? string.Empty, 25, baseline, font, textPaint);
            }
            y += RowHeight + contentH; if (y > Height) break;
        }
    }

    private void DrawRow(SKCanvas canvas, ListViewItem row, float y, bool isGroupItem = false)
    {
        using var backPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var gridPaint = new SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.OutlineVariant.ToSKColor(),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.5f
        };
        using var rowFont = CreateFont(row.Font ?? Font);
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.OnSurface.ToSKColor(),
            Style = SKPaintStyle.Fill
        };

        // Row background is expensive; only draw when needed (selection/explicit custom backcolor).
        // row.BackColor can represent an inherited value (e.g., ListView.BackColor), so we must
        // check whether the item actually has a custom color set.
        bool hasCustomBack = row.SubItems.Count > 0 && row.SubItems[0].CustomBackColor;
        bool shouldFillBackground = row.StateSelected || hasCustomBack;
        var rect = new SKRect(0, y, Width, y + RowHeight);
        if (shouldFillBackground)
        {
            backPaint.Color = row.StateSelected
                ? ColorScheme.PrimaryContainer.ToSKColor()
                : ToSKColor(row.SubItems[0].BackColor);
            canvas.DrawRect(rect, backPaint);
        }

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
            var defaultFore = row.StateSelected
                ? ColorScheme.OnPrimaryContainer.ToSKColor()
                : ColorScheme.OnSurface.ToSKColor();
            var foreColor = !row.ForeColor.IsEmpty ? ToSKColor(row.ForeColor) : defaultFore;
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
            Color = ColorScheme.OutlineVariant.Alpha(140).ToSKColor(),
            Style = SKPaintStyle.StrokeAndFill
        };

        // Horizontal (içerik üstüne overlay)
        int totalColumnsWidth = Columns.Sum(c => c.Width);
        if (totalColumnsWidth > Width)
        {
            float scrollbarWidth = Width * (Width / (float)totalColumnsWidth);
            float scrollbarX = _horizontalScrollOffset * (Width - scrollbarWidth) / (totalColumnsWidth - Width);
            var rect = new SKRect(scrollbarX + 5, Height - 15, scrollbarX + scrollbarWidth - 5, Height - 5);
            canvas.DrawRoundRect(rect, 4, 4, paint);
        }

        // Vertical (header'dan itibaren içerik üstüne overlay)
        int contentHeight = GetContentHeight();
        int viewportHeight = Math.Max(0, Height - 30);
        if (contentHeight > viewportHeight && viewportHeight > 0)
        {
            float scrollbarHeight = viewportHeight * (viewportHeight / (float)contentHeight);
            float scrollbarY = _verticalScrollOffset * (viewportHeight - scrollbarHeight) / (contentHeight - viewportHeight);
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
            rows += 1; // Group header

            if (group.CollapsedState != ListViewGroupCollapsedState.Collapsed)
            {
                // Animasyon progress'ini dikkate al
                double animProgress = 1.0;
                if (_groupAnimations.TryGetValue(group, out var animation))
                {
                    animProgress = animation.GetProgress();
                }

                // Animasyonlu row count
                rows += (int)Math.Ceiling(group.Items.Count * animProgress);
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
        if (group is null) throw new ArgumentNullException(nameof(group));
        if (Groups.Contains(group)) return;
        group.ListView = this;
        Groups.Insert(index, group);
        var anim = new Animation.AnimationManager(singular: true)
        {
            Increment = 0.15,
            AnimationType = Animation.AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        anim.OnAnimationProgress += _ => Invalidate();
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
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        // 1) Scrollbar hit test
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
        // 2) Column resize only in header area
        if (e.Y <= 30)
        {
            int sep = HitTestHeaderSeparatorEnhanced(e.X);
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
        float y = 30 - _verticalScrollOffset;
        foreach (ListViewGroup group in Groups)
        {
            // Header rect
            var headerRect = new SKRect(0, y, Width, y + RowHeight);
            if (headerRect.Contains(e.X, e.Y))
            {
                bool isExpanded = group.CollapsedState == ListViewGroupCollapsedState.Expanded;
                if (isExpanded)
                {
                    if (_groupAnimations.TryGetValue(group, out var anim))
                    {
                        _pendingCollapse.Add(group);
                        anim.StartNewAnimation(Animation.AnimationDirection.Out);
                    }
                }
                else
                {
                    group.CollapsedState = ListViewGroupCollapsedState.Expanded;
                    if (_groupAnimations.TryGetValue(group, out var anim))
                        anim.StartNewAnimation(Animation.AnimationDirection.In);
                }
                Invalidate();
                return;
            }

            // advance into items area for this group
            y += RowHeight;

            // Items hit-test
            bool expandedOrAnimatingOut = group.CollapsedState == ListViewGroupCollapsedState.Expanded || _pendingCollapse.Contains(group);
            if (expandedOrAnimatingOut && group.Items.Count > 0)
            {
                double prog = _groupAnimations.TryGetValue(group, out var anim) ? anim.GetProgress() : 1.0;
                int visible = (int)Math.Ceiling(group.Items.Count * Math.Clamp(prog, 0.0, 1.0));
                for (int i = 0; i < visible && i < group.Items.Count; i++)
                {
                    var item = group.Items[i];
                    float slide = (float)(1.0 - prog) * (RowHeight * 0.3f);
                    float itemY = y - slide;
                    var itemRect = new SKRect(0, itemY, Width, itemY + RowHeight);
                    if (itemRect.Contains(e.X, e.Y))
                    {
                        // Clear previous selection
                        if (_listViewItems != null)
                        {
                            foreach (var r in _listViewItems) r.StateSelected = false;
                        }
                        item.StateSelected = true;
                        _selectedIndex = _listViewItems != null ? _listViewItems.IndexOf(item) : -1;
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
        {
            foreach (var item in _listViewItems)
            {
                if (item._group == null)
                {
                    var itemRect = new SKRect(0, y, Width, y + RowHeight);
                    if (itemRect.Contains(e.X, e.Y))
                    {
                        foreach (var r in _listViewItems) r.StateSelected = false;
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
            var anim = new Animation.AnimationManager(singular: true)
            {
                Increment = 0.15,
                AnimationType = Animation.AnimationType.EaseInOut,
                InterruptAnimation = true
            };
            anim.OnAnimationProgress += _ => Invalidate();
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

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // Draw background once; rows only draw overlays when needed.
        using (var bg = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill })
        {
            // Treat Transparent as "no explicit background" for this control so it
            // doesn't visually fall back to the host window's (often white) background.
            var bgColor = (BackColor.IsEmpty || BackColor.A == 0) ? ColorScheme.Surface : BackColor;
            bg.Color = ToSKColor(bgColor);
            canvas.DrawRect(0, 0, Width, Height, bg);
        }

        EnsureGroupAnimations();
        DrawGroups(canvas);
        DrawStickyGroupHeaders(canvas); // header'lar içerik üstünde
        DrawColumns(canvas);            // kolon header en üstte
        DrawScrollBars(canvas);         // scrollbarlar en üstte kalsın
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        bool needInvalidate = false;

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
            int newHover = HitTestHeaderSeparatorEnhanced(e.X);
            if (newHover != _hoverSeparatorIndex)
            {
                _hoverSeparatorIndex = newHover;
                var desired = (newHover >= 0) ? Cursors.SizeWE : Cursors.Default;
                if (Cursor != desired)
                {
                    Cursor = desired;
                    if (ParentWindow is UIWindow pw) pw.UpdateCursor(this);
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
                if (contentHeight > viewportHeight && viewportHeight > 0)
                {
                    float scrollbarHeight = viewportHeight * (viewportHeight / (float)contentHeight);
                    float trackLength = Math.Max(1f, viewportHeight - scrollbarHeight);
                    int deltaY = e.Y - _scrollbarDragStart.Y;
                    float contentRange = Math.Max(0, contentHeight - viewportHeight);
                    float scrollDelta = (deltaY / trackLength) * contentRange;
                    float newOffset = Math.Max(0, Math.Min(_dragStartVOffset + scrollDelta, contentRange));
                    if (Math.Abs(newOffset - _verticalScrollOffset) > 0.5f)
                    {
                        _verticalScrollOffset = newOffset;
                        needInvalidate = true;
                    }
                }
            }
            else if (totalColumnsWidth > Width)
            {
                float scrollbarWidth = Width * (Width / (float)totalColumnsWidth);
                float trackLength = Math.Max(1f, Width - scrollbarWidth);
                int deltaX = e.X - _scrollbarDragStart.X;
                float contentRange = Math.Max(0, totalColumnsWidth - Width);
                float scrollDelta = (deltaX / trackLength) * contentRange;
                float newOffset = Math.Max(0, Math.Min(_dragStartHOffset + scrollDelta, contentRange));
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
            int delta = e.X - _dragStartX;
            int newWidth = Math.Max(30, _dragInitialWidth + delta);
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
        float xAccum = -_horizontalScrollOffset;
        for (int i = 0; i < Columns.Count; i++)
        {
            float edge = xAccum + Columns[i].Width;
            // Ignore if edge outside viewport entirely
            if (edge >= -padding && edge <= Width + padding)
            {
                if (mouseX >= edge - padding && mouseX <= edge + padding)
                    return i;
            }
            xAccum = edge;
        }
        return -1;
    }
}