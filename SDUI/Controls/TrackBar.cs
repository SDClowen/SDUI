using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class TrackBar : UIElementBase
{
    public TrackBar()
    {
        Size = new Size(200, 22);
        MinimumSize = new Size(50, 22);

        _thumbHoverAnimation = new AnimationManager(true)
        {
            Increment = 0.12f,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        _thumbPressAnimation = new AnimationManager(true)
        {
            Increment = 0.20f,
            AnimationType = AnimationType.EaseOut,
            InterruptAnimation = true
        };
        _trackHoverAnimation = new AnimationManager(true)
        {
            Increment = 0.10f,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };
        _valueAnimation = new AnimationManager(true)
        {
            Increment = 0.15f,
            AnimationType = AnimationType.EaseInOut,
            InterruptAnimation = true
        };

        _thumbHoverAnimation.OnAnimationProgress += s => Invalidate();
        _thumbPressAnimation.OnAnimationProgress += s => Invalidate();
        _trackHoverAnimation.OnAnimationProgress += s => Invalidate();
        _valueAnimation.OnAnimationProgress += s =>
        {
            var pVal = (int)(s as AnimationManager).GetProgress();
            _animatedValue = _value * pVal + _animatedValue * (1 - pVal);
            Invalidate();
        };

        _tooltip = new ToolTip();

        // Varsayılan renkler
        _trackColor = ColorScheme.BorderColor;
        _thumbColor = ColorScheme.AccentColor;
        _tickColor = ColorScheme.ForeColor;
    }

    private void UpdateControlSize()
    {
        if (_orientation == Orientation.Horizontal)
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
            case Keys.Left when _orientation == Orientation.Horizontal:
            case Keys.Down when _orientation == Orientation.Vertical:
                Value -= _smallChange;
                break;

            case Keys.Right when _orientation == Orientation.Horizontal:
            case Keys.Up when _orientation == Orientation.Vertical:
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

        if (JumpToMouse || _isDragging)
        {
            var position = _orientation == Orientation.Horizontal ? e.X - trackRect.X : e.Y - trackRect.Y;
            var length = _orientation == Orientation.Horizontal ? trackRect.Width : trackRect.Height;
            var percentage = Math.Clamp(position / length, 0, 1);
            Value = (int)(_minimum + (_maximum - _minimum) * percentage);
        }

        if (_isDragging)
        {
            _thumbPressAnimation.StartNewAnimation(AnimationDirection.In);
            if (ShowTooltip)
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
            var position = _orientation == Orientation.Horizontal ? e.X - trackRect.X : e.Y - trackRect.Y;
            var length = _orientation == Orientation.Horizontal ? trackRect.Width : trackRect.Height;
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
            if (ShowTooltip)
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
        if (_orientation == Orientation.Horizontal)
            return new RectangleF(10 * base.ScaleFactor, Height / 2f - base.ScaleFactor, Width - 20 * base.ScaleFactor,
                2 * base.ScaleFactor);
        return new RectangleF(Width / 2f - base.ScaleFactor, 10 * base.ScaleFactor, 2 * base.ScaleFactor,
            Height - 20 * base.ScaleFactor);
    }

    private RectangleF GetInteractionTrackRect()
    {
        var rect = GetTrackRect();

        if (_orientation == Orientation.Horizontal)
            rect.Inflate(0, Math.Max(_thumbSize.Height / 2f, 6 * ScaleFactor));
        else
            rect.Inflate(Math.Max(_thumbSize.Width / 2f, 6 * ScaleFactor), 0);

        return rect;
    }

    private RectangleF GetThumbRect()
    {
        var trackRect = GetTrackRect();
        var percentage = (_value - _minimum) / (float)(_maximum - _minimum);

        if (_orientation == Orientation.Horizontal)
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
                EnsureSkiaCaches();
                _thumbTrianglePath ??= new SKPath();
                _thumbTrianglePath.Reset();

                if (_orientation == Orientation.Horizontal)
                {
                    _thumbTrianglePath.MoveTo(centerX, centerY - halfWidth);
                    _thumbTrianglePath.LineTo(centerX + halfWidth, centerY + halfWidth);
                    _thumbTrianglePath.LineTo(centerX - halfWidth, centerY + halfWidth);
                }
                else
                {
                    _thumbTrianglePath.MoveTo(centerX - halfWidth, centerY);
                    _thumbTrianglePath.LineTo(centerX + halfWidth, centerY - halfWidth);
                    _thumbTrianglePath.LineTo(centerX + halfWidth, centerY + halfWidth);
                }

                _thumbTrianglePath.Close();
                canvas.DrawPath(_thumbTrianglePath, paint);
                break;

            case ThumbShape.Diamond:
                EnsureSkiaCaches();
                _thumbDiamondPath ??= new SKPath();
                _thumbDiamondPath.Reset();
                _thumbDiamondPath.MoveTo(centerX, centerY - halfWidth);
                _thumbDiamondPath.LineTo(centerX + halfWidth, centerY);
                _thumbDiamondPath.LineTo(centerX, centerY + halfWidth);
                _thumbDiamondPath.LineTo(centerX - halfWidth, centerY);
                _thumbDiamondPath.Close();
                canvas.DrawPath(_thumbDiamondPath, paint);
                break;
        }
    }

    private void DrawThumbIndicator(SKCanvas canvas, RectangleF thumbRect, RectangleF trackRect, SKPaint paint)
    {
        var thickness = 6 * ScaleFactor;

        EnsureSkiaCaches();
        _thumbIndicatorPath ??= new SKPath();
        _thumbIndicatorPath.Reset();

        if (_orientation == Orientation.Horizontal)
        {
            var tipY = trackRect.Top + trackRect.Height / 2f; // point into the track
            var baseY = thumbRect.Bottom - 1 * ScaleFactor; // anchor under the thumb
            var centerX = thumbRect.Left + thumbRect.Width / 2f;

            _thumbIndicatorPath.MoveTo(centerX, tipY);
            _thumbIndicatorPath.LineTo(centerX - thickness, baseY);
            _thumbIndicatorPath.LineTo(centerX + thickness, baseY);
        }
        else
        {
            var tipX = trackRect.Left + trackRect.Width / 2f;
            var baseX = thumbRect.Right - 1 * ScaleFactor;
            var centerY = thumbRect.Top + thumbRect.Height / 2f;

            _thumbIndicatorPath.MoveTo(tipX, centerY);
            _thumbIndicatorPath.LineTo(baseX, centerY - thickness);
            _thumbIndicatorPath.LineTo(baseX, centerY + thickness);
        }

        _thumbIndicatorPath.Close();
        canvas.DrawPath(_thumbIndicatorPath, paint);
    }

    private void DrawTicks(SKCanvas canvas, RectangleF trackRect)
    {
        if (!_showTicks) return;

        EnsureSkiaCaches();

        var tickPaint = _tickPaint!;
        tickPaint.Color = (_tickColor == Color.Empty ? ColorScheme.ForeColor : _tickColor)
            .Alpha(150).ToSKColor();
        tickPaint.StrokeWidth = 1 * ScaleFactor;

        var textPaint = _tickTextPaint!;
        textPaint.Color = tickPaint.Color;

        var font = GetTickSkFont();

        var tickCount = (_maximum - _minimum) / _tickFrequency;
        var isHorizontal = _orientation == Orientation.Horizontal;

        for (var i = 0; i <= tickCount; i++)
        {
            var position = i / (float)tickCount;
            float x, y;

            if (isHorizontal)
            {
                x = trackRect.Left + trackRect.Width * position;
                y = trackRect.Bottom + 3 * ScaleFactor;
                canvas.DrawLine(x, y, x, y + 5 * ScaleFactor, tickPaint);
            }
            else
            {
                x = trackRect.Right + 3 * ScaleFactor;
                y = trackRect.Top + trackRect.Height * (1 - position);
                canvas.DrawLine(x, y, x + 5 * ScaleFactor, y, tickPaint);
            }

            // Tick değerini yaz
            if (i % 2 == 0) // Her ikinci tick'te değer göster
            {
                var value = _minimum + i * _tickFrequency;
                var text = (value / (float)_dividedValue).ToString();

                var textAlign = isHorizontal ? SKTextAlign.Center : SKTextAlign.Left;

                if (isHorizontal)
                    TextRenderingHelper.DrawText(canvas, text, x, y + 15 * ScaleFactor, textAlign, font, textPaint);
                else
                    TextRenderingHelper.DrawText(canvas, text, x + 8 * ScaleFactor, y + 4 * ScaleFactor, textAlign,
                        font, textPaint);
            }
        }
    }

    private void EnsureSkiaCaches()
    {
        _trackEmptyPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _trackFilledPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _thumbShadowPaint ??= new SKPaint { IsAntialias = true };
        _thumbFillPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _thumbBorderPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        _thumbIndicatorPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        _tickPaint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        _tickTextPaint ??= new SKPaint { IsAntialias = true };
        _valueTextPaint ??= new SKPaint { IsAntialias = true };
    }

    private SKFont GetTickSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        if (_tickFont == null || _tickFontDpi != dpi)
        {
            _tickFont?.Dispose();
            _tickFont = new SKFont
            {
                Size = 8f.PtToPx(this),
                Typeface = FontManager.GetSKTypeface("Segoe UI"),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            _tickFontDpi = dpi;
        }

        return _tickFont;
    }

    private SKFont GetValueSkFont()
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        if (_valueFont == null || _valueFontDpi != dpi || !ReferenceEquals(_valueFontSource, Font))
        {
            _valueFont?.Dispose();
            _valueFont = new SKFont
            {
                Size = 9f.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            _valueFontDpi = dpi;
            _valueFontSource = Font;
        }

        return _valueFont;
    }

    private void EnsureThumbShadowFilter()
    {
        // Recreate only when scale factor changes meaningfully
        if (_thumbShadowFilter == null || Math.Abs(_thumbShadowFilterScale - ScaleFactor) > 0.0001f)
        {
            _thumbShadowFilter?.Dispose();
            _thumbShadowFilter = SKImageFilter.CreateDropShadow(
                0,
                1 * ScaleFactor,
                3 * ScaleFactor,
                3 * ScaleFactor,
                SKColors.Black.WithAlpha(40));
            _thumbShadowFilterScale = ScaleFactor;
        }
    }

    private SKPaint UpdateThumbFillPaint(RectangleF rect, Color startColor, Color endColor, Color defaultColor,
        byte alpha = 255)
    {
        EnsureSkiaCaches();

        var paint = _thumbFillPaint!;
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Fill;

        if (startColor != Color.Empty && endColor != Color.Empty)
        {
            // NOTE: shader coordinates depend on rect position, so this is updated per paint.
            var newShader = SKShader.CreateLinearGradient(
                new SKPoint(rect.Left, rect.Top),
                new SKPoint(rect.Right, rect.Bottom),
                new[]
                {
                    startColor.ToSKColor().WithAlpha(alpha),
                    endColor.ToSKColor().WithAlpha(alpha)
                },
                null,
                SKShaderTileMode.Clamp);

            paint.Color = SKColors.Transparent;
            var oldShader = _thumbFillShader;
            _thumbFillShader = newShader;
            paint.Shader = _thumbFillShader;
            oldShader?.Dispose();
        }
        else
        {
            paint.Shader = null;
            _thumbFillShader?.Dispose();
            _thumbFillShader = null;
            paint.Color = defaultColor.ToSKColor().WithAlpha(alpha);
        }

        return paint;
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);
        EnsureSkiaCaches();

        var trackRect = GetTrackRect();
        var thumbRect = GetThumbRect();
        var accentColor = _thumbColor == Color.Empty ? ColorScheme.AccentColor : _thumbColor;
        var lightAccent = Color.FromArgb(accentColor.A,
            Math.Min(255, accentColor.R + 35),
            Math.Min(255, accentColor.G + 35),
            Math.Min(255, accentColor.B + 35));

        // Visual adjustments for track
        var trackThickness = 4 * ScaleFactor;
        var visualTrackRect = trackRect;

        if (_orientation == Orientation.Horizontal)
            visualTrackRect.Inflate(0, (trackThickness - trackRect.Height) / 2);
        else
            visualTrackRect.Inflate((trackThickness - trackRect.Width) / 2, 0);

        // Draw Empty Track
        var emptyTrackPaint = _trackEmptyPaint!;
        emptyTrackPaint.Color = (_trackColor == Color.Empty ? ColorScheme.BorderColor : _trackColor).ToSKColor();
        canvas.DrawRoundRect(visualTrackRect.ToSKRect(), trackThickness / 2, trackThickness / 2, emptyTrackPaint);

        // Draw Filled Track
        var filledTrackPaint = _trackFilledPaint!;
        filledTrackPaint.Color = (_thumbColor == Color.Empty ? ColorScheme.AccentColor : _thumbColor).ToSKColor();

        SKRect filledRect;
        if (_orientation == Orientation.Horizontal)
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

        canvas.DrawRoundRect(filledRect, trackThickness / 2, trackThickness / 2, filledTrackPaint);

        // Draw Ticks
        DrawTicks(canvas, trackRect);

        // Draw Thumb Shadow
        EnsureThumbShadowFilter();
        var shadowPaint = _thumbShadowPaint!;
        shadowPaint.Color = SKColors.Black.WithAlpha(40);
        shadowPaint.ImageFilter = _thumbShadowFilter;
        DrawThumb(canvas, thumbRect, shadowPaint);

        // Draw Thumb Fill
        var thumbFillPaint = UpdateThumbFillPaint(thumbRect, lightAccent, accentColor, accentColor);
        DrawThumb(canvas, thumbRect, thumbFillPaint);

        // Draw Thumb Border
        var thumbBorderPaint = _thumbBorderPaint!;
        thumbBorderPaint.StrokeWidth = 2 * ScaleFactor;
        thumbBorderPaint.Color = accentColor.ToSKColor();
        DrawThumb(canvas, thumbRect, thumbBorderPaint);

        // Draw indicator (arrow) to anchor the thumb visually to the track
        var indicatorPaint = _thumbIndicatorPaint!;
        indicatorPaint.Color = accentColor.ToSKColor().WithAlpha(200);
        DrawThumbIndicator(canvas, thumbRect, trackRect, indicatorPaint);

        // Değer metni
        if (_drawValueString)
        {
            var font = GetValueSkFont();
            var paint = _valueTextPaint!;
            paint.Color = ColorScheme.ForeColor.ToSKColor();
            paint.IsAntialias = true;

            var formattedValue = string.Format(_valueFormat, ValueToSet);

            if (_orientation == Orientation.Horizontal)
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

            _trackEmptyPaint?.Dispose();
            _trackFilledPaint?.Dispose();
            _thumbShadowPaint?.Dispose();
            _thumbFillPaint?.Dispose();
            _thumbBorderPaint?.Dispose();
            _thumbIndicatorPaint?.Dispose();
            _tickPaint?.Dispose();
            _tickTextPaint?.Dispose();
            _valueTextPaint?.Dispose();

            _thumbTrianglePath?.Dispose();
            _thumbDiamondPath?.Dispose();
            _thumbIndicatorPath?.Dispose();

            _tickFont?.Dispose();
            _valueFont?.Dispose();

            _thumbShadowFilter?.Dispose();
            _thumbFillShader?.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Variables

    private bool _isDragging;
    private bool _isHovered;
    private Point _mouseLocation;
    private readonly AnimationManager _thumbHoverAnimation;
    private readonly AnimationManager _thumbPressAnimation;
    private readonly AnimationManager _trackHoverAnimation;
    private readonly AnimationManager _valueAnimation;
    private readonly ToolTip _tooltip;

    private int _minimum;
    private int _maximum = 100;
    private int _value;
    private int _animatedValue;
    private int _smallChange = 1;
    private int _largeChange = 10;
    private bool _drawValueString;
    private bool _showTicks;
    private int _tickFrequency = 10;
    private ValueDivisor _dividedValue = ValueDivisor.By1;
    private Size _thumbSize = new(16, 16);
    private Orientation _orientation = Orientation.Horizontal;
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

    // Skia caches (to avoid per-frame allocations)
    private SKPaint? _trackEmptyPaint;
    private SKPaint? _trackFilledPaint;
    private SKPaint? _thumbShadowPaint;
    private SKPaint? _thumbFillPaint;
    private SKPaint? _thumbBorderPaint;
    private SKPaint? _thumbIndicatorPaint;
    private SKPaint? _tickPaint;
    private SKPaint? _tickTextPaint;
    private SKPaint? _valueTextPaint;

    private SKPath? _thumbTrianglePath;
    private SKPath? _thumbDiamondPath;
    private SKPath? _thumbIndicatorPath;

    private SKFont? _tickFont;
    private int _tickFontDpi;
    private SKFont? _valueFont;
    private Font? _valueFontSource;
    private int _valueFontDpi;

    private SKImageFilter? _thumbShadowFilter;
    private float _thumbShadowFilterScale;
    private SKShader? _thumbFillShader;

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

            if (ShowTooltip && _isDragging)
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

    [Category("Behavior")] public bool JumpToMouse { get; set; }

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
    public Orientation Orientation
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

    [Category("Behavior")] public bool ShowTooltip { get; set; } = true;

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
}