using System;
using System.Drawing;
using System.Drawing.Imaging;
using SDUI.Helpers;
using SkiaSharp;

public static class SkiaExtensions
{
    // System.Drawing.Point*

    public static SKPoint ToSKPoint(this PointF point)
    {
        return new SKPoint(point.X, point.Y);
    }

    public static SKPointI ToSKPoint(this Point point)
    {
        return new SKPointI(point.X, point.Y);
    }

    public static PointF ToDrawingPoint(this SKPoint point)
    {
        return new PointF(point.X, point.Y);
    }

    public static Point ToDrawingPoint(this SKPointI point)
    {
        return new Point(point.X, point.Y);
    }

    // System.Drawing.Rectangle*

    public static SKRect ToSKRect(this RectangleF rect)
    {
        return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }

    public static SKRectI ToSKRect(this Rectangle rect)
    {
        return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }

    public static RectangleF ToDrawingRect(this SKRect rect)
    {
        return RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }

    public static Rectangle ToDrawingRect(this SKRectI rect)
    {
        return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }

    // System.Drawing.Size*

    public static SKSize ToSKSize(this SizeF size)
    {
        return new SKSize(size.Width, size.Height);
    }

    public static SKSizeI ToSKSize(this Size size)
    {
        return new SKSizeI(size.Width, size.Height);
    }

    public static SizeF ToDrawingSize(this SKSize size)
    {
        return new SizeF(size.Width, size.Height);
    }

    public static Size ToDrawingSize(this SKSizeI size)
    {
        return new Size(size.Width, size.Height);
    }

    // System.Drawing.Bitmap

    public static Bitmap ToBitmap(this SKPicture picture, SKSizeI dimensions)
    {
        using (var image = SKImage.FromPicture(picture, dimensions))
        {
            return image.ToBitmap();
        }
    }

    public static Bitmap ToBitmap(this SKImage skiaImage)
    {
        // TODO: maybe keep the same color types where we can, instead of just going to the platform default

        var bitmap = new Bitmap(skiaImage.Width, skiaImage.Height, PixelFormat.Format32bppPArgb);
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly,
            bitmap.PixelFormat);

        // copy
        using (var pixmap = new SKPixmap(new SKImageInfo(data.Width, data.Height), data.Scan0, data.Stride))
        {
            skiaImage.ReadPixels(pixmap, 0, 0);
        }

        bitmap.UnlockBits(data);
        return bitmap;
    }

    public static Bitmap ToBitmap(this SKBitmap skiaBitmap)
    {
        using (var pixmap = skiaBitmap.PeekPixels())
        using (var image = SKImage.FromPixels(pixmap))
        {
            var bmp = image.ToBitmap();
            GC.KeepAlive(skiaBitmap);
            return bmp;
        }
    }

    public static Bitmap ToBitmap(this SKPixmap pixmap)
    {
        using (var image = SKImage.FromPixels(pixmap))
        {
            return image.ToBitmap();
        }
    }

    public static SKBitmap ToSKBitmap(this Bitmap bitmap)
    {
        // TODO: maybe keep the same color types where we can, instead of just going to the platform default

        var info = new SKImageInfo(bitmap.Width, bitmap.Height);
        var skiaBitmap = new SKBitmap(info);
        using (var pixmap = skiaBitmap.PeekPixels())
        {
            bitmap.ToSKPixmap(pixmap);
        }

        return skiaBitmap;
    }

    public static SKImage ToSKImage(this Bitmap bitmap)
    {
        // TODO: maybe keep the same color types where we can, instead of just going to the platform default

        var info = new SKImageInfo(bitmap.Width, bitmap.Height);
        var image = SKImage.Create(info);
        using (var pixmap = image.PeekPixels())
        {
            bitmap.ToSKPixmap(pixmap);
        }

        return image;
    }

    public static void ToSKPixmap(this Bitmap bitmap, SKPixmap pixmap)
    {
        // TODO: maybe keep the same color types where we can, instead of just going to the platform default

        if (pixmap.ColorType == SKImageInfo.PlatformColorType)
        {
            var info = pixmap.Info;
            using (var tempBitmap = new Bitmap(info.Width, info.Height, info.RowBytes, PixelFormat.Format32bppPArgb,
                       pixmap.GetPixels()))
            using (var gr = Graphics.FromImage(tempBitmap))
            {
                // Clear graphic to prevent display artifacts with transparent bitmaps					
                gr.Clear(Color.Transparent);

                gr.DrawImageUnscaled(bitmap, 0, 0);
            }
        }
        else
        {
            // we have to copy the pixels into a format that we understand
            // and then into a desired format
            // TODO: we can still do a bit more for other cases where the color types are the same
            using (var tempImage = bitmap.ToSKImage())
            {
                tempImage.ReadPixels(pixmap, 0, 0);
            }
        }
    }

    public static SKBitmap ToSKBitmap(this Image image)
    {
        using (var bitmap = new Bitmap(image))
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppPArgb);
            var skBitmap = new SKBitmap();
            skBitmap.InstallPixels(
                new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul), data.Scan0,
                data.Stride);
            bitmap.UnlockBits(data);
            return skBitmap.Copy();
        }
    }

    public static SKFont ToSKFont(this Font drawingFont)
    {
        if (drawingFont == null)
            throw new ArgumentNullException(nameof(drawingFont));

        // Extract the font family and style
        var fontFamily = drawingFont.FontFamily.Name;
        var weight = SKFontStyleWeight.Normal;
        var width = SKFontStyleWidth.Normal;
        var slant = SKFontStyleSlant.Upright;

        // Map styles
        if (drawingFont.Bold)
            weight = SKFontStyleWeight.Bold;
        if (drawingFont.Italic)
            slant = SKFontStyleSlant.Italic;

        // Create the SKTypeface
        var typeface = FontManager.GetSKTypeface(drawingFont);

        // Create the SKFont with proper Unicode support
        var skFont = new SKFont(typeface, drawingFont.Size)
        {
            Edging = SKFontEdging.SubpixelAntialias,
            Subpixel = true,
            Hinting = SKFontHinting.Full
        };

        return skFont;
    }
}