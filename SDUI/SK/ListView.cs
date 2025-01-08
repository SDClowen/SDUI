using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;

namespace SDUI.SK;

public class ListView : SKControl
{
    public View View { get; set; } = View.Details;
    public BorderStyle BorderStyle { get; set; } = BorderStyle.None;
    public bool FullRowSelect { get; set; } = false;
    public bool CheckBoxes { get; set; } = false;
    public bool ShowItemToolTips { get; set; } = false;
    public bool UseCompatibleStateImageBehavior { get; set; } = false;

    public List<ColumnHeader> Columns { get; } = new List<ColumnHeader>();
    public List<ListViewItem> Items { get; } = new List<ListViewItem>();
    public List<ListViewGroup> Groups { get; } = new List<ListViewGroup>();

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

    public event EventHandler<int> RowClicked;

    public ListView()
    {
        _elasticTimer = new(ElasticInterval);
        _elasticTimer.Elapsed += ElasticTimer_Tick;
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

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

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

        foreach (var group in Groups)
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
        if (Items.Sum(r => rowBoundsHeight) > Height - 30)
        {
            float scrollbarHeight = (Height - 30) * ((Height - 30) / (float)Items.Sum(r => rowBoundsHeight));
            float scrollbarY = _verticalScrollOffset * ((Height - 30) - scrollbarHeight) / (Items.Sum(r => rowBoundsHeight) - (Height - 30));
            var rect = new SKRect(Width - 5, 35 + scrollbarY, Width - 15, 15 + scrollbarY + scrollbarHeight);
            canvas.DrawRoundRect(rect, 8, 8, paint);
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
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

    protected override void OnMouseDown(MouseEventArgs e)
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
        foreach (var group in Groups)
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
                        Items.ForEach(r => r.Selected = false);
                        item.Selected = true;
                        RowClicked?.Invoke(this, Items.IndexOf(item));

                        Invalidate();
                        return;
                    }
                    y += rowBoundsHeight;
                }
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isResizingColumn = false;
        _isDraggingScrollbar = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
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
            this.Cursor = Cursors.SizeWE; // Resize cursor
        }
        else
        {
            this.Cursor = Cursors.Default; // Default cursor
        }

        if (_isDraggingScrollbar)
        {
            int delta = _isVerticalScrollbar ? e.Y - _scrollbarDragStart.Y : e.X - _scrollbarDragStart.X;
            if (_isVerticalScrollbar)
            {
                _verticalScrollOffset = Math.Max(-Height / 4, Math.Min(_verticalScrollOffset + delta * 3, Items.Sum(r => rowBoundsHeight) - Height + 30 + Height / 4));
            }
            else
            {
                _horizontalScrollOffset = Math.Max(-Width / 4, Math.Min(_horizontalScrollOffset + delta * 3, Columns.Sum(c => c.Width) - Width + Width / 4));
            }
        }
        _scrollbarDragStart = e.Location;
        Invalidate();
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
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
                    foreach (var row in Items)
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
}