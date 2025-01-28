﻿using SDUI.Extensions;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ChatBubble : SKControl
{
    private SizeF _textSize;

    [Category("Appearance")]
    public Color Color { get; set; } = ColorScheme.PrimaryColor;

    private float _radius = 12f;
    [Category("Appearance")]
    public float Radius
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

    private float _tailSize = 8f;
    [Category("Appearance")]
    public float TailSize
    {
        get => _tailSize;
        set
        {
            if (_tailSize == value)
                return;

            _tailSize = value;
            Invalidate();
        }
    }

    private bool _isIncoming = true;
    [Category("Appearance")]
    public bool IsIncoming
    {
        get => _isIncoming;
        set
        {
            if (_isIncoming == value)
                return;

            _isIncoming = value;
            Invalidate();
        }
    }

    public ChatBubble()
    {
        SetStyle(ControlStyles.Selectable, true);
        MinimumSize = new Size(32, 32);
        TextAlign = ContentAlignment.MiddleLeft;
        Padding = new Padding(12);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        using (var paint = new SKPaint())
        {
            paint.TextSize = Font.Size.PtToPx(this);
            paint.Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name, SKFontStyle.Normal);
            var metrics = paint.FontMetrics;
            _textSize = new SizeF(paint.MeasureText(Text), metrics.Descent - metrics.Ascent);
        }
        if (AutoSize)
            Size = GetPreferredSize(Size.Empty);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (string.IsNullOrEmpty(Text))
            return;

        // Baloncuk şeklini çiz
        var bubblePath = new SKPath();
        var rect = new SKRect(0, 0, Width, Height);

        // Tail (kuyruk) için alan bırak
        if (IsIncoming)
            rect.Left += _tailSize;
        else
            rect.Right -= _tailSize;

        // Ana baloncuk şekli
        bubblePath.AddRoundRect(rect, _radius, _radius);

        // Tail (kuyruk) şekli
        var tailPoints = new SKPoint[3];
        if (IsIncoming)
        {
            tailPoints[0] = new SKPoint(_tailSize, Height / 2 - _tailSize);
            tailPoints[1] = new SKPoint(0, Height / 2);
            tailPoints[2] = new SKPoint(_tailSize, Height / 2 + _tailSize);
        }
        else
        {
            tailPoints[0] = new SKPoint(Width - _tailSize, Height / 2 - _tailSize);
            tailPoints[1] = new SKPoint(Width, Height / 2);
            tailPoints[2] = new SKPoint(Width - _tailSize, Height / 2 + _tailSize);
        }
        bubblePath.MoveTo(tailPoints[0]);
        bubblePath.LineTo(tailPoints[1]);
        bubblePath.LineTo(tailPoints[2]);
        bubblePath.Close();

        // Baloncuğu çiz
        using (var paint = new SKPaint
        {
            Color = Color.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawPath(bubblePath, paint);
        }

        // Text çizimi
        using (var textPaint = canvas.CreateTextPaint(Font, ForeColor, this, TextAlign))
        {
            var x = textPaint.GetTextX(Width - Padding.Horizontal - (_tailSize * 2), textPaint.MeasureText(Text), TextAlign);
            var y = textPaint.GetTextY(Height - Padding.Vertical, TextAlign);

            // Text pozisyonunu tail'e göre ayarla
            x += IsIncoming ? _tailSize + Padding.Left : Padding.Left;

            if (AutoEllipsis)
            {
                var maxWidth = Width - Padding.Horizontal - (_tailSize * 2);
                canvas.DrawTextWithEllipsis(Text, textPaint, x, y, maxWidth);
            }
            else
            {
                canvas.DrawText(Text, x, y, textPaint);
            }
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var width = (int)Math.Ceiling(_textSize.Width) + Padding.Horizontal + (int)(_tailSize * 2);
        var height = (int)Math.Ceiling(_textSize.Height) + Padding.Vertical;
        
        // Minimum boyut kontrolü
        width = Math.Max(width, MinimumSize.Width);
        height = Math.Max(height, MinimumSize.Height);

        return new Size(width, height);
    }
}