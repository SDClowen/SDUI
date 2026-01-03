using SkiaSharp;
using System;
using System.Drawing;

namespace SDUI.Controls;

// Partial class extension for icon drawing support
public partial class ListView
{
    /// <summary>
    /// Draws an icon for a list view item if available
    /// </summary>
    private void DrawItemIcon(SKCanvas canvas, ListViewItem item, float x, float y, float iconSize)
    {
        if (SmallImageList == null && LargeImageList == null)
            return;

        var imageList = View == System.Windows.Forms.View.LargeIcon || View == System.Windows.Forms.View.Tile
            ? LargeImageList
            : SmallImageList;

        if (imageList == null)
            return;

        Image icon = null;

        // Try to get icon by key first
        if (!string.IsNullOrEmpty(item.ImageKey) && imageList.Images.ContainsKey(item.ImageKey))
        {
            icon = imageList.Images[item.ImageKey];
        }
        // Then try by index
        else if (item.ImageIndex >= 0 && item.ImageIndex < imageList.Images.Count)
        {
            icon = imageList.Images[item.ImageIndex];
        }

        if (icon == null)
            return;

        // Convert System.Drawing.Image to SKBitmap
        using var ms = new System.IO.MemoryStream();
        icon.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        ms.Position = 0;

        using var skBitmap = SKBitmap.Decode(ms);
        if (skBitmap == null)
            return;

        // Calculate icon position centered vertically
        float iconY = y + (RowHeight - iconSize) / 2f;

        // Draw the icon
        var destRect = new SKRect(x, iconY, x + iconSize, iconY + iconSize);
        canvas.DrawBitmap(skBitmap, destRect);
    }

    /// <summary>
    /// Gets the appropriate icon size based on the view mode
    /// </summary>
    private float GetIconSize()
    {
        if (SmallImageList != null && (View == System.Windows.Forms.View.Details || View == System.Windows.Forms.View.List || View == System.Windows.Forms.View.SmallIcon))
        {
            return Math.Min(SmallImageList.ImageSize.Width, RowHeight - 4);
        }

        if (LargeImageList != null && (View == System.Windows.Forms.View.LargeIcon || View == System.Windows.Forms.View.Tile))
        {
            return Math.Min(LargeImageList.ImageSize.Width, RowHeight - 4);
        }

        return 16; // Default icon size
    }

    /// <summary>
    /// Calculates the text offset based on whether an icon is displayed
    /// </summary>
    private float GetTextXOffset(ListViewItem item)
    {
        float baseOffset = 5f;

        if ((SmallImageList == null && LargeImageList == null))
            return baseOffset;

        // Check if item has an icon
        bool hasIcon = false;
        var imageList = View == System.Windows.Forms.View.LargeIcon || View == System.Windows.Forms.View.Tile
            ? LargeImageList
            : SmallImageList;

        if (imageList != null)
        {
            hasIcon = (!string.IsNullOrEmpty(item.ImageKey) && imageList.Images.ContainsKey(item.ImageKey))
                   || (item.ImageIndex >= 0 && item.ImageIndex < imageList.Images.Count);
        }

        if (hasIcon)
        {
            float iconSize = GetIconSize();
            return baseOffset + iconSize + 5f; // Add icon size + padding
        }

        return baseOffset;
    }
}
