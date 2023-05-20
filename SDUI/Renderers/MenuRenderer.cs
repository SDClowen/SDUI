using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Renderers;

public class MenuRenderer : ToolStripRenderer
{
    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var g = e.Graphics;

        g.FillRectangle(new SolidBrush(ColorScheme.BorderColor), e.Item.Bounds);
        g.DrawLine(
            new Pen(ColorScheme.BorderColor),
            new Point(e.Item.Bounds.Left, e.Item.Bounds.Height / 2),
            new Point(e.Item.Bounds.Right, e.Item.Bounds.Height / 2));
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        Graphics g = e.Graphics;
        Rectangle dropDownRect = e.ArrowRectangle;
        using Brush brush = new SolidBrush(ColorScheme.ForeColor);
        Point middle = new Point(dropDownRect.Left + dropDownRect.Width / 2, dropDownRect.Top + dropDownRect.Height / 2);

        Point[] arrow;

        int hor = 3;
        int ver = 3;

        switch (e.Direction)
        {
            case ArrowDirection.Up:

                arrow = new Point[] {
                                 new Point(middle.X - hor, middle.Y + 1),
                                 new Point(middle.X + hor + 1, middle.Y + 1),
                                 new Point(middle.X, middle.Y - ver)};

                break;

            case ArrowDirection.Left:
                arrow = new Point[] {
                                 new Point(middle.X + hor, middle.Y - 2 * ver),
                                 new Point(middle.X + hor, middle.Y + 2 * ver),
                                 new Point(middle.X - hor, middle.Y)};

                break;

            case ArrowDirection.Right:
                arrow = new Point[] {
                                 new Point(middle.X - hor, middle.Y - 2 * ver),
                                 new Point(middle.X - hor, middle.Y + 2 * ver),
                                 new Point(middle.X + hor, middle.Y)};

                break;

            case ArrowDirection.Down:
            default:
                arrow = new Point[] {
                             new Point(middle.X - hor, middle.Y - 1),
                             new Point(middle.X + hor + 1, middle.Y - 1),
                             new Point(middle.X, middle.Y + ver) };
                break;
        }
        g.FillPolygon(brush, arrow);
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        Rectangle imageRect = e.ImageRectangle;
        Image image = e.Image;

        if (imageRect != Rectangle.Empty && image != null)
        {
            bool disposeImage = false;

            if (!e.Item.Enabled)
            {
                image = CreateDisabledImage(image);
                disposeImage = true;
            }

            // Draw the checkmark background (providing no image)
            base.OnRenderItemCheck(new ToolStripItemImageRenderEventArgs(e.Graphics, e.Item, null, e.ImageRectangle));

            // Draw the checkmark image scaled to the image rectangle
            e.Graphics.DrawImage(image, imageRect, new Rectangle(Point.Empty, image.Size), GraphicsUnit.Pixel);

            if (disposeImage)
            {
                image.Dispose();
            }
        }
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        base.OnRenderItemText(e);

        if (e.Item.ForeColor != ColorScheme.ForeColor && e.Item.Tag?.ToString() != "private")
            e.Item.ForeColor = ColorScheme.ForeColor;
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        //base.OnRenderToolStripBorder(e);
        var rectangle = e.ToolStrip.ClientRectangle;
        if (e.ToolStrip is ContextMenuStrip ||
            e.ToolStrip is ToolStripDropDownMenu ||
            e.ToolStrip is StatusStrip)
        {
            e.Graphics.DrawPath(new Pen(ColorScheme.BorderColor, 1), rectangle.Radius(16));
        }
        else
        {
            base.OnRenderToolStripBorder(e);
        }
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        //base.OnRenderMenuItemBackground(e);
        var rectangle = new Rectangle(Point.Empty, e.Item.Size);
        if (e.ToolStrip is ToolStripDropDown)
            rectangle.Inflate(-4, 0);

        if (e.Item.Tag?.ToString() == "private")
        {
            using var pbrush = new SolidBrush(e.Item.BackColor);

            e.Graphics.FillPath(pbrush, rectangle.Radius(6));
        }

        if (!e.Item.Selected)
            return;

        var backColor = ColorScheme.BackColor.Brightness(.1f);

        if (!backColor.IsDark())
            backColor = ColorScheme.BackColor.Brightness(-.1f);


        using var brush = new SolidBrush(backColor);

        e.Graphics.FillPath(brush, rectangle.Radius(6));
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        base.OnRenderToolStripBackground(e);
        var rectangle = e.ToolStrip.ClientRectangle;

        e.Graphics.FillRectangle(new SolidBrush(e.ToolStrip is ToolStripDropDown ? ColorScheme.BackColor : ColorScheme.BackColor2), rectangle);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        base.OnRenderImageMargin(e);
        var bounds = e.AffectedBounds;
        e.Graphics.FillRectangle(new SolidBrush(ColorScheme.BackColor), bounds);
    }
}