using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ChatBubble : Control
{
    private GraphicsPath _shape;
    private Color _bubbleColor = Color.FromArgb(217, 217, 217);
    private bool _drawBubbleArrow = true;

    private AnchorStyles _arrowPosition;
    public AnchorStyles ArrowPosition
    {
        get => _arrowPosition;
        set
        {
            _arrowPosition = value;
            InitializeShape();
            Invalidate();
        }
    }

    public override Color ForeColor
    {
        get => base.ForeColor;
        set
        {
            base.ForeColor = value;
            this.Invalidate();
        }
    }

    public Color BubbleColor
    {
        get => _bubbleColor;
        set
        {
            _bubbleColor = value;
            this.Invalidate();
        }
    }

    public bool DrawBubbleArrow
    {
        get { return _drawBubbleArrow; }
        set
        {
            _drawBubbleArrow = value;
            InitializeShape();
            Invalidate();
        }
    }

    public ChatBubble()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);

        _arrowPosition = AnchorStyles.Left;
        Size = new Size(152, 38);

        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(52, 52, 52);
        InitializeShape();

        UpdateStyles();
    }

    private void InitializeShape()
    {
        var shape = new GraphicsPath();

        switch (_arrowPosition)
        {
            case AnchorStyles.Top:
                break;
            case AnchorStyles.Bottom:
                break;
            case AnchorStyles.Left:
                shape.AddArc(9, 0, 10, 10, 180, 90);
                shape.AddArc(Width - 11, 0, 10, 10, -90, 90);
                shape.AddArc(Width - 11, Height - 11, 10, 10, 0, 90);
                shape.AddArc(9, Height - 11, 10, 10, 90, 90);
                break;
            case AnchorStyles.Right:
                shape.AddArc(0, 0, 10, 10, 180, 90);
                shape.AddArc(Width - 18, 0, 10, 10, -90, 90);
                shape.AddArc(Width - 18, Height - 11, 10, 10, 0, 90);
                shape.AddArc(0, Height - 11, 10, 10, 90, 90);
                break;
            case AnchorStyles.None:
                break;
            default:
                break;
        }

        shape.CloseAllFigures();
        _shape = shape;
    }

    protected override void OnResize(System.EventArgs e)
    {
        InitializeShape();

        base.OnResize(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var bitmap = new Bitmap(this.Width, this.Height))
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(BackColor);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                graphics.FillPath(new SolidBrush(_bubbleColor), _shape);

                // Draw a polygon on the right side of the bubble
                if (_drawBubbleArrow == true)
                {
                    Point[] p = null;
                    switch (_arrowPosition)
                    {
                        case AnchorStyles.Left:
                            p = new Point[] {
                            new Point(9, Height - 19),
                            new Point(0, Height - 25),
                            new Point(9, Height - 30)
                        };
                            break;
                        case AnchorStyles.Right:
                            p = new Point[] {
                            new Point(Width - 8, Height - 19),
                            new Point(Width, Height - 25),
                            new Point(Width - 8, Height - 30)
                        };
                            break;
                        default:
                            return;
                    }

                    graphics.FillPolygon(new SolidBrush(_bubbleColor), p);
                    graphics.DrawPolygon(new Pen(new SolidBrush(_bubbleColor)), p);
                }

                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImageUnscaled(bitmap, 0, 0);
            }

            var textRect = new Rectangle(15, 4, Width - 17, Height - 5);

            this.DrawString(e.Graphics, ContentAlignment.TopLeft, ForeColor, textRect);
        }
    }
}