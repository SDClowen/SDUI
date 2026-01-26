using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SDUI.Animation;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using SDUI.Controls;

namespace SDUI.Controls;

public class ColorPicker : ComboBox
{
    private Color _selectedColor = ColorScheme.AccentColor;
    private Color _borderColor = ColorScheme.BorderColor;
    private bool _showHex = true;
    private int _columns = 8;
    
    // Parent ComboBox fields are private. We redefine interaction animations.
    private readonly AnimationManager _hoverAnimation;
    private readonly AnimationManager _pressAnimation;
    private bool _hovered;

    public ColorPicker()
    {
        Size = new Size((int)(180 * ScaleFactor), (int)(32 * ScaleFactor));
        DropDownStyle = ComboBoxStyle.DropDownList; 
        
        _hoverAnimation = new AnimationManager(true)
        {
            Increment = 0.2,
            AnimationType = AnimationType.EaseInOut
        };
        _hoverAnimation.OnAnimationProgress += _ => Invalidate();

        _pressAnimation = new AnimationManager(true)
        {
            Increment = 0.25,
            AnimationType = AnimationType.EaseInOut
        };
        _pressAnimation.OnAnimationProgress += _ => Invalidate();

        ResetPalette();
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
    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor == value) return;
            _selectedColor = value;
            
            // Sync with ComboBox selection
            var index = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] is Color c && c.ToArgb() == value.ToArgb())
                {
                    index = i;
                    break;
                }
            }
            SelectedIndex = index;
            
            Invalidate();
            OnSelectedColorChanged();
        }
    }

    [Category("Appearance")]
    public bool ShowHex
    {
        get => _showHex;
        set
        {
            if (_showHex == value) return;
            _showHex = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    public int Columns
    {
        get => _columns;
        set
        {
            _columns = Math.Max(1, value);
        }
    }

    public event EventHandler SelectedColorChanged;

    private void OnSelectedColorChanged()
    {
        SelectedColorChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetPalette(IEnumerable<Color> colors)
    {
        if (colors == null) throw new ArgumentNullException(nameof(colors));
        Items.Clear();
        foreach(var c in colors) Items.Add(c);
        Items.Add("Custom..."); 
        
        if (Items.Count > 0 && SelectedIndex == -1)
        {
            if (Items[0] is Color c) 
            {
                SelectedIndex = 0;
                _selectedColor = c;
            }
        }
    }

    public void ResetPalette()
    {
        Items.Clear();
        var colors = new[]
        {
            ColorScheme.AccentColor,
            Color.FromArgb(33, 150, 243),
            Color.FromArgb(3, 169, 244),
            Color.FromArgb(0, 188, 212),
            Color.FromArgb(0, 150, 136),
            Color.FromArgb(76, 175, 80),
            Color.FromArgb(139, 195, 74),
            Color.FromArgb(205, 220, 57),
            Color.FromArgb(255, 235, 59),
            Color.FromArgb(255, 193, 7),
            Color.FromArgb(255, 152, 0),
            Color.FromArgb(255, 87, 34),
            Color.FromArgb(244, 67, 54),
            Color.FromArgb(233, 30, 99),
            Color.FromArgb(156, 39, 176),
            Color.FromArgb(103, 58, 183),
            Color.FromArgb(63, 81, 181),
            Color.FromArgb(121, 85, 72),
            Color.FromArgb(96, 125, 139),
            Color.FromArgb(120, 120, 120),
            Color.FromArgb(160, 160, 160),
            Color.FromArgb(200, 200, 200),
            Color.White,
            Color.Black,
            Color.Red,
            Color.Lime,
            Color.Blue,
            Color.Yellow,
            Color.Magenta,
            Color.Cyan
        };
        foreach(var c in colors) Items.Add(c);
        Items.Add("Custom...");
    }

    protected override DropDownPanel CreateDropDownPanel()
    {
        return new ColorDropDownPanel(this);
    }
    
    // We handle custom commitment via the panel interactions.
    // If base triggers ChangeCommited, it uses SelectedIndex.
    protected override void OnSelectionChangeCommitted(EventArgs e)
    {
        if (SelectedItem is Color c)
        {
            _selectedColor = c;
            OnSelectedColorChanged();
        }
        else if (SelectedItem is string s && s == "Custom...")
        {
            // Should have been intercepted by DropDownPanel
        }
        
        base.OnSelectionChangeCommitted(e);
    }

    public override void OnPaint(SKCanvas canvas)
    {
        var bounds = ClientRectangle;
        var radius = 6f * ScaleFactor;

        var hoverProgress = (float)_hoverAnimation.GetProgress();
        var pressProgress = (float)_pressAnimation.GetProgress();

        var bgColor = BackColor;
        if (_hovered)
        {
            var dark = BackColor.IsDark();
            var delta = dark ? 14 : -10;
            bgColor = Color.FromArgb(
                Math.Clamp(BackColor.R + delta, 0, 255),
                Math.Clamp(BackColor.G + delta, 0, 255),
                Math.Clamp(BackColor.B + delta, 0, 255));
        }

        var blend = Color.FromArgb(
            (int)(bgColor.R + (ColorScheme.AccentColor.R - bgColor.R) * (pressProgress * 0.15f)),
            (int)(bgColor.G + (ColorScheme.AccentColor.G - bgColor.G) * (pressProgress * 0.15f)),
            (int)(bgColor.B + (ColorScheme.AccentColor.B - bgColor.B) * (pressProgress * 0.15f))
        );

        using var bgPaint = new SKPaint { Color = blend.ToSKColor(), IsAntialias = true };
        using var borderPaint = new SKPaint
        {
            Color = BorderColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f
        };

        var rect = new SKRect(0, 0, bounds.Width, bounds.Height);
        canvas.DrawRoundRect(rect, radius, radius, bgPaint);
        canvas.DrawRoundRect(rect, radius, radius, borderPaint);

        // Content
        var padding = 6f * ScaleFactor;
        var swatchSize = bounds.Height - (int)(padding * 2);
        var swatchRect = new SKRect(padding, padding, padding + swatchSize, padding + swatchSize);

        using var swatchPaint = new SKPaint { Color = _selectedColor.ToSKColor(), IsAntialias = true };
        canvas.DrawRoundRect(swatchRect, 4f * ScaleFactor, 4f * ScaleFactor, swatchPaint);
        
        using var swatchBorder = new SKPaint 
        { 
            Color = ColorScheme.BorderColor.Alpha(120).ToSKColor(),
            Style = SKPaintStyle.Stroke, 
            IsAntialias = true,
            StrokeWidth = 1f
        };
        canvas.DrawRoundRect(swatchRect, 4f * ScaleFactor, 4f * ScaleFactor, swatchBorder);

        // Text
        if (_showHex)
        {
            var textLeft = swatchRect.Right + padding;
            var arrowWidth = 16f * ScaleFactor;
            var textRight = bounds.Width - padding - arrowWidth;

            var hex = _selectedColor.A == 255
                ? $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}"
                : $"#{_selectedColor.A:X2}{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
            
            using var font = new SKFont
            {
                Size = Font.Size.PtToPx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            
            using var textPaint = new SKPaint
            {
                Color = ForeColor.ToSKColor(),
                IsAntialias = true
            };

            var textY = bounds.Height / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;
            canvas.DrawTextWithEllipsis(hex, textLeft, textY, textRight - textLeft, textPaint, font);
        }

        // Arrow
        var arrowCenterX = bounds.Width - padding - (16f * ScaleFactor / 2f);
        var arrowCenterY = bounds.Height / 2f;
        var arrowSize = 4f * ScaleFactor;

        using var arrowPaint = new SKPaint
        {
            Color = ForeColor.Alpha((int)(200 + 40 * hoverProgress)).ToSKColor(),
            StrokeWidth = 2f,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        canvas.DrawLine(arrowCenterX - arrowSize, arrowCenterY - arrowSize, arrowCenterX, arrowCenterY + arrowSize, arrowPaint);
        canvas.DrawLine(arrowCenterX, arrowCenterY + arrowSize, arrowCenterX + arrowSize, arrowCenterY - arrowSize, arrowPaint);
    }

    internal override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        _hoverAnimation.StartNewAnimation(AnimationDirection.In);
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        _hoverAnimation.StartNewAnimation(AnimationDirection.Out);
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            _pressAnimation.StartNewAnimation(AnimationDirection.In);
            
        base.OnMouseDown(e); 
    }

    public class ColorDropDownPanel : ComboBox.DropDownPanel
    {
        private const int ItemMargin = 10;
        
        private ColorPicker _picker;
        private bool _isInCustomMode;
        private HsvColor _currentHsv;
        private bool _draggingSv;
        private bool _draggingHue;
        
        private Size _defaultSize;

        public ColorDropDownPanel(ColorPicker owner) : base(owner)
        {
            _picker = owner;
        }

        public override int ItemHeight 
        {
            get
            {
                if (_picker.Columns > 1)
                {
                    var cw = Width - (VERTICAL_PADDING * 2);
                    if (cw <= 0) return base.ItemHeight;
                    var size = cw / _picker.Columns;
                    return Math.Max(20, size);
                }
                return base.ItemHeight;
            }
        }
        
        protected override int GetItemIndexAtPoint(Point point)
        {
            if (_isInCustomMode) return -1; // No items in custom mode
            
            if (_picker.Columns <= 1)
                return base.GetItemIndexAtPoint(point);

            if (point.Y < VERTICAL_PADDING) return -1;
            
            var itemSize = ItemHeight;
            if (itemSize <= 0) return -1;

            var relativeY = point.Y - VERTICAL_PADDING;
            var col = (point.X - VERTICAL_PADDING) / itemSize;
            
            if (col < 0 || col >= _picker.Columns) return -1;
            
            var rowVisual = relativeY / itemSize;
            var index = _scrollOffset + rowVisual * _picker.Columns + col;
            
            if (index >= 0 && index < _owner.Items.Count) return index;
            return -1;
        }

        public override void ShowItems()
        {
            _isInCustomMode = false;
            // Restore default behaviour
            base.ShowItems();
            
            // Adjust scrolling for grid
            if (_picker.Columns > 1)
            {
               var rows = Math.Max(1, (Height - 2 * VERTICAL_PADDING) / ItemHeight);
               _scrollBar.LargeChange = rows * _picker.Columns; 
            }
            
            // Capture default size for later restoration
            _defaultSize = Size;
        }

        private void SwitchToCustomMode()
        {
            _isInCustomMode = true;
            _currentHsv = HsvColor.FromColor(_picker.SelectedColor);
            
            // Resize panel
            var newWidth = (int)(240 * ScaleFactor);
            var newHeight = (int)(280 * ScaleFactor);
            
            // Align to bounds
            // Adjust location if width changes?
            // ComboBox usually centers or left aligns.
            // Let's just set Size.
            Size = new Size(newWidth, newHeight);
            
            _scrollBar.Visible = false;
            Invalidate();
        }

        private void SwitchToPaletteMode()
        {
            _isInCustomMode = false;
            Size = _defaultSize;
            ShowItems(); // Re-layout
            Invalidate();
        }

        internal override void OnMouseDown(MouseEventArgs e)
        {
            if (_isInCustomMode)
            {
                HandleCustomMouseDown(e);
                return;
            }
            
            if (e.Button == MouseButtons.Left)
            {
                var index = GetItemIndexAtPoint(e.Location);
                if (index >= 0 && index < _owner.Items.Count)
                {
                    var item = _owner.Items[index];
                    if (item is string s && s == "Custom...")
                    {
                        SwitchToCustomMode();
                        return;
                    }
                }
            }
            
            base.OnMouseDown(e);
        }

        internal override void OnMouseMove(MouseEventArgs e)
        {
            if (_isInCustomMode)
            {
                HandleCustomMouseMove(e);
                return;
            }
            base.OnMouseMove(e);
        }

        internal override void OnMouseUp(MouseEventArgs e)
        {
            if (_isInCustomMode)
            {
                _draggingSv = false;
                _draggingHue = false;
                return;
            }
            base.OnMouseUp(e);
        }

        public override void OnPaint(SKCanvas canvas)
        {
            if (_isInCustomMode)
            {
                DrawCustomPicker(canvas);
                return;
            }

            if (_picker.Columns > 1) DrawGrid(canvas);
            else DrawList(canvas);
        }

        #region Custom Picker Logic

        private void DrawCustomPicker(SKCanvas canvas)
        {
            // Background
            var surfaceColor = ColorScheme.Surface.ToSKColor();
            using var bgPaint = new SKPaint { Color = surfaceColor, IsAntialias = true };
            canvas.DrawRect(new SKRect(0, 0, Width, Height), bgPaint);

            var bounds = ClientRectangle;
            float pad = 12 * ScaleFactor;
            
            // Layout:
            // Top: SV Box
            // Middle: Hue Slider
            // Bottom Right: Buttons
            
            float svSize = bounds.Width - pad * 2;
            float hueHeight = 20 * ScaleFactor;
            float buttonHeight = 28 * ScaleFactor;
            
            // Ensure squares? Or just fill width. 
            // If Window is 240, pad 12 => 216 width. Height can comprise rest.
            // Let's make SV Box square-ish but adjustable.
            var svRect = new SKRect(pad, pad, bounds.Width - pad, bounds.Width - pad);
            
            var hueRect = new SKRect(pad, svRect.Bottom + pad, bounds.Width - pad, svRect.Bottom + pad + hueHeight);
            
            // Draw SV Box
            // 1. Pure Hue
            var pureHue = ColorHelper.FromHsv(_currentHsv.H, 1f, 1f).ToSKColor();
            using var huePaint = new SKPaint { Color = pureHue };
            canvas.DrawRect(svRect, huePaint);
            
            // 2. White Gradient (Horizontal)
            using var whiteGrad = SKShader.CreateLinearGradient(
                new SKPoint(svRect.Left, 0), new SKPoint(svRect.Right, 0),
                new[] { SKColors.White, SKColors.White.WithAlpha(0) },
                null, SKShaderTileMode.Clamp);
            using var whitePaint = new SKPaint { Shader = whiteGrad };
            canvas.DrawRect(svRect, whitePaint);
            
            // 3. Black Gradient (Vertical)
            using var blackGrad = SKShader.CreateLinearGradient(
                new SKPoint(0, svRect.Top), new SKPoint(0, svRect.Bottom),
                new[] { SKColors.Black.WithAlpha(0), SKColors.Black },
                null, SKShaderTileMode.Clamp);
            using var blackPaint = new SKPaint { Shader = blackGrad };
            canvas.DrawRect(svRect, blackPaint);
            
            // Selection Circle on SV Box
            // x corresponds to S, y corresponds to (1-V)? 
            // S = (x - left) / width
            // V = 1 - (y - top) / height
            float selX = svRect.Left + _currentHsv.S * svRect.Width;
            float selY = svRect.Top + (1f - _currentHsv.V) * svRect.Height;
            
            using var ringPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = (_currentHsv.V > 0.5f ? SKColors.Black : SKColors.White), IsAntialias = true };
            canvas.DrawCircle(selX, selY, 5 * ScaleFactor, ringPaint);

            // Draw Hue Slider
            var rainbow = new[] { SKColors.Red, SKColors.Yellow, SKColors.Green, SKColors.Cyan, SKColors.Blue, SKColors.Magenta, SKColors.Red };
            using var hueShader = SKShader.CreateLinearGradient(
                new SKPoint(hueRect.Left, 0), new SKPoint(hueRect.Right, 0),
                rainbow, null, SKShaderTileMode.Clamp);
            using var hueBarPaint = new SKPaint { Shader = hueShader };
            canvas.DrawRect(hueRect, hueBarPaint);
            
            // Hue Selection
            float hueX = hueRect.Left + (_currentHsv.H / 360f) * hueRect.Width;
            using var hueSelPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = SKColors.Black, IsAntialias = true };
            canvas.DrawRect(new SKRect(hueX - 2, hueRect.Top, hueX + 2, hueRect.Bottom), hueSelPaint);
            
            // Buttons
            var btnAreaY = hueRect.Bottom + pad;
            var w = 80 * ScaleFactor;
            var cancelRect = new SKRect(bounds.Width - pad - w - 10 - w, btnAreaY, bounds.Width - pad - w - 10, btnAreaY + buttonHeight);
            var saveRect = new SKRect(bounds.Width - pad - w, btnAreaY, bounds.Width - pad, btnAreaY + buttonHeight);
            
            DrawButton(canvas, cancelRect, "Back", false);
            DrawButton(canvas, saveRect, "Set", true);
        }
        
        private void DrawButton(SKCanvas canvas, SKRect rect, string text, bool primary)
        {
            var paint = new SKPaint { Color = primary ? ColorScheme.AccentColor.ToSKColor() : ColorScheme.Surface.ToSKColor().WithAlpha(200), IsAntialias = true };
            if (!primary) 
            {
                 paint.Style = SKPaintStyle.Stroke;
                 paint.Color = ColorScheme.BorderColor.ToSKColor();
            }
            canvas.DrawRoundRect(rect, 4, 4, paint);
            
            var textP = new SKPaint { Color = primary ? SKColors.White : ColorScheme.ForeColor.ToSKColor(), IsAntialias = true, TextAlign = SKTextAlign.Center };
            var font = GetCachedFont();
            canvas.DrawText(text, rect.MidX, rect.MidY + font.Size/2 - 2, font, textP);
        }

        private void HandleCustomMouseDown(MouseEventArgs e)
        {
            float pad = 12 * ScaleFactor;
            var bounds = ClientRectangle;
            var svRect = new SKRect(pad, pad, bounds.Width - pad, bounds.Width - pad);
            var hueHeight = 20 * ScaleFactor;
            var hueRect = new SKRect(pad, svRect.Bottom + pad, bounds.Width - pad, svRect.Bottom + pad + hueHeight);
            
             var btnAreaY = hueRect.Bottom + pad;
             var buttonHeight = 28 * ScaleFactor;
             var w = 80 * ScaleFactor;
             var cancelRect = new SKRect(bounds.Width - pad - w - 10 - w, btnAreaY, bounds.Width - pad - w - 10, btnAreaY + buttonHeight);
             var saveRect = new SKRect(bounds.Width - pad - w, btnAreaY, bounds.Width - pad, btnAreaY + buttonHeight);

            if (svRect.Contains(e.X, e.Y))
            {
                _draggingSv = true;
                UpdateSv(e.X, e.Y, svRect);
            }
            else if (hueRect.Contains(e.X, e.Y))
            {
                _draggingHue = true;
                UpdateHue(e.X, hueRect);
            }
            else if (saveRect.Contains(e.X, e.Y))
            {
                // Commit
                _picker.Items.Insert(_picker.Items.Count - 1, _currentHsv.ToColor());
                _picker.SelectedColor = _currentHsv.ToColor();
                Hide();
            }
            else if (cancelRect.Contains(e.X, e.Y))
            {
                SwitchToPaletteMode();
            }
        }
        
        private void HandleCustomMouseMove(MouseEventArgs e)
        {
            float pad = 12 * ScaleFactor;
            var bounds = ClientRectangle;
             var svRect = new SKRect(pad, pad, bounds.Width - pad, bounds.Width - pad);
             var hueHeight = 20 * ScaleFactor;
             var hueRect = new SKRect(pad, svRect.Bottom + pad, bounds.Width - pad, svRect.Bottom + pad + hueHeight);

            if (_draggingSv) UpdateSv(e.X, e.Y, svRect);
            if (_draggingHue) UpdateHue(e.X, hueRect);
        }

        private void UpdateSv(float x, float y, SKRect rect)
        {
            float s = Math.Clamp((x - rect.Left) / rect.Width, 0f, 1f);
            float v = Math.Clamp(1f - (y - rect.Top) / rect.Height, 0f, 1f);
            _currentHsv.S = s;
            _currentHsv.V = v;
            Invalidate();
        }

        private void UpdateHue(float x, SKRect rect)
        {
            float h = Math.Clamp((x - rect.Left) / rect.Width, 0f, 1f) * 360f;
            _currentHsv.H = h;
            Invalidate();
        }

        #endregion

        #region Helper Structs

        private struct HsvColor
        {
            public float H; 
            public float S; 
            public float V; 

            public HsvColor(float h, float s, float v) { H = h; S = s; V = v; }

            public Color ToColor()
            {
                return ColorHelper.FromHsv(H, S, V);
            }

            public static HsvColor FromColor(Color color)
            {
                return ColorHelper.ToHsv(color);
            }
        }
        
        private static class ColorHelper
        {
            public static Color FromHsv(float h, float s, float v)
            {
                int hi = Convert.ToInt32(Math.Floor(h / 60)) % 6;
                float f = (float)(h / 60 - Math.Floor(h / 60));

                v = v * 255;
                int vInt = Convert.ToInt32(v);
                int p = Convert.ToInt32(v * (1 - s));
                int q = Convert.ToInt32(v * (1 - f * s));
                int t = Convert.ToInt32(v * (1 - (1 - f) * s));

                return hi switch
                {
                    0 => Color.FromArgb(255, vInt, t, p),
                    1 => Color.FromArgb(255, q, vInt, p),
                    2 => Color.FromArgb(255, p, vInt, t),
                    3 => Color.FromArgb(255, p, q, vInt),
                    4 => Color.FromArgb(255, t, p, vInt),
                    _ => Color.FromArgb(255, vInt, p, q)
                };
            }

            public static HsvColor ToHsv(Color color)
            {
                float max = Math.Max(color.R, Math.Max(color.G, color.B));
                float min = Math.Min(color.R, Math.Min(color.G, color.B));
                float hue = color.GetHue();
                float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
                float value = max / 255f;
                return new HsvColor(hue, saturation, value);
            }
        }

        #endregion

        #region Grid/List Draw

        private void DrawList(SKCanvas canvas)
        {
             if (Width <= 0 || Height <= 0 || _owner.Items.Count == 0) return;

             var surfaceColor = ColorScheme.Surface.ToSKColor();
             _bgPaint ??= new SKPaint { Color = surfaceColor, IsAntialias = true };
             canvas.DrawRect(new SKRect(0, 0, Width, Height), _bgPaint);
             
             var font = GetCachedFont();
             _textPaint ??= new SKPaint { IsAntialias = true, Color = ColorScheme.ForeColor.ToSKColor() };

             int startIndex = _scrollOffset;
             float currentY = VERTICAL_PADDING;
             
             for (int i = startIndex; i < _owner.Items.Count; i++)
             {
                 if (currentY + ItemHeight > Height - VERTICAL_PADDING) break;
                 
                 var w = Width - (_scrollBar.Visible ? SCROLL_BAR_WIDTH + 4 : 0);
                 var itemMargin = (int)(10 * ScaleFactor);
                 var itemRect = new SKRect(itemMargin, currentY, w - itemMargin, currentY + ItemHeight);
                 
                 bool isSelected = i == _selectedIndex;
                 bool isHovered = i == _hoveredIndex;
                 
                  if (isSelected || isHovered)
                 {
                     _selectionPaint ??= new SKPaint { IsAntialias = true, Color = ColorScheme.AccentColor.Alpha(70).ToSKColor() };
                     canvas.DrawRoundRect(new SKRoundRect(itemRect, CORNER_RADIUS), _selectionPaint);
                 }
                 
                 var item = _owner.Items[i];
                 
                 if (item is Color c)
                 {
                     var swatchSize = ItemHeight - 12;
                     var swatchRect = new SKRect(itemRect.Left + 6, itemRect.Top + 6, itemRect.Left + 6 + swatchSize, itemRect.Top + 6 + swatchSize);
                     using var p = new SKPaint { Color = c.ToSKColor(), IsAntialias = true };
                     canvas.DrawRoundRect(swatchRect, 4, 4, p);
                     using var pb = new SKPaint { Color = SKColors.Gray, IsStroke = true, IsAntialias = true };
                     canvas.DrawRoundRect(swatchRect, 4, 4, pb);
                     
                     string text = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
                     TextRenderingHelper.DrawText(canvas, text, swatchRect.Right + 12, currentY + ItemHeight/2 - font.Size/2, font, _textPaint);
                 }
                 else
                 {
                      string text = item?.ToString() ?? "";
                      TextRenderingHelper.DrawText(canvas, text, itemRect.Left + 10, currentY + ItemHeight/2 - font.Size/2, font, _textPaint);
                 }
                 
                 currentY += ItemHeight;
             }
        }

        private void DrawGrid(SKCanvas canvas)
        {
             if (Width <= 0 || Height <= 0 || _owner.Items.Count == 0) return;

             var surfaceColor = ColorScheme.Surface.ToSKColor();
             _bgPaint ??= new SKPaint { Color = surfaceColor, IsAntialias = true };
             canvas.DrawRect(new SKRect(0, 0, Width, Height), _bgPaint);
             
             var cellSize = ItemHeight; 
             int startIndex = _scrollOffset;
             
             float currentY = VERTICAL_PADDING;
             float startX = VERTICAL_PADDING;
             
             for (int i = 0; startIndex + i < _owner.Items.Count; i++)
             {
                 var idx = startIndex + i;
                 var item = _owner.Items[idx];
                 
                 var col = i % _picker.Columns;
                 var row = i / _picker.Columns;
                 
                 var x = startX + col * cellSize;
                 var y = currentY + row * cellSize;
                 
                 if (y >= Height - VERTICAL_PADDING) break;
                 
                 var rect = new SKRect(x + 2, y + 2, x + cellSize - 2, y + cellSize - 2);
                 
                 bool isSelected = idx == _selectedIndex;
                 bool isHovered = idx == _hoveredIndex;
                 
                 if (item is Color c)
                 {
                     using var p = new SKPaint { Color = c.ToSKColor(), IsAntialias = true };
                     canvas.DrawRoundRect(rect, 4, 4, p);
                     
                      if (isSelected || isHovered)
                     {
                         using var pb = new SKPaint { Color = isSelected ? SKColors.Black : SKColors.Gray, IsStroke = true, StrokeWidth = 2, IsAntialias = true };
                         canvas.DrawRoundRect(rect, 4, 4, pb);
                     }
                 }
                 else
                 {
                     using var p = new SKPaint { Color = SKColors.LightGray, IsAntialias = true };
                     canvas.DrawRoundRect(rect, 4, 4, p);
                     using var textP = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextAlign = SKTextAlign.Center };
                      var font = GetCachedFont(); 
                     canvas.DrawText("...", rect.MidX, rect.MidY + font.Size/2 - 2, font, textP);
                     
                      if (isSelected || isHovered)
                     {
                         using var pb = new SKPaint { Color = isSelected ? SKColors.Black : SKColors.Gray, IsStroke = true, StrokeWidth = 2, IsAntialias = true };
                         canvas.DrawRoundRect(rect, 4, 4, pb);
                     }
                 }
             }
        }
        #endregion
    }
}
