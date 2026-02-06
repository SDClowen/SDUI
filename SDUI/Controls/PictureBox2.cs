using System;
using System.ComponentModel;

using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using SkiaSharp;

namespace SDUI.Controls;

public class PictureBox2 : UIElementBase
{
    private ErrorImage _errorImage;
    private SKImage _image;
    private string _imageLocation;
    private bool _ownsImage;
    private PictureBoxSizeMode _sizeMode;
    private SKBitmap _skBitmap;

    public PictureBox2()
    {
        _sizeMode = PictureBoxSizeMode.Normal;
        WaitOnLoad = true;
        _errorImage = ErrorImage.NoImage;
        BackColor = SKColors.Transparent;
    }

    [DefaultValue(null)]
    [Category("Appearance")]
    public SKImage Image
    {
        get => _image;
        set
        {
            if (_image == value) return;

            if (_ownsImage) _image?.Dispose();

            _ownsImage = false;
            _image = value;
            LoadBitmap();
            OnImageChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    [DefaultValue("")]
    [Category("Appearance")]
    public string ImageLocation
    {
        get => _imageLocation;
        set
        {
            if (_imageLocation == value) return;
            _imageLocation = value;
            if (!string.IsNullOrEmpty(value))
                LoadAsync();
        }
    }

    [DefaultValue(PictureBoxSizeMode.Normal)]
    [Category("Appearance")]
    public PictureBoxSizeMode SizeMode
    {
        get => _sizeMode;
        set
        {
            if (_sizeMode == value) return;
            _sizeMode = value;
            Invalidate();
        }
    }

    [DefaultValue(true)]
    [Category("Behavior")]
    public bool WaitOnLoad { get; set; }

    [DefaultValue(ErrorImage.NoImage)]
    [Category("Appearance")]
    public ErrorImage ErrorImage
    {
        get => _errorImage;
        set
        {
            if (_errorImage == value) return;
            _errorImage = value;
            Invalidate();
        }
    }

    public event EventHandler LoadCompleted;
    public event EventHandler LoadFailed;

    public void Load()
    {
        LoadAsync();
    }

    public async void LoadAsync()
    {
        if (string.IsNullOrEmpty(_imageLocation))
            return;

        try
        {
            using var stream = new FileStream(_imageLocation, FileMode.Open, FileAccess.Read);

            if (_ownsImage) _image?.Dispose();
            _ownsImage = true;

            _image = Image.FromStream(stream);
            LoadBitmap();
            LoadCompleted?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
        catch
        {
            LoadFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void LoadBitmap()
    {
        _skBitmap?.Dispose();
        _skBitmap = null;

        if (_image == null) return;

        try
        {
            using var ms = new MemoryStream();
            _image.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            _skBitmap = SKBitmap.Decode(ms);
        }
        catch
        {
            _skBitmap = null;
        }
    }

    public override void OnPaint(SKCanvas canvas)
    {
        if (BackColor != SKColors.Transparent)
            canvas.DrawRect(0, 0, Width, Height, new SKPaint { Color = BackColor });

        if (_skBitmap == null)
        {
            DrawErrorImage(canvas);
            return;
        }

        var rect = CalculateImageRect();
        using var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        };

        canvas.DrawBitmap(_skBitmap, rect, paint);
        base.OnPaint(canvas);
    }

    private SkiaSharp.SKRect CalculateImageRect()
    {
        if (_skBitmap == null)
            return new SkiaSharp.SKRect(0, 0, Width, Height);

        float destX = 0, destY = 0;
        float destWidth = Width, destHeight = Height;

        switch (_sizeMode)
        {
            case PictureBoxSizeMode.Normal:
                destWidth = _skBitmap.Width;
                destHeight = _skBitmap.Height;
                break;

            case PictureBoxSizeMode.StretchImage:
                break;

            case PictureBoxSizeMode.AutoSize:
                Width = _skBitmap.Width;
                Height = _skBitmap.Height;
                destWidth = Width;
                destHeight = Height;
                break;

            case PictureBoxSizeMode.CenterImage:
                destWidth = _skBitmap.Width;
                destHeight = _skBitmap.Height;
                destX = (Width - destWidth) / 2;
                destY = (Height - destHeight) / 2;
                break;

            case PictureBoxSizeMode.Zoom:
                var ratio = Math.Min((float)Width / _skBitmap.Width, (float)Height / _skBitmap.Height);
                destWidth = _skBitmap.Width * ratio;
                destHeight = _skBitmap.Height * ratio;
                destX = (Width - destWidth) / 2;
                destY = (Height - destHeight) / 2;
                break;
        }

        return new SkiaSharp.SKRect(destX, destY, destX + destWidth, destY + destHeight);
    }

    private void DrawErrorImage(SKCanvas canvas)
    {
        if (_errorImage == ErrorImage.NoImage)
            return;

        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };

        // Basit bir X i�areti �iz
        canvas.DrawLine(0, 0, Width, Height, paint);
        canvas.DrawLine(Width, 0, 0, Height, paint);
    }

    protected virtual void OnImageChanged(EventArgs e)
    {
        if (_sizeMode == PictureBoxSizeMode.AutoSize)
            PerformLayout();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image?.Dispose();
            _skBitmap?.Dispose();
        }

        base.Dispose(disposing);
    }
}

public enum ErrorImage
{
    NoImage,
    DisplayError
}