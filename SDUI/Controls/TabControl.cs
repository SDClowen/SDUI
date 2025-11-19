using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using SDUI.Animation;

namespace SDUI.Controls
{
    public class TabControl : UIElementBase
    {
        private readonly List<TabPage> _pages = new();
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
        private int _tabGap = 0; // Chrome'da gap yok, tab'lar bitiþik
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

        // Scroll support
        private float _scrollOffset = 0f;
        private float _maxScrollOffset = 0f;
        private bool _showLeftChevron = false;
        private bool _showRightChevron = false;
        private bool _leftChevronHovered = false;
        private bool _rightChevronHovered = false;
        private const float CHEVRON_WIDTH = 24f;

        // Add/Remove animations
        private readonly Dictionary<TabPage, AnimationManager> _addAnimations = new();
        private readonly Dictionary<TabPage, AnimationManager> _removeAnimations = new();

        private readonly AnimationManager _selectionAnim;
        private int _prevSelectedIndex = -1;
        private double _selectionAnimIncrement = 0.18;
        private AnimationType _selectionAnimType = AnimationType.EaseInOut;
        private readonly Dictionary<int, AnimationManager> _hoverAnims = new();
        private readonly List<RectangleF> _tabRects = new();

        public event EventHandler NewPageButtonClicked;
        public event EventHandler SelectedIndexChanged;

        public TabControl()
        {
            Size = new Size(500, 320);
            BackColor = ColorScheme.BackColor;

            // Modern, kontrastlý renkler - tema uyumlu
            _headerBackColor = ColorScheme.BackColor;
            _headerForeColor = ColorScheme.ForeColor;
            _selectedTabColor = ColorScheme.BackColor;
            _selectedTabForeColor = ColorScheme.AccentColor;
            _borderColor = ColorScheme.BorderColor;

            _selectionAnim = new AnimationManager(singular: true)
            {
                Increment = _selectionAnimIncrement,
                AnimationType = _selectionAnimType,
                InterruptAnimation = true
            };
            _selectionAnim.OnAnimationProgress += _ => Invalidate();
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
                    _prevSelectedIndex = _selectedIndex;
                    var oldIndex = _selectedIndex;
                    _selectedIndex = value;
                    _selectionAnim.SetProgress(0);
                    _selectionAnim.StartNewAnimation(AnimationDirection.In);
                    OnSelectedIndexChanged(oldIndex, _selectedIndex);
                    
                    // Scroll to selected tab
                    ScrollToTab(_selectedIndex);
                    
                    Invalidate();
                }
            }
        }

        // Appearance properties
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

        public void AddPage(TabPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            
            _pages.Add(page);
            Controls.Add(page);
            page.Visible = false;
            EnsureHoverAnim(_pages.Count - 1);
            
            // === DÜZGÜN AÇILIÞ ANÝMASYONU ===
            var addAnim = new AnimationManager(singular: true)
            {
                Increment = 0.12,
                AnimationType = AnimationType.EaseOut,
                InterruptAnimation = false
            };
            addAnim.OnAnimationProgress += _ => Invalidate();
            addAnim.OnAnimationFinished += _ => 
            {
                _addAnimations.Remove(page);
                Invalidate();
            };
            _addAnimations[page] = addAnim;
            addAnim.StartNewAnimation(AnimationDirection.In);
            
            if (_selectedIndex == -1) 
            {
                SelectedIndex = 0;
            }
            else
            {
                SelectedIndex = _pages.Count - 1;
            }
            
            Invalidate();
        }

        public void RemovePage(TabPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            var index = _pages.IndexOf(page);
            if (index >= 0)
            {
                // === SMOOTH KAPANIÞ - KASMA YOK ===
                var removeAnim = new AnimationManager(singular: true)
                {
                    Increment = 0.08,
                    AnimationType = AnimationType.EaseOut,
                    InterruptAnimation = false
                };
                
                removeAnim.OnAnimationProgress += _ => Invalidate();
                
                removeAnim.OnAnimationFinished += _ => 
                {
                    _pages.Remove(page);
                    Controls.Remove(page);
                    _removeAnimations.Remove(page);
                    
                    if (_hoverAnims.ContainsKey(index)) _hoverAnims.Remove(index);
                    if (_selectedIndex >= _pages.Count) SelectedIndex = _pages.Count - 1;
                    else if (_pages.Count == 0) SelectedIndex = -1;
                    
                    Invalidate();
                };
                
                _removeAnimations[page] = removeAnim;
                removeAnim.StartNewAnimation(AnimationDirection.Out);
                
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
                var ae = new AnimationManager(singular: true)
                {
                    Increment = 0.2,
                    AnimationType = AnimationType.EaseInOut,
                    InterruptAnimation = true
                };
                ae.OnAnimationProgress += _ => Invalidate();
                _hoverAnims[index] = ae;
            }
        }

        private void UpdateTabRects(SKPaint measurePaint)
        {
            _tabRects.Clear();

            float startX = _showLeftChevron ? CHEVRON_WIDTH + 4 : 4;
            float x = startX - _scrollOffset;
            float maxWidth = 200f;
            float minWidth = 120f;

            // Reserve right side for chevron (+ button moves with tabs)
            float rightReserved = _showRightChevron ? CHEVRON_WIDTH + 4 : 0;
            float endX = Width - rightReserved - 40;
            float availableWidth = Math.Max(0, endX - startX);

            // Calculate tab width
            float tabWidth = _pages.Count > 0 ? availableWidth / _pages.Count : availableWidth;
            tabWidth = Math.Clamp(tabWidth, minWidth, maxWidth);

            // Build rectangles - SIFIR MARGIN
            float totalWidth = 0f;
            for (int i = 0; i < _pages.Count; i++)
            {
                _tabRects.Add(new RectangleF(x, 0, tabWidth, HeaderHeight));
                x += tabWidth;  // SIFIR MARGIN - bitiþik
                totalWidth += tabWidth;
            }

            // Chevron overflow detection
            bool overflow = totalWidth > availableWidth;
            _maxScrollOffset = Math.Max(0, totalWidth - availableWidth);
            _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);

            _showLeftChevron = overflow && _scrollOffset > 1f;
            _showRightChevron = overflow && _scrollOffset < _maxScrollOffset - 1f;
        }

        private void ScrollToTab(int index)
        {
            if (index < 0 || index >= _tabRects.Count) return;
            
            var tabRect = _tabRects[index];
            float startX = _showLeftChevron ? CHEVRON_WIDTH : 0;
            float endX = Width - (_showRightChevron ? CHEVRON_WIDTH : 0) - 32;
            
            // Scroll if tab is not fully visible
            if (tabRect.Left < startX)
            {
                _scrollOffset -= (startX - tabRect.Left);
            }
            else if (tabRect.Right > endX)
            {
                _scrollOffset += (tabRect.Right - endX);
            }
            
            _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);
        }
        
        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            base.OnPaint(e);
            var canvas = e.Surface.Canvas;
            var bounds = ClientRectangle;

            canvas.Clear(BackColor.ToSKColor());

            using var fontPaint = new SKPaint 
            { 
                TextSize = Font.Size.PtToPx(this), 
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name), 
                IsAntialias = true, 
                SubpixelText = true 
            };
            
            UpdateTabRects(fontPaint);

            // === HEADER - Modern uyumlu ===
            var isDarkTheme = ColorScheme.BackColor.IsDark();
            var headerColor = isDarkTheme 
                ? Color.FromArgb(
                    Math.Min(255, ColorScheme.BackColor.R + 12),
                    Math.Min(255, ColorScheme.BackColor.G + 12),
                    Math.Min(255, ColorScheme.BackColor.B + 12)
                )
                : Color.FromArgb(
                    Math.Max(0, ColorScheme.BackColor.R - 6),
                    Math.Max(0, ColorScheme.BackColor.G - 6),
                    Math.Max(0, ColorScheme.BackColor.B - 6)
                );
            using (var headerPaint = new SKPaint { Color = headerColor.ToSKColor(), IsAntialias = true })
                canvas.DrawRect(new SKRect(0, 0, bounds.Width, HeaderHeight), headerPaint);

            // === TAB AREA CLIPPING ===
            canvas.Save();
            float clipStart = _showLeftChevron ? CHEVRON_WIDTH + 4 : 4;
            
            // + BUTTON VARSA VE SABÝTSE, onun soluna kadar clip et
            float clipEnd;
            if (_showRightChevron && RenderNewPageButton)
            {
                // Chevron varsa + button sabit, onun soluna kadar clip
                float buttonSize = 24f;
                float buttonX = Width - CHEVRON_WIDTH - buttonSize - 12;
                clipEnd = buttonX - 4;  // + button'dan 4px önce kes
            }
            else
            {
                // Normal: sað chevron veya kenara kadar
                clipEnd = Width - (_showRightChevron ? CHEVRON_WIDTH + 4 : 4);
            }
            
            canvas.ClipRect(new SKRect(clipStart, 0, clipEnd, HeaderHeight));

            // === DRAW TABS ===
            for (int i = 0; i < _pages.Count; i++)
            {
                var page = _pages[i];
                
                // Animation check
                float animScale = 1f;
                float animAlpha = 1f;
                
                if (_addAnimations.ContainsKey(page))
                {
                    animScale = (float)_addAnimations[page].GetProgress();
                    animAlpha = animScale;
                }
                
                if (_removeAnimations.ContainsKey(page))
                {
                    animScale = 1f - (float)_removeAnimations[page].GetProgress();
                    animAlpha = animScale;
                    if (animScale < 0.1f) continue;
                }
                
                EnsureHoverAnim(i);
                var rect = _tabRects[i];
                var isSelected = i == _selectedIndex;
                var isHovered = i == _hoveredTabIndex && !isSelected;
                
                if (isHovered) _hoverAnims[i].StartNewAnimation(AnimationDirection.In);
                else _hoverAnims[i].StartNewAnimation(AnimationDirection.Out);
                var hoverProgress = (float)_hoverAnims[i].GetProgress();

                // === TAB RECTANGLE ===
                float tabPadding = 1f;
                float tabHeight = 28f;
                float verticalMargin = (HeaderHeight - tabHeight) / 2f;
                float topRadius = 6f * ScaleFactor;
                
                var tabRect = new SKRect(
                    rect.Left + tabPadding, 
                    verticalMargin, 
                    rect.Right - tabPadding, 
                    HeaderHeight - verticalMargin
                );

                if (animScale < 1f)
                {
                    float targetWidth = tabRect.Width * animScale;
                    tabRect.Right = tabRect.Left + targetWidth;
                }

                // === TAB BACKGROUND ===
                using (var bgPaint = new SKPaint { IsAntialias = true })
                {
                    float selectionProgress = 0f;
                    if (i == _selectedIndex)
                    {
                        selectionProgress = (float)_selectionAnim.GetProgress();
                    }
                    else if (i == _prevSelectedIndex && _selectionAnim.IsAnimating())
                    {
                        selectionProgress = 1f - (float)_selectionAnim.GetProgress();
                    }
                    
                    if (isSelected || (i == _prevSelectedIndex && _selectionAnim.IsAnimating()))
                    {
                        // Seçili tab: BackColor (belirgin)
                        var selectedColor = ColorScheme.BackColor;
                        var headerBg = headerColor;
                        
                        var blendedColor = Color.FromArgb(
                            (int)(headerBg.R + (selectedColor.R - headerBg.R) * selectionProgress),
                            (int)(headerBg.G + (selectedColor.G - headerBg.G) * selectionProgress),
                            (int)(headerBg.B + (selectedColor.B - headerBg.B) * selectionProgress)
                        );
                        
                        bgPaint.Color = Color.FromArgb((int)(255 * animAlpha), blendedColor.R, blendedColor.G, blendedColor.B).ToSKColor();
                    }
                    else
                    {
                        // Seçili olmayan: header rengi + hover efekti
                        var baseBg = headerColor;
                        
                        var hoverBg = isDarkTheme
                            ? Color.FromArgb(
                                Math.Min(255, ColorScheme.BackColor.R + 20),
                                Math.Min(255, ColorScheme.BackColor.G + 20),
                                Math.Min(255, ColorScheme.BackColor.B + 20)
                            )
                            : Color.FromArgb(
                                Math.Max(0, ColorScheme.BackColor.R - 12),
                                Math.Max(0, ColorScheme.BackColor.G - 12),
                                Math.Max(0, ColorScheme.BackColor.B - 12)
                            );
                        
                        var blended = Color.FromArgb(
                            (int)(baseBg.R + (hoverBg.R - baseBg.R) * hoverProgress),
                            (int)(baseBg.G + (hoverBg.G - baseBg.G) * hoverProgress),
                            (int)(baseBg.B + (hoverBg.B - baseBg.B) * hoverProgress)
                        );
                        bgPaint.Color = Color.FromArgb((int)(255 * animAlpha), blended.R, blended.G, blended.B).ToSKColor();
                    }
                    
                    canvas.DrawRoundRect(tabRect, topRadius, topRadius, bgPaint);
                }

                // === SUBTLE BORDER - daha belirgin ===
                if (isSelected)
                {
                    using var borderPaint = new SKPaint 
                    { 
                        Color = Color.FromArgb((int)(255 * animAlpha), ColorScheme.BorderColor.R, ColorScheme.BorderColor.G, ColorScheme.BorderColor.B).ToSKColor(), 
                        Style = SKPaintStyle.Stroke, 
                        StrokeWidth = 1f, 
                        IsAntialias = true 
                    };
                    canvas.DrawRoundRect(tabRect, topRadius, topRadius, borderPaint);
                }
               
                // === TAB TEXT ===
                float textSelectionProgress = 0f;
                if (i == _selectedIndex)
                {
                    textSelectionProgress = (float)_selectionAnim.GetProgress();
                }
                else if (i == _prevSelectedIndex && _selectionAnim.IsAnimating())
                {
                    textSelectionProgress = 1f - (float)_selectionAnim.GetProgress();
                }
                
                var selectedTextColor = ColorScheme.ForeColor;
                var unselectedTextColor = Color.FromArgb(
                    Math.Max(0, (int)(ColorScheme.ForeColor.R * 0.6)),
                    Math.Max(0, (int)(ColorScheme.ForeColor.G * 0.6)),
                    Math.Max(0, (int)(ColorScheme.ForeColor.B * 0.6))
                );
                
                Color textColor;
                if (isSelected || (i == _prevSelectedIndex && _selectionAnim.IsAnimating()))
                {
                    textColor = Color.FromArgb(
                        (int)(unselectedTextColor.R + (selectedTextColor.R - unselectedTextColor.R) * textSelectionProgress),
                        (int)(unselectedTextColor.G + (selectedTextColor.G - unselectedTextColor.G) * textSelectionProgress),
                        (int)(unselectedTextColor.B + (selectedTextColor.B - unselectedTextColor.B) * textSelectionProgress)
                    );
                }
                else
                {
                    textColor = unselectedTextColor;
                }
                    
                using (var textPaint = new SKPaint 
                { 
                    Color = Color.FromArgb((int)(255 * animAlpha), textColor.R, textColor.G, textColor.B).ToSKColor(), 
                    TextSize = 13f * ScaleFactor, 
                    Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 
                    IsAntialias = true, 
                    SubpixelText = true,
                    LcdRenderText = true
                })
                {
                    float textX = tabRect.Left + 16;
                    float textY = tabRect.MidY + textPaint.FontMetrics.CapHeight / 2f;
                    float maxTextWidth = tabRect.Width - 32 - (RenderPageClose ? 24 : 0);
                    canvas.DrawTextWithEllipsis(_pages[i].Text, textPaint, textX, textY, maxTextWidth);
                }

                // === CLOSE BUTTON ===
                if (RenderPageClose && (isSelected || isHovered || i == _hoveredCloseButtonIndex) && animAlpha > 0.3f)
                {
                    float closeSize = 16f;
                    float closeX = tabRect.Right - closeSize - 8;
                    float closeY = tabRect.MidY - closeSize / 2;
                    var closeRect = new SKRect(closeX, closeY, closeX + closeSize, closeY + closeSize);
                    var isCloseHovered = i == _hoveredCloseButtonIndex;

                    if (isCloseHovered)
                    {
                        var circleColor = isDarkTheme
                            ? Color.FromArgb(200, 255, 255, 255)
                            : Color.FromArgb(180, 0, 0, 0);
                            
                        using var closeBgPaint = new SKPaint 
                        { 
                            Color = Color.FromArgb((int)(circleColor.A * animAlpha), circleColor.R, circleColor.G, circleColor.B).ToSKColor(), 
                            IsAntialias = true
                        };
                        canvas.DrawCircle(closeRect.MidX, closeRect.MidY, closeSize / 2, closeBgPaint);
                    }

                    var lineColor = isCloseHovered 
                        ? (isDarkTheme ? Color.FromArgb(30, 30, 30) : Color.FromArgb(255, 255, 255))
                        : ColorScheme.ForeColor;
                        
                    using var linePaint = new SKPaint 
                    { 
                        Color = Color.FromArgb((int)(255 * animAlpha), lineColor.R, lineColor.G, lineColor.B).ToSKColor(), 
                        StrokeWidth = 1.2f, 
                        IsAntialias = true,
                        StrokeCap = SKStrokeCap.Round
                    };
                    
                    float padding = 4.5f;
                    canvas.DrawLine(closeRect.Left + padding, closeRect.Top + padding, closeRect.Right - padding, closeRect.Bottom - padding, linePaint);
                    canvas.DrawLine(closeRect.Right - padding, closeRect.Top + padding, closeRect.Left + padding, closeRect.Bottom - padding, linePaint);
                }
            }

            canvas.Restore();

            DrawChevrons(canvas, bounds);

            using (var borderPaint = new SKPaint { Color = ColorScheme.BorderColor.ToSKColor(), StrokeWidth = 1f })
                canvas.DrawLine(0, HeaderHeight - 1, bounds.Width, HeaderHeight - 1, borderPaint);

            DrawNewPageButton(canvas, bounds);

            using (var contentBgPaint = new SKPaint { Color = ColorScheme.BackColor.ToSKColor(), IsAntialias = true })
                canvas.DrawRect(new SKRect(0, HeaderHeight, bounds.Width, bounds.Height), contentBgPaint);

            if (_selectedIndex >= 0 && _selectedIndex < _pages.Count)
            {
                var selectedPage = _pages[_selectedIndex];
                if (selectedPage.Visible)
                {
                    selectedPage.Location = new Point(0, HeaderHeight);
                    selectedPage.Size = new Size(bounds.Width, bounds.Height - HeaderHeight);
                    
                    canvas.Save();
                    canvas.Translate(0, HeaderHeight);
                    var snapshot = selectedPage.RenderSnapshot();
                    if (snapshot != null) canvas.DrawImage(snapshot, 0, 0);
                    canvas.Restore();
                }
            }
        }

        internal override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            _hoveredTabIndex = GetTabIndexAtPoint(e.Location);
            _hoveredCloseButtonIndex = GetCloseButtonIndexAtPoint(e.Location);
            
            bool wasLeftHovered = _leftChevronHovered;
            bool wasRightHovered = _rightChevronHovered;

            var leftArea = new RectangleF(0, 0, CHEVRON_WIDTH, HeaderHeight);
            var rightArea = new RectangleF(Width - CHEVRON_WIDTH, 0, CHEVRON_WIDTH, HeaderHeight);

            _leftChevronHovered = _showLeftChevron && leftArea.Contains(e.Location);
            _rightChevronHovered = _showRightChevron && rightArea.Contains(e.Location);

            if (RenderNewPageButton)
            {
                var buttonSize = 24f;
                float buttonX;

                if (_showRightChevron)
                {
                    buttonX = Width - CHEVRON_WIDTH - buttonSize - 12;
                }
                else if (_tabRects.Count > 0)
                {
                    var lastTab = _tabRects[_tabRects.Count - 1];
                    buttonX = lastTab.Right + 8f;
                }
                else
                {
                    buttonX = _showLeftChevron ? CHEVRON_WIDTH + 8 : 8;
                }

                var buttonRect = new RectangleF(buttonX, (HeaderHeight - buttonSize) / 2f, buttonSize, buttonSize);
                var wasHovered = _isNewPageButtonHovered;
                _isNewPageButtonHovered = buttonRect.Contains(e.Location);
                if (wasHovered != _isNewPageButtonHovered) Invalidate();
            }

            if (wasLeftHovered != _leftChevronHovered || wasRightHovered != _rightChevronHovered)
                Invalidate();
            
            Invalidate();
        }

        internal override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Focus();
            
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (_leftChevronHovered && _showLeftChevron)
                {
                    _scrollOffset = Math.Max(0, _scrollOffset - 100);
                    Invalidate();
                    return;
                }
                
                if (_rightChevronHovered && _showRightChevron)
                {
                    _scrollOffset = Math.Min(_maxScrollOffset, _scrollOffset + 100);
                    Invalidate();
                    return;
                }
                
                if (RenderNewPageButton && _isNewPageButtonHovered)
                {
                    NewPageButtonClicked?.Invoke(this, EventArgs.Empty);
                    return;
                }
                
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
                    RemovePageAt(_hoveredCloseButtonIndex);
                
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
            _leftChevronHovered = false;
            _rightChevronHovered = false;
            Invalidate();
        }

        internal override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            
            if (e.Y < HeaderHeight && (_showLeftChevron || _showRightChevron))
            {
                _scrollOffset -= e.Delta / 4f;
                _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);
                Invalidate();
            }
        }

        private int GetTabIndexAtPoint(Point point)
        {
            if (point.Y > HeaderHeight) return -1;
            
            for (int i = 0; i < _tabRects.Count; i++) 
            {
                if (_tabRects[i].Contains(point)) 
                    return i;
            }
            return -1;
        }

        private int GetCloseButtonIndexAtPoint(Point point)
        {
            if (!RenderPageClose || point.Y > HeaderHeight) return -1;
            
            for (int i = 0; i < _tabRects.Count; i++)
            {
                var rect = _tabRects[i];
                
                float tabPadding = 1f;
                float tabHeight = 28f;
                float verticalMargin = (HeaderHeight - tabHeight) / 2f;
                
                var tabRect = new SKRect(
                    rect.Left + tabPadding, 
                    verticalMargin, 
                    rect.Right - tabPadding, 
                    HeaderHeight - verticalMargin
                );
                
                float closeSize = 16f;
                float closeX = tabRect.Right - closeSize - 8;
                float closeY = tabRect.MidY - closeSize / 2;
                var closeRect = new RectangleF(closeX, closeY, closeSize, closeSize);
                
                if (closeRect.Contains(point)) 
                    return i;
            }
            return -1;
        }

        protected virtual void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            if (oldIndex >= 0 && oldIndex < _pages.Count) _pages[oldIndex].OnDeselected();
            if (newIndex >= 0 && newIndex < _pages.Count) _pages[newIndex].OnSelected();
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DrawChevrons(SkiaSharp.SKCanvas canvas, Rectangle bounds)
        {
            var isDarkTheme = ColorScheme.BackColor.IsDark();
            var chevBg = isDarkTheme 
                ? Color.FromArgb(
                    Math.Min(255, ColorScheme.BackColor.R + 12),
                    Math.Min(255, ColorScheme.BackColor.G + 12),
                    Math.Min(255, ColorScheme.BackColor.B + 12)
                )
                : Color.FromArgb(
                    Math.Max(0, ColorScheme.BackColor.R - 6),
                    Math.Max(0, ColorScheme.BackColor.G - 6),
                    Math.Max(0, ColorScheme.BackColor.B - 6)
                );

            if (_showLeftChevron)
            {
                var leftRect = new SKRect(0, 0, CHEVRON_WIDTH, HeaderHeight);
                using var bg = new SKPaint { Color = chevBg.ToSKColor(), IsAntialias = true };
                canvas.DrawRect(leftRect, bg);

                var chevColor = _leftChevronHovered 
                    ? ColorScheme.AccentColor
                    : Color.FromArgb(
                        Math.Max(0, (int)(ColorScheme.ForeColor.R * 0.55)),
                        Math.Max(0, (int)(ColorScheme.ForeColor.G * 0.55)),
                        Math.Max(0, (int)(ColorScheme.ForeColor.B * 0.55))
                    );
                    
                using var p = new SKPaint 
                { 
                    Color = chevColor.ToSKColor(), 
                    StrokeWidth = 2.5f, 
                    IsAntialias = true, 
                    Style = SKPaintStyle.Stroke, 
                    StrokeCap = SKStrokeCap.Round 
                };
                
                var mid = new SKPoint(leftRect.MidX, leftRect.MidY);
                canvas.DrawLine(mid.X + 4, mid.Y - 6, mid.X - 2, mid.Y, p);
                canvas.DrawLine(mid.X - 2, mid.Y, mid.X + 4, mid.Y + 6, p);
            }

            if (_showRightChevron)
            {
                var rightRect = new SKRect(bounds.Width - CHEVRON_WIDTH, 0, bounds.Width, HeaderHeight);
                using var bg2 = new SKPaint { Color = chevBg.ToSKColor(), IsAntialias = true };
                canvas.DrawRect(rightRect, bg2);

                var chevColor = _rightChevronHovered 
                    ? ColorScheme.AccentColor
                    : Color.FromArgb(
                        Math.Max(0, (int)(ColorScheme.ForeColor.R * 0.55)),
                        Math.Max(0, (int)(ColorScheme.ForeColor.G * 0.55)),
                        Math.Max(0, (int)(ColorScheme.ForeColor.B * 0.55))
                    );
                    
                using var p2 = new SKPaint 
                { 
                    Color = chevColor.ToSKColor(), 
                    StrokeWidth = 2.5f, 
                    IsAntialias = true, 
                    Style = SKPaintStyle.Stroke, 
                    StrokeCap = SKStrokeCap.Round 
                };
                
                var mid2 = new SKPoint(rightRect.MidX, rightRect.MidY);
                canvas.DrawLine(mid2.X - 4, mid2.Y - 6, mid2.X + 2, mid2.Y, p2);
                canvas.DrawLine(mid2.X + 2, mid2.Y, mid2.X - 4, mid2.Y + 6, p2);
            }
        }

        private void DrawNewPageButton(SkiaSharp.SKCanvas canvas, Rectangle bounds)
        {
            if (!RenderNewPageButton) return;

            float buttonSize = 24f;
            float buttonX;
            float buttonY = (HeaderHeight - buttonSize) / 2f;

            if (_showRightChevron)
            {
                buttonX = bounds.Width - CHEVRON_WIDTH - buttonSize - 12;
            }
            else if (_tabRects.Count > 0)
            {
                var lastTab = _tabRects[_tabRects.Count - 1];
                buttonX = lastTab.Right + 8f;
            }
            else
            {
                buttonX = _showLeftChevron ? CHEVRON_WIDTH + 8 : 8;
            }

            var buttonRect = new SKRect(buttonX, buttonY, buttonX + buttonSize, buttonY + buttonSize);

            if (_isNewPageButtonHovered)
            {
                var isDarkTheme = ColorScheme.BackColor.IsDark();
                var btnBg = isDarkTheme 
                    ? Color.FromArgb(
                        Math.Min(255, ColorScheme.BackColor.R + 25),
                        Math.Min(255, ColorScheme.BackColor.G + 25),
                        Math.Min(255, ColorScheme.BackColor.B + 25)
                    )
                    : Color.FromArgb(
                        Math.Max(0, ColorScheme.BackColor.R - 20),
                        Math.Max(0, ColorScheme.BackColor.G - 20),
                        Math.Max(0, ColorScheme.BackColor.B - 20)
                    );
                using var bgPaint = new SKPaint { Color = btnBg.ToSKColor(), IsAntialias = true };
                canvas.DrawRect(buttonRect, bgPaint);
            }

            var iconColor = _isNewPageButtonHovered 
                ? ColorScheme.AccentColor
                : ColorScheme.ForeColor;
                
            using var plusPaint = new SKPaint
            {
                Color = iconColor.ToSKColor(),
                StrokeWidth = 2f,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            var cx = buttonRect.MidX;
            var cy = buttonRect.MidY;
            var iconSize = 6f;
            canvas.DrawLine(cx - iconSize, cy, cx + iconSize, cy, plusPaint);
            canvas.DrawLine(cx, cy - iconSize, cx, cy + iconSize, plusPaint);
        }
    }

    public class TabPage : UIElementBase
    {
        private string _text = "Yeni Sayfa";
        
        public TabPage()
        {
            Visible = true;
            BackColor = ColorScheme.BackColor;
            Size = new Size(400, 260);
        }

        [Category("Appearance")] 
        public override string Text 
        { 
            get => _text; 
            set 
            { 
                if (_text == value) return; 
                _text = value; 
                if (Parent is TabControl tc) tc.Invalidate(); 
            } 
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            if (BackColor != Color.Transparent)
            {
                var canvas = e.Surface.Canvas;
                using var bgPaint = new SKPaint { Color = BackColor.ToSKColor(), IsAntialias = true };
                canvas.DrawRect(0, 0, Width, Height, bgPaint);
            }
            
            base.OnPaint(e);
        }
        
        internal virtual void OnSelected() 
        {
            Visible = true;
            Invalidate();
            Selected?.Invoke(this, EventArgs.Empty);
        }
        
        internal virtual void OnDeselected() 
        {
            Visible = false;
            Invalidate();
            Deselected?.Invoke(this, EventArgs.Empty);
        }
        
        public event EventHandler Selected;
        public event EventHandler Deselected;
    }
}
