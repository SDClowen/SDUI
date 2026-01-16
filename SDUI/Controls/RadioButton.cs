using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class Radio : UIElementBase
{
    private float RadioButtonSize => 18f * ScaleFactor;
    private float RadioButtonSizeHalf => RadioButtonSize / 2f;
    private float TextPadding => 4f * ScaleFactor;
    private float OuterCircleWidth => 2f * ScaleFactor;
    private float InnerCircleSize => RadioButtonSize - 2 * OuterCircleWidth;

    // animation managers
    private readonly AnimationManager animationManager;

    private readonly AnimationManager rippleAnimationManager;

    private bool _checked;
    private int _mouseState;

    private float boxOffset;

    // size related variables which should be recalculated onsizechanged
    private RectangleF radioButtonBounds;

    private bool ripple;

    public Radio()
    {
        animationManager = new AnimationManager
        {
            AnimationType = AnimationType.EaseInOut,
            Increment = 0.06
        };

        rippleAnimationManager = new AnimationManager(false)
        {
            AnimationType = AnimationType.Linear,
            Increment = 0.10,
            SecondaryIncrement = 0.08
        };

        animationManager.OnAnimationProgress += _ => Invalidate();
        rippleAnimationManager.OnAnimationProgress += _ => Invalidate();

        CheckedChanged += (_, _) =>
            animationManager.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);

        Ripple = true;
        _mouseLocation = new Point(-1, -1);

        _mouseState = 0;
        MouseEnter += (_, _) => { _mouseState = 1; };
        MouseLeave += (_, _) =>
        {
            _mouseLocation = new Point(-1, -1);
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
            _mouseLocation = e.Location;
            Cursor = IsMouseInCheckArea() ? Cursors.Hand : Cursors.Default;
        };
    }

    [Browsable(false)] private Point _mouseLocation { get; set; }

    [Category("Behavior")]
    public bool Ripple
    {
        get => ripple;
        set
        {
            ripple = value;
            AutoSize = AutoSize; //Make AutoSize directly set the bounds.

            if (value) Margin = new Padding(0);

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
            OnCheckedChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    public event EventHandler CheckedChanged;

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
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
        var height = Ripple ? 30f * ScaleFactor : 20f * ScaleFactor;
        var fullHeight = height + Padding.Vertical;

        // Calculate expected box offset, matching OnSizeChanged logic
        var boxOffset = fullHeight / 2 - (float)Math.Ceiling(RadioButtonSize / 2d);

        // Width = boxOffset (left space) + Radio + Gap + Text + Padding + Buffer
        var width = boxOffset + RadioButtonSize + TextPadding + (int)Math.Ceiling(textWidth) + Padding.Horizontal + 2;

        return new Size((int)Math.Ceiling(width), (int)Math.Ceiling(fullHeight));
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        var radioButtonCenter = boxOffset + RadioButtonSizeHalf;
        var animationProgress = (float)animationManager.GetProgress();

        var colorAlpha = Enabled ? (int)(animationProgress * 255.0) : 128;
        var backgroundAlpha = Enabled ? (int)(ColorScheme.Outline.A * (1.0 - animationProgress)) : 128;
        var animationSize = animationProgress * 7f * ScaleFactor;
        var animationSizeHalf = animationSize / 2;

        var accentColor = Enabled ? ColorScheme.Primary : ColorScheme.OnSurface.Alpha(50);

        // Ripple efekti
        if (Ripple && rippleAnimationManager.IsAnimating()) DrawRippleEffect(canvas, accentColor, radioButtonCenter);

        // Modern circular outline with Primary color when checked
        var outlineColor = Checked
            ? ColorScheme.Primary.ToSKColor()
            : Enabled
                ? ColorScheme.Outline.ToSKColor()
                : ColorScheme.OnSurface.Alpha(50).ToSKColor();

        using (var paint = new SKPaint
               {
                   Color = outlineColor,
                   IsAntialias = true,
                   Style = SKPaintStyle.Stroke,
                   StrokeWidth = 2f * ScaleFactor
               })
        {
            canvas.DrawCircle(
                radioButtonCenter,
                radioButtonCenter,
                RadioButtonSize / 2f - 1,
                paint
            );
        }

        // İç dolgu (background)
        using (var paint = new SKPaint
               {
                   Color = ColorScheme.Surface.ToSKColor(),
                   IsAntialias = true,
                   Style = SKPaintStyle.Fill
               })
        {
            canvas.DrawCircle(
                radioButtonCenter,
                radioButtonCenter,
                InnerCircleSize / 2f,
                paint
            );
        }

        // Seçili durumda iç nokta - Primary color
        if (Checked)
        {
            var primaryColor = ColorScheme.Primary.ToSKColor();
            var surfaceColor = ColorScheme.Surface.ToSKColor();
            var interpolatedColor = primaryColor.InterpolateColor(surfaceColor, 1f - animationProgress);

            using var paint = new SKPaint
            {
                Color = Enabled ? interpolatedColor : ColorScheme.OnSurface.Alpha(50).ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawCircle(
                radioButtonCenter,
                radioButtonCenter,
                animationSizeHalf,
                paint
            );
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

    private bool IsMouseInCheckArea()
    {
        return radioButtonBounds.Contains(_mouseLocation);
    }

    private void DrawRippleEffect(SKCanvas canvas, Color accentColor, float center)
    {
        for (var i = 0; i < rippleAnimationManager.GetAnimationCount(); i++)
        {
            var animationValue = (float)rippleAnimationManager.GetProgress(i);
            var animationSource = new SKPoint(center, center);

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

        float textX = boxOffset + RadioButtonSize + TextPadding;
        var textBounds = SKRect.Create(textX, Padding.Top, Width - textX - Padding.Right, Height - Padding.Vertical);

        canvas.DrawControlText(Text, textBounds, textPaint, font, ContentAlignment.MiddleLeft, AutoEllipsis,
            UseMnemonic);
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        boxOffset = Height / 2 - (float)Math.Ceiling(RadioButtonSize / 2d);
        radioButtonBounds = new RectangleF(boxOffset, boxOffset, RadioButtonSize, RadioButtonSize);
    }

    public override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
    }
}