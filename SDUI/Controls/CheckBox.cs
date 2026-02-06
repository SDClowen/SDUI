using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.ComponentModel;


namespace SDUI.Controls;

public class CheckBox : UIElementBase
{
    private float CheckboxSize => 16f * ScaleFactor;
    private float CheckboxSizeHalf => CheckboxSize / 2f;
    private float TextPadding => 4f * ScaleFactor;

    // Tik işareti koordinatları - checkbox içinde ortalı olacak şekilde ayarlandı
    private SKPoint[] CheckmarkLine
    {
        get
        {
            var scale = CheckboxSize / 16f;
            return [ 
                new(4f * scale, 8f * scale), 
                new(7f * scale, 11f * scale), 
                new(12f * scale, 6f * scale) 
            ];
        }
    }

    private readonly AnimationManager animationManager;
    private readonly AnimationManager rippleAnimationManager;
    private bool _checked;
    private CheckState _checkState = CheckState.Unchecked;
    private bool _inputHandlersAttached;
    private int _mouseState;
    private bool _threeState;
    private bool ripple;
    private float boxOffset;
    private SkiaSharp.SKRect boxRectangle;
    private SKPoint mouseLocation;

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
        MouseLocation = new SKPoint(-1, -1);
        AttachInputHandlers();
    }

    [Browsable(false)] public int Depth { get; set; }

    [Browsable(false)]
    public SKPoint MouseLocation
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
            if (value) Margin = new Thickness(0);
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
        return boxRectangle.Contains(MouseLocation.X, MouseLocation.Y);
    }

    public override SKSize GetPreferredSize(SKSize proposedSize)
    {
        using var font = new SKFont
        {
            Size = Font.Size.Topx(this),
            Typeface = FontManager.GetSKTypeface(Font),
            Edging = SKFontEdging.SubpixelAntialias
        };

        var textWidth = string.IsNullOrEmpty(Text) ? 0 : font.MeasureText(Text);
        var height = Ripple ? 30f * ScaleFactor : 20f * ScaleFactor;
        var fullHeight = height + Padding.Vertical;

        // Calculate expected box offset (visual centering)
        var boxOffset = (fullHeight - CheckboxSize) / 2f;

        // Width = boxOffset (left space) + CheckBox + Gap + Text + Padding + Buffer
        var width = boxOffset + CheckboxSize + TextPadding + (float)Math.Ceiling(textWidth) + Padding.Horizontal + 2;

        return new SKSize((int)Math.Ceiling(width), (int)Math.Ceiling(fullHeight));
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
        var boxRect = new SkiaSharp.SKRect(
            boxOffset,
            boxOffset,
            boxOffset + CheckboxSize - 1,
            boxOffset + CheckboxSize - 1
        );

        // Modern checkbox rendering
        if (Checked || CheckState == CheckState.Indeterminate)
        {
            // Filled state with color interpolation
            var primaryColor = ColorScheme.Primary;
            var containerColor = ColorScheme.PrimaryContainer;
            var interpolatedColor = primaryColor.InterpolateColor(containerColor, 1f - animationProgress);

            using (var paint = new SKPaint
                   {
                       Color = Enabled
                           ? interpolatedColor
                           : ColorScheme.OnSurface.WithAlpha(50),
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
                           ? ColorScheme.Outline
                           : ColorScheme.OnSurface.WithAlpha(50),
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

    private void DrawRippleEffect(SKCanvas canvas, SKColor accentColor)
    {
        var checkboxCenter = boxOffset + CheckboxSizeHalf - 1;

        for (var i = 0; i < rippleAnimationManager.GetAnimationCount(); i++)
        {
            var animationValue = (float)rippleAnimationManager.GetProgress(i);
            var animationSource = new SKPoint(checkboxCenter, checkboxCenter);

            var rippleColor = (bool)rippleAnimationManager.GetData(i)[0]
                ? SKColors.Black.WithAlpha((byte)(animationValue * 40))
                : accentColor.WithAlpha((byte)(animationValue * 40));

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

    private void DrawCheckboxFill(SKCanvas canvas, SkiaSharp.SKRect boxRect, SKColor accentColor, int colorAlpha,
        float animationProgress)
    {
        using var checkPaint = new SKPaint
        {
            Color = accentColor.WithAlpha((byte)colorAlpha),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f * ScaleFactor,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        if (CheckState == CheckState.Indeterminate)
        {
            canvas.DrawLine(
                boxOffset + 4f * ScaleFactor,
                boxOffset + CheckboxSizeHalf,
                boxOffset + CheckboxSize - 4f * ScaleFactor,
                boxOffset + CheckboxSizeHalf,
                checkPaint
            );
        }
        else
        {
            var checkMarkRect = new SkiaSharp.SKRect(
                boxOffset,
                boxOffset,
                boxOffset + CheckboxSize * animationProgress,
                boxOffset + CheckboxSize
            );

            using var clipPath = new SKPath();
            clipPath.AddRect(checkMarkRect);
            canvas.Save();
            canvas.ClipPath(clipPath);

            var path = new SKPath();
            var checkMarkLine = CheckmarkLine;
            path.MoveTo(boxOffset + checkMarkLine[0].X, boxOffset + checkMarkLine[0].Y);
            path.LineTo(boxOffset + checkMarkLine[1].X, boxOffset + checkMarkLine[1].Y);
            path.LineTo(boxOffset + checkMarkLine[2].X, boxOffset + checkMarkLine[2].Y);

            canvas.DrawPath(path, checkPaint);
            canvas.Restore();
        }
    }

    private void DrawText(SKCanvas canvas)
    {
        using var font = new SKFont
        {
            Size = Font.Size.Topx(this),
            Typeface = FontManager.GetSKTypeface(Font),
            Edging = SKFontEdging.SubpixelAntialias
        };

        using var textPaint = new SKPaint
        {
            Color = (Enabled ? ColorScheme.ForeColor : SKColors.Gray),
            IsAntialias = true
        };

        float textX = boxOffset + CheckboxSize + TextPadding;
        var textBounds = SkiaSharp.SKRect.Create(textX, base.Padding.Top, Width - textX - base.Padding.Right, Height - base.Padding.Vertical);

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
            MouseLocation = new SKPoint(-1, -1);
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
        boxOffset = (Height - CheckboxSize) / 2f;
        boxRectangle = new SkiaSharp.SKRect(boxOffset, boxOffset, boxOffset + CheckboxSize - 1, boxOffset + CheckboxSize - 1);
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