using System;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class NumUpDown : UIElementBase
{
    private const int LONG_PRESS_TIMER_INTERVAL = 250;

    private const int SIZE = 20;
    private readonly Timer _longPressTimer = new();
    private readonly Font _symbolFont;
    private readonly AnimationManager downButtonHoverAnimation;
    private readonly AnimationManager downButtonPressAnimation;

    private readonly AnimationManager upButtonHoverAnimation;
    private readonly AnimationManager upButtonPressAnimation;
    private RectangleF _downButtonRect;

    private bool _inUpButton, _inDownButton;
    private bool _isUsingKeyboard;
    private decimal _max;
    private decimal _min;
    private Point _mouseLocation;
    private bool _upButtonPressed, _downButtonPressed;

    private RectangleF _upButtonRect;

    private decimal _value;

    public NumUpDown()
    {
        _min = 0;
        _max = 100;
        Size = new Size(80, 25);
        MinimumSize = Size;

        upButtonHoverAnimation = new AnimationManager
        {
            Increment = 0.08f,
            AnimationType = AnimationType.Linear
        };
        downButtonHoverAnimation = new AnimationManager
        {
            Increment = 0.08f,
            AnimationType = AnimationType.Linear
        };

        upButtonPressAnimation = new AnimationManager
        {
            Increment = 0.15f,
            AnimationType = AnimationType.Linear
        };
        downButtonPressAnimation = new AnimationManager
        {
            Increment = 0.15f,
            AnimationType = AnimationType.Linear
        };

        upButtonHoverAnimation.OnAnimationProgress += sender => Invalidate();
        downButtonHoverAnimation.OnAnimationProgress += sender => Invalidate();
        upButtonPressAnimation.OnAnimationProgress += sender => Invalidate();
        downButtonPressAnimation.OnAnimationProgress += sender => Invalidate();

        _longPressTimer.Tick += LongPressTimer_Tick;
        _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;

        _symbolFont = new Font("Segoe UI Symbol", 9f, FontStyle.Regular);

        UpdateButtonRects();
    }

    public decimal Value
    {
        get => _value;
        set
        {
            if ((value <= _max) & (value >= _min))
            {
                _value = value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }

            Invalidate();
        }
    }

    public decimal Minimum
    {
        get => _min;
        set
        {
            if (value < _max) _min = value;

            if (_value < _min)
                Value = _min;

            Invalidate();
        }
    }

    public decimal Maximum
    {
        get => _max;
        set
        {
            if (value > _min)
                _max = value;

            if (_value > _max)
                Value = _max;

            Invalidate();
        }
    }

    public event EventHandler ValueChanged;

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            base.Dispose(disposing);
            return;
        }

        if (disposing)
        {
            _longPressTimer.Stop();
            _longPressTimer.Tick -= LongPressTimer_Tick;
            _longPressTimer.Dispose();

            upButtonHoverAnimation.Dispose();
            downButtonHoverAnimation.Dispose();
            upButtonPressAnimation.Dispose();
            downButtonPressAnimation.Dispose();

            _symbolFont.Dispose();
        }

        base.Dispose(disposing);
    }

    private void UpdateButtonRects()
    {
        _upButtonRect = new RectangleF(Width - SIZE * ScaleFactor, 0, SIZE * ScaleFactor, Height / 2f);
        _downButtonRect = new RectangleF(Width - SIZE * ScaleFactor, Height / 2f, SIZE * ScaleFactor, Height / 2f);
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _mouseLocation = e.Location;

        var inUpButton = e.Location.InRect(_upButtonRect);
        var inDownButton = e.Location.InRect(_downButtonRect);

        if (inUpButton != _inUpButton)
        {
            _inUpButton = inUpButton;
            upButtonHoverAnimation.StartNewAnimation(_inUpButton ? AnimationDirection.In : AnimationDirection.Out);
        }

        if (inDownButton != _inDownButton)
        {
            _inDownButton = inDownButton;
            downButtonHoverAnimation.StartNewAnimation(_inDownButton ? AnimationDirection.In : AnimationDirection.Out);
        }

        // Mouse sürüklenip buton dışına çıktıysa timer'ı durdur ve animasyonu sıfırla
        if (_longPressTimer.Enabled)
        {
            var stillInUpButton = _upButtonPressed && inUpButton;
            var stillInDownButton = _downButtonPressed && inDownButton;

            if (!stillInUpButton && !stillInDownButton)
            {
                _longPressTimer.Stop();
                _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;

                // Press animasyonlarını sıfırla
                if (_upButtonPressed)
                {
                    _upButtonPressed = false;
                    upButtonPressAnimation.StartNewAnimation(AnimationDirection.Out);
                }

                if (_downButtonPressed)
                {
                    _downButtonPressed = false;
                    downButtonPressAnimation.StartNewAnimation(AnimationDirection.Out);
                }
            }
        }

        Invalidate();
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _inUpButton = _inDownButton = false;
        upButtonHoverAnimation.StartNewAnimation(AnimationDirection.Out);
        downButtonHoverAnimation.StartNewAnimation(AnimationDirection.Out);

        // Mouse kontrolden çıktıysa timer'ı durdur ve animasyonu sıfırla
        if (_longPressTimer.Enabled)
        {
            _longPressTimer.Stop();
            _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;
        }

        // Press animasyonlarını sıfırla
        if (_upButtonPressed)
        {
            _upButtonPressed = false;
            upButtonPressAnimation.StartNewAnimation(AnimationDirection.Out);
        }

        if (_downButtonPressed)
        {
            _downButtonPressed = false;
            downButtonPressAnimation.StartNewAnimation(AnimationDirection.Out);
        }

        Invalidate();
    }

    private void ClickButton()
    {
        if (_mouseLocation.InRect(_upButtonRect))
        {
            if (_value + 1 <= _max)
                Value++;
        }
        else if (_mouseLocation.InRect(_downButtonRect))
        {
            if (_value - 1 >= _min)
                Value--;
        }
        else
        {
            _isUsingKeyboard = !_isUsingKeyboard;
        }

        Focus();
        Invalidate();
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (_mouseLocation.InRect(_upButtonRect))
        {
            _upButtonPressed = true;
            upButtonPressAnimation.StartNewAnimation(AnimationDirection.In);
            _longPressTimer.Start();
        }
        else if (_mouseLocation.InRect(_downButtonRect))
        {
            _downButtonPressed = true;
            downButtonPressAnimation.StartNewAnimation(AnimationDirection.In);
            _longPressTimer.Start();
        }

        ClickButton();
    }

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (_upButtonPressed)
        {
            _upButtonPressed = false;
            upButtonPressAnimation.StartNewAnimation(AnimationDirection.Out);
        }

        if (_downButtonPressed)
        {
            _downButtonPressed = false;
            downButtonPressAnimation.StartNewAnimation(AnimationDirection.Out);
        }

        _longPressTimer.Stop();
        _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;
    }

    private void LongPressTimer_Tick(object sender, EventArgs e)
    {
        // Mouse hâlâ basılı butonun üzerindeyse devam et
        var stillInUpButton = _upButtonPressed && _mouseLocation.InRect(_upButtonRect);
        var stillInDownButton = _downButtonPressed && _mouseLocation.InRect(_downButtonRect);

        if (stillInUpButton || stillInDownButton)
        {
            ClickButton();

            if (_longPressTimer.Interval == LONG_PRESS_TIMER_INTERVAL)
                _longPressTimer.Interval = 50;
        }
        else
        {
            // Mouse sürüklendiyse durdur
            _longPressTimer.Stop();
            _longPressTimer.Interval = LONG_PRESS_TIMER_INTERVAL;
        }
    }

    internal override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        try
        {
            if (_isUsingKeyboard && _value < _max)
                Value = long.Parse(_value.ToString() + e.KeyChar.ToString());
        }
        catch (Exception)
        {
        }
    }

    internal override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.KeyCode == Keys.Back)
        {
            var tempVal = _value.ToString();
            tempVal = tempVal.Remove(Convert.ToInt32(tempVal.Length - 1));
            if (tempVal.Length == 0)
                tempVal = "0";

            Value = Convert.ToInt32(tempVal);
        }

        Invalidate();
    }

    internal override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (e.Delta > 0)
        {
            if (_value + 1 <= _max) Value++;
            Invalidate();
        }
        else
        {
            if (_value - 1 >= _min) Value--;
            Invalidate();
        }
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateButtonRects();
    }

    internal override void OnDpiChanged(float newDpi, float oldDpi)
    {
        base.OnDpiChanged(newDpi, oldDpi);
        UpdateButtonRects();
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        if (Width <= 0 || Height <= 0)
            return;

        // Arka plan çizimi
        using (var backColorBrush = new SKPaint { Color = ColorScheme.BackColor.Alpha(20).ToSKColor() })
        using (var path = new SKPath())
        {
            path.AddRoundRect(new SKRect(0, 0, Width, Height), 6 * ScaleFactor, 6 * ScaleFactor);
            canvas.DrawPath(path, backColorBrush);
        }

        // Kenarlık çizimi
        using (var borderPaint = new SKPaint
               {
                   Color = ColorScheme.BorderColor.Alpha(80).ToSKColor(),
                   Style = SKPaintStyle.Stroke,
                   StrokeWidth = 1f,
                   IsAntialias = true
               })
        using (var path = new SKPath())
        {
            path.AddRoundRect(new SKRect(0.5f, 0.5f, Width - 0.5f, Height - 0.5f), 6 * ScaleFactor, 6 * ScaleFactor);
            canvas.DrawPath(path, borderPaint);
        }

        // Buton ayraç çizgileri
        using (var linePaint = new SKPaint
               {
                   Color = ColorScheme.BorderColor.Alpha(80).ToSKColor(),
                   StrokeWidth = 1f,
                   IsAntialias = true
               })
        {
            canvas.DrawLine(_upButtonRect.X, 0, _upButtonRect.X, Height, linePaint);
            canvas.DrawLine(_upButtonRect.X, Height / 2f, Width, Height / 2f, linePaint);
        }

        // Buton hover ve press efektleri
        var upHoverProgress = upButtonHoverAnimation.GetProgress();
        var upPressProgress = upButtonPressAnimation.GetProgress();
        if (upHoverProgress > 0 || upPressProgress > 0)
        {
            using var buttonPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor()
                    .WithAlpha((byte)Math.Max(upHoverProgress * 30, upPressProgress * 80)),
                IsAntialias = true
            };

            using var path = new SKPath();
            var rect = _upButtonRect.ToSKRect();
            path.AddRoundRect(new SKRect(rect.Left + 1, rect.Top + 1, rect.Right - 1, rect.Bottom),
                6 * ScaleFactor, 0);
            canvas.DrawPath(path, buttonPaint);
        }

        var downHoverProgress = downButtonHoverAnimation.GetProgress();
        var downPressProgress = downButtonPressAnimation.GetProgress();
        if (downHoverProgress > 0 || downPressProgress > 0)
        {
            using var buttonPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor()
                    .WithAlpha((byte)Math.Max(downHoverProgress * 30, downPressProgress * 80)),
                IsAntialias = true
            };

            using var path = new SKPath();
            var rect = _downButtonRect.ToSKRect();
            path.AddRoundRect(new SKRect(rect.Left + 1, rect.Top, rect.Right - 1, rect.Bottom - 1),
                6 * ScaleFactor, 0);
            canvas.DrawPath(path, buttonPaint);
        }

        // Buton simgeleri
        using (var font = new SKFont
               {
                   Size = 9f.PtToPx(this),
                   Typeface = FontManager.GetSKTypeface(_symbolFont),
                   Subpixel = true
               })
        using (var textPaint = new SKPaint
               {
                   Color = ColorScheme.ForeColor.ToSKColor(),
                   IsAntialias = true
               })
        {
            var metrics = font.Metrics;
            var textHeight = Math.Abs(metrics.Ascent + metrics.Descent);

            // Up button
            var upColor = ColorScheme.ForeColor;
            if (_upButtonPressed)
                upColor = upColor.Alpha(255);
            else if (_inUpButton)
                upColor = upColor.Alpha(230);
            else
                upColor = upColor.Alpha(180);

            textPaint.Color = upColor.ToSKColor();
            TextRenderingHelper.DrawText(canvas,
                "▲",
                _upButtonRect.X + _upButtonRect.Width / 2,
                _upButtonRect.Height / 2 + textHeight / 3,
                SKTextAlign.Center,
                font,
                textPaint);

            // Down button
            var downColor = ColorScheme.ForeColor;
            if (_downButtonPressed)
                downColor = downColor.Alpha(255);
            else if (_inDownButton)
                downColor = downColor.Alpha(230);
            else
                downColor = downColor.Alpha(180);

            textPaint.Color = downColor.ToSKColor();
            TextRenderingHelper.DrawText(canvas,
                "▼",
                _downButtonRect.X + _downButtonRect.Width / 2,
                _downButtonRect.Height * 1.5f + textHeight / 3,
                SKTextAlign.Center, font, textPaint);
        }

        // Değer metni
        using (var font = new SKFont
               {
                   Size = Font.Size.PtToPx(this),
                   Typeface = FontManager.GetSKTypeface(Font),
                   Subpixel = true
               })
        using (var textPaint = new SKPaint
               {
                   Color = ColorScheme.ForeColor.ToSKColor(),
                   IsAntialias = true
               })
        {
            var textY = Height / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;

            TextRenderingHelper.DrawText(canvas,
                Value.ToString(),
                10 * ScaleFactor,
                textY,
                SKTextAlign.Left,
                font,
                textPaint);
        }
    }
}