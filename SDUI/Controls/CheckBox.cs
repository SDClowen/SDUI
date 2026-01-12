using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class CheckBox : UIElementBase
{
    private const int CHECKBOX_SIZE = 16;
    private const int CHECKBOX_SIZE_HALF = CHECKBOX_SIZE / 2;
    private const int TEXT_PADDING = 4;

    // Tik işareti koordinatları - checkbox içinde ortalı olacak şekilde ayarlandı
    private static readonly Point[] CHECKMARK_LINE = { new(4, 8), new(7, 11), new(12, 6) };

    private readonly AnimationManager animationManager;
    private readonly AnimationManager rippleAnimationManager;
    private bool _checked;
    private CheckState _checkState = CheckState.Unchecked;
    private bool _inputHandlersAttached;
    private int _mouseState;
    private bool _threeState;

    private int boxOffset;
    private RectangleF boxRectangle;
    private Point mouseLocation;
    private bool ripple;

    public CheckBox()
    {
        animationManager = new AnimationManager
        {
            AnimationType = AnimationType.EaseInOut,
            Increment = 0.10
        };

        rippleAnimationManager = new AnimationManager(false)
        {
            AnimationType = AnimationType.Linear,
            Increment = 0.10,
            SecondaryIncrement = 0.07
        };

        animationManager.OnAnimationProgress += _ => Invalidate();
        rippleAnimationManager.OnAnimationProgress += _ => Invalidate();

        CheckedChanged += (_, _) =>
            animationManager.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);

        Ripple = true;
        MouseLocation = new Point(-1, -1);
        AttachInputHandlers();
    }

    [Browsable(false)] public int Depth { get; set; }

    [Browsable(false)]
    public Point MouseLocation
    {
        get => mouseLocation;
        set
        {
            mouseLocation = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    public bool Ripple
    {
        get => ripple;
        set
        {
            ripple = value;
            if (AutoSize)
                AdjustSize();
            if (value) Margin = new Padding(0);
            Invalidate();
        }
    }

    [Category("Behavior")]
    public bool ThreeState
    {
        get => _threeState;
        set
        {
            if (_threeState == value) return;
            _threeState = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    public CheckState CheckState
    {
        get => _checkState;
        set
        {
            if (_checkState == value) return;
            _checkState = value;
            _checked = value != CheckState.Unchecked;
            OnCheckStateChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    [Category("Behavior")]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            CheckState = value ? CheckState.Checked : CheckState.Unchecked;
            OnCheckedChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    public event EventHandler CheckedChanged;
    public event EventHandler CheckStateChanged;

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }

    protected virtual void OnCheckStateChanged(EventArgs e)
    {
        CheckStateChanged?.Invoke(this, e);
    }

    private bool IsMouseInCheckArea()
    {
        return boxRectangle.Contains(MouseLocation);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        using var font = new SKFont
        {
            Size = Font.Size.PtToPx(this),
            Typeface = FontManager.GetSKTypeface(Font),
            Edging = SKFontEdging.SubpixelAntialias
        };

        var textWidth = string.IsNullOrEmpty(Text) ? 0 : font.MeasureText(Text);
        var height = Ripple ? 30 : 20;
        var fullHeight = height + Padding.Vertical;

        // Calculate expected box offset (visual centering)
        var boxOffset = (fullHeight - CHECKBOX_SIZE) / 2;

        // Width = boxOffset (left space) + CheckBox + Gap + Text + Padding + Buffer
        var width = boxOffset + CHECKBOX_SIZE + TEXT_PADDING + (int)Math.Ceiling(textWidth) + Padding.Horizontal + 2;

        return new Size(width, fullHeight);
    }

    public override void OnCreateControl()
    {
        base.OnCreateControl();
        AttachInputHandlers();
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        var animationProgress = (float)animationManager.GetProgress();
        var checkboxRadius = 4f;

        // Ripple efekti
        if (Ripple && rippleAnimationManager.IsAnimating()) DrawRippleEffect(canvas, ColorScheme.Primary);

        // Checkbox çerçeve ve arka plan
        var boxRect = new SKRect(
            boxOffset,
            boxOffset,
            boxOffset + CHECKBOX_SIZE - 1,
            boxOffset + CHECKBOX_SIZE - 1
        );

        // Modern checkbox rendering
        if (Checked || CheckState == CheckState.Indeterminate)
        {
            // Filled state with color interpolation
            var primaryColor = ColorScheme.Primary.ToSKColor();
            var containerColor = ColorScheme.PrimaryContainer.ToSKColor();
            var interpolatedColor = primaryColor.InterpolateColor(containerColor, 1f - animationProgress);

            using (var paint = new SKPaint
                   {
                       Color = Enabled
                           ? interpolatedColor
                           : ColorScheme.OnSurface.Alpha(50).ToSKColor(),
                       IsAntialias = true,
                       Style = SKPaintStyle.Fill
                   })
            {
                canvas.DrawRoundRect(boxRect, checkboxRadius, checkboxRadius, paint);
            }

            // Checkmark
            DrawCheckboxFill(canvas, boxRect, ColorScheme.OnPrimary, 255, animationProgress);
        }
        else
        {
            // Unchecked outline
            using (var paint = new SKPaint
                   {
                       Color = Enabled
                           ? ColorScheme.Outline.ToSKColor()
                           : ColorScheme.OnSurface.Alpha(50).ToSKColor(),
                       IsAntialias = true,
                       Style = SKPaintStyle.Stroke,
                       StrokeWidth = 2f
                   })
            {
                canvas.DrawRoundRect(boxRect, checkboxRadius, checkboxRadius, paint);
            }
        }

        // Text
        if (!string.IsNullOrEmpty(Text)) DrawText(canvas);

        // Debug çerçevesi
        if (ColorScheme.DrawDebugBorders)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);
        }
    }

    private void DrawRippleEffect(SKCanvas canvas, Color accentColor)
    {
        var CHECKBOX_CENTER = boxOffset + CHECKBOX_SIZE_HALF - 1;

        for (var i = 0; i < rippleAnimationManager.GetAnimationCount(); i++)
        {
            var animationValue = (float)rippleAnimationManager.GetProgress(i);
            var animationSource = new SKPoint(CHECKBOX_CENTER, CHECKBOX_CENTER);

            var rippleColor = (bool)rippleAnimationManager.GetData(i)[0]
                ? SKColors.Black.WithAlpha((byte)(animationValue * 40))
                : accentColor.ToSKColor().WithAlpha((byte)(animationValue * 40));

            var rippleHeight = Height % 2 == 0 ? Height - 3f : Height - 2f;
            var rippleSize = rippleAnimationManager.GetDirection(i) == AnimationDirection.InOutIn
                ? rippleHeight * (0.8f + 0.2f * animationValue)
                : rippleHeight;

            using var paint = new SKPaint
            {
                Color = rippleColor,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            canvas.DrawCircle(animationSource.X, animationSource.Y, rippleSize / 2, paint);
        }
    }

    private void DrawCheckboxFill(SKCanvas canvas, SKRect boxRect, Color accentColor, int colorAlpha,
        float animationProgress)
    {
        using var checkPaint = new SKPaint
        {
            Color = accentColor.ToSKColor().WithAlpha((byte)colorAlpha),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        if (CheckState == CheckState.Indeterminate)
        {
            canvas.DrawLine(
                boxOffset + 4,
                boxOffset + CHECKBOX_SIZE_HALF,
                boxOffset + CHECKBOX_SIZE - 4,
                boxOffset + CHECKBOX_SIZE_HALF,
                checkPaint
            );
        }
        else
        {
            var checkMarkRect = new SKRect(
                boxOffset,
                boxOffset,
                boxOffset + CHECKBOX_SIZE * animationProgress,
                boxOffset + CHECKBOX_SIZE
            );

            using var clipPath = new SKPath();
            clipPath.AddRect(checkMarkRect);
            canvas.Save();
            canvas.ClipPath(clipPath);

            var path = new SKPath();
            path.MoveTo(boxOffset + CHECKMARK_LINE[0].X, boxOffset + CHECKMARK_LINE[0].Y);
            path.LineTo(boxOffset + CHECKMARK_LINE[1].X, boxOffset + CHECKMARK_LINE[1].Y);
            path.LineTo(boxOffset + CHECKMARK_LINE[2].X, boxOffset + CHECKMARK_LINE[2].Y);

            canvas.DrawPath(path, checkPaint);
            canvas.Restore();
        }
    }

    private void DrawText(SKCanvas canvas)
    {
        using var font = new SKFont
        {
            Size = Font.Size.PtToPx(this),
            Typeface = FontManager.GetSKTypeface(Font),
            Edging = SKFontEdging.SubpixelAntialias
        };

        using var textPaint = new SKPaint
        {
            Color = (Enabled ? ColorScheme.ForeColor : Color.Gray).ToSKColor(),
            IsAntialias = true
        };

        float textX = boxOffset + CHECKBOX_SIZE + TEXT_PADDING;
        var textBounds = SKRect.Create(textX, Padding.Top, Width - textX - Padding.Right, Height - Padding.Vertical);

        canvas.DrawControlText(Text, textBounds, textPaint, font, ContentAlignment.MiddleLeft, AutoEllipsis,
            UseMnemonic);
    }

    private void AttachInputHandlers()
    {
        if (_inputHandlersAttached)
            return;

        MouseEnter += (_, _) => _mouseState = 1;
        MouseLeave += (_, _) =>
        {
            MouseLocation = new Point(-1, -1);
            _mouseState = 0;
        };
        MouseDown += (_, e) =>
        {
            _mouseState = 2;
            if (Ripple && e.Button == MouseButtons.Left && IsMouseInCheckArea())
            {
                rippleAnimationManager.SecondaryIncrement = 0;
                rippleAnimationManager.StartNewAnimation(AnimationDirection.InOutIn, new object[] { Checked });
            }
        };
        MouseUp += (_, _) =>
        {
            _mouseState = 1;
            rippleAnimationManager.SecondaryIncrement = 0.08;
        };
        MouseMove += (_, e) =>
        {
            MouseLocation = e.Location;
            Cursor = IsMouseInCheckArea() ? Cursors.Hand : Cursors.Default;
        };

        _inputHandlersAttached = true;
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        boxOffset = (Height - CHECKBOX_SIZE) / 2;
        boxRectangle = new RectangleF(boxOffset, boxOffset, CHECKBOX_SIZE - 1, CHECKBOX_SIZE - 1);
    }

    public override void OnClick(EventArgs e)
    {
        if (ThreeState)
            CheckState = CheckState switch
            {
                CheckState.Unchecked => CheckState.Checked,
                CheckState.Checked => CheckState.Indeterminate,
                CheckState.Indeterminate => CheckState.Unchecked,
                _ => CheckState.Unchecked
            };
        else
            Checked = !Checked;
        base.OnClick(e);
    }
}