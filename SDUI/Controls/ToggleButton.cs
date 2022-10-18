using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ToggleButton : CheckBox
{

    [Browsable(true)]
    public override string Text
    {
        get { return base.Text; }
        set { }
    }

    public ToggleButton()
    {
        this.DoubleBuffered = true;
        this.MinimumSize = new Size(46, 22);
    }

    private GraphicsPath GetFigurePath()
    {
        int arcSize = this.Height - 1;
        var leftArc = new Rectangle(0, 0, arcSize, arcSize);
        var rightArc = new Rectangle(this.Width - arcSize - 2, 0, arcSize, arcSize);

        var path = new GraphicsPath();
        path.StartFigure();
        path.AddArc(leftArc, 90, 180);
        path.AddArc(rightArc, 270, 180);
        path.CloseFigure();

        return path;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        CheckBoxRenderer.DrawParentBackground(e.Graphics, ClientRectangle, this);
        int toggleSize = this.Height - 5;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var path = GetFigurePath();

        e.Graphics.FillPath(new SolidBrush(ColorScheme.BackColor2), path);
        e.Graphics.DrawPath(new Pen(ColorScheme.BorderColor, 1), path);

        using var solidBrush = new SolidBrush(ColorScheme.BorderColor.Alpha(50));
        if (this.Checked)
            e.Graphics.FillEllipse(solidBrush, new Rectangle(this.Width - this.Height + 1, 2, toggleSize, toggleSize));
        else
            e.Graphics.FillEllipse(solidBrush, new Rectangle(2, 2, toggleSize, toggleSize));
    }
}