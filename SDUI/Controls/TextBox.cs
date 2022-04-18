using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class TextBox : Control
    {
        private System.Windows.Forms.TextBox _textbox;

        private bool _passmask = false;
        public bool UseSystemPasswordChar
        {
            get { return _passmask; }
            set
            {
                _textbox.UseSystemPasswordChar = UseSystemPasswordChar;
                _passmask = value;
                Invalidate();
            }
        }

        private int _maxchars = 32767;
        public int MaxLength
        {
            get { return _maxchars; }
            set
            {
                _maxchars = value;
                _textbox.MaxLength = MaxLength;
                Invalidate();
            }
        }

        private HorizontalAlignment _align;
        public HorizontalAlignment TextAlignment
        {
            get { return _align; }
            set
            {
                _align = value;
                Invalidate();
            }
        }

        private bool _multiline = false;
        public bool MultiLine
        {
            get { return _multiline; }
            set
            {
                _multiline = value;
                Invalidate();
            }
        }

        protected override void OnTextChanged(System.EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnBackColorChanged(System.EventArgs e)
        {
            base.OnBackColorChanged(e);
            _textbox.BackColor = BackColor;
            Invalidate();
        }

        protected override void OnForeColorChanged(System.EventArgs e)
        {
            base.OnForeColorChanged(e);
            _textbox.ForeColor = ForeColor;
            Invalidate();
        }

        protected override void OnFontChanged(System.EventArgs e)
        {
            base.OnFontChanged(e);
            _textbox.Font = Font;
        }

        protected override void OnGotFocus(System.EventArgs e)
        {
            base.OnGotFocus(e);
            _textbox.Focus();
        }

        public TextBox()
        {
            _textbox = new System.Windows.Forms.TextBox();
            _textbox.Multiline = false;
            _textbox.Text = string.Empty;
            _textbox.TextAlign = HorizontalAlignment.Center;
            _textbox.BorderStyle = BorderStyle.None;
            _textbox.Location = new Point(3, 2);
            _textbox.Font = Font;
            _textbox.Size = new Size(Width - 10, Height - 11);
            _textbox.UseSystemPasswordChar = UseSystemPasswordChar;
            Controls.Add(_textbox);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(135, 35);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var bitmap = new Bitmap(Width, Height);
            var gfx = Graphics.FromImage(bitmap);
            gfx.SmoothingMode = SmoothingMode.HighQuality;

            var determinedColor = ColorScheme.BackColor.Determine();
            var backColor = ColorScheme.BackColor.Brightness(-.2f);

            Height = _textbox.Height + 5;
            var _with2 = _textbox;
            _with2.Width = Width - 10;
            _with2.TextAlign = TextAlignment;
            _with2.UseSystemPasswordChar = UseSystemPasswordChar;
            _with2.ForeColor = ColorScheme.ForeColor;
            _with2.BackColor = backColor;

            gfx.Clear(Color.Transparent);
            gfx.FillRectangle(new SolidBrush(backColor), new Rectangle(0, 0, Width - 1, Height - 1));

            var colorBegin = Color.FromArgb(90, determinedColor.Brightness(.5f));
            var colorEnd = Color.FromArgb(60, determinedColor.Brightness(-.5f));

            var innerBorderBrush = new LinearGradientBrush(new Rectangle(1, 1, Width - 3, Height - 3), colorBegin, colorEnd, 90);
            var innerBorderPen = new Pen(innerBorderBrush);

            gfx.DrawRectangle(innerBorderPen, new Rectangle(1, 1, Width - 3, Height - 3));
            gfx.DrawLine(new Pen(ColorScheme.BorderColor), new Point(1, 1), new Point(Width - 3, 1));

            e.Graphics.DrawImage((Bitmap)bitmap.Clone(), 0, 0);

            gfx.Dispose();
            bitmap.Dispose();
        }
    }
}