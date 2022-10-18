using SDUI.Extensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class CheckBox : System.Windows.Forms.CheckBox
{
    private bool isHovered = false;
    private bool isPressed = false;

    private int _shadowDepth = 1;
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

    public CheckBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        AutoSize = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        CheckBoxRenderer.DrawParentBackground(graphics, ClientRectangle, this);

        if (ColorScheme.DrawDebugBorders)
        {
            var redPen = new Pen(Color.Red, 1);
            redPen.Alignment = PenAlignment.Outset;
            graphics.DrawRectangle(redPen, 0, 0, Width - 1, Height - 1);
        }

        Color borderColor, foreColor;

        if (isHovered && !isPressed && Enabled)
        {
            foreColor = ColorScheme.ForeColor.Alpha(100);
            borderColor = ColorScheme.BorderColor.Alpha(100);
        }
        else if (isHovered && isPressed && Enabled)
        {
            foreColor = ColorScheme.ForeColor.Alpha(150);
            borderColor = ColorScheme.BorderColor.Alpha(150);
        }
        else if (!Enabled)
        {
            foreColor = Color.Gray;
            borderColor = ColorScheme.BorderColor.Alpha(50);
        }
        else
        {
            foreColor = ColorScheme.ForeColor;
            borderColor = ColorScheme.BorderColor;
        }

        var boxRect = new Rectangle(0, Height / 2 - 7, 14, 14);

        using (var path = boxRect.Radius(1))
        {
            if (Checked)
            {
                using (var brush = new LinearGradientBrush(boxRect, Color.Blue, Color.DarkBlue, 90f))
                {
                    graphics.FillPath(brush, path);
                }

                TextRenderer.DrawText(e.Graphics, "a", new Font("Marlett", 11), boxRect, Color.White);
            }

            Helpers.ControlPaintHelper.DrawShadow(graphics, boxRect, _shadowDepth, 1);

            using (var p = new Pen(borderColor))
                graphics.DrawPath(p, path);
        }

        var textRect = new Rectangle(16, -1, Width - 16, Height);
        TextRenderer.DrawText(graphics, Text, Font, textRect, foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        isHovered = false;
        isPressed = false;
        Invalidate();

        base.OnLostFocus(e);
    }

    protected override void OnLeave(EventArgs e)
    {
        isHovered = false;
        isPressed = false;
        Invalidate();

        base.OnLeave(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        isHovered = true;
        Invalidate();

        base.OnMouseEnter(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            isPressed = true;
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        isPressed = false;
        Invalidate();

        base.OnMouseUp(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        isHovered = false;
        Invalidate();

        base.OnMouseLeave(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        Size preferredSize;
        base.GetPreferredSize(proposedSize);

        using (var g = CreateGraphics())
        {
            proposedSize = new Size(int.MaxValue, int.MaxValue);
            preferredSize = TextRenderer.MeasureText(g, Text, Font, proposedSize, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            preferredSize.Width += 16;
        }

        return preferredSize;
    }
}