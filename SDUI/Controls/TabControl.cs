using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls;

public class TabControl : UIElementBase
{
    private sealed class TabIconCache
    {
        public Image? Image;
        public SKBitmap? Bitmap;
    }

    private float ChevronWidth => 24f * ScaleFactor;

    // Add/Remove animations
    private readonly Dictionary<TabPage, AnimationManager> _addAnimations = new();
    private readonly Dictionary<int, AnimationManager> _hoverAnims = new();
    private readonly Dictionary<TabPage, TabIconCache> _iconCache = new();
    private readonly List<TabPage> _pages = new();
    private readonly Dictionary<TabPage, AnimationManager> _removeAnimations = new();

    private readonly AnimationManager _selectionAnim;
    private readonly double _selectionAnimIncrement = 0.18;
    private readonly AnimationType _selectionAnimType = AnimationType.EaseInOut;
    private readonly List<RectangleF> _tabRects = new();
    private Color _borderColor = ColorScheme.BorderColor;
    private float _borderWidth = 1.0f;
    private float _cornerRadius = 8.0f;
    private int _draggedTabIndex = -1;
    private Point _dragStartPoint;
    private bool _suppressPageSync;

    // Chrome-like styles and animation
    private Color _headerBackColor = ColorScheme.BackColor;

    private Color _headerForeColor = ColorScheme.ForeColor;
    private int _headerHeight = 40;

    private int _hoveredCloseButtonIndex = -1;
    private int _hoveredTabIndex = -1;
    private int _indicatorHeight = 3;
    private bool _isDragging;
    private bool _isNewPageButtonHovered;
    private bool _leftChevronHovered;
    private float _maxScrollOffset;
    private int _prevSelectedIndex = -1;
    private bool _renderNewPageButton;
    private bool _renderPageClose;
    private bool _renderPageIcon;
    private bool _rightChevronHovered;

    // Scroll support
    private float _scrollOffset;
    private int _selectedIndex = -1;
    private Color _selectedTabColor = ColorScheme.BackColor;
    private Color _selectedTabForeColor = ColorScheme.AccentColor;
    private bool _showLeftChevron;
    private bool _showRightChevron;
    private int _tabGap;

    public TabControl()
    {
        Size = new Size(500, 320);
        BackColor = ColorScheme.BackColor;

        _headerBackColor = ColorScheme.BackColor2;
        _headerForeColor = ColorScheme.ForeColor;
        _selectedTabColor = ColorScheme.OnSurface;
        _selectedTabForeColor = ColorScheme.AccentColor;
        _borderColor = ColorScheme.BorderColor;

        _selectionAnim = new AnimationManager()
        {
            Increment = _selectionAnimIncrement,
            AnimationType = _selectionAnimType,
            InterruptAnimation = true
        };
        _selectionAnim.OnAnimationProgress += _ => Invalidate();
    }

    internal IReadOnlyList<TabPage> Pages => _pages;

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

    public int HeaderHeight
    {
        get => _headerHeight;
        set
        {
            var newVal = Math.Max(24, value);
            if (_headerHeight == newVal) return;
            _headerHeight = newVal;
            UpdatePagesLayout();
            Invalidate();
        }
    }

    public int TabGap
    {
        get => _tabGap;
        set
        {
            if (_tabGap == value) return;
            _tabGap = Math.Max(0, value);
            Invalidate();
        }
    }

    public int IndicatorHeight
    {
        get => _indicatorHeight;
        set
        {
            if (_indicatorHeight == value) return;
            _indicatorHeight = Math.Max(1, value);
            Invalidate();
        }
    }



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

    public event EventHandler NewPageButtonClicked;
    public event EventHandler SelectedIndexChanged;

    public void AddPage(TabPage page)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));

        _pages.Add(page);
        Controls.Add(page);
        page.Visible = false;
        EnsureHoverAnim(_pages.Count - 1);

        var addAnim = new AnimationManager()
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
            SelectedIndex = 0;
        else
            SelectedIndex = _pages.Count - 1;

        Invalidate();
    }

    public void RemovePage(TabPage page)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));
        var index = _pages.IndexOf(page);
        if (index >= 0)
        {
            var removeAnim = new AnimationManager()
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
                ClearTabIconCache(page);

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

    public void MovePage(TabPage page, int newIndex)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));
        var oldIndex = _pages.IndexOf(page);
        if (oldIndex < 0) throw new InvalidOperationException("Tab page not found in this control.");

        newIndex = Math.Clamp(newIndex, 0, _pages.Count - 1);
        if (oldIndex == newIndex)
            return;

        _pages.RemoveAt(oldIndex);
        _pages.Insert(newIndex, page);

        if (_selectedIndex == oldIndex)
        {
            _selectedIndex = newIndex;
        }
        else if (_selectedIndex >= 0)
        {
            if (oldIndex < _selectedIndex && newIndex >= _selectedIndex)
                _selectedIndex--;
            else if (oldIndex > _selectedIndex && newIndex <= _selectedIndex)
                _selectedIndex++;
        }

        _suppressPageSync = true;
        try
        {
            Controls.Remove(page);
            Controls.Insert(newIndex, page);
        }
        finally
        {
            _suppressPageSync = false;
        }

        _hoveredTabIndex = -1;
        _hoveredCloseButtonIndex = -1;
        _draggedTabIndex = -1;
        _isDragging = false;

        RebuildHoverAnimations();
        UpdatePagesLayout();
        Invalidate();
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
            var ae = new AnimationManager()
            {
                Increment = 0.2,
                AnimationType = AnimationType.EaseInOut,
                InterruptAnimation = true
            };
            ae.OnAnimationProgress += _ => Invalidate();
            _hoverAnims[index] = ae;
        }
    }

    private void RebuildHoverAnimations()
    {
        _hoverAnims.Clear();
        for (var i = 0; i < _pages.Count; i++)
            EnsureHoverAnim(i);
    }

    private void ClearTabIconCache(TabPage page)
    {
        if (page == null) return;
        if (_iconCache.TryGetValue(page, out var cache))
        {
            cache.Bitmap?.Dispose();
            _iconCache.Remove(page);
        }
    }

    private SKBitmap GetTabIconBitmap(TabPage page)
    {
        if (page == null || page.Image == null)
        {
            ClearTabIconCache(page);
            return null;
        }

        if (_iconCache.TryGetValue(page, out var cache))
        {
            if (cache.Image == page.Image && cache.Bitmap != null)
                return cache.Bitmap;

            cache.Bitmap?.Dispose();
            _iconCache.Remove(page);
        }

        using var ms = new MemoryStream();
        page.Image.Save(ms, ImageFormat.Png);
        ms.Position = 0;
        var skBitmap = SKBitmap.Decode(ms);
        if (skBitmap == null) return null;

        _iconCache[page] = new TabIconCache { Image = page.Image, Bitmap = skBitmap };
        return skBitmap;
    }

    private void UpdateTabRects()
    {
        _tabRects.Clear();

        // Dynamic metrics based on font size
        var fontSize = 9.Topx(this);
        var margin = 4f * ScaleFactor;
        
        using var font = new SKFont
        {
            Size = fontSize,
            Typeface = FontManager.GetSKTypeface(Font),
        };

        var sidePadding = fontSize * 1.2f;
        var closeButtonSpace = RenderPageClose ? (fontSize * 1.5f) : 0f;
        var iconSize = Math.Min(fontSize * 1.2f, HeaderHeight - 6f);
        var iconPad = fontSize * 0.5f;

        var minTabWidth = fontSize * 12f; 
        var maxTabWidth = fontSize * 30f;

        // 1. Measure all tabs first
        var tabWidths = new float[_pages.Count];
        var totalWidth = 0f;

        for (var i = 0; i < _pages.Count; i++)
        {
            var page = _pages[i];
            
            // Measure text: Text padding + CloseButton
            var textWidth = font.MeasureText(page.Text);
            var iconSpace = RenderPageIcon && page.Image != null ? iconSize + iconPad : 0f;
            var requiredWidth = textWidth + (sidePadding * 2) + closeButtonSpace + iconSpace;

            var w = Math.Clamp(requiredWidth, minTabWidth, maxTabWidth);
            tabWidths[i] = w;
            totalWidth += w;
            
            // Add gap for all except the last one technically, 
            // but usually gap follows every item in coordinate calculation logic
            if (i < _pages.Count - 1)
                totalWidth += _tabGap;
        }

        // 2. Determine available space and chevron visibility
        var buttonSize = fontSize * 1.8f;
        var buttonReserved = RenderNewPageButton ? (buttonSize + 12f * ScaleFactor) : 0;
        var baseAvailableWidth = Width - margin - buttonReserved;
        var overflow = totalWidth > baseAvailableWidth;

        if (!overflow)
        {
            _showLeftChevron = false;
            _showRightChevron = false;
            _maxScrollOffset = 0;
            _scrollOffset = 0;
        }
        else
        {
            // Determine Left Chevron based on current scroll
            // Note: This relies on the previous frame's scroll offset or user input
            _showLeftChevron = _scrollOffset > 1f;

            // Determine Right Chevron
            // We first estimate available width based on Left Chevron only
            var leftReserved = _showLeftChevron ? ChevronWidth + margin : margin;
            var spaceForTabs = Width - leftReserved - buttonReserved;
            
            var estMaxScroll = Math.Max(0, totalWidth - spaceForTabs);
            
            // Check if right chevron is needed
            // If we are not at the very end, we need right chevron
            _showRightChevron = _scrollOffset < estMaxScroll - 1f;

            // If right chevron IS shown, available space shrinks, maxScroll increases.
            if (_showRightChevron)
            {
                var rightReserved = ChevronWidth + margin;
                spaceForTabs -= rightReserved;
            }

            // Final recalc of max scroll with accurate chevron state
            _maxScrollOffset = Math.Max(0, totalWidth - spaceForTabs);
            _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);
        }

        // 3. Generate Rects
        var startX = _showLeftChevron ? ChevronWidth + margin : margin;
        var x = startX - _scrollOffset;

        for (var i = 0; i < _pages.Count; i++)
        {
            var w = tabWidths[i];
            _tabRects.Add(new RectangleF(x, 0, w, HeaderHeight));
            x += w + _tabGap;
        }
    }

    private void ScrollToTab(int index)
    {
        if (index < 0 || index >= _tabRects.Count) return;

        var tabRect = _tabRects[index];
        var startX = _showLeftChevron ? ChevronWidth : 0;
        var endX = Width - (_showRightChevron ? ChevronWidth : 0) - 32f * ScaleFactor;

        // Scroll if tab is not fully visible
        if (tabRect.Left < startX)
            _scrollOffset -= startX - tabRect.Left;
        else if (tabRect.Right > endX) _scrollOffset += tabRect.Right - endX;

        _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);

        // After scroll change, invalidate and update tab layout so that tab visibility is correct
        UpdateTabRects();
        Invalidate();
    }

    private void UpdatePagesLayout()
    {
        var contentY = HeaderHeight;
        var contentH = Math.Max(0, Height - HeaderHeight);
        var contentRect = new Rectangle(0, contentY, Width, contentH);

        for (var i = 0; i < _pages.Count; i++)
        {
            var page = _pages[i];
            page.Location = contentRect.Location;
            page.Size = contentRect.Size;
            page.Visible = i == _selectedIndex;
            if (page.Visible)
            {
                page.BringToFront();
                page.PerformLayout();
            }
        }
    }

    internal override void OnControlAdded(UIElementEventArgs e)
    {
        base.OnControlAdded(e);

        if (_suppressPageSync)
            return;

        if (e.Element is TabPage page && !_pages.Contains(page))
        {
            _pages.Add(page);
            page.Visible = false;

            if (_selectedIndex == -1)
                SelectedIndex = 0;

            Invalidate();
        }
    }

    internal override void OnControlRemoved(UIElementEventArgs e)
    {
        base.OnControlRemoved(e);

        if (_suppressPageSync)
            return;

        if (e.Element is TabPage page && _pages.Contains(page))
        {
            _pages.Remove(page);
            ClearTabIconCache(page);

            if (_selectedIndex >= _pages.Count)
                SelectedIndex = Math.Max(-1, _pages.Count - 1);

            Invalidate();
        }
    }

    internal override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdatePagesLayout();
    }

    protected override void OnLayout(UILayoutEventArgs e)
    {
        base.OnLayout(e);

        UpdatePagesLayout();
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);
        var bounds = ClientRectangle;

        // canvas.Clear(BackColor.ToSKColor());


        UpdateTabRects();

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
        {
            canvas.DrawRect(new SKRect(0, 0, bounds.Width, HeaderHeight), headerPaint);
        }

        // === TAB AREA CLIPPING ===
        canvas.Save();
        var fontSize = 9.Topx(this);
        var margin = 4f * ScaleFactor;
        var clipStart = _showLeftChevron ? ChevronWidth + margin : margin;

        float clipEnd;
        if (_showRightChevron && RenderNewPageButton)
        {
            var buttonSize = fontSize * 1.8f;
            var buttonSpacing = 12f * ScaleFactor;
            var buttonX = Width - ChevronWidth - buttonSize - buttonSpacing;
            clipEnd = buttonX - margin;
        }
        else
        {
            clipEnd = Width - (_showRightChevron ? ChevronWidth + margin : margin);
        }

        canvas.ClipRect(new SKRect(clipStart, 0, clipEnd, HeaderHeight));

        // === DRAW TABS ===
        for (var i = 0; i < _pages.Count; i++)
        {
            var page = _pages[i];

            // Animation check
            var animScale = 1f;
            var animAlpha = 1f;

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
            var tabPadding = 1f;
            var tabHeight = fontSize * 2.2f; 
            var verticalMargin = (HeaderHeight - tabHeight) / 2f;
            var topRadius = fontSize / 2f;

            var tabRect = new SKRect(
                rect.Left + tabPadding,
                verticalMargin,
                rect.Right - tabPadding,
                HeaderHeight - verticalMargin
            );

            if (animScale < 1f)
            {
                var targetWidth = tabRect.Width * animScale;
                tabRect.Right = tabRect.Left + targetWidth;
            }

            // === TAB BACKGROUND ===
            using (var bgPaint = new SKPaint { IsAntialias = true })
            {
                var selectionProgress = 0f;
                if (i == _selectedIndex)
                    selectionProgress = (float)_selectionAnim.GetProgress();
                else if (i == _prevSelectedIndex && _selectionAnim.IsAnimating())
                    selectionProgress = 1f - (float)_selectionAnim.GetProgress();

                if (isSelected || (i == _prevSelectedIndex && _selectionAnim.IsAnimating()))
                {
                    // Se�ili tab: BackColor (belirgin)
                    var selectedColor = ColorScheme.BackColor;
                    var headerBg = headerColor;

                    var blendedColor = Color.FromArgb(
                        (int)(headerBg.R + (selectedColor.R - headerBg.R) * selectionProgress),
                        (int)(headerBg.G + (selectedColor.G - headerBg.G) * selectionProgress),
                        (int)(headerBg.B + (selectedColor.B - headerBg.B) * selectionProgress)
                    );

                    bgPaint.Color = Color
                        .FromArgb((int)(255 * animAlpha), blendedColor.R, blendedColor.G, blendedColor.B).ToSKColor();
                }
                else
                {
                    // Se�ili olmayan: header rengi + hover efekti
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
                    Color = ColorScheme.BorderColor.Alpha((int)(animAlpha * 32)).ToSKColor(),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1f,
                    IsAntialias = true
                };
                canvas.DrawRoundRect(tabRect, topRadius, topRadius, borderPaint);
            }

            // === TAB TEXT ===
            var textSelectionProgress = 0f;
            if (i == _selectedIndex)
                textSelectionProgress = (float)_selectionAnim.GetProgress();
            else if (i == _prevSelectedIndex && _selectionAnim.IsAnimating())
                textSelectionProgress = 1f - (float)_selectionAnim.GetProgress();

            var selectedTextColor = ColorScheme.ForeColor;
            var unselectedTextColor = Color.FromArgb(
                Math.Max(0, (int)(ColorScheme.ForeColor.R * 0.6)),
                Math.Max(0, (int)(ColorScheme.ForeColor.G * 0.6)),
                Math.Max(0, (int)(ColorScheme.ForeColor.B * 0.6))
            );

            Color textColor;
            if (isSelected || (i == _prevSelectedIndex && _selectionAnim.IsAnimating()))
                textColor = Color.FromArgb(
                    (int)(unselectedTextColor.R +
                          (selectedTextColor.R - unselectedTextColor.R) * textSelectionProgress),
                    (int)(unselectedTextColor.G +
                          (selectedTextColor.G - unselectedTextColor.G) * textSelectionProgress),
                    (int)(unselectedTextColor.B + (selectedTextColor.B - unselectedTextColor.B) * textSelectionProgress)
                );
            else
                textColor = unselectedTextColor;

            var sidePadding = fontSize * 1.2f;
            var closeButtonSpace = RenderPageClose ? (fontSize * 1.5f) : 0f;
            var closeSize = fontSize * 1.2f;
            var iconSize = Math.Min(fontSize * 1.2f, tabRect.Height - 6f);
            var iconPad = fontSize * 0.5f;
            var iconSpace = RenderPageIcon && page.Image != null ? iconSize + iconPad : 0f;

            using (var font = new SKFont
                   {
                       Size = fontSize,
                       Typeface = FontManager.GetSKTypeface(Font),
                       Subpixel = true,
                       Edging = SKFontEdging.SubpixelAntialias
                   })
            using (var textPaint = new SKPaint
                   {
                       Color =
                           Color.FromArgb((int)(255 * animAlpha), textColor.R, textColor.G, textColor.B).ToSKColor(),
                       IsAntialias = true
                   })
            {
                var maxTextWidth = tabRect.Width - (sidePadding * 2) - closeButtonSpace - iconSpace;
                var textWidth = font.MeasureText(_pages[i].Text);

                if (RenderPageIcon && page.Image != null)
                {
                    var iconBitmap = GetTabIconBitmap(page);
                    if (iconBitmap != null)
                    {
                        var iconX = tabRect.Left + sidePadding;
                        var iconY = tabRect.MidY - iconSize / 2f;
                        var iconRect = new SKRect(iconX, iconY, iconX + iconSize, iconY + iconSize);
                        using var iconPaint = new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.Medium,
                            Color = new SKColor(255, 255, 255, (byte)(255 * animAlpha))
                        };
                        canvas.DrawBitmap(iconBitmap, iconRect, iconPaint);
                    }
                }

                // Center text if we have space, otherwise left align (ellipsized)
                var textX = tabRect.Left + sidePadding + iconSpace;
                if (textWidth < maxTextWidth)
                {
                    // Available width within padding/closeBtn
                    var availableWidth = tabRect.Width - (sidePadding * 2) - closeButtonSpace - iconSpace;
                    // Start of available area
                    var areaStart = tabRect.Left + sidePadding + iconSpace;
                    // Center point
                    var center = areaStart + availableWidth / 2f;
                    textX = center - textWidth / 2f;
                }

                var textY = tabRect.MidY - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;
                canvas.DrawTextWithEllipsis(_pages[i].Text, textX, textY, maxTextWidth, textPaint, font);
            }

            // === CLOSE BUTTON ===
            if (RenderPageClose && (isSelected || isHovered || i == _hoveredCloseButtonIndex) && animAlpha > 0.3f)
            {
                var closeRightMargin = sidePadding / 2f;
                var closeX = tabRect.Right - closeSize - closeRightMargin;
                var closeY = tabRect.MidY - closeSize / 2;
                var closeRect = new SKRect(closeX, closeY, closeX + closeSize, closeY + closeSize);
                var isCloseHovered = i == _hoveredCloseButtonIndex;

                if (isCloseHovered)
                {
                    var circleColor = isDarkTheme
                        ? Color.FromArgb(200, 255, 255, 255)
                        : Color.FromArgb(180, 0, 0, 0);

                    using var closeBgPaint = new SKPaint
                    {
                        Color = Color.FromArgb((int)(circleColor.A * animAlpha), circleColor.R, circleColor.G,
                            circleColor.B).ToSKColor(),
                        IsAntialias = true
                    };
                    canvas.DrawCircle(closeRect.MidX, closeRect.MidY, closeSize / 2, closeBgPaint);
                }

                var lineColor = isCloseHovered
                    ? isDarkTheme ? Color.FromArgb(30, 30, 30) : Color.FromArgb(255, 255, 255)
                    : ColorScheme.ForeColor;

                using var linePaint = new SKPaint
                {
                    Color = Color.FromArgb((int)(255 * animAlpha), lineColor.R, lineColor.G, lineColor.B).ToSKColor(),
                    StrokeWidth = 1.2f,
                    IsAntialias = true,
                    StrokeCap = SKStrokeCap.Round
                };

                var padding = closeSize * 0.28f;
                canvas.DrawLine(closeRect.Left + padding, closeRect.Top + padding, closeRect.Right - padding,
                    closeRect.Bottom - padding, linePaint);
                canvas.DrawLine(closeRect.Right - padding, closeRect.Top + padding, closeRect.Left + padding,
                    closeRect.Bottom - padding, linePaint);
            }
        }

        canvas.Restore();

        DrawChevrons(canvas, bounds);

        using (var borderPaint = new SKPaint { Color = ColorScheme.BorderColor.ToSKColor(), StrokeWidth = 1f })
        {
            canvas.DrawLine(0, HeaderHeight - 1, bounds.Width, HeaderHeight - 1, borderPaint);
        }

        DrawNewPageButton(canvas, bounds);

        using (var contentBgPaint = new SKPaint { Color = ColorScheme.BackColor.ToSKColor(), IsAntialias = true })
        {
            canvas.DrawRect(new SKRect(0, HeaderHeight, bounds.Width, bounds.Height), contentBgPaint);
        }

        if (_selectedIndex >= 0 && _selectedIndex < _pages.Count)
        {
            var selectedPage = _pages[_selectedIndex];
            if (selectedPage.Visible)
            {
                selectedPage.Location = new Point(0, HeaderHeight);
                selectedPage.Size = new Size(bounds.Width, bounds.Height - HeaderHeight);

                canvas.Save();
                canvas.ClipRect(new SKRect(0, HeaderHeight, bounds.Width, bounds.Height));
                selectedPage.Render(canvas);
                canvas.Restore();
            }
        }
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isDragging && _draggedTabIndex >= 0 && e.Button == MouseButtons.Left)
        {
            var dx = Math.Abs(e.Location.X - _dragStartPoint.X);
            var dy = Math.Abs(e.Location.Y - _dragStartPoint.Y);
            var dragThreshold = 4f * ScaleFactor;

            if (dx > dragThreshold || dy > dragThreshold)
            {
                var hoverIndex = GetTabIndexAtPoint(e.Location);
                if (hoverIndex >= 0 && hoverIndex != _draggedTabIndex && _draggedTabIndex < _pages.Count)
                {
                    var draggedPage = _pages[_draggedTabIndex];
                    MovePage(draggedPage, hoverIndex);
                    _draggedTabIndex = hoverIndex;
                }
            }
        }

        _hoveredTabIndex = GetTabIndexAtPoint(e.Location);
        _hoveredCloseButtonIndex = GetCloseButtonIndexAtPoint(e.Location);

        var wasLeftHovered = _leftChevronHovered;
        var wasRightHovered = _rightChevronHovered;

        var leftArea = new RectangleF(0, 0, ChevronWidth, HeaderHeight);
        var rightArea = new RectangleF(Width - ChevronWidth, 0, ChevronWidth, HeaderHeight);

        _leftChevronHovered = _showLeftChevron && leftArea.Contains(e.Location);
        _rightChevronHovered = _showRightChevron && rightArea.Contains(e.Location);

        var fontSize = 9.Topx(this);

        if (RenderNewPageButton)
        {
            var buttonSize = fontSize * 1.8f;
            float buttonX;

            if (_showRightChevron)
            {
                buttonX = Width - ChevronWidth - buttonSize - 12f * ScaleFactor;
            }
            else if (_tabRects.Count > 0)
            {
                var lastTab = _tabRects[_tabRects.Count - 1];
                buttonX = lastTab.Right + 8f * ScaleFactor;
            }
            else
            {
                buttonX = _showLeftChevron ? ChevronWidth + 8f * ScaleFactor : 8f * ScaleFactor;
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

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        Focus();

        if (e.Button == MouseButtons.Left)
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

    internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Left)
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

    internal override void OnMouseWheel(MouseEventArgs e)
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

        for (var i = 0; i < _tabRects.Count; i++)
            if (_tabRects[i].Contains(point))
                return i;
        return -1;
    }

    private int GetCloseButtonIndexAtPoint(Point point)
    {
        if (!RenderPageClose || point.Y > HeaderHeight) return -1;
        
        var fontSize = 9.Topx(this);

        for (var i = 0; i < _tabRects.Count; i++)
        {
            var rect = _tabRects[i];

            var tabPadding = 1f;
            var tabHeight = fontSize * 2.2f;
            var verticalMargin = (HeaderHeight - tabHeight) / 2f;

            var tabRect = new SKRect(
                rect.Left + tabPadding,
                verticalMargin,
                rect.Right - tabPadding,
                HeaderHeight - verticalMargin
            );

            var sidePadding = fontSize * 1.2f;
            var closeRightMargin = sidePadding / 2f;
            var closeSize = fontSize * 1.2f;
            var closeX = tabRect.Right - closeSize - closeRightMargin;
            var closeY = tabRect.MidY - closeSize / 2;
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

        // Ensure pages are positioned and visible according to the selected index
        UpdatePagesLayout();
        Invalidate();
    }

    private void DrawChevrons(SKCanvas canvas, Rectangle bounds)
    {
        var isDarkTheme = ColorScheme.BackColor.IsDark();

        // Ensure page bounds are updated when header area changes
        UpdatePagesLayout();
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
            var leftRect = new SKRect(0, 0, ChevronWidth, HeaderHeight);
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
            var xOffsetBig = 4f * ScaleFactor;
            var yOffset = 6f * ScaleFactor;
            var xOffsetSmall = 2f * ScaleFactor;
            
            canvas.DrawLine(mid.X + xOffsetBig, mid.Y - yOffset, mid.X - xOffsetSmall, mid.Y, p);
            canvas.DrawLine(mid.X - xOffsetSmall, mid.Y, mid.X + xOffsetBig, mid.Y + yOffset, p);
        }

        if (_showRightChevron)
        {
            var rightRect = new SKRect(bounds.Width - ChevronWidth, 0, bounds.Width, HeaderHeight);
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
            var xOffsetBig = 4f * ScaleFactor;
            var yOffset = 6f * ScaleFactor;
            var xOffsetSmall = 2f * ScaleFactor;

            canvas.DrawLine(mid2.X - xOffsetBig, mid2.Y - yOffset, mid2.X + xOffsetSmall, mid2.Y, p2);
            canvas.DrawLine(mid2.X + xOffsetSmall, mid2.Y, mid2.X - xOffsetBig, mid2.Y + yOffset, p2);
        }
    }

    private void DrawNewPageButton(SKCanvas canvas, Rectangle bounds)
    {
        if (!RenderNewPageButton) return;

        var fontSize = 9.Topx(this);
        var buttonSize = fontSize * 1.8f;
        float buttonX;
        var buttonY = (HeaderHeight - buttonSize) / 2f;

        if (_showRightChevron)
        {
            buttonX = bounds.Width - ChevronWidth - buttonSize - 12f * ScaleFactor;
        }
        else if (_tabRects.Count > 0)
        {
            var lastTab = _tabRects[_tabRects.Count - 1];
            buttonX = lastTab.Right + 8f * ScaleFactor;
        }
        else
        {
            // No tabs - position near left with padding
            // No tabs - position near left with padding
            buttonX = _showLeftChevron ? ChevronWidth + 8f * ScaleFactor : 8f * ScaleFactor;
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
        var iconSize = fontSize * 0.45f;
        canvas.DrawLine(cx - iconSize, cy, cx + iconSize, cy, plusPaint);
        canvas.DrawLine(cx, cy - iconSize, cx, cy + iconSize, plusPaint);
    }
}

public class TabPage : UIElementBase
{
    private string _text = nameof(TabPage);
    private string _iconPath = string.Empty;

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
            if (_text == value)
                return;

            _text = value;

            if (Parent is TabControl tc)
                tc.Invalidate();
        }
    }

    [Category("Appearance")]
    public string IconPath
    {
        get => _iconPath;
        set
        {
            var newValue = value ?? string.Empty;
            if (_iconPath == newValue)
                return;

            _iconPath = newValue;

            if (string.IsNullOrWhiteSpace(_iconPath))
            {
                Image = null;
            }
            else
            {
                if (!File.Exists(_iconPath))
                    throw new FileNotFoundException($"TabPage icon file not found: {_iconPath}", _iconPath);

                using var stream = new FileStream(_iconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var loaded = Image.FromStream(stream);
                Image = (Image)loaded.Clone();
            }

            if (Parent is TabControl tc)
                tc.Invalidate();
        }
    }

    public override void OnPaint(SKCanvas canvas)
    {
        if (BackColor != Color.Transparent)
        {
            using var bgPaint = new SKPaint { Color = BackColor.ToSKColor(), IsAntialias = true };
            canvas.DrawRect(0, 0, Width, Height, bgPaint);
        }

        base.OnPaint(canvas);
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