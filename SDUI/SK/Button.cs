using SDUI.Animation;
using SkiaSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.SK;

public class Button : SKControl
{
    public DialogResult DialogResult { get; set; } = DialogResult.None;
    public System.Windows.Forms.AutoSizeMode AutoSizeMode { get; set; } = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
    public bool UseVisualStyleBackColor { get; set; } = true;
    public Color Color { get; set; } = Color.Transparent;
    public Bitmap Image { get; set; }
    private int _mouseState = 0;
    private SKSize textSize;
    private string _text;
    public override string Text
    {
        get => _text;
        set
        {
            _text = value;
            using (var paint = new SKPaint { TextSize = Font.Size * 1.3333f })
            {
                var s = paint.MeasureText(value);
                textSize = new(s, s);
            }

            if (AutoSize)
                Size = GetPreferredSize();
            Invalidate();
        }
    }

    private float _shadowDepth = 4f;
    public float ShadowDepth
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

    private int _radius = 6;
    public int Radius
    {
        get => _radius;
        set
        {
            if (_radius == value)
                return;

            _radius = value;
            Invalidate();
        }
    }

    private readonly Animation.AnimationEngine animationManager;
    private readonly Animation.AnimationEngine hoverAnimationManager;

    public Button()
    {
        animationManager = new Animation.AnimationEngine(false)
        {
            Increment = 0.03,
            AnimationType = AnimationType.EaseOut
        };

        hoverAnimationManager = new Animation.AnimationEngine
        {
            Increment = 0.07,
            AnimationType = AnimationType.Linear,
        };

        hoverAnimationManager.OnAnimationFinished += (sender) => { };
        hoverAnimationManager.OnAnimationProgress += sender => Invalidate();

        animationManager.OnAnimationProgress += sender => Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _mouseState = 2;
        animationManager.StartNewAnimation(AnimationDirection.In, new Point(e.X, e.Y));
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _mouseState = 1;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _mouseState = 1;
        hoverAnimationManager.StartNewAnimation(AnimationDirection.In);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _mouseState = 0;
        hoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        Invalidate();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var rectf = new SKRect(0, 0, Width, Height);

        var color = Color != Color.Transparent ? Color.ToSKColor() : ColorScheme.BorderColor.ToSKColor();
        using var brush = new SKPaint { Color = color, IsAntialias = true };
        using var outerPen = new SKPaint { Color = ColorScheme.BackColor2.ToSKColor(), IsStroke = true };

        using (var path = RoundedRect(rectf, _radius))
        {
            if (Color.ToSKColor() == SKColors.Transparent)
                canvas.DrawPath(path, outerPen);

            canvas.DrawPath(path, brush);

            var animationColor = Color.ToSKColor() != SKColors.Transparent
                ? Color.ToSKColor().WithAlpha((byte)(hoverAnimationManager.GetProgress() * 65)) : color.WithAlpha((byte)(hoverAnimationManager.GetProgress() * color.Alpha));

            using var b = new SKPaint { Color = animationColor, IsAntialias = true };
            canvas.DrawPath(path, b);

            DrawShadow(canvas, rectf, _shadowDepth, _radius);


            // Ripple
            if (animationManager.IsAnimating())
            {
                for (int i = 0; i < animationManager.GetAnimationCount(); i++)
                {
                    var animationValue = animationManager.GetProgress(i);
                    var animationSource = animationManager.GetSource(i);
                    using var rippleBrush = new SKPaint { Color = new SKColor(255, 255, 255, (byte)(101 - (animationValue * 100))), IsAntialias = true };
                    var rippleSize = (float)(animationValue * Width * 2.0);

                    var rippleRect = new SKRect(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize);
                    path.AddOval(rippleRect);
                    canvas.DrawPath(path, rippleBrush);
                }
            }
        }

        var foreColor = Color.ToSKColor() == SKColors.Transparent ? ColorScheme.ForeColor.ToSKColor() : ForeColor.ToSKColor();
        if (!Enabled)
            foreColor = ColorScheme.ForeColor.ToSKColor().WithAlpha(200);

        var textRect = rectf;
        if (Image != null)
        {
            // Image
            var imageRect = new SKRect(8, 6, 24, 24);

            if (string.IsNullOrEmpty(Text))
                // Center Image
                imageRect.Offset(2, 0);

            if (Image != null)
                canvas.DrawBitmap(Image.ToSKBitmap(), imageRect);

            // Adjust text rectangle
            textRect.Inflate(-8 - 24 - 4 - 8, 0);
            textRect.Offset(8 + 24 + 4, 0);
        }

        using var textPaint = new SKPaint { Color = foreColor, IsAntialias = true, TextSize = 13.333f, HintingLevel = SKPaintHinting.Full, IsLinearText = true, TextAlign = SKTextAlign.Center };
        DrawText(canvas, _text, textRect, textPaint);
    }

    private static SKPath RoundedRect(SKRect rect, float radius)
    {
        radius /= 2;
        var path = new SKPath();
        path.AddRoundRect(rect, radius, radius);
        return path;
    }

    private void DrawShadow(SKCanvas canvas, SKRect rect, float shadowDepth, float radius)
    {
        shadowDepth /= 3;
        using var paint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 50),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Outer, shadowDepth)
        };

        rect.Offset(0, shadowDepth);
        canvas.DrawRoundRect(rect, radius, radius, paint);
    }

    private void DrawText(SKCanvas canvas, string text, SKRect rect, SKPaint paint)
    {
        canvas.DrawText(text, rect.MidX, rect.MidY, paint);
    }

    private Size GetPreferredSize()
    {
        return GetPreferredSize(Size.Empty);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        int extra = 16;

        if (Image != null)
            extra += 24 + 4;

        return new Size((int)Math.Ceiling(textSize.Width) + extra, 23);
    }
}