using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class PictureBox2 : UIElementBase
    {
        private Image _image;
        private PictureBoxSizeMode _sizeMode;
        private bool _waitOnLoad;
        private SKBitmap _skBitmap;
        private string _imageLocation;
        private ErrorImage _errorImage;

        public event EventHandler LoadCompleted;
        public event EventHandler LoadFailed;

        public PictureBox2()
        {
            _sizeMode = PictureBoxSizeMode.Normal;
            _waitOnLoad = true;
            _errorImage = ErrorImage.NoImage;
            BackColor = Color.Transparent;
        }

        [DefaultValue(null)]
        [Category("Appearance")]
        public Image Image
        {
            get => _image;
            set
            {
                if (_image == value) return;
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
        public bool WaitOnLoad
        {
            get => _waitOnLoad;
            set => _waitOnLoad = value;
        }

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

        public void Load() => LoadAsync();

        public async void LoadAsync()
        {
            if (string.IsNullOrEmpty(_imageLocation))
                return;

            try
            {
                using var stream = new FileStream(_imageLocation, FileMode.Open, FileAccess.Read);
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
            if (_image == null)
            {
                _skBitmap = null;
                return;
            }

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

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(BackColor.ToSKColor());

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
            base.OnPaint(e);
        }

        private SKRect CalculateImageRect()
        {
            if (_skBitmap == null)
                return new SKRect(0, 0, Width, Height);

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
                    float ratio = Math.Min((float)Width / _skBitmap.Width, (float)Height / _skBitmap.Height);
                    destWidth = _skBitmap.Width * ratio;
                    destHeight = _skBitmap.Height * ratio;
                    destX = (Width - destWidth) / 2;
                    destY = (Height - destHeight) / 2;
                    break;
            }

            return new SKRect(destX, destY, destX + destWidth, destY + destHeight);
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

            // Basit bir X iþareti çiz
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
}
