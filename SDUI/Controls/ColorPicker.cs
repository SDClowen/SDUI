using SDUI.Helpers;
using SkiaSharp;
using System;
using System.ComponentModel;

namespace SDUI.Controls;

public class ColorPicker : ComboBox
{
    private bool _showHex = true;
    private int _columns = 8;

    public ColorPicker()
    {
        Size = new SKSize((int)(180 * ScaleFactor), (int)(32 * ScaleFactor));
        DropDownStyle = ComboBoxStyle.DropDownList;

        ResetPalette();
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

    public SkiaSharp.SKColor SelectedColor
    {
        get => SelectedIndex == -1 ? SKColors.Empty : (SkiaSharp.SKColor)SelectedItem;
        set => SelectedItem = value;
    }

    protected override void OnSelectedItemChanged(EventArgs e)
    {
        base.OnSelectedItemChanged(e);
    }

    public void ResetPalette()
    {
        Items.Clear();
        var colors = new[]
        {
            ColorScheme.AccentColor,
            new SkiaSharp.SKColor(33, 150, 243),
            new SkiaSharp.SKColor(3, 169, 244),
            new SkiaSharp.SKColor(0, 188, 212),
            new SkiaSharp.SKColor(0, 150, 136),
            new SkiaSharp.SKColor(76, 175, 80),
            new SkiaSharp.SKColor(139, 195, 74),
            new SkiaSharp.SKColor(205, 220, 57),
            new SkiaSharp.SKColor(255, 235, 59),
            new SkiaSharp.SKColor(255, 193, 7),
            new SkiaSharp.SKColor(255, 152, 0),
            new SkiaSharp.SKColor(255, 87, 34),
            new SkiaSharp.SKColor(244, 67, 54),
            new SkiaSharp.SKColor(233, 30, 99),
            new SkiaSharp.SKColor(156, 39, 176),
            new SkiaSharp.SKColor(103, 58, 183),
            new SkiaSharp.SKColor(63, 81, 181),
            new SkiaSharp.SKColor(121, 85, 72),
            new SkiaSharp.SKColor(96, 125, 139),
            new SkiaSharp.SKColor(120, 120, 120),
            new SkiaSharp.SKColor(160, 160, 160),
            new SkiaSharp.SKColor(200, 200, 200),
            SKColors.White,
            SKColors.Black,
            SKColors.Red,
            SKColors.Lime,
            SKColors.Blue,
            SKColors.Yellow,
            SKColors.Magenta,
            SKColors.Cyan
        };

        Items.AddRange(colors);
        Items.Add("Custom...");
    }

    protected override DropDownPanel CreateDropDownPanel()
    {
        return new ColorDropDownPanel(this);
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        var bounds = ClientRectangle;

        var selectedColor = SelectedIndex >= 0 && SelectedIndex < Items.Count && Items[SelectedIndex] is SkiaSharp.SKColor c
            ? c
            : ColorScheme.AccentColor;

        // Content
        var padding = 6f * ScaleFactor;
        var swatchSize = bounds.Height - (int)(padding * 2);
        var swatchRect = new SkiaSharp.SKRect(padding, padding, padding + swatchSize, padding + swatchSize);

        using var swatchPaint = new SKPaint { Color = selectedColor, IsAntialias = true };
        canvas.DrawRoundRect(swatchRect, 4f * ScaleFactor, 4f * ScaleFactor, swatchPaint);
        

        // Text
        if (_showHex)
        {
            var textLeft = swatchRect.Right + padding;
            var arrowWidth = 16f * ScaleFactor;
            var textRight = bounds.Width - padding - arrowWidth;

            var hex = selectedColor.Alpha == 255
                ? $"#{selectedColor.Red:X2}{selectedColor.Green:X2}{selectedColor.Blue:X2}"
                : $"#{selectedColor.Alpha:X2}{selectedColor.Red:X2}{selectedColor.Green:X2}{selectedColor.Blue:X2}";
            
            using var font = new SKFont
            {
                Size = Font.Size.Topx(this),
                Typeface = FontManager.GetSKTypeface(Font),
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };
            
            using var textPaint = new SKPaint
            {
                Color = ForeColor,
                IsAntialias = true
            };

            var textY = bounds.Height / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2f;
            canvas.DrawTextWithEllipsis(hex, textLeft, textY, textRight - textLeft, textPaint, font);
        }

        // Arrow
        var arrowCenterX = bounds.Width - padding - (16f * ScaleFactor / 2f);
        var arrowCenterY = bounds.Height / 2f;
        var arrowSize = 4f * ScaleFactor;

    }

    public class ColorDropDownPanel : DropDownPanel
    {
        private const int ItemMargin = 10;
        
        private ColorPicker _picker;
        private bool _isInCustomMode;
        private HsvColor _currentHsv;
        private bool _draggingSv;
        private bool _draggingHue;
        
        private SKSize _defaultSize;

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
        
        protected override int GetItemIndexAtPoint(SKPoint point)
        {
            if (_isInCustomMode) return -1; // No items in custom mode
            
            if (_picker.Columns <= 1)
                return base.GetItemIndexAtPoint(point);

            if (point.Y < VERTICAL_PADDING) return -1;
            
            var itemSize = ItemHeight;
            if (itemSize <= 0) return -1;

            var relativeY = point.Y - VERTICAL_PADDING;
            var col = ((int)point.X - VERTICAL_PADDING) / itemSize;
            
            if (col < 0 || col >= _picker.Columns) return -1;
            
            var rowVisual = relativeY / itemSize;
            var index = _scrollOffset + (int)rowVisual * _picker.Columns + col;
            
            if (index >= 0 && index < _owner.Items.Count) 
                return index;

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
            var selectedColor = _picker.SelectedIndex >= 0 && _picker.SelectedIndex < _picker.Items.Count && _picker.Items[_picker.SelectedIndex] is SkiaSharp.SKColor c
                ? c
                : ColorScheme.AccentColor;

            _isInCustomMode = true;
            _currentHsv = HsvColor.FromColor(selectedColor);
            
            // Resize panel
            var newWidth = (int)(240 * ScaleFactor);
            var newHeight = (int)(280 * ScaleFactor);
            
            // Align to bounds
            // Adjust location if width changes?
            // ComboBox usually centers or left aligns.
            // Let's just set Size.
            Size = new SKSize(newWidth, newHeight);
            
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
            var surfaceColor = ColorScheme.Surface;
            using var bgPaint = new SKPaint { Color = surfaceColor, IsAntialias = true };
            canvas.DrawRect(new SkiaSharp.SKRect(0, 0, Width, Height), bgPaint);

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
            var svRect = new SkiaSharp.SKRect(pad, pad, bounds.Width - pad, bounds.Width - pad);
            
            var hueRect = new SkiaSharp.SKRect(pad, svRect.Bottom + pad, bounds.Width - pad, svRect.Bottom + pad + hueHeight);

            // Draw SV Box
            // 1. Pure Hue
            var pureHue = SkiaSharp.SKColor.FromHsv(_currentHsv.H, 1f, 1f);
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
            canvas.DrawRect(new SkiaSharp.SKRect(hueX - 2, hueRect.Top, hueX + 2, hueRect.Bottom), hueSelPaint);
            
            // Buttons
            var btnAreaY = hueRect.Bottom + pad;
            var w = 80 * ScaleFactor;
            var cancelRect = new SkiaSharp.SKRect(bounds.Width - pad - w - 10 - w, btnAreaY, bounds.Width - pad - w - 10, btnAreaY + buttonHeight);
            var saveRect = new SkiaSharp.SKRect(bounds.Width - pad - w, btnAreaY, bounds.Width - pad, btnAreaY + buttonHeight);
            
            DrawButton(canvas, cancelRect, "Back", false);
            DrawButton(canvas, saveRect, "Set", true);
        }
        
        private void DrawButton(SKCanvas canvas, SkiaSharp.SKRect rect, string text, bool primary)
        {
            var paint = new SKPaint { Color = primary ? ColorScheme.AccentColor : ColorScheme.Surface.WithAlpha(200), IsAntialias = true };
            if (!primary) 
            {
                 paint.Style = SKPaintStyle.Stroke;
                 paint.Color = ColorScheme.BorderColor;
            }
            canvas.DrawRoundRect(rect, 4, 4, paint);
            
            var textP = new SKPaint { Color = primary ? SKColors.White : ColorScheme.ForeColor, IsAntialias = true, TextAlign = SKTextAlign.Center };
            var font = GetCachedFont();
            canvas.DrawText(text, rect.MidX, rect.MidY + font.Size/2 - 2, font, textP);
        }

        private void HandleCustomMouseDown(MouseEventArgs e)
        {
            float pad = 12 * ScaleFactor;
            var bounds = ClientRectangle;
            var svRect = new SkiaSharp.SKRect(pad, pad, bounds.Width - pad, bounds.Width - pad);
            var hueHeight = 20 * ScaleFactor;
            var hueRect = new SkiaSharp.SKRect(pad, svRect.Bottom + pad, bounds.Width - pad, svRect.Bottom + pad + hueHeight);
            
             var btnAreaY = hueRect.Bottom + pad;
             var buttonHeight = 28 * ScaleFactor;
             var w = 80 * ScaleFactor;
             var cancelRect = new SkiaSharp.SKRect(bounds.Width - pad - w - 10 - w, btnAreaY, bounds.Width - pad - w - 10, btnAreaY + buttonHeight);
             var saveRect = new SkiaSharp.SKRect(bounds.Width - pad - w, btnAreaY, bounds.Width - pad, btnAreaY + buttonHeight);

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
                _picker.SelectedItem = _currentHsv.ToColor();
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
             var svRect = new SkiaSharp.SKRect(pad, pad, bounds.Width - pad, bounds.Width - pad);
             var hueHeight = 20 * ScaleFactor;
             var hueRect = new SkiaSharp.SKRect(pad, svRect.Bottom + pad, bounds.Width - pad, svRect.Bottom + pad + hueHeight);

            if (_draggingSv) UpdateSv(e.X, e.Y, svRect);
            if (_draggingHue) UpdateHue(e.X, hueRect);
        }

        private void UpdateSv(float x, float y, SkiaSharp.SKRect rect)
        {
            float s = Math.Clamp((x - rect.Left) / rect.Width, 0f, 1f);
            float v = Math.Clamp(1f - (y - rect.Top) / rect.Height, 0f, 1f);
            _currentHsv.S = s;
            _currentHsv.V = v;
            Invalidate();
        }

        private void UpdateHue(float x, SkiaSharp.SKRect rect)
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

            public SkiaSharp.SKColor ToColor()
            {
                return SkiaSharp.SKColor.FromHsv(H, S, V);
            }

            public static HsvColor FromColor(SkiaSharp.SKColor color)
            {
                color.ToHsv(out var h, out var s, out var v);

                return new HsvColor(h, s, v);
            }
        }

        #endregion

        #region Grid/List Draw

        private void DrawList(SKCanvas canvas)
        {
             if (Width <= 0 || Height <= 0 || _owner.Items.Count == 0) return;

             var surfaceColor = ColorScheme.Surface;
             _bgPaint ??= new SKPaint { Color = surfaceColor, IsAntialias = true };
             canvas.DrawRect(new SkiaSharp.SKRect(0, 0, Width, Height), _bgPaint);
             
             var font = GetCachedFont();
             _textPaint ??= new SKPaint { IsAntialias = true, Color = ColorScheme.ForeColor };

             int startIndex = (int)_scrollOffset;
             float currentY = VERTICAL_PADDING;
             
             for (int i = startIndex; i < _owner.Items.Count; i++)
             {
                 if (currentY + ItemHeight > Height - VERTICAL_PADDING) break;
                 
                 var w = Width - (_scrollBar.Visible ? SCROLL_BAR_WIDTH + 4 : 0);
                 var itemMargin = (int)(10 * ScaleFactor);
                 var itemRect = new SkiaSharp.SKRect(itemMargin, currentY, w - itemMargin, currentY + ItemHeight);
                 
                 bool isSelected = i == _selectedIndex;
                 bool isHovered = i == _hoveredIndex;
                 
                  if (isSelected || isHovered)
                 {
                     _selectionPaint ??= new SKPaint { IsAntialias = true, Color = ColorScheme.AccentColor.WithAlpha(70) };
                     canvas.DrawRoundRect(new SKRoundRect(itemRect, CORNER_RADIUS), _selectionPaint);
                 }
                 
                 var item = _owner.Items[i];
                 
                 if (item is SkiaSharp.SKColor c)
                 {
                     var swatchSize = ItemHeight - 12;
                     var swatchRect = new SkiaSharp.SKRect(itemRect.Left + 6, itemRect.Top + 6, itemRect.Left + 6 + swatchSize, itemRect.Top + 6 + swatchSize);
                     using var p = new SKPaint { Color = c, IsAntialias = true };
                     canvas.DrawRoundRect(swatchRect, 4, 4, p);
                     using var pb = new SKPaint { Color = SKColors.Gray, IsStroke = true, IsAntialias = true };
                     canvas.DrawRoundRect(swatchRect, 4, 4, pb);
                     
                     string text = $"#{c.Red:X2}{c.Green:X2}{c.Blue:X2}";
                    TextRenderingHelper.DrawText(canvas, text, swatchRect.Right + 12, currentY + ItemHeight / 2 - font.Size/2, font, _textPaint);
                 }
                 else
                 {
                      string text = item?.ToString() ?? "";
                    TextRenderingHelper.DrawText(canvas, text, itemRect.Left + 10, currentY + ItemHeight / 2 - font.Size/2, font, _textPaint);
                 }
                 
                 currentY += ItemHeight;
             }
        }

        private void DrawGrid(SKCanvas canvas)
        {
             if (Width <= 0 || Height <= 0 || _owner.Items.Count == 0) return;

             var surfaceColor = ColorScheme.Surface;
             _bgPaint ??= new SKPaint { Color = surfaceColor, IsAntialias = true };
             canvas.DrawRect(new SkiaSharp.SKRect(0, 0, Width, Height), _bgPaint);
             
             var cellSize = ItemHeight; 
             int startIndex = (int)_scrollOffset;
             
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
                 
                 var rect = new SkiaSharp.SKRect(x + 2, y + 2, x + cellSize - 2, y + cellSize - 2);
                 
                 bool isSelected = idx == _selectedIndex;
                 bool isHovered = idx == _hoveredIndex;
                 
                 if (item is SKColor c)
                 {
                     using var p = new SKPaint { Color = c, IsAntialias = true };
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
