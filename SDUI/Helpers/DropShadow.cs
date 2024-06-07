using System.Collections.Generic;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SDUI.Helpers;

using static System.Math;
public static class DropShadow
{
    const int CHANNELS = 4;
    const int InflateMultiple = 2;//单边外延radius的倍数

    /// <summary>
    /// 获取阴影边界。供外部定位阴影用
    /// </summary>
    /// <param name="path">形状</param>
    /// <param name="radius">模糊半径</param>
    /// <param name="pathBounds">形状边界</param>
    /// <param name="inflate">单边外延像素</param>
    public static Rectangle GetBounds(GraphicsPath path, int radius, out Rectangle pathBounds, out int inflate)
    {
        var bounds = pathBounds = Rectangle.Ceiling(path.GetBounds());
        inflate = radius * InflateMultiple;
        bounds.Inflate(inflate, inflate);
        return bounds;
    }

    /// <summary>
    /// 获取阴影边界
    /// </summary>
    /// <param name="source">原边界</param>
    /// <param name="radius">模糊半径</param>
    public static Rectangle GetBounds(Rectangle source, int radius)
    {
        var inflate = radius * InflateMultiple;
        source.Inflate(inflate, inflate);
        return source;
    }

    /// <summary>
    /// 创建阴影图片
    /// </summary>
    /// <param name="path">阴影形状</param>
    /// <param name="color">阴影颜色</param>
    /// <param name="radius">模糊半径</param>
    public static Bitmap Create(GraphicsPath path, Color color, int radius = 5)
    {
        var bounds = GetBounds(path, radius, out Rectangle pathBounds, out int inflate);
        var shadow = new Bitmap(bounds.Width, bounds.Height);

        if (color.A == 0)
        {
            return shadow;
        }

        //将形状用color色画在阴影区中心
        Graphics g = null;
        GraphicsPath pathCopy = null;
        Matrix matrix = null;
        SolidBrush brush = null;
        try
        {
            matrix = new Matrix();
            matrix.Translate(-pathBounds.X + inflate, -pathBounds.Y + inflate);//先清除形状原有偏移再向中心偏移
            pathCopy = (GraphicsPath)path.Clone();                             //基于形状副本操作
            pathCopy.Transform(matrix);

            brush = new SolidBrush(color);

            g = Graphics.FromImage(shadow);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.FillPath(brush, pathCopy);
        }
        finally
        {
            g?.Dispose();
            brush?.Dispose();
            pathCopy?.Dispose();
            matrix?.Dispose();
        }

        if (radius <= 0)
        {
            return shadow;
        }

        BitmapData data = null;
        try
        {
            data = shadow.LockBits(new Rectangle(0, 0, shadow.Width, shadow.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            //两次方框模糊就能达到不错的效果
            //var boxes = DetermineBoxes(radius, 3);
            BoxBlur(data, radius, color);
            BoxBlur(data, radius, color);
            //BoxBlur(shadowData, radius);

            return shadow;
        }
        finally
        {
            shadow.UnlockBits(data);
        }
    }

    /// <summary>
    /// 方框模糊
    /// </summary>
    /// <param name="data">图像内存数据</param>
    /// <param name="radius">模糊半径</param>
    /// <param name="color">透明色值</param>
#if UNSAFE
        private static unsafe void BoxBlur(BitmapData data, int radius, Color color)
#else
    private static void BoxBlur(BitmapData data, int radius, Color color)
#endif
    {
#if UNSAFE //unsafe项目下请定义编译条件：UNSAFE
            IntPtr p1 = data1.Scan0;
#else
        byte[] p1 = new byte[data.Stride * data.Height];
        Marshal.Copy(data.Scan0, p1, 0, p1.Length);
#endif
        //色值处理
        //这步的意义在于让图片中的透明像素拥有color的色值（但仍然保持透明）
        //这样在混合时才能合出基于color的颜色（只是透明度不同），
        //否则它是与RGB(0,0,0)合，就会得到乌黑的渣特技
        byte R = color.R, G = color.G, B = color.B;
        for (int i = 3; i < p1.Length; i += 4)
        {
            if (p1[i] == 0)
            {
                p1[i - 1] = R;
                p1[i - 2] = G;
                p1[i - 3] = B;
            }
        }

        byte[] p2 = new byte[p1.Length];
        int radius2 = 2 * radius + 1;
        int First, Last, Sum;
        int stride = data.Stride,
            width = data.Width,
            height = data.Height;

        //只处理Alpha通道

        //横向
        for (int r = 0; r < height; r++)
        {
            int start = r * stride;
            int left = start;
            int right = start + radius * CHANNELS;

            First = p1[start + 3];
            Last = p1[start + stride - 1];
            Sum = (radius + 1) * First;

            for (int column = 0; column < radius; column++)
            {
                Sum += p1[start + column * CHANNELS + 3];
            }
            for (var column = 0; column <= radius; column++, right += CHANNELS, start += CHANNELS)
            {
                Sum += p1[right + 3] - First;
                p2[start + 3] = (byte)(Sum / radius2);
            }
            for (var column = radius + 1; column < width - radius; column++, left += CHANNELS, right += CHANNELS, start += CHANNELS)
            {
                Sum += p1[right + 3] - p1[left + 3];
                p2[start + 3] = (byte)(Sum / radius2);
            }
            for (var column = width - radius; column < width; column++, left += CHANNELS, start += CHANNELS)
            {
                Sum += Last - p1[left + 3];
                p2[start + 3] = (byte)(Sum / radius2);
            }
        }

        //纵向
        for (int column = 0; column < width; column++)
        {
            int start = column * CHANNELS;
            int top = start;
            int bottom = start + radius * stride;

            First = p2[start + 3];
            Last = p2[start + (height - 1) * stride + 3];
            Sum = (radius + 1) * First;

            for (int row = 0; row < radius; row++)
            {
                Sum += p2[start + row * stride + 3];
            }
            for (int row = 0; row <= radius; row++, bottom += stride, start += stride)
            {
                Sum += p2[bottom + 3] - First;
                p1[start + 3] = (byte)(Sum / radius2);
            }
            for (int row = radius + 1; row < height - radius; row++, top += stride, bottom += stride, start += stride)
            {
                Sum += p2[bottom + 3] - p2[top + 3];
                p1[start + 3] = (byte)(Sum / radius2);
            }
            for (int row = height - radius; row < height; row++, top += stride, start += stride)
            {
                Sum += Last - p2[top + 3];
                p1[start + 3] = (byte)(Sum / radius2);
            }
        }
#if !UNSAFE
        Marshal.Copy(p1, 0, data.Scan0, p1.Length);
#endif
    }

    // private static int[] DetermineBoxes(double Sigma, int BoxCount)
    // {
    //     double IdealWidth = Math.Sqrt((12 * Sigma * Sigma / BoxCount) + 1);
    //     int Lower = (int)Math.Floor(IdealWidth);
    //     if (Lower % 2 == 0)
    //         Lower--;
    //     int Upper = Lower + 2;
    //
    //     double MedianWidth = (12 * Sigma * Sigma - BoxCount * Lower * Lower - 4 * BoxCount * Lower - 3 * BoxCount) / (-4 * Lower - 4);
    //     int Median = (int)Math.Round(MedianWidth);
    //
    //     int[] BoxSizes = new int[BoxCount];
    //     for (int i = 0; i < BoxCount; i++)
    //         BoxSizes[i] = (i < Median) ? Lower : Upper;
    //     return BoxSizes;
    // }
}

public static class ShadowUtils
{
    public interface IShadowController
    {
        bool ShouldShowShadow();
    }
    enum RenderSide
    {
        Top,
        Bottom,
        Left,
        Right
    }

    static RenderSide[] VisibleTop = { RenderSide.Bottom/*, RenderSide.Top*/ };
    static RenderSide[] VisibleBottom = { RenderSide.Top/*, RenderSide.Bottom*/ };
    static RenderSide[] VisibleLeft = { RenderSide.Right };
    static RenderSide[] VisibleRight = { RenderSide.Left };

    static bool IsVisible(RenderSide side, DockStyle st)
    {
        switch (st)
        {
            case DockStyle.Top:
                return VisibleTop.Contains(side);
            case DockStyle.Bottom:
                return VisibleBottom.Contains(side);
            case DockStyle.Left:
                return VisibleLeft.Contains(side);
            case DockStyle.Right:
                return VisibleRight.Contains(side);
            case DockStyle.Fill:
                return false;
        }
        return true;
    }


    public static void DrawShadow(Graphics G, Color c, Rectangle r, int d, DockStyle st = DockStyle.None)
    {
        Color[] colors = GetColorVector(c, d).ToArray();

        if (IsVisible(RenderSide.Top, st))
            for (int i = 1; i < d; i++)
            {
                //TOP
                using (Pen pen = new Pen(colors[i], 1f))
                    G.DrawLine(pen, new Point(r.Left - Max(i - 1, 0), r.Top - i), new Point(r.Right + Max(i - 1, 0), r.Top - i));
            }

        if (IsVisible(RenderSide.Bottom, st))
            for (int i = 0; i < d; i++)
            {
                //BOTTOM
                using (Pen pen = new Pen(colors[i], 1f))
                    G.DrawLine(pen, new Point(r.Left - Max(i - 1, 0), r.Bottom + i), new Point(r.Right + i, r.Bottom + i));
            }
        if (IsVisible(RenderSide.Left, st))
            for (int i = 1; i < d; i++)
            {
                //LEFT
                using (Pen pen = new Pen(colors[i], 1f))
                    G.DrawLine(pen, new Point(r.Left - i, r.Top - i), new Point(r.Left - i, r.Bottom + i));
            }
        if (IsVisible(RenderSide.Right, st))
            for (int i = 0; i < d; i++)
            {
                //RIGHT
                using (Pen pen = new Pen(colors[i], 1f))
                    G.DrawLine(pen, new Point(r.Right + i, r.Top - i), new Point(r.Right + i, r.Bottom + Max(i - 1, 0)));
            }
    }

    //Code taken and adapted from StackOverflow (https://stackoverflow.com/a/13653167).
    //All credits go to Marino Šimić (https://stackoverflow.com/users/610204/marino-%c5%a0imi%c4%87).
    public static void DrawRoundedRectangle(this Graphics gfx, Rectangle bounds, int cornerRadius, Pen drawPen, Color fillColor)
    {
        int strokeOffset = Convert.ToInt32(Ceiling(drawPen.Width));
        bounds = Rectangle.Inflate(bounds, -strokeOffset, -strokeOffset);

        var gfxPath = new GraphicsPath();
        if (cornerRadius > 0)
        {
            gfxPath.AddArc(bounds.X, bounds.Y, cornerRadius, cornerRadius, 180, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y, cornerRadius, cornerRadius, 270, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y + bounds.Height - cornerRadius, cornerRadius,
                           cornerRadius, 0, 90);
            gfxPath.AddArc(bounds.X, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
        }
        else
        {
            gfxPath.AddRectangle(bounds);
        }
        gfxPath.CloseAllFigures();
        using (SolidBrush brush = new SolidBrush(fillColor))
        {
            gfx.FillPath(brush, gfxPath);
            if (drawPen != Pens.Transparent)
            {
                var pen = new Pen(drawPen.Color);
                pen.EndCap = pen.StartCap = LineCap.Round;
                gfx.DrawPath(pen, gfxPath);
                pen.Dispose();
            }
        }
        gfxPath.Dispose();
    }

    //Code taken and adapted from StackOverflow (https://stackoverflow.com/a/13653167).
    //All credits go to Marino Šimić (https://stackoverflow.com/users/610204/marino-%c5%a0imi%c4%87).
    public static void DrawOutsetShadow(Graphics g, Color shadowColor, int hShadow, int vShadow, int blur, int spread, Control control)
    {
        var rOuter = Rectangle.Inflate(control.Bounds, blur / 2, blur / 2);
        var rInner = Rectangle.Inflate(control.Bounds, blur / 2, blur / 2);
        //rInner.Offset(hShadow, vShadow);
        rInner.Inflate(-blur, -blur);
        rOuter.Inflate(spread, spread);
        //rOuter.Offset(hShadow, vShadow);
        var originalOuter = rOuter;

        var img = new Bitmap(originalOuter.Width, originalOuter.Height, g);
        var g2 = Graphics.FromImage(img);

        var currentBlur = 0;

        do
        {
            var transparency = (rOuter.Height - rInner.Height) / (double)(blur * 2 + spread * 2);
            var color = Color.FromArgb(((int)(200 * (transparency * transparency))), shadowColor);
            var rOutput = rInner;
            rOutput.Offset(-originalOuter.Left, -originalOuter.Top);
            g2.DrawRoundedRectangle(rOutput, 5, Pens.Transparent, color);
            rInner.Inflate(1, 1);
            currentBlur = (int)((double)blur * (1 - (transparency * transparency)));
        } while (rOuter.Contains(rInner));

        g2.Flush();
        g2.Dispose();

        g.DrawImage(img, originalOuter);

        img.Dispose();
    }

    //Code taken and adapted from https://stackoverflow.com/a/25741405
    //All credits go to TaW (https://stackoverflow.com/users/3152130/taw)
    static List<Color> GetColorVector(Color fc, int depth)
    {
        List<Color> cv = new List<Color>();
        int baseC = 65;
        float div = baseC / depth;
        for (int d = 1; d <= depth; d++)
        {
            cv.Add(Color.FromArgb(Max(0, baseC), fc));
            baseC -= (int)div;
        }
        return cv;
    }


    //Code taken and adapted from https://stackoverflow.com/a/25741405
    //All credits go to TaW (https://stackoverflow.com/users/3152130/taw)
    static GraphicsPath GetRectPath(Rectangle R)
    {
        byte[] fm = new byte[3];
        for (int b = 0; b < 3; b++) fm[b] = 1;
        List<Point> points = new List<Point>
            {
                new Point(R.Left, R.Bottom),
                new Point(R.Right, R.Bottom),
                new Point(R.Right, R.Top)
            };
        return new GraphicsPath(points.ToArray(), fm);
    }

    public static void CreateDropShadow(this Control ctrl)
    {
        if (ctrl.Parent != null)
        {
            ctrl.Parent.Paint += (s, e) => {
                if (ctrl.Parent != null && ctrl.Visible && (!(ctrl is IShadowController) || ((IShadowController)ctrl).ShouldShowShadow()))
                    DrawShadow(e.Graphics, Color.Black, ctrl.Bounds, 7, ctrl.Dock);
            };
        }
    }
}
