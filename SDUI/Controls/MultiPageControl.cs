using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace SDUI.Controls
{
    public class MultiPageControl : UIElementBase
    {
        private readonly List<Page> _pages = new();
        private int _selectedIndex = -1;

        // Chrome-like styles and animation
        private Color _headerBackColor = ColorScheme.BackColor;
        private Color _headerForeColor = ColorScheme.ForeColor;
        private Color _selectedTabColor = ColorScheme.BackColor;
        private Color _selectedTabForeColor = ColorScheme.AccentColor;
        private Color _borderColor = ColorScheme.BorderColor;
        private float _borderWidth = 1.0f;
        private float _cornerRadius = 8.0f;
        private int _headerHeight = 40;
        private int _tabGap = 6;
        private int _indicatorHeight = 3;
        private Size _headerControlSize = new Size(20, 20);
        private bool _renderNewPageButton = true;
        private bool _renderPageClose = true;
        private bool _renderPageIcon = false;

        private int _hoveredCloseButtonIndex = -1;
        private int _hoveredTabIndex = -1;
        private bool _isDragging;
        private Point _dragStartPoint;
        private int _draggedTabIndex = -1;
        private bool _isNewPageButtonHovered;

        private readonly SDUI.Animation.AnimationEngine _selectionAnim;
        private int _prevSelectedIndex = -1;
        private double _selectionAnimIncrement = 0.18;
        private SDUI.Animation.AnimationType _selectionAnimType = SDUI.Animation.AnimationType.EaseInOut;
        private readonly Dictionary<int, SDUI.Animation.AnimationEngine> _hoverAnims = new();
        private readonly List<RectangleF> _tabRects = new();

        public event EventHandler NewPageButtonClicked;
        public event EventHandler SelectedIndexChanged;

        public MultiPageControl()
        {
            Size = new Size(500, 320);
            BackColor = ColorScheme.BackColor;

            _selectionAnim = new SDUI.Animation.AnimationEngine(singular: true)
            {
                Increment = _selectionAnimIncrement,
                AnimationType = _selectionAnimType,
                InterruptAnimation = true
            };
            _selectionAnim.OnAnimationProgress += _ => Invalidate();
        }

        [Browsable(false)]
        public Page SelectedPage => _selectedIndex >= 0 && _selectedIndex < _pages.Count ? _pages[_selectedIndex] : null;

        [Browsable(false)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                if (value >= -1 && value < _pages.Count)
                {
                    _prevSelectedIndex = _selectedIndex;
                    var oldIndex = _selectedIndex;
                    _selectedIndex = value;
                    _selectionAnim.SetProgress(0);
                    _selectionAnim.StartNewAnimation(SDUI.Animation.AnimationDirection.In);
                    OnSelectedIndexChanged(oldIndex, _selectedIndex);
                    Invalidate();
                }
            }
        }

        // Appearance and behavior properties similar to TabControl (omitted attributes for brevity)
        public Color HeaderBackColor { get => _headerBackColor; set { if (_headerBackColor == value) return; _headerBackColor = value; Invalidate(); } }
        public Color HeaderForeColor { get => _headerForeColor; set { if (_headerForeColor == value) return; _headerForeColor = value; Invalidate(); } }
        public Color SelectedTabColor { get => _selectedTabColor; set { if (_selectedTabColor == value) return; _selectedTabColor = value; Invalidate(); } }
        public Color SelectedTabForeColor { get => _selectedTabForeColor; set { if (_selectedTabForeColor == value) return; _selectedTabForeColor = value; Invalidate(); } }
        public Color BorderColor { get => _borderColor; set { if (_borderColor == value) return; _borderColor = value; Invalidate(); } }
        public float BorderWidth { get => _borderWidth; set { if (_borderWidth == value) return; _borderWidth = value; Invalidate(); } }
        public float CornerRadius { get => _cornerRadius; set { if (_cornerRadius == value) return; _cornerRadius = value; Invalidate(); } }
        public int HeaderHeight { get => _headerHeight; set { if (_headerHeight == value) return; _headerHeight = value; Invalidate(); } }
        public int TabGap { get => _tabGap; set { if (_tabGap == value) return; _tabGap = Math.Max(0, value); Invalidate(); } }
        public int IndicatorHeight { get => _indicatorHeight; set { if (_indicatorHeight == value) return; _indicatorHeight = Math.Max(1, value); Invalidate(); } }
        public Size HeaderControlSize { get => _headerControlSize; set { if (_headerControlSize == value) return; _headerControlSize = value; Invalidate(); } }
        public bool RenderNewPageButton { get => _renderNewPageButton; set { if (_renderNewPageButton == value) return; _renderNewPageButton = value; Invalidate(); } }
        public bool RenderPageClose { get => _renderPageClose; set { if (_renderPageClose == value) return; _renderPageClose = value; Invalidate(); } }
        public bool RenderPageIcon { get => _renderPageIcon; set { if (_renderPageIcon == value) return; _renderPageIcon = value; Invalidate(); } }

        public void AddPage(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            _pages.Add(page);
            page.Parent = this;
            EnsureHoverAnim(_pages.Count - 1);
            if (_selectedIndex == -1) SelectedIndex = 0;
            Invalidate();
        }

        public void RemovePage(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            var index = _pages.IndexOf(page);
            if (index >= 0)
            {
                _pages.RemoveAt(index);
                page.Parent = null;
                if (_hoverAnims.ContainsKey(index)) _hoverAnims.Remove(index);
                if (_selectedIndex >= _pages.Count) SelectedIndex = _pages.Count - 1;
                else if (_pages.Count == 0) SelectedIndex = -1;
                Invalidate();
            }
        }

        public void RemovePageAt(int index)
        {
            if (index < 0 || index >= _pages.Count) throw new ArgumentOutOfRangeException(nameof(index));
            RemovePage(_pages[index]);
        }

        private void EnsureHoverAnim(int index)
        {
            if (!_hoverAnims.ContainsKey(index))
            {
                var ae = new SDUI.Animation.AnimationEngine(singular: true)
                {
                    Increment = 0.2,
                    AnimationType = SDUI.Animation.AnimationType.EaseInOut,
                    InterruptAnimation = true
                };
                ae.OnAnimationProgress += _ => Invalidate();
                _hoverAnims[index] = ae;
            }
        }

        private void UpdateTabRects(SKPaint measurePaint)
        {
            _tabRects.Clear();
            float x = _borderWidth;
            for (int i = 0; i < _pages.Count; i++)
            {
                var textWidth = measurePaint.MeasureText(_pages[i].Title);
                float tabWidth = 16 + textWidth + 16;
                if (RenderPageIcon) tabWidth += HeaderControlSize.Width;
                if (RenderPageClose) tabWidth += HeaderControlSize.Width;
                _tabRects.Add(new RectangleF(x, 0, tabWidth, HeaderHeight));
                x += tabWidth + _tabGap;
            }
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            base.OnPaint(e);
            var canvas = e.Surface.Canvas;
            var bounds = ClientRectangle;

            using var fontPaint = new SKPaint { TextSize = Font.Size.PtToPx(this), Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name), IsAntialias = true, SubpixelText = true };
            UpdateTabRects(fontPaint);

            using (var headerPaint = new SKPaint { Color = HeaderBackColor.ToSKColor(), IsAntialias = true })
                canvas.DrawRect(new SKRect(0, 0, bounds.Width, HeaderHeight), headerPaint);

            for (int i = 0; i < _pages.Count; i++)
            {
                EnsureHoverAnim(i);
                var rect = _tabRects[i];
                var isSelected = i == _selectedIndex;
                var isHovered = i == _hoveredTabIndex;
                if (isHovered) _hoverAnims[i].StartNewAnimation(SDUI.Animation.AnimationDirection.In);
                else _hoverAnims[i].StartNewAnimation(SDUI.Animation.AnimationDirection.Out);
                var hoverProgress = (float)_hoverAnims[i].GetProgress();

                using (var bgPaint = new SKPaint { IsAntialias = true })
                {
                    var baseColor = isSelected ? SelectedTabColor : HeaderBackColor;
                    var hoverColor = baseColor.BlendWith(ColorScheme.ForeColor, isSelected ? 0.05f : 0.12f);
                    var color = baseColor.BlendWith(hoverColor, hoverProgress);
                    bgPaint.Color = color.ToSKColor();
                    var r = new SKRect(rect.X, rect.Y + 4, rect.Right, rect.Bottom - 2);
                    canvas.DrawRoundRect(r, CornerRadius, CornerRadius, bgPaint);
                }

                using (var textPaint = new SKPaint { Color = (isSelected ? SelectedTabForeColor : HeaderForeColor).ToSKColor(), TextSize = Font.Size.PtToPx(this), Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name), IsAntialias = true, SubpixelText = true })
                {
                    float textX = rect.X + 16 + (RenderPageIcon ? HeaderControlSize.Width : 0);
                    float textY = rect.Y + HeaderHeight / 2f + textPaint.FontMetrics.CapHeight / 2f;
                    canvas.DrawText(_pages[i].Title, textX, textY, textPaint);
                }

                if (RenderPageClose)
                {
                    float midY = rect.Y + rect.Height / 2f;
                    var closeRect = new SKRect(rect.Right - HeaderControlSize.Width - 8, midY - HeaderControlSize.Height / 2f, rect.Right - 8, midY + HeaderControlSize.Height / 2f);
                    var isCloseHovered = i == _hoveredCloseButtonIndex;
                    using var linePaint = new SKPaint { Color = (isCloseHovered ? Color.FromArgb(255, 80, 80) : Color.FromArgb(150, 150, 150)).ToSKColor(), StrokeWidth = 2, IsAntialias = true };
                    canvas.DrawLine(closeRect.Left + 6, closeRect.Top + 6, closeRect.Right - 6, closeRect.Bottom - 6, linePaint);
                    canvas.DrawLine(closeRect.Right - 6, closeRect.Top + 6, closeRect.Left + 6, closeRect.Bottom - 6, linePaint);
                }
            }

            if (_selectedIndex >= 0 && _selectedIndex < _tabRects.Count)
            {
                var progress = (float)_selectionAnim.GetProgress();
                int prev = _prevSelectedIndex;
                if (prev < 0 || prev >= _tabRects.Count) prev = _selectedIndex;
                var fromRect = _tabRects[prev];
                var toRect = _tabRects[_selectedIndex];
                float x = fromRect.X + (toRect.X - fromRect.X) * progress;
                float w = fromRect.Width + (toRect.Width - fromRect.Width) * progress;
                using var indPaint = new SKPaint { Color = SelectedTabForeColor.ToSKColor(), IsAntialias = true };
                canvas.DrawRect(new SKRect(x + 12, HeaderHeight - IndicatorHeight, x + w - 12, HeaderHeight), indPaint);
            }

            using (var paint = new SKPaint { Color = BorderColor.ToSKColor(), IsAntialias = true, StrokeWidth = BorderWidth })
                canvas.DrawLine(0, HeaderHeight - 0.5f, bounds.Width, HeaderHeight - 0.5f, paint);
        }

        internal override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _hoveredTabIndex = GetTabIndexAtPoint(e.Location);
            _hoveredCloseButtonIndex = GetCloseButtonIndexAtPoint(e.Location);
            Invalidate();
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
                if (_hoveredCloseButtonIndex >= 0 && RenderPageClose) RemovePageAt(_hoveredCloseButtonIndex);
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
            for (int i = 0; i < _tabRects.Count; i++) if (_tabRects[i].Contains(point)) return i;
            return -1;
        }

        private int GetCloseButtonIndexAtPoint(Point point)
        {
            if (!RenderPageClose || point.Y > HeaderHeight) return -1;
            for (int i = 0; i < _tabRects.Count; i++)
            {
                var rect = _tabRects[i];
                var closeRect = new RectangleF(rect.Right - HeaderControlSize.Width - 8, rect.Y + (HeaderHeight - HeaderControlSize.Height) / 2f, HeaderControlSize.Width, HeaderControlSize.Height);
                if (closeRect.Contains(point)) return i;
            }
            return -1;
        }

        protected virtual void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            if (oldIndex >= 0 && oldIndex < _pages.Count) _pages[oldIndex].OnDeselected();
            if (newIndex >= 0 && newIndex < _pages.Count) _pages[newIndex].OnSelected();
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class Page : UIElementBase
    {
        private string _title = "Yeni Sayfa";
        [Category("Appearance")] public string Title { get => _title; set { if (_title == value) return; _title = value; if (Parent is MultiPageControl mpc) mpc.Invalidate(); } }
        internal virtual void OnSelected() => Selected?.Invoke(this, EventArgs.Empty);
        internal virtual void OnDeselected() => Deselected?.Invoke(this, EventArgs.Empty);
        public event EventHandler Selected;
        public event EventHandler Deselected;
    }
}