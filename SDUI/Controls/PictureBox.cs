using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SkiaSharp;

namespace SDUI.Controls;

public class PictureBox : UIElementBase
{
    private Padding _border;
    private Color _borderColor;
    private Image _image;
    private int _radius;
    private float _shadowDepth;
    private PictureBoxSizeMode _sizeMode;

    public PictureBox()
    {
        _sizeMode = PictureBoxSizeMode.Normal;
        _radius = 0;
        _border = new Padding(0);
        _borderColor = Color.Transparent;
        _shadowDepth = 0;
        BackColor = Color.Transparent;
    }

    [Category("Appearance")]
    public Image Image
    {
        get => _image;
        set
        {
            _image = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public PictureBoxSizeMode SizeMode
    {
        get => _sizeMode;
        set
        {
            _sizeMode = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
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

    [Category("Appearance")]
    public Padding Border
    {
        get => _border;
        set
        {
            if (_border == value)
                return;

            _border = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor == value)
                return;

            _borderColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
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

    public override void OnPaint(SKCanvas canvas)
    {
        var rect = new SKRect(0, 0, Width, Height);
        if (_shadowDepth > 0)
        {
            var blur = 3f;
            var margin = 2f;
            var rightBottomMargin = _shadowDepth + blur + margin;
            rect = new SKRect(margin, margin, Width - rightBottomMargin, Height - rightBottomMargin);
        }

        var color = BackColor == Color.Transparent ? ColorScheme.BackColor2 : BackColor;
        var borderColor = _borderColor == Color.Transparent ? ColorScheme.BorderColor : _borderColor;

        // Gölge çizimi
        if (_shadowDepth > 0)
        {
            using var shadowPaint = new SKPaint();
            try
            {
                shadowPaint.Color = SKColors.Black.WithAlpha(30);
                using var shadowFilter = SKImageFilter.CreateDropShadow(
                    _shadowDepth,
                    _shadowDepth,
                    3,
                    3,
                    SKColors.Black.WithAlpha(30));
                shadowPaint.ImageFilter = shadowFilter;
                shadowPaint.IsAntialias = true;

                if (_radius > 0)
                {
                    using var path = new SKPath();
                    path.AddRoundRect(rect, _radius * ScaleFactor, _radius * ScaleFactor);
                    canvas.DrawPath(path, shadowPaint);
                }
                else
                {
                    canvas.DrawRect(rect, shadowPaint);
                }
            }
            catch
            {
            }
        }

        // Panel arka planı
        using var paint = new SKPaint();
        try
        {
            paint.Color = color.ToSKColor();
            paint.IsAntialias = true;

            if (_radius > 0)
            {
                using var path = new SKPath();
                path.AddRoundRect(rect, _radius * ScaleFactor, _radius * ScaleFactor);
                canvas.DrawPath(path, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }
        catch
        {
        }

        // Kenarlık çizimi
        if (_border.All > 0 || _border.Left > 0 || _border.Top > 0 || _border.Right > 0 || _border.Bottom > 0)
            try
            {
                paint.Color = borderColor.ToSKColor();
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1;
                paint.IsAntialias = true;

                if (_radius > 0)
                {
                    using var path = new SKPath();
                    path.AddRoundRect(rect, _radius * ScaleFactor, _radius * ScaleFactor);

                    if (_border.All > 0)
                    {
                        paint.StrokeWidth = _border.All;
                        canvas.DrawPath(path, paint);
                    }
                    else
                    {
                        // Sol kenarlık
                        if (_border.Left > 0)
                        {
                            paint.StrokeWidth = _border.Left;
                            var left = new SKPath();
                            left.MoveTo(rect.Left + _radius * ScaleFactor, rect.Top);
                            left.LineTo(rect.Left + _radius * ScaleFactor, rect.Bottom);
                            canvas.DrawPath(left, paint);
                        }

                        // Üst kenarlık
                        if (_border.Top > 0)
                        {
                            paint.StrokeWidth = _border.Top;
                            var top = new SKPath();
                            top.MoveTo(rect.Left, rect.Top + _radius * ScaleFactor);
                            top.LineTo(rect.Right, rect.Top + _radius * ScaleFactor);
                            canvas.DrawPath(top, paint);
                        }

                        // Sağ kenarlık
                        if (_border.Right > 0)
                        {
                            paint.StrokeWidth = _border.Right;
                            var right = new SKPath();
                            right.MoveTo(rect.Right - _radius * ScaleFactor, rect.Top);
                            right.LineTo(rect.Right - _radius * ScaleFactor, rect.Bottom);
                            canvas.DrawPath(right, paint);
                        }

                        // Alt kenarlık
                        if (_border.Bottom > 0)
                        {
                            paint.StrokeWidth = _border.Bottom;
                            var bottom = new SKPath();
                            bottom.MoveTo(rect.Left, rect.Bottom - _radius * ScaleFactor);
                            bottom.LineTo(rect.Right, rect.Bottom - _radius * ScaleFactor);
                            canvas.DrawPath(bottom, paint);
                        }
                    }
                }
                else
                {
                    if (_border.All > 0)
                    {
                        paint.StrokeWidth = _border.All;
                        canvas.DrawRect(rect, paint);
                    }
                    else
                    {
                        // Sol kenarlık
                        if (_border.Left > 0)
                        {
                            paint.StrokeWidth = _border.Left;
                            canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Bottom, paint);
                        }

                        // Üst kenarlık
                        if (_border.Top > 0)
                        {
                            paint.StrokeWidth = _border.Top;
                            canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Top, paint);
                        }

                        // Sağ kenarlık
                        if (_border.Right > 0)
                        {
                            paint.StrokeWidth = _border.Right;
                            canvas.DrawLine(rect.Right, rect.Top, rect.Right, rect.Bottom, paint);
                        }

                        // Alt kenarlık
                        if (_border.Bottom > 0)
                        {
                            paint.StrokeWidth = _border.Bottom;
                            canvas.DrawLine(rect.Left, rect.Bottom, rect.Right, rect.Bottom, paint);
                        }
                    }
                }
            }
            catch
            {
            }

        // Resim çizimi
        if (_image != null)
        {
            var imageRect = GetImageRectangle();
            using var imagePaint = new SKPaint();
            using var skImage = new Bitmap(_image).ToSKImage();
            canvas.DrawImage(skImage, imageRect, imagePaint);
        }

        // Debug çerçevesi
        if (ColorScheme.DrawDebugBorders)
            try
            {
                paint.Color = SKColors.Red;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1;
                paint.IsAntialias = true;
                canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);
            }
            catch
            {
            }

        // Alt elementleri render et
        base.OnPaint(canvas);
    }

    private SKRect GetImageRectangle()
    {
        if (_image == null)
            return SKRect.Empty;

        var imageWidth = _image.Width;
        var imageHeight = _image.Height;
        var controlWidth = Width;
        var controlHeight = Height;

        switch (_sizeMode)
        {
            case PictureBoxSizeMode.Normal:
                return new SKRect(0, 0, imageWidth, imageHeight);
            case PictureBoxSizeMode.StretchImage:
                return new SKRect(0, 0, controlWidth, controlHeight);
            case PictureBoxSizeMode.AutoSize:
                Width = imageWidth;
                Height = imageHeight;
                return new SKRect(0, 0, imageWidth, imageHeight);
            case PictureBoxSizeMode.CenterImage:
                var x = (controlWidth - imageWidth) / 2;
                var y = (controlHeight - imageHeight) / 2;
                return new SKRect(x, y, x + imageWidth, y + imageHeight);
            case PictureBoxSizeMode.Zoom:
                var ratioX = (float)controlWidth / imageWidth;
                var ratioY = (float)controlHeight / imageHeight;
                var ratio = Math.Min(ratioX, ratioY);
                var newWidth = imageWidth * ratio;
                var newHeight = imageHeight * ratio;
                var newX = (controlWidth - newWidth) / 2;
                var newY = (controlHeight - newHeight) / 2;
                return new SKRect(newX, newY, newX + newWidth, newY + newHeight);
            default:
                return new SKRect(0, 0, imageWidth, imageHeight);
        }
    }
}