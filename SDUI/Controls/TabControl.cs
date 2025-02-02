using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class TabControl : UIElementBase
    {
        public Size ItemSize { get; set; } = new Size(100, 23);
        private readonly List<TabPage> _pages = new();
        private int _selectedIndex = -1;
        private Color _headerBackColor = Color.FromArgb(240, 240, 240);
        private Color _headerForeColor = Color.FromArgb(68, 68, 68);
        private Color _selectedTabColor = Color.White;
        private Color _selectedTabForeColor = Color.FromArgb(0, 120, 215);
        private Color _borderColor = Color.FromArgb(171, 173, 179);
        private float _borderWidth = 1.0f;
        private float _cornerRadius = 4.0f;
        private int _headerHeight = 32;
        private bool _showCloseButton = true;
        private Color _closeButtonColor = Color.FromArgb(150, 150, 150);
        private Color _closeButtonHoverColor = Color.FromArgb(255, 80, 80);
        private int _hoveredCloseButtonIndex = -1;
        private int _hoveredTabIndex = -1;
        private bool _isDragging;
        private Point _dragStartPoint;
        private int _draggedTabIndex = -1;
        private Size _headerControlSize = new Size(24, 24);
        private bool _renderNewPageButton = true;
        private bool _renderPageClose = true;
        private bool _renderPageIcon = true;
        private bool _isNewPageButtonHovered;

        public event EventHandler NewPageButtonClicked;

        public TabControl()
        {
            Size = new Size(400, 300);
            BackColor = Color.White;
        }

        [Browsable(false)]
        public TabPage SelectedPage => _selectedIndex >= 0 && _selectedIndex < _pages.Count ? _pages[_selectedIndex] : null;

        [Browsable(false)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                if (value >= -1 && value < _pages.Count)
                {
                    var oldIndex = _selectedIndex;
                    _selectedIndex = value;
                    OnSelectedIndexChanged(oldIndex, _selectedIndex);
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        public Color HeaderBackColor
        {
            get => _headerBackColor;
            set
            {
                if (_headerBackColor == value) return;
                _headerBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color HeaderForeColor
        {
            get => _headerForeColor;
            set
            {
                if (_headerForeColor == value) return;
                _headerForeColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color SelectedTabColor
        {
            get => _selectedTabColor;
            set
            {
                if (_selectedTabColor == value) return;
                _selectedTabColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color SelectedTabForeColor
        {
            get => _selectedTabForeColor;
            set
            {
                if (_selectedTabForeColor == value) return;
                _selectedTabForeColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor == value) return;
                _borderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public float BorderWidth
        {
            get => _borderWidth;
            set
            {
                if (_borderWidth == value) return;
                _borderWidth = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public float CornerRadius
        {
            get => _cornerRadius;
            set
            {
                if (_cornerRadius == value) return;
                _cornerRadius = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public int HeaderHeight
        {
            get => _headerHeight;
            set
            {
                if (_headerHeight == value) return;
                _headerHeight = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public bool ShowCloseButton
        {
            get => _showCloseButton;
            set
            {
                if (_showCloseButton == value) return;
                _showCloseButton = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Size HeaderControlSize
        {
            get => _headerControlSize;
            set
            {
                if (_headerControlSize == value) return;
                _headerControlSize = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool RenderNewPageButton
        {
            get => _renderNewPageButton;
            set
            {
                if (_renderNewPageButton == value) return;
                _renderNewPageButton = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool RenderPageClose
        {
            get => _renderPageClose;
            set
            {
                if (_renderPageClose == value) return;
                _renderPageClose = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool RenderPageIcon
        {
            get => _renderPageIcon;
            set
            {
                if (_renderPageIcon == value) return;
                _renderPageIcon = value;
                Invalidate();
            }
        }

        public void AddPage(TabPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));

            _pages.Add(page);
            page.Parent = this;

            if (_selectedIndex == -1)
                SelectedIndex = 0;

            Invalidate();
        }

        public void RemovePage(TabPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));

            var index = _pages.IndexOf(page);
            if (index >= 0)
            {
                _pages.RemoveAt(index);
                page.Parent = null;

                if (_selectedIndex >= _pages.Count)
                    SelectedIndex = _pages.Count - 1;
                else if (_pages.Count == 0)
                    SelectedIndex = -1;

                Invalidate();
            }
        }

        public void RemovePageAt(int index)
        {
            if (index < 0 || index >= _pages.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var page = _pages[index];
            RemovePage(page);
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            base.OnPaint(e);

            var canvas = e.Surface.Canvas;
            var bounds = ClientRectangle;

            // Arka plan
            using (var paint = new SKPaint
            {
                Color = BackColor.ToSKColor(),
                IsAntialias = true
            })
            {
                canvas.DrawRoundRect(
                    new SKRect(0, 0, bounds.Width, bounds.Height),
                    CornerRadius, CornerRadius, paint);
            }

            // Header arka planı
            using (var paint = new SKPaint
            {
                Color = HeaderBackColor.ToSKColor(),
                IsAntialias = true
            })
            {
                canvas.DrawRoundRect(
                    new SKRect(0, 0, bounds.Width, HeaderHeight),
                    CornerRadius, CornerRadius, paint);
            }

            // Sekmeleri çiz
            var tabX = BorderWidth;
            for (int i = 0; i < _pages.Count; i++)
            {
                var page = _pages[i];
                var isSelected = i == _selectedIndex;
                var isHovered = i == _hoveredTabIndex;

                using (var paint = new SKPaint
                {
                    TextSize = Font.Size.PtToPx(this),
                    Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                    IsAntialias = true
                })
                {
                    // Tab genişliğini hesapla
                    var tabWidth = paint.MeasureText(page.Text) + 20;
                    if (RenderPageIcon) tabWidth += HeaderControlSize.Width;
                    if (RenderPageClose) tabWidth += HeaderControlSize.Width;

                    // Tab arka planı
                    using (var bgPaint = new SKPaint
                    {
                        Color = isSelected ? SelectedTabColor.ToSKColor() :
                               isHovered ? Color.FromArgb(250, 250, 250).ToSKColor() :
                               HeaderBackColor.ToSKColor(),
                        IsAntialias = true
                    })
                    {
                        var tabRect = new SKRect(tabX, 0, tabX + tabWidth, HeaderHeight);
                        canvas.DrawRoundRect(tabRect, CornerRadius, CornerRadius, bgPaint);
                    }

                    float textX = tabX + 10;

                    // Sayfa ikonu
                    if (RenderPageIcon)
                    {
                        var iconY = (HeaderHeight - HeaderControlSize.Height) / 2;
                        // Burada sayfa ikonu çizimi yapılabilir
                        textX += HeaderControlSize.Width;
                    }

                    // Tab başlığı
                    paint.Color = isSelected ? SelectedTabForeColor.ToSKColor() : HeaderForeColor.ToSKColor();
                    var textY = (HeaderHeight + paint.TextSize) / 2;
                    canvas.DrawText(page.Text, textX, textY, paint);

                    // Kapatma düğmesi
                    if (RenderPageClose)
                    {
                        var closeButtonX = tabX + tabWidth - HeaderControlSize.Width;
                        var closeButtonY = (HeaderHeight - HeaderControlSize.Height) / 2;
                        var isCloseHovered = i == _hoveredCloseButtonIndex;

                        using (var closePaint = new SKPaint
                        {
                            Color = isCloseHovered ? _closeButtonHoverColor.ToSKColor() : _closeButtonColor.ToSKColor(),
                            IsAntialias = true,
                            StrokeWidth = 2
                        })
                        {
                            canvas.DrawLine(
                                closeButtonX + 8, closeButtonY + 8,
                                closeButtonX + HeaderControlSize.Width - 8, closeButtonY + HeaderControlSize.Height - 8,
                                closePaint);
                            canvas.DrawLine(
                                closeButtonX + HeaderControlSize.Width - 8, closeButtonY + 8,
                                closeButtonX + 8, closeButtonY + HeaderControlSize.Height - 8,
                                closePaint);
                        }
                    }

                    tabX += tabWidth;
                }
            }

            // Yeni sayfa düğmesi
            if (RenderNewPageButton)
            {
                var newPageButtonX = tabX + 5;
                var newPageButtonY = (HeaderHeight - HeaderControlSize.Height) / 2;

                using (var paint = new SKPaint
                {
                    Color = _isNewPageButtonHovered ? SelectedTabForeColor.ToSKColor() : HeaderForeColor.ToSKColor(),
                    IsAntialias = true,
                    StrokeWidth = 2
                })
                {
                    // + işareti çiz
                    var centerX = newPageButtonX + HeaderControlSize.Width / 2;
                    var centerY = newPageButtonY + HeaderControlSize.Height / 2;
                    canvas.DrawLine(
                        centerX - 6, centerY,
                        centerX + 6, centerY,
                        paint);
                    canvas.DrawLine(
                        centerX, centerY - 6,
                        centerX, centerY + 6,
                        paint);
                }
            }

            // Alt çizgi
            using (var paint = new SKPaint
            {
                Color = BorderColor.ToSKColor(),
                IsAntialias = true,
                IsStroke = true,
                StrokeWidth = BorderWidth
            })
            {
                canvas.DrawLine(
                    0, HeaderHeight,
                    bounds.Width, HeaderHeight,
                    paint);
            }

            // Seçili sayfayı çiz
            if (SelectedPage != null)
            {
                var pageRect = new SKRect(
                    0, HeaderHeight,
                    bounds.Width, bounds.Height);

                using var surface = SKSurface.Create(e.Info);
                SelectedPage.OnPaint(new SKPaintSurfaceEventArgs(surface, new SKImageInfo(
                    (int)pageRect.Width,
                    (int)pageRect.Height)));

                var pageImage = surface.Snapshot();
                canvas.DrawImage(pageImage, pageRect.Left, pageRect.Top);
            }
        }

        internal override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var oldNewPageButtonHovered = _isNewPageButtonHovered;
            _isNewPageButtonHovered = IsPointInNewPageButton(e.Location);

            if (_isDragging && _draggedTabIndex >= 0)
            {
                // Sürükleme işlemi
                var dragDistance = e.X - _dragStartPoint.X;
                if (Math.Abs(dragDistance) > 10)
                {
                    var targetIndex = GetTabIndexAtPoint(e.Location);
                    if (targetIndex >= 0 && targetIndex != _draggedTabIndex)
                    {
                        var page = _pages[_draggedTabIndex];
                        _pages.RemoveAt(_draggedTabIndex);
                        _pages.Insert(targetIndex, page);
                        _draggedTabIndex = targetIndex;
                        if (_selectedIndex == _draggedTabIndex)
                            _selectedIndex = targetIndex;
                        Invalidate();
                    }
                }
            }
            else
            {
                // Hover efektleri
                var oldHoverTab = _hoveredTabIndex;
                var oldHoverClose = _hoveredCloseButtonIndex;

                _hoveredTabIndex = GetTabIndexAtPoint(e.Location);
                _hoveredCloseButtonIndex = GetCloseButtonIndexAtPoint(e.Location);

                if (oldHoverTab != _hoveredTabIndex ||
                    oldHoverClose != _hoveredCloseButtonIndex ||
                    oldNewPageButtonHovered != _isNewPageButtonHovered)
                {
                    Invalidate();
                }
            }
        }

        internal override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var tabIndex = GetTabIndexAtPoint(e.Location);
                if (tabIndex >= 0)
                {
                    _isDragging = true;
                    _dragStartPoint = e.Location;
                    _draggedTabIndex = tabIndex;
                    SelectedIndex = tabIndex;
                }
            }
        }

        internal override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (_hoveredCloseButtonIndex >= 0 && RenderPageClose)
                {
                    RemovePageAt(_hoveredCloseButtonIndex);
                }
                else if (_isNewPageButtonHovered && RenderNewPageButton)
                {
                    NewPageButtonClicked?.Invoke(this, EventArgs.Empty);
                }

                _isDragging = false;
                _draggedTabIndex = -1;
            }
        }

        internal override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _hoveredTabIndex = -1;
            _hoveredCloseButtonIndex = -1;
            _isNewPageButtonHovered = false;
            Invalidate();
        }

        private int GetTabIndexAtPoint(Point point)
        {
            if (point.Y > HeaderHeight) return -1;

            var x = BorderWidth;
            for (int i = 0; i < _pages.Count; i++)
            {
                using (var paint = new SKPaint
                {
                    TextSize = Font.Size.PtToPx(this),
                    Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name)
                })
                {
                    var tabWidth = paint.MeasureText(_pages[i].Text) + 20;
                    if (RenderPageIcon) tabWidth += HeaderControlSize.Width;
                    if (RenderPageClose) tabWidth += HeaderControlSize.Width;

                    if (point.X >= x && point.X < x + tabWidth)
                        return i;

                    x += tabWidth;
                }
            }

            return -1;
        }

        private int GetCloseButtonIndexAtPoint(Point point)
        {
            if (!RenderPageClose || point.Y > HeaderHeight) return -1;

            var x = BorderWidth;
            for (int i = 0; i < _pages.Count; i++)
            {
                using (var paint = new SKPaint
                {
                    TextSize = Font.Size.PtToPx(this),
                    Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name)
                })
                {
                    var tabWidth = paint.MeasureText(_pages[i].Text) + 20;
                    if (RenderPageIcon) tabWidth += HeaderControlSize.Width;
                    if (RenderPageClose) tabWidth += HeaderControlSize.Width;

                    var closeButtonX = x + tabWidth - HeaderControlSize.Width;
                    var closeButtonY = (HeaderHeight - HeaderControlSize.Height) / 2;

                    if (point.X >= closeButtonX && point.X <= closeButtonX + HeaderControlSize.Width &&
                        point.Y >= closeButtonY && point.Y <= closeButtonY + HeaderControlSize.Height)
                        return i;

                    x += tabWidth;
                }
            }

            return -1;
        }

        private bool IsPointInNewPageButton(Point point)
        {
            if (!RenderNewPageButton || point.Y > HeaderHeight) return false;

            var x = BorderWidth;
            foreach (var page in _pages)
            {
                using (var paint = new SKPaint
                {
                    TextSize = Font.Size.PtToPx(this),
                    Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name)
                })
                {
                    var tabWidth = paint.MeasureText(page.Text) + 20;
                    if (RenderPageIcon) tabWidth += HeaderControlSize.Width;
                    if (RenderPageClose) tabWidth += HeaderControlSize.Width;
                    x += tabWidth;
                }
            }

            var newPageButtonX = x + 5;
            var newPageButtonY = (HeaderHeight - HeaderControlSize.Height) / 2;
            var buttonRect = new Rectangle(
                (int)newPageButtonX,
                (int)newPageButtonY,
                HeaderControlSize.Width,
                HeaderControlSize.Height);

            return buttonRect.Contains(point);
        }

        protected virtual void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            if (oldIndex >= 0 && oldIndex < _pages.Count)
                _pages[oldIndex].OnDeselected();

            if (newIndex >= 0 && newIndex < _pages.Count)
                _pages[newIndex].OnSelected();

            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SelectedIndexChanged;
    }

    public class TabPage : UIElementBase
    {
        public Size ItemSize { get; set; } = new Size(100, 23);

        private string _title = "Yeni Sayfa";

        [Category("Appearance")]
        public new string Text
        {
            get => _title;
            set
            {
                if (_title == value) return;
                _title = value;
                if (Parent is MultiPageControl mpc)
                    mpc.Invalidate();
            }
        }

        internal virtual void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        internal virtual void OnDeselected()
        {
            Deselected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Selected;
        public event EventHandler Deselected;
    }
}