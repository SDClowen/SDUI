using System.Drawing;
using System.Drawing.Drawing2D;

public static class RectangleExtensions
{
    public static GraphicsPath Radius(this Rectangle rectangle, int radius)
    {
        return rectangle.ToRectangleF().Radius(radius);
    }

    public static GraphicsPath Radius(this RectangleF r, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, radius, radius, 180, 90);
        path.AddArc(r.X + r.Width - radius - 1, r.Y, radius, radius, 270, 90);
        path.AddArc(r.X + r.Width - radius - 1, r.Y + r.Height - radius - 1, radius, radius, 0, 90);
        path.AddArc(r.X, r.Y + r.Height - radius - 1, radius, radius, 90, 90);
        path.CloseAllFigures();

        //var path = new GraphicsPath();
        //path.AddLine(r.X + radius, r.Y, r.X + r.Width - (radius * 2), r.Y);
        //path.AddArc(r.X + r.Width - (radius * 2), r.Y, radius * 2, radius * 2, 270, 90);
        //path.AddLine(r.X + r.Width, r.Y + radius, r.X + r.Width, r.Y + r.Height - (radius * 2));
        //path.AddArc(r.X + r.Width - (radius * 2), r.Y + r.Height - (radius * 2), radius * 2, radius * 2, 0, 90);
        //path.AddLine(r.X + r.Width - (radius * 2), r.Y + r.Height, r.X + radius, r.Y + r.Height);
        //path.AddArc(r.X, r.Y + r.Height - (radius * 2), radius * 2, radius * 2, 90, 90);
        //path.AddLine(r.X, r.Y + r.Height - (radius * 2), r.X, r.Y + radius);
        //path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        //path.CloseFigure();

        return path;
    }

    public static GraphicsPath Radius(this RectangleF r, float radius)
    {
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, radius, radius, 180, 90);
        path.AddArc(r.X + r.Width - radius - 1, r.Y, radius, radius, 270, 90);
        path.AddArc(r.X + r.Width - radius - 1, r.Y + r.Height - radius - 1, radius, radius, 0, 90);
        path.AddArc(r.X, r.Y + r.Height - radius - 1, radius, radius, 90, 90);
        path.CloseAllFigures();

        //var path = new GraphicsPath();
        //path.AddLine(r.X + radius, r.Y, r.X + r.Width - (radius * 2), r.Y);
        //path.AddArc(r.X + r.Width - (radius * 2), r.Y, radius * 2, radius * 2, 270, 90);
        //path.AddLine(r.X + r.Width, r.Y + radius, r.X + r.Width, r.Y + r.Height - (radius * 2));
        //path.AddArc(r.X + r.Width - (radius * 2), r.Y + r.Height - (radius * 2), radius * 2, radius * 2, 0, 90);
        //path.AddLine(r.X + r.Width - (radius * 2), r.Y + r.Height, r.X + radius, r.Y + r.Height);
        //path.AddArc(r.X, r.Y + r.Height - (radius * 2), radius * 2, radius * 2, 90, 90);
        //path.AddLine(r.X, r.Y + r.Height - (radius * 2), r.X, r.Y + radius);
        //path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        //path.CloseFigure();

        return path;
    }

    public static GraphicsPath Radius(this RectangleF bounds, float topLeft = 0, float topRight = 0, float bottomLeft = 0, float bottomRight = 0)
    {
        var diameter1 = topLeft * 2;
        var diameter2 = topRight * 2;
        var diameter3 = bottomLeft * 2;
        var diameter4 = bottomRight * 2;

        var arc1 = new RectangleF(bounds.Location, new SizeF(diameter1, diameter1));
        var arc2 = new RectangleF(bounds.Location, new SizeF(diameter2, diameter2));
        var arc3 = new RectangleF(bounds.Location, new SizeF(diameter3, diameter3));
        var arc4 = new RectangleF(bounds.Location, new SizeF(diameter4, diameter4));
        var path = new GraphicsPath();

        // top left arc  
        if (topLeft == 0)
        {
            path.AddLine(arc1.Location, arc1.Location);
        }
        else
        {
            path.AddArc(arc1, 180, 90);
        }

        // top right arc  
        arc2.X = bounds.Right - diameter2;
        if (topRight == 0)
        {
            path.AddLine(arc2.Location, arc2.Location);
        }
        else
        {
            path.AddArc(arc2, 270, 90);
        }

        // bottom right arc  

        arc3.X = bounds.Right - diameter3;
        arc3.Y = bounds.Bottom - diameter3;
        if (bottomLeft == 0)
        {
            path.AddLine(arc3.Location, arc3.Location);
        }
        else
        {
            path.AddArc(arc3, 0, 90);
        }

        // bottom left arc 
        arc4.X = bounds.Right - diameter4;
        arc4.Y = bounds.Bottom - diameter4;
        arc4.X = bounds.Left;
        if (bottomRight == 0)
        {
            path.AddLine(arc4.Location, arc4.Location);
        }
        else
        {
            path.AddArc(arc4, 90, 90);
        }

        path.CloseFigure();
        return path;
    }
    public static GraphicsPath Radius(this RectangleF bounds, int topLeft = 0, int topRight = 0, int bottomLeft = 0, int bottomRight = 0)
    {
        int diameter1 = topLeft * 2;
        int diameter2 = topRight * 2;
        int diameter3 = bottomLeft * 2;
        int diameter4 = bottomRight * 2;

        var arc1 = new RectangleF(bounds.Location, new SizeF(diameter1, diameter1));
        var arc2 = new RectangleF(bounds.Location, new SizeF(diameter2, diameter2));
        var arc3 = new RectangleF(bounds.Location, new SizeF(diameter3, diameter3));
        var arc4 = new RectangleF(bounds.Location, new SizeF(diameter4, diameter4));
        var path = new GraphicsPath();

        // top left arc  
        if (topLeft == 0)
        {
            path.AddLine(arc1.Location, arc1.Location);
        }
        else
        {
            path.AddArc(arc1, 180, 90);
        }

        // top right arc  
        arc2.X = bounds.Right - diameter2;
        if (topRight == 0)
        {
            path.AddLine(arc2.Location, arc2.Location);
        }
        else
        {
            path.AddArc(arc2, 270, 90);
        }

        // bottom right arc  

        arc3.X = bounds.Right - diameter3;
        arc3.Y = bounds.Bottom - diameter3;
        if (bottomLeft == 0)
        {
            path.AddLine(arc3.Location, arc3.Location);
        }
        else
        {
            path.AddArc(arc3, 0, 90);
        }

        // bottom left arc 
        arc4.X = bounds.Right - diameter4;
        arc4.Y = bounds.Bottom - diameter4;
        arc4.X = bounds.Left;
        if (bottomRight == 0)
        {
            path.AddLine(arc4.Location, arc4.Location);
        }
        else
        {
            path.AddArc(arc4, 90, 90);
        }

        path.CloseFigure();
        return path;
    }
    
    public static Rectangle ToRectangle(this RectangleF rect)
    {
        return Rectangle.Round(rect);
    }

    public static RectangleF ToRectangleF(this Rectangle rect)
    {
        return (RectangleF)rect;
    }

    public static bool InRect(this Point point, Rectangle rect)
    {
        return point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }

    public static bool InRect(this Point point, RectangleF rect)
    {
        return point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }

    public static bool InRect(this PointF point, Rectangle rect)
    {
        return point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }

    public static bool InRect(this PointF point, RectangleF rect)
    {
        return point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }
}