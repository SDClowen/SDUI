using SDUI.Animation;
using SDUI.Extensions;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class Radio : UIElementBase
{
    private const int RADIOBUTTON_INNER_CIRCLE_SIZE = RADIOBUTTON_SIZE - (2 * RADIOBUTTON_OUTER_CIRCLE_WIDTH);

    private const int RADIOBUTTON_OUTER_CIRCLE_WIDTH = 2;

    // size constants
    private const int RADIOBUTTON_SIZE = 18;

    private const int RADIOBUTTON_SIZE_HALF = RADIOBUTTON_SIZE / 2;

    private const int TEXT_PADDING = 4;

    // animation managers
    private readonly AnimationManager animationManager;

    private readonly AnimationManager rippleAnimationManager;

    private int boxOffset;
    private int _mouseState;

    // size related variables which should be recalculated onsizechanged
    private Rectangle radioButtonBounds;

    private bool ripple;

    private bool _checked;

    [Browsable(false)]
    private Point _mouseLocation { get; set; }

    [Category("Behavior")]
    public bool Ripple
    {
        get { return ripple; }
        set
        {
            ripple = value;
            AutoSize = AutoSize; //Make AutoSize directly set the bounds.

            if (value)
            {
                Margin = new Padding(0);
            }

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

    protected virtual void OnCheckedChanged(EventArgs e) => CheckedChanged?.Invoke(this, e);

    public Radio()
    {
        animationManager = new()
        {
            AnimationType = AnimationType.EaseInOut,
            Increment = 0.06
        };

        rippleAnimationManager = new(false)
        {
            AnimationType = AnimationType.Linear,
            Increment = 0.10,
            SecondaryIncrement = 0.08
        };

        animationManager.OnAnimationProgress += _ => Invalidate();
        rippleAnimationManager.OnAnimationProgress += _ => Invalidate();

        CheckedChanged += (_, _) => animationManager.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);

        Ripple = true;
        _mouseLocation = new Point(-1, -1);

        _mouseState = 0;
        MouseEnter += (_, _) =>
        {
            _mouseState = 1;
        };
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

    public override Size GetPreferredSize(Size proposedSize)
    {
        using var font = new SKFont
        {
            Size = Font.Size.PtToPx(this),
            Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
            Edging = SKFontEdging.SubpixelAntialias
        };

        float textWidth = string.IsNullOrEmpty(Text) ? 0 : font.MeasureText(Text);
        int width = RADIOBUTTON_SIZE + TEXT_PADDING + (int)Math.Ceiling(textWidth);
        int height = Ripple ? 30 : 20;

        return new Size(width + Padding.Horizontal, height + Padding.Vertical);
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var RADIOBUTTON_CENTER = boxOffset + RADIOBUTTON_SIZE_HALF;
        var animationProgress = (float)animationManager.GetProgress();

        int colorAlpha = Enabled ? (int)(animationProgress * 255.0) : 128;
        int backgroundAlpha = Enabled ? (int)(ColorScheme.Outline.A * (1.0 - animationProgress)) : 128;
        float animationSize = (float)(animationProgress * 7f);
        float animationSizeHalf = animationSize / 2;

        var accentColor = Enabled ? ColorScheme.Primary : ColorScheme.OnSurface.Alpha(50);

        // Ripple efekti
        if (Ripple && rippleAnimationManager.IsAnimating())
        {
            DrawRippleEffect(canvas, accentColor, RADIOBUTTON_CENTER);
        }

        // Modern circular outline with Primary color when checked
        var outlineColor = Checked 
            ? ColorScheme.Primary.ToSKColor()
            : (Enabled ? ColorScheme.Outline.ToSKColor() : ColorScheme.OnSurface.Alpha(50).ToSKColor());
        
        using (var paint = new SKPaint
        {
            Color = outlineColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f
        })
        {
            canvas.DrawCircle(
                RADIOBUTTON_CENTER,
                RADIOBUTTON_CENTER,
                RADIOBUTTON_SIZE / 2f - 1,
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
                RADIOBUTTON_CENTER,
                RADIOBUTTON_CENTER,
                RADIOBUTTON_INNER_CIRCLE_SIZE / 2f,
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
                RADIOBUTTON_CENTER,
                RADIOBUTTON_CENTER,
                animationSizeHalf,
                paint
            );
        }

        // Text
        if (!string.IsNullOrEmpty(Text))
        {
            DrawText(canvas);
        }

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
        for (int i = 0; i < rippleAnimationManager.GetAnimationCount(); i++)
        {
            var animationValue = (float)rippleAnimationManager.GetProgress(i);
            var animationSource = new SKPoint(center, center);

            var rippleColor = ((bool)rippleAnimationManager.GetData(i)[0]) ?
                SKColors.Black.WithAlpha((byte)(animationValue * 40)) :
                accentColor.ToSKColor().WithAlpha((byte)(animationValue * 40));

            var rippleHeight = (Height % 2 == 0) ? Height - 3f : Height - 2f;
            var rippleSize = (rippleAnimationManager.GetDirection(i) == AnimationDirection.InOutIn) ?
                rippleHeight * (0.8f + (0.2f * animationValue)) : rippleHeight;

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
            Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
            Edging = SKFontEdging.SubpixelAntialias
        };

        using var textPaint = new SKPaint
        {
            Color = (Enabled ? ColorScheme.ForeColor : Color.Gray).ToSKColor(),
            IsAntialias = true
        };

        float textX = boxOffset + RADIOBUTTON_SIZE + TEXT_PADDING;
        var textBounds = SKRect.Create(textX, 0, Width - textX - Padding.Right, Height);

        canvas.DrawControlText(Text, textBounds, textPaint, font, ContentAlignment.MiddleLeft, AutoEllipsis, UseMnemonic);
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        boxOffset = Height / 2 - (int)Math.Ceiling(RADIOBUTTON_SIZE / 2d);
        radioButtonBounds = new Rectangle(boxOffset, boxOffset, RADIOBUTTON_SIZE, RADIOBUTTON_SIZE);
    }

    public override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
    }
}