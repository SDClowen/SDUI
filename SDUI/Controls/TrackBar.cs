using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class TrackBar : UIElementBase
{
    #region Variables

    private bool _isDragging;
    private bool _isHovered;
    private Point _mouseLocation;
    private readonly AnimationManager _thumbHoverAnimation;
    private readonly AnimationManager _thumbPressAnimation;
    private readonly AnimationManager _trackHoverAnimation;
    private readonly AnimationManager _valueAnimation;
    private readonly ToolTip _tooltip;

    private int _minimum = 0;
    private int _maximum = 100;
    private int _value = 0;
    private int _animatedValue = 0;
    private int _smallChange = 1;
    private int _largeChange = 10;
    private bool _drawValueString = false;
    private bool _jumpToMouse = false;
    private bool _showTooltip = true;
    private bool _showTicks = false;
    private int _tickFrequency = 10;
    private ValueDivisor _dividedValue = ValueDivisor.By1;
    private Size _thumbSize = new(16, 16);
    private SDUI.Orientation _orientation = SDUI.Orientation.Horizontal;
    private ThumbShape _thumbShape = ThumbShape.Circle;
    private Color _trackColor;
    private Color _thumbColor;
    private Color _tickColor;
    private TrackStyle _trackStyle = TrackStyle.Simple;
    private Color _trackGradientStart = Color.Empty;
    private Color _trackGradientEnd = Color.Empty;
    private Color _thumbGradientStart = Color.Empty;
    private Color _thumbGradientEnd = Color.Empty;
    private string _valueFormat = "{0}";
    private Size _customThumbSize = Size.Empty;

    #endregion

    #region Properties

    [Category("Behavior")]
    public int Minimum
    {
        get => _minimum;
        set
        {
            if (value >= _maximum)
                value = _maximum - 10;

            if (_value < value)
                _value = value;

            _minimum = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    public int Maximum
    {
        get => _maximum;
        set
        {
            if (value <= _minimum)
                value = _minimum + 10;

            if (_value > value)
                _value = value;

            _maximum = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    public int Value
    {
        get => _value;
        set
        {
            if (_value == value) return;

            var oldValue = _value;
            if (value < _minimum)
                _value = _minimum;
            else if (value > _maximum)
                _value = _maximum;
            else
                _value = value;

            if (_showTooltip && _isDragging)
                UpdateTooltip();

            // Değer değişim animasyonu
            _animatedValue = oldValue;
            _valueAnimation.StartNewAnimation(AnimationDirection.In);

            OnValueChanged();
            Invalidate();
        }
    }

    [Category("Behavior")]
    public int SmallChange
    {
        get => _smallChange;
        set => _smallChange = Math.Max(1, value);
    }

    [Category("Behavior")]
    public int LargeChange
    {
        get => _largeChange;
        set => _largeChange = Math.Max(1, value);
    }

    [Category("Behavior")]
    public ValueDivisor ValueDivision
    {
        get => _dividedValue;
        set
        {
            _dividedValue = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    public bool JumpToMouse
    {
        get => _jumpToMouse;
        set => _jumpToMouse = value;
    }

    [Category("Appearance")]
    public bool DrawValueString
    {
        get => _drawValueString;
        set
        {
            _drawValueString = value;
            UpdateControlSize();
            Invalidate();
        }
    }

    [Category("Appearance")]
    public SDUI.Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation == value) return;
            _orientation = value;
            UpdateControlSize();
            Invalidate();
        }
    }

    [Category("Behavior")]
    public bool ShowTooltip
    {
        get => _showTooltip;
        set => _showTooltip = value;
    }

    [Category("Appearance")]
    public ThumbShape ThumbStyle
    {
        get => _thumbShape;
        set
        {
            if (_thumbShape == value) return;
            _thumbShape = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public bool ShowTicks
    {
        get => _showTicks;
        set
        {
            if (_showTicks == value) return;
            _showTicks = value;
            UpdateControlSize();
            Invalidate();
        }
    }

    [Category("Appearance")]
    public int TickFrequency
    {
        get => _tickFrequency;
        set
        {
            if (value < 1) value = 1;
            if (_tickFrequency == value) return;
            _tickFrequency = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color TrackColor
    {
        get => _trackColor;
        set
        {
            if (_trackColor == value) return;
            _trackColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color ThumbColor
    {
        get => _thumbColor;
        set
        {
            if (_thumbColor == value) return;
            _thumbColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color TickColor
    {
        get => _tickColor;
        set
        {
            if (_tickColor == value) return;
            _tickColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public TrackStyle TrackStyle
    {
        get => _trackStyle;
        set
        {
            if (_trackStyle == value) return;
            _trackStyle = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color TrackGradientStart
    {
        get => _trackGradientStart;
        set
        {
            if (_trackGradientStart == value) return;
            _trackGradientStart = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color TrackGradientEnd
    {
        get => _trackGradientEnd;
        set
        {
            if (_trackGradientEnd == value) return;
            _trackGradientEnd = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color ThumbGradientStart
    {
        get => _thumbGradientStart;
        set
        {
            if (_thumbGradientStart == value) return;
            _thumbGradientStart = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color ThumbGradientEnd
    {
        get => _thumbGradientEnd;
        set
        {
            if (_thumbGradientEnd == value) return;
            _thumbGradientEnd = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Size CustomThumbSize
    {
        get => _customThumbSize;
        set
        {
            if (_customThumbSize == value) return;
            _customThumbSize = value;
            if (!value.IsEmpty)
                _thumbSize = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public string ValueFormat
    {
        get => _valueFormat;
        set
        {
            if (_valueFormat == value) return;
            _valueFormat = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public float ValueToSet
    {
        get => _value / (int)_dividedValue;
        set => Value = (int)(value * (int)_dividedValue);
    }

    public event EventHandler ValueChanged;

    #endregion

    public TrackBar()
    {
        Size = new Size(200, 22);
        MinimumSize = new Size(50, 22);

        _thumbHoverAnimation = new AnimationManager(singular: true)
        {
            Increment = 0.12f,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        _thumbPressAnimation = new AnimationManager(singular: true)
        {
            Increment = 0.20f,
            AnimationType = AnimationType.EaseOut,
            InterruptAnimation = true
        };
        _trackHoverAnimation = new AnimationManager(singular: true)
        {
            Increment = 0.10f,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        _valueAnimation = new AnimationManager(singular: true)
        {
            Increment = 0.15f,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };

        _thumbHoverAnimation.OnAnimationProgress += (s) => Invalidate();
        _thumbPressAnimation.OnAnimationProgress += (s) => Invalidate();
        _trackHoverAnimation.OnAnimationProgress += (s) => Invalidate();
        _valueAnimation.OnAnimationProgress += (s) =>
        {
            int pVal = (int)(s as AnimationManager).GetProgress();
            _animatedValue = (int)(_value * pVal + _animatedValue * (1 - pVal));
            Invalidate();
        };

        _tooltip = new();

        // Varsayılan renkler
        _trackColor = ColorScheme.BorderColor;
        _thumbColor = ColorScheme.AccentColor;
        _tickColor = ColorScheme.ForeColor;
    }

    private void UpdateControlSize()
    {
        if (_orientation == SDUI.Orientation.Horizontal)
        {
            Height = (_drawValueString ? 35 : 22) + (_showTicks ? 15 : 0);
            base.MinimumSize = new Size(50, Height);
        }
        else
        {
            Width = (_drawValueString ? 35 : 22) + (_showTicks ? 15 : 0);
            base.MinimumSize = new Size(Width, 50);
        }
    }

    private void UpdateTooltip()
    {
        if (_isDragging)
            _tooltip.Show(this, ValueToSet.ToString());
        else
            _tooltip.SetToolTip(this, ValueToSet.ToString());
    }

    protected virtual void OnValueChanged()
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    internal override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.KeyCode)
        {
            case Keys.Left when _orientation == SDUI.Orientation.Horizontal:
            case Keys.Down when _orientation == SDUI.Orientation.Vertical:
                Value -= _smallChange;
                break;

            case Keys.Right when _orientation == SDUI.Orientation.Horizontal:
            case Keys.Up when _orientation == SDUI.Orientation.Vertical:
                Value += _smallChange;
                break;

            case Keys.PageDown:
                Value -= _largeChange;
                break;

            case Keys.PageUp:
                Value += _largeChange;
                break;

            case Keys.Home:
                Value = _minimum;
                break;

            case Keys.End:
                Value = _maximum;
                break;
        }
    }

    internal override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Value += Math.Sign(e.Delta) * _smallChange;
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;

        var thumbRect = GetThumbRect();
        var trackRect = GetTrackRect();
        var hitRect = GetInteractionTrackRect();
        var onTrack = hitRect.Contains(e.Location);
        _isDragging = thumbRect.Contains(e.Location) || onTrack;

        if (_jumpToMouse || _isDragging)
        {
            var position = _orientation == SDUI.Orientation.Horizontal ?
                e.X - trackRect.X :
                e.Y - trackRect.Y;
            var length = _orientation == SDUI.Orientation.Horizontal ?
                trackRect.Width :
                trackRect.Height;
            var percentage = Math.Clamp(position / length, 0, 1);
            Value = (int)(_minimum + (_maximum - _minimum) * percentage);
        }

        if (_isDragging)
        {
            _thumbPressAnimation.StartNewAnimation(AnimationDirection.In);
            if (_showTooltip)
                UpdateTooltip();
        }

        Focus();
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _mouseLocation = e.Location;

        if (_isDragging)
        {
            var trackRect = GetTrackRect();
            var position = _orientation == SDUI.Orientation.Horizontal ?
                e.X - trackRect.X :
                e.Y - trackRect.Y;
            var length = _orientation == SDUI.Orientation.Horizontal ?
                trackRect.Width :
                trackRect.Height;
            var percentage = Math.Clamp(position / length, 0, 1);
            Value = (int)(_minimum + (_maximum - _minimum) * percentage);
        }
        else
        {
            var thumbRect = GetThumbRect();
            var wasHovered = _isHovered;
            _isHovered = thumbRect.Contains(e.Location) || GetInteractionTrackRect().Contains(e.Location);

            if (wasHovered != _isHovered)
            {
                _thumbHoverAnimation.StartNewAnimation(_isHovered ? AnimationDirection.In : AnimationDirection.Out);
                _trackHoverAnimation.StartNewAnimation(_isHovered ? AnimationDirection.In : AnimationDirection.Out);
            }
        }

        Invalidate();
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_isDragging)
        {
            _isDragging = false;
            _thumbPressAnimation.StartNewAnimation(AnimationDirection.Out);
            if (_showTooltip)
                _tooltip.Hide();
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        _thumbHoverAnimation.StartNewAnimation(AnimationDirection.Out);
        _trackHoverAnimation.StartNewAnimation(AnimationDirection.Out);
    }

    private RectangleF GetTrackRect()
    {
        if (_orientation == SDUI.Orientation.Horizontal)
            return new RectangleF(10 * base.ScaleFactor, Height / 2f - base.ScaleFactor, Width - 20 * base.ScaleFactor, 2 * base.ScaleFactor);
        else
            return new RectangleF(Width / 2f - base.ScaleFactor, 10 * base.ScaleFactor, 2 * base.ScaleFactor, Height - 20 * base.ScaleFactor);
    }
    private RectangleF GetInteractionTrackRect()
    {
        var rect = GetTrackRect();

        if (_orientation == SDUI.Orientation.Horizontal)
            rect.Inflate(0, Math.Max(_thumbSize.Height / 2f, 6 * ScaleFactor));
        else
            rect.Inflate(Math.Max(_thumbSize.Width / 2f, 6 * ScaleFactor), 0);

        return rect;
    }

    private RectangleF GetThumbRect()
    {
        var trackRect = GetTrackRect();
        var percentage = (_value - _minimum) / (float)(_maximum - _minimum);

        if (_orientation == SDUI.Orientation.Horizontal)
        {
            var thumbX = trackRect.X + trackRect.Width * percentage - _thumbSize.Width / 2f;
            var thumbY = Height / 2f - _thumbSize.Height / 2f;
            return new RectangleF(thumbX, thumbY, _thumbSize.Width, _thumbSize.Height);
        }
        else
        {
            var thumbX = Width / 2f - _thumbSize.Width / 2f;
            var thumbY = trackRect.Y + trackRect.Height * (1 - percentage) - _thumbSize.Height / 2f;
            return new RectangleF(thumbX, thumbY, _thumbSize.Width, _thumbSize.Height);
        }
    }

    private void DrawThumb(SKCanvas canvas, RectangleF thumbRect, SKPaint paint)
    {
        var centerX = thumbRect.X + thumbRect.Width / 2f;
        var centerY = thumbRect.Y + thumbRect.Height / 2f;
        var halfWidth = thumbRect.Width / 2;

        switch (_thumbShape)
        {
            case ThumbShape.Circle:
                canvas.DrawCircle(centerX, centerY, halfWidth, paint);
                break;

            case ThumbShape.Square:
                canvas.DrawRect(
                    new SKRect(
                        centerX - halfWidth,
                        centerY - halfWidth,
                        centerX + halfWidth,
                        centerY + halfWidth
                    ),
                    paint
                );
                break;

            case ThumbShape.Triangle:
                using (var path = new SKPath())
                {
                    if (_orientation == SDUI.Orientation.Horizontal)
                    {
                        path.MoveTo(centerX, centerY - halfWidth);
                        path.LineTo(centerX + halfWidth, centerY + halfWidth);
                        path.LineTo(centerX - halfWidth, centerY + halfWidth);
                    }
                    else
                    {
                        path.MoveTo(centerX - halfWidth, centerY);
                        path.LineTo(centerX + halfWidth, centerY - halfWidth);
                        path.LineTo(centerX + halfWidth, centerY + halfWidth);
                    }
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
                break;

            case ThumbShape.Diamond:
                using (var path = new SKPath())
                {
                    path.MoveTo(centerX, centerY - halfWidth);
                    path.LineTo(centerX + halfWidth, centerY);
                    path.LineTo(centerX, centerY + halfWidth);
                    path.LineTo(centerX - halfWidth, centerY);
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
                break;
        }
    }
    private void DrawThumbIndicator(SKCanvas canvas, RectangleF thumbRect, RectangleF trackRect, SKPaint paint)
    {
        var thickness = 6 * ScaleFactor;

        using var path = new SKPath();

        if (_orientation == SDUI.Orientation.Horizontal)
        {
            var tipY = trackRect.Top + trackRect.Height / 2f; // point into the track
            var baseY = thumbRect.Bottom - 1 * ScaleFactor;   // anchor under the thumb
            var centerX = thumbRect.Left + thumbRect.Width / 2f;

            path.MoveTo(centerX, tipY);
            path.LineTo(centerX - thickness, baseY);
            path.LineTo(centerX + thickness, baseY);
        }
        else
        {
            var tipX = trackRect.Left + trackRect.Width / 2f;
            var baseX = thumbRect.Right - 1 * ScaleFactor;
            var centerY = thumbRect.Top + thumbRect.Height / 2f;

            path.MoveTo(tipX, centerY);
            path.LineTo(baseX, centerY - thickness);
            path.LineTo(baseX, centerY + thickness);
        }

        path.Close();
        canvas.DrawPath(path, paint);
    }

    private void DrawTicks(SKCanvas canvas, RectangleF trackRect)
    {
        if (!_showTicks) return;

        using var paint = new SKPaint
        {
            Color = (_tickColor == Color.Empty ? ColorScheme.ForeColor : _tickColor)
                .Alpha(150).ToSKColor(),
            StrokeWidth = 1 * ScaleFactor,
            IsAntialias = true
        };

        var tickCount = (_maximum - _minimum) / _tickFrequency;
        var isHorizontal = _orientation == SDUI.Orientation.Horizontal;

        for (int i = 0; i <= tickCount; i++)
        {
            var position = i / (float)tickCount;
            float x, y;

            if (isHorizontal)
            {
                x = trackRect.Left + trackRect.Width * position;
                y = trackRect.Bottom + 3 * ScaleFactor;
                canvas.DrawLine(x, y, x, y + 5 * ScaleFactor, paint);
            }
            else
            {
                x = trackRect.Right + 3 * ScaleFactor;
                y = trackRect.Top + trackRect.Height * (1 - position);
                canvas.DrawLine(x, y, x + 5 * ScaleFactor, y, paint);
            }

            // Tick değerini yaz
            if (i % 2 == 0) // Her ikinci tick'te değer göster
            {
                var value = _minimum + i * _tickFrequency;
                var text = (value / (float)_dividedValue).ToString();

                using var font = new SKFont
                {
                    Size = 8f.PtToPx(this),
                    Typeface = SDUI.Helpers.FontManager.GetSKTypeface("Segoe UI"),
                    Subpixel = true,
                    Edging = SKFontEdging.SubpixelAntialias
                };
                using var textPaint = new SKPaint
                {
                    Color = paint.Color,
                    IsAntialias = true
                };

                var textAlign = isHorizontal ? SKTextAlign.Center : SKTextAlign.Left;

                if (isHorizontal)
                    TextRenderingHelper.DrawText(canvas, text, x, y + 15 * ScaleFactor, textAlign, font, textPaint);
                else
                    TextRenderingHelper.DrawText(canvas, text, x + 8 * ScaleFactor, y + 4 * ScaleFactor, textAlign, font, textPaint);
            }
        }
    }

    private void DrawTrack(SKCanvas canvas, RectangleF trackRect, SKPaint paint)
    {
        switch (_trackStyle)
        {
            case TrackStyle.Simple:
                canvas.DrawRoundRect(trackRect.ToSKRect(), 1 * ScaleFactor, 1 * ScaleFactor, paint);
                break;

            case TrackStyle.Rounded:
                canvas.DrawRoundRect(trackRect.ToSKRect(), trackRect.Height / 2, trackRect.Height / 2, paint);
                break;

            case TrackStyle.Groove:
                var oldMaskFilter = paint.MaskFilter;
                paint.MaskFilter = null;

                paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1f * ScaleFactor);
                canvas.DrawRoundRect(trackRect.ToSKRect(), 2 * ScaleFactor, 2 * ScaleFactor, paint);
                paint.MaskFilter = oldMaskFilter;
                oldMaskFilter?.Dispose();

                break;

            case TrackStyle.Glass:
                using (var path = new SKPath())
                {
                    path.AddRoundRect(trackRect.ToSKRect(), 2 * ScaleFactor, 2 * ScaleFactor);

                    // Ana track
                    canvas.DrawPath(path, paint);

                    // Parlama efekti
                    using var shimmerPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = SKColors.White.WithAlpha(40),
                        IsAntialias = true,
                        ImageFilter = SKImageFilter.CreateBlur(2 * ScaleFactor, 2 * ScaleFactor)
                    };

                    var shimmerRect = trackRect;
                    shimmerRect.Height /= 2;
                    using var shimmerPath = new SKPath();
                    shimmerPath.AddRoundRect(shimmerRect.ToSKRect(), 2 * ScaleFactor, 2 * ScaleFactor);
                    canvas.DrawPath(shimmerPath, shimmerPaint);
                }
                break;
        }
    }

    private SKPaint CreateGradientPaint(RectangleF rect, Color startColor, Color endColor, Color defaultColor, byte alpha = 255)
    {
        var paint = new SKPaint { IsAntialias = true };

        if (startColor != Color.Empty && endColor != Color.Empty)
        {
            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(rect.Left, rect.Top),
                new SKPoint(rect.Right, rect.Bottom),
                new[]
                {
                    startColor.ToSKColor().WithAlpha(alpha),
                    endColor.ToSKColor().WithAlpha(alpha)
                },
                null,
                SKShaderTileMode.Clamp
            );
        }
        else
        {
            paint.Color = defaultColor.ToSKColor().WithAlpha(alpha);
        }

        return paint;
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var trackRect = GetTrackRect();
        var thumbRect = GetThumbRect();
        var accentColor = (_thumbColor == Color.Empty ? ColorScheme.AccentColor : _thumbColor);
        var lightAccent = Color.FromArgb(accentColor.A,
            Math.Min(255, accentColor.R + 35),
            Math.Min(255, accentColor.G + 35),
            Math.Min(255, accentColor.B + 35));

        // Visual adjustments for track
        var trackThickness = 4 * ScaleFactor;
        var visualTrackRect = trackRect;

        if (_orientation == SDUI.Orientation.Horizontal)
            visualTrackRect.Inflate(0, (trackThickness - trackRect.Height) / 2);
        else
            visualTrackRect.Inflate((trackThickness - trackRect.Width) / 2, 0);

        // Draw Empty Track
        using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill })
        {
            paint.Color = (_trackColor == Color.Empty ? ColorScheme.BorderColor : _trackColor).ToSKColor();
            canvas.DrawRoundRect(visualTrackRect.ToSKRect(), trackThickness / 2, trackThickness / 2, paint);
        }

        // Draw Filled Track
        using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill })
        {
            paint.Color = (_thumbColor == Color.Empty ? ColorScheme.AccentColor : _thumbColor).ToSKColor();

            SKRect filledRect;
            if (_orientation == SDUI.Orientation.Horizontal)
            {
                var minFilled = trackThickness; // ensure visible fill at minimum
                filledRect = new SKRect(
                    visualTrackRect.Left,
                    visualTrackRect.Top,
                    Math.Max(visualTrackRect.Left + minFilled, thumbRect.Left + thumbRect.Width / 2),
                    visualTrackRect.Bottom);
            }
            else
            {
                var minFilled = trackThickness;
                filledRect = new SKRect(
                    visualTrackRect.Left,
                    thumbRect.Top + thumbRect.Height / 2,
                    visualTrackRect.Right,
                    Math.Max(visualTrackRect.Top + minFilled, visualTrackRect.Bottom));
            }

            canvas.DrawRoundRect(filledRect, trackThickness / 2, trackThickness / 2, paint);
        }

        // Draw Ticks
        DrawTicks(canvas, trackRect);

        // Draw Thumb Shadow
        using (var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(40),
            IsAntialias = true,
            ImageFilter = SKImageFilter.CreateDropShadow(0, 1 * ScaleFactor, 3 * ScaleFactor, 3 * ScaleFactor, SKColors.Black.WithAlpha(40))
        })
        {
            DrawThumb(canvas, thumbRect, shadowPaint);
        }

        // Draw Thumb Fill
        using (var paint = CreateGradientPaint(thumbRect, lightAccent, accentColor, accentColor))
        {
            paint.Style = SKPaintStyle.Fill;
            paint.IsAntialias = true;
            DrawThumb(canvas, thumbRect, paint);
        }

        // Draw Thumb Border
        using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 * ScaleFactor })
        {
            paint.Color = accentColor.ToSKColor();
            DrawThumb(canvas, thumbRect, paint);
        }
        
        // Draw indicator (arrow) to anchor the thumb visually to the track
        using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill })
        {
            paint.Color = accentColor.ToSKColor().WithAlpha(200);
            DrawThumbIndicator(canvas, thumbRect, trackRect, paint);
        }

        // Değer metni
        if (_drawValueString)
        {
            using var font = new SKFont
            {
                Size = 9f.PtToPx(this),
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            using var paint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                IsAntialias = true
            };

            var formattedValue = string.Format(_valueFormat, ValueToSet);

            if (_orientation == SDUI.Orientation.Horizontal)
            {
                TextRenderingHelper.DrawText(
                    canvas,
                    formattedValue,
                    5 * base.ScaleFactor,
                    Height - 8 * base.ScaleFactor,
                    SKTextAlign.Left,
                    font,
                    paint
                );
            }
            else
            {
                canvas.Save();
                canvas.RotateDegrees(90);
                TextRenderingHelper.DrawText(
                    canvas,
                    formattedValue,
                    5 * base.ScaleFactor,
                    -Width + 8 * base.ScaleFactor,
                    SKTextAlign.Left,
                    font,
                    paint
                );
                canvas.Restore();
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tooltip?.Dispose();
        }
        base.Dispose(disposing);
    }
}
