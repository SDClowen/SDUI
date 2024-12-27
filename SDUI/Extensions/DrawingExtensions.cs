using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

public static class DrawingExtensions
{
    private static readonly ContentAlignment anyRight = ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight;
    private static readonly ContentAlignment anyBottom = ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight;
    private static readonly ContentAlignment anyCenter = ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter;
    private static readonly ContentAlignment anyMiddle = ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight;

    public static bool InRegion(this Point point, Region region)
    {
        return region.IsVisible(point);
    }

    public static bool InRegion(this Point point, Point[] points)
    {
        using (var path = points.Path())
        {
            using (var region = path.Region())
            {
                return region.IsVisible(point);
            }
        }
    }

    public static bool InRegion(this PointF point, PointF[] points)
    {
        using (var path = points.Path())
        {
            using (var region = path.Region())
            {
                return region.IsVisible(point);
            }
        }
    }

    public static GraphicsPath ChromePath(this RectangleF bounds, float radius)
    {
        var diameter = radius * 2;
        var x = bounds.Location.X;
        var y = bounds.Location.Y;
        var path = new GraphicsPath();

        if (radius == 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        // Top left arc
        path.AddArc(x, y, diameter, diameter, 180, 90);

        // Top right arc
        x = bounds.Right - diameter;
        path.AddArc(x, y, diameter, diameter, 270, 90);

        y = bounds.Bottom - diameter;

        // bottom right
        path.AddArc(x + diameter, y, diameter, diameter, 180, -90);  // Sağ alt dışa doğru eğri

        // Bottom left arc
        x = bounds.Left - diameter;
        path.AddArc(x, y, diameter, diameter, 90, -90);

        path.CloseFigure();
        return path;
    }

    public static GraphicsPath Path(this Point[] points)
    {
        var path = new GraphicsPath();
        path.Reset();
        path.AddPolygon(points);
        return path;
    }

    public static GraphicsPath Path(this PointF[] points)
    {
        GraphicsPath path = new GraphicsPath();
        path.Reset();
        path.AddPolygon(points);
        return path;
    }

    public static Region Region(this GraphicsPath path)
    {
        Region region = new Region();
        region.MakeEmpty();
        region.Union(path);
        return region;
    }

    public static GraphicsPath GraphicsPath(this RectangleF rect)
    {
        var points = new PointF[] {
                new PointF(rect.Left, rect.Top),
                new PointF(rect.Right, rect.Top),
                new PointF(rect.Right, rect.Bottom),
                new PointF(rect.Left, rect.Bottom),

                new PointF(rect.Left, rect.Top) };
        return points.Path();
    }

    public static GraphicsPath CreateFanPath(this Graphics g, Point center, float d1, float d2, float startAngle, float sweepAngle)
    {
        return center.CreateFanPath(d1, d2, startAngle, sweepAngle);
    }

    public static GraphicsPath CreateFanPath(this Graphics g, PointF center, float d1, float d2, float startAngle, float sweepAngle)
    {
        return center.CreateFanPath(d1, d2, startAngle, sweepAngle);
    }

    public static GraphicsPath CreateFanPath(this Point center, float d1, float d2, float startAngle, float sweepAngle)
    {
        return new PointF(center.X, center.Y).CreateFanPath(d1, d2, startAngle, sweepAngle);
    }

    public static GraphicsPath CreateFanPath(this PointF center, float d1, float d2, float startAngle, float sweepAngle)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(center.X - d1, center.Y - d1, d1 * 2, d1 * 2, startAngle, sweepAngle);
        path.AddArc(center.X - d2, center.Y - d2, d2 * 2, d2 * 2, startAngle + sweepAngle, -sweepAngle);
        path.AddArc(center.X - d1, center.Y - d1, d1 * 2, d1 * 2, startAngle, 0.1f);
        return path;
    }

    public static void SetHighQuality(this Graphics g)
    {
        //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.CompositingQuality = CompositingQuality.HighQuality;
    }

    public static void SetDefaultQuality(this Graphics g)
    {
        g.SmoothingMode = SmoothingMode.Default;
        g.InterpolationMode = InterpolationMode.Default;
        g.CompositingQuality = CompositingQuality.Default;
    }

    public static void FillRectangle(this Graphics gfx, Color color, Rectangle rect)
    {
        using var brush = color.Brush();
        gfx.FillRectangle(brush, rect);
    }

    public static void DrawPath(this Graphics gfx, Color color, GraphicsPath path)
    {
        using var pen = color.Pen();
        gfx.DrawPath(pen, path);
    }

    public static void FillRectangle(this Graphics gfx, Color color, int x, int y, int width, int height)
    {
        using var brush = color.Brush();
        gfx.FillRectangle(brush, new Rectangle(x, y, width, height));
    }
    public static void FillRectangle(this Graphics gfx, Color color, RectangleF rect)
    {
        using var brush = color.Brush();
        gfx.FillRectangle(brush, rect);
    }

    public static void FillRectangle(this Graphics gfx, Color color, float x, float y, float width, float height)
    {
        using var brush = color.Brush();
        gfx.FillRectangle(brush, new RectangleF(x, y, width, height));
    }

    public static void DrawRectangle(this Graphics gfx, Color color, Rectangle rect)
    {
        using var pen = color.Pen();
        gfx.DrawRectangle(pen, rect);
    }

    public static void DrawRectangle(this Graphics gfx, Color color, int x, int y, int width, int height)
    {
        using var pen = color.Pen();
        gfx.DrawRectangle(pen, x, y, width, height);
    }

    public static void DrawRectangle(this Graphics gfx, Color color, float x, float y, float width, float height)
    {
        using var pen = color.Pen();
        gfx.DrawRectangle(pen, x, y, width, height);
    }

    public static void DrawLine(this Graphics gfx, Color color, int x1, int y1, int x2, int y2)
    {
        using var pen = color.Pen();
        gfx.DrawLine(pen, x1, y1, x2, y2);
    }

    public static void DrawLine(this Graphics gfx, Color color, float x1, float y1, float x2, float y2)
    {
        using var pen = color.Pen();
        gfx.DrawLine(pen, x1, y1, x2, y2);
    }

    public static void DrawLine(this Graphics gfx, Color color, Point p1, Point p2)
    {
        using var pen = color.Pen();
        gfx.DrawLine(pen, p1, p2);
    }

    public static GraphicsPath CreateRoundPath(float v1, float v2, float v3, float v4, int v5)
    {
        return new RectangleF(v1, v2, v3, v4).Radius(v5);
    }

    public static void DrawShadow(this Graphics graphics, Rectangle rect, float size, int radius, Color color = default)
        => DrawShadow(graphics, rect.ToRectangleF(), size, radius, color);

    public static void DrawShadow(this Graphics graphics, RectangleF rect, float size, int radius, Color color = default)
    {
        if (size <= 0)
            return;

        if (color == default)
            color = SDUI.ColorScheme.ShadowColor;

        for (float i = 0; i < size; i++)
        {
            var v = i * (size / 2);
            using (var pen = new Pen(color.Alpha(color.A / ((int)i + 1)), 1))
            {
                var rectF = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
                rectF.Inflate(v / size, v / size);
                using (var rectPath = rectF.Radius(radius))
                    graphics.DrawPath(pen, rectPath);
            }
        }
    }

    public static StringAlignment TranslateAlignment(ContentAlignment align)
    {
        StringAlignment result;
        if ((align & anyRight) != 0)
            result = StringAlignment.Far;
        else if ((align & anyCenter) != 0)
            result = StringAlignment.Center;
        else
            result = StringAlignment.Near;
        return result;
    }
    public static StringAlignment TranslateLineAlignment(ContentAlignment align)
    {
        StringAlignment result;
        if ((align & anyBottom) != 0)
        {
            result = StringAlignment.Far;
        }
        else if ((align & anyMiddle) != 0)
        {
            result = StringAlignment.Center;
        }
        else
        {
            result = StringAlignment.Near;
        }
        return result;
    }

    public static StringFormat StringFormatForAlignment(ContentAlignment align)
    {
        return new StringFormat { Alignment = TranslateAlignment(align), LineAlignment = TranslateLineAlignment(align) };
    }

    public static StringFormat CreateStringFormat(this Control ctl, ContentAlignment textAlign, bool showEllipsis, bool useMnemonic)
    {
        StringFormat format = StringFormatForAlignment(textAlign);
        if (ctl.RightToLeft == RightToLeft.Yes)
        {
            format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
        }
        if (showEllipsis)
        {
            format.Trimming = StringTrimming.EllipsisCharacter;
            format.FormatFlags |= StringFormatFlags.LineLimit;
        }
        if (!useMnemonic)
        {
            format.HotkeyPrefix = HotkeyPrefix.None;
        }
        /*else if (ctl.ShowKeyboardCues)
        {
            format.HotkeyPrefix = HotkeyPrefix.Show;
        }*/
        else
        {
            format.HotkeyPrefix = HotkeyPrefix.Hide;
        }
        if (ctl.AutoSize)
        {
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        }
        return format;
    }

    public static void DrawString(this Control control, string text, Graphics graphics, ContentAlignment contentAlignment, bool showEllipsis = false, bool useMnemonic = false)
    {
        graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
        using var textFormat = control.CreateStringFormat(contentAlignment, showEllipsis, useMnemonic);
        using var textBrush = new SolidBrush(control.ForeColor);

        graphics.DrawString(text, control.Font, textBrush, control.ClientRectangle, textFormat);
    }

    public static void DrawString(this Control control, Graphics graphics, ContentAlignment contentAlignment, bool showEllipsis = false, bool useMnemonic = false)
    {
        graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
        using var textFormat = control.CreateStringFormat(contentAlignment, showEllipsis, useMnemonic);
        using var textBrush = new SolidBrush(control.ForeColor);

        graphics.DrawString(control.Text, control.Font, textBrush, control.ClientRectangle, textFormat);
    }

    public static void DrawString(this Control control, Graphics graphics, ContentAlignment contentAlignment, Color color, bool showEllipsis = false, bool useMnemonic = false)
    {
        graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
        using var textFormat = control.CreateStringFormat(contentAlignment, showEllipsis, useMnemonic);
        using var textBrush = new SolidBrush(color);

        graphics.DrawString(control.Text, control.Font, textBrush, control.ClientRectangle, textFormat);
    }

    public static void DrawString(this Control control, Graphics graphics, ContentAlignment contentAlignment, Color color, RectangleF rectangle, bool showEllipsis = false, bool useMnemonic = false)
    {
        graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
        using var textFormat = control.CreateStringFormat(contentAlignment, showEllipsis, useMnemonic);
        using var textBrush = new SolidBrush(color);

        graphics.DrawString(control.Text, control.Font, textBrush, rectangle, textFormat);
    }

    public static void DrawString(this Control control, Graphics graphics, string text, Color color, RectangleF rectangle)
    {
        graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
        using var textBrush = new SolidBrush(color);
        using var textFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };

        graphics.DrawString(text, control.Font, textBrush, rectangle, textFormat);
    }
    public static void DrawString(this Control control, Graphics graphics, Color color, RectangleF rectangle)
    {
        graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
        using var textBrush = new SolidBrush(color);
        using var textFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };

        graphics.DrawString(control.Text, control.Font, textBrush, rectangle, textFormat);
    }

    public static void DrawSvg(this Graphics graphics, string svg, Color color, RectangleF rectangle)
    {
        var svgDocument = Svg.SvgDocument.FromSvg<Svg.SvgDocument>(svg);
        svgDocument.Color = new Svg.SvgColourServer(color);

        using var bitmap = svgDocument.Draw();
        graphics.DrawImage(bitmap, rectangle);

    }
}