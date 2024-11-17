using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Label : System.Windows.Forms.Label
{
    public float Angle = 45;
    public bool ApplyGradient { get; set; }

    private bool _gradientAnimation;
    public bool GradientAnimation
    {
        get => _gradientAnimation;
        set
        {
            _gradientAnimation = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gradient text colors
    /// </summary>
    private Color[] _gradient = new[] { Color.Gray, Color.Black };
    public Color[] Gradient
    {
        get => _gradient;
        set
        {
            _gradient = value; Invalidate();
        }
    }

    public Label()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        ButtonRenderer.DrawParentBackground(e.Graphics, ClientRectangle, this);
        e.Graphics.FillRectangle(BackColor.Brush(), ClientRectangle);

        if (GradientAnimation)
            Angle = Angle % 360 + 1;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        if (ApplyGradient)
        {
            using var brush = new LinearGradientBrush(ClientRectangle, _gradient[0], _gradient[1], Angle/*LinearGradientMode.Horizontal */);

            using var format = this.CreateStringFormat(TextAlign, AutoEllipsis, UseMnemonic);
            e.Graphics.DrawString(Text, Font, brush, ClientRectangle, format);
        }
        else
            this.DrawString(e.Graphics, TextAlign, ColorScheme.ForeColor, AutoEllipsis, UseMnemonic);
    }
}