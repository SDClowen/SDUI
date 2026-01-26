using SkiaSharp;

namespace SDUI.Controls;

// Partial class extension to update DrawRow with icon support
public partial class ListView
{
    /// <summary>
    ///     Enhanced DrawRow method with icon drawing support
    /// </summary>
    private void DrawRowWithIcon(SKCanvas canvas, ListViewItem row, float y, bool isGroupItem = false)
    {
        var backPaint = GetFillPaint();
        var gridPaint = GetGridPaint();
        gridPaint.Color = ColorScheme.OutlineVariant.ToSKColor();
        gridPaint.StrokeWidth = 0.5f;
        var textPaint = GetTextPaint();
        textPaint.Color = ColorScheme.OnSurface.ToSKColor();

        SKFont rowFont = null;
        var disposeRowFont = false;
        if (row.Font == null || ReferenceEquals(row.Font, Font))
        {
            rowFont = GetDefaultSkFont();
        }
        else
        {
            rowFont = CreateFont(row.Font);
            disposeRowFont = true;
        }

        // Row background is expensive; only draw when needed (selection/explicit custom backcolor).
        var hasCustomBack = row.SubItems.Count > 0 && row.SubItems[0].CustomBackColor;
        var shouldFillBackground = row.StateSelected || hasCustomBack;
        var rect = new SKRect(0, y, Width, y + RowHeight);
        if (shouldFillBackground)
        {
            backPaint.Color = row.StateSelected
                ? ColorScheme.PrimaryContainer.ToSKColor()
                : row.SubItems[0].BackColor.ToSKColor();
            canvas.DrawRect(rect, backPaint);
        }

        var iconSize = GetIconSize();
        var textOffset = -_horizontalScrollOffset;

        // Draw icon if available (first column only)
        var iconDrawn = false;
        if (Columns.Count > 0)
            try
            {
                DrawItemIcon(canvas, row, textOffset + 5, y, iconSize);
                iconDrawn = (SmallImageList != null || LargeImageList != null) &&
                            ((!string.IsNullOrEmpty(row.ImageKey) &&
                              ((SmallImageList?.Images.ContainsKey(row.ImageKey) ?? false) ||
                               (LargeImageList?.Images.ContainsKey(row.ImageKey) ?? false))) ||
                             (row.ImageIndex >= 0 && row.ImageIndex <
                                 (SmallImageList?.Images.Count ?? LargeImageList?.Images.Count ?? 0)));
            }
            catch
            {
                // Icon drawing failed, continue without icon
            }

        var x = textOffset;
        var i = 0;
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
            var foreColor = !row.ForeColor.IsEmpty ? row.ForeColor.ToSKColor() : defaultFore;
            textPaint.Color = foreColor;

            // Vertical centering using font metrics from SKFont
            var fm = rowFont.Metrics;
            var textY = y + (RowHeight - (fm.Descent - fm.Ascent)) / 2f - fm.Ascent;

            // For the first column, add icon offset if icon was drawn
            var textX = x + 5;
            if (i == 0 && iconDrawn) textX += iconSize + 5; // Icon size + padding

            DrawTextCompat(canvas, row.SubItems[i].Text ?? string.Empty, textX, textY, rowFont, textPaint.Color);
            x += Columns[i].Width;
        }

        canvas.DrawLine(0, y, Width, y, gridPaint);

        if (disposeRowFont)
            rowFont.Dispose();
    }
}