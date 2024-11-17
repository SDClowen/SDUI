using SDUI.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class GroupBox : System.Windows.Forms.GroupBox
{
    private int _shadowDepth = 4;
    public int ShadowDepth
    {
        get => _shadowDepth;
        set
        {
            if (_shadowDepth == value)
                return;

            _shadowDepth = value;
            Invalidate();
        }
    }

    private int _radius = 10;
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;

            Invalidate();
        }
    }

    public GroupBox()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
            ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.DoubleBuffer |
                  ControlStyles.ResizeRedraw |
                  ControlStyles.Opaque |
                  ControlStyles.UserPaint, true);

        UpdateStyles();
        this.DoubleBuffered = true;
        this.BackColor = Color.Transparent;
        this.Padding = new Padding(3, 8, 3, 3);
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        Invalidate(true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        GroupBoxRenderer.DrawParentBackground(graphics, ClientRectangle, this);
        if (ColorScheme.DrawDebugBorders)
        {
            using var redPen = new Pen(Color.Red, 1);
            redPen.Alignment = PenAlignment.Inset;
            e.Graphics.DrawRectangle(redPen, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        var rect = ClientRectangle.ToRectangleF();
        var inflate = _shadowDepth / 4f;
        rect.Inflate(-inflate, -inflate);
        var shadowRect = rect;

        //using (var path = e.Graphics.GenerateRoundedRectangle(rect, _radius))
        using var path = rect.Radius(_radius);
        rect = new RectangleF(0, 0, rect.Width, Font.Height + 7);

        var color = ColorScheme.BorderColor;
        BackColor = Color.Transparent;

        using (var brush = new SolidBrush(ColorScheme.BackColor2))
            e.Graphics.FillPath(brush, path);

        using var backColorBrush = new SolidBrush(ColorScheme.BackColor2.Alpha(15));

        var clip = e.Graphics.ClipBounds;
        e.Graphics.SetClip(rect);
        e.Graphics.DrawLine(ColorScheme.BorderColor, 0, rect.Height - 1, rect.Width, rect.Height - 1);
        e.Graphics.FillPath(backColorBrush, path);

        this.DrawString(graphics, ColorScheme.ForeColor, rect);

        e.Graphics.SetClip(clip);
        e.Graphics.DrawShadow(shadowRect, _shadowDepth, _radius);
        e.Graphics.DrawPath(ColorScheme.BorderColor, path);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var preferredSize = base.GetPreferredSize(proposedSize);
        preferredSize.Width += _shadowDepth;
        preferredSize.Height += _shadowDepth;

        return preferredSize;
    }
}