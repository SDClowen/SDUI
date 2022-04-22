using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Renderers
{
    public class MenuRenderer : ToolStripProfessionalRenderer
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
            using (Brush brush = new SolidBrush(ColorScheme.BorderColor))
            {
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

            if(e.Item.ForeColor != ColorScheme.ForeColor)
                e.Item.ForeColor = ColorScheme.ForeColor;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            //base.OnRenderMenuItemBackground(e);
            if (!e.Item.Selected)
                return;

            var rectangle = new Rectangle(Point.Empty, e.Item.Size);

            e.Graphics.FillRectangle(new SolidBrush(ColorScheme.BackColor.Brightness(.2f)), rectangle);
            e.Graphics.DrawRectangle(new Pen(ColorScheme.BorderColor), 0, 0, rectangle.Width - 1, rectangle.Height - 1);

        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            var rectangle = e.ToolStrip.ClientRectangle;
            e.Graphics.FillRectangle(new SolidBrush(ColorScheme.BackColor), rectangle);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            base.OnRenderImageMargin(e);
            var bounds = e.AffectedBounds;
            e.Graphics.FillRectangle(new SolidBrush(ColorScheme.BackColor), bounds);
        }
    }
}
