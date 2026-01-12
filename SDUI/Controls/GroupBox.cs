using System;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class GroupBox : UIElementBase
{
    private int _radius = 10;
    private int _shadowDepth = 4;
    private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;

    public GroupBox()
    {
        BackColor = Color.Transparent;
        Padding = new Padding(3, 8, 3, 3);
    }

    public ContentAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            if (_textAlign == value) return;
            _textAlign = value;
            Invalidate();
        }
    }

    public int ShadowDepth
    {
        get => _shadowDepth;
        set
        {
            if (_shadowDepth == value) return;
            _shadowDepth = value;
            Invalidate();
        }
    }

    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            Invalidate();
        }
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        // Debug �er�evesi
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

        var rect = new SKRect(0, 0, Width, Height);
        var inflate = _shadowDepth / 4f;
        rect.Inflate(-inflate, -inflate);
        var shadowRect = rect;

        // Ba�l�k �l��leri (padding uygulanm�� geni�lik)
        var titleHeight = Font.Height + 7;
        float titleX = Padding.Left;
        var titleWidth = Math.Max(0, rect.Width - Padding.Horizontal);
        var titleRect = new SKRect(titleX, 0, titleX + titleWidth, titleHeight);

        // G�lge �izimi
        using (var shadowMaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _shadowDepth / 2f))
        using (var paint = new SKPaint
               {
                   Color = SKColors.Black.WithAlpha(20),
                   IsAntialias = true,
                   MaskFilter = shadowMaskFilter
               })
        {
            canvas.DrawRoundRect(shadowRect, _radius, _radius, paint);
        }

        // Arka plan �izimi
        using (var paint = new SKPaint
               {
                   Color = ColorScheme.BackColor2.ToSKColor(),
                   IsAntialias = true,
                   Style = SKPaintStyle.Fill
               })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, paint);
        }

        // Ba�l�k alan� �izimi (padding uygulanm�� s�n�rlar i�inde)
        canvas.Save();
        canvas.ClipRect(titleRect);

        // Ba�l�k �izgisi
        using (var paint = new SKPaint
               {
                   Color = ColorScheme.BorderColor.ToSKColor(),
                   IsAntialias = true,
                   Style = SKPaintStyle.Stroke,
                   StrokeWidth = 1
               })
        {
            canvas.DrawLine(titleRect.Left, titleRect.Height - 1, titleRect.Right, titleRect.Height - 1, paint);
        }

        // Ba�l�k arka plan (hafif)
        using (var paint = new SKPaint
               {
                   Color = ColorScheme.BackColor2.ToSKColor().WithAlpha(15),
                   IsAntialias = true,
                   Style = SKPaintStyle.Fill
               })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, paint);
        }

        canvas.Restore();

        // Ba�l�k metni �izimi
        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Edging = SKFontEdging.SubpixelAntialias
            };

            using var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                IsAntialias = true
            };

            var textWidth = font.MeasureText(Text);
            var textY = titleRect.Height / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;
            float textX;

            switch (TextAlign)
            {
                case ContentAlignment.MiddleLeft:
                    textX = titleRect.Left;
                    break;
                case ContentAlignment.MiddleRight:
                    textX = titleRect.Right - textWidth;
                    break;
                case ContentAlignment.MiddleCenter:
                default:
                    textX = titleRect.Left + (titleWidth - textWidth) / 2f;
                    break;
            }

            TextRenderingHelper.DrawText(canvas, Text, textX, textY, SKTextAlign.Left, font, textPaint);
        }

        // �er�eve �izimi
        using (var paint = new SKPaint
               {
                   Color = ColorScheme.BorderColor.ToSKColor(),
                   IsAntialias = true,
                   Style = SKPaintStyle.Stroke,
                   StrokeWidth = 1
               })
        {
            canvas.DrawRoundRect(rect, _radius, _radius, paint);
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var preferredSize = base.GetPreferredSize(proposedSize);
        preferredSize.Width += _shadowDepth;
        preferredSize.Height += _shadowDepth;

        return preferredSize;
    }

    protected override void OnLayout(UILayoutEventArgs e)
    {
        if (Controls.Count == 0)
        {
            base.OnLayout(e);
            return;
        }

        // Title height for offset
        var titleHeight = Font.Height + 7;

        // Apply padding and title offset to child bounds
        var clientRect = new Rectangle(
            Padding.Left,
            Padding.Top + titleHeight,
            Width - Padding.Horizontal - _shadowDepth / 2,
            Height - Padding.Vertical - titleHeight - _shadowDepth / 2
        );

        // Use LayoutEngine to layout children within client area
        var remaining = clientRect;
        foreach (UIElementBase control in Controls)
        {
            if (!control.Visible)
                continue;
            PerformDefaultLayout(control, clientRect, ref remaining);
        }

        Invalidate();
    }
}
