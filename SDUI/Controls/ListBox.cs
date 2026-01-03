using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Collections;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;

namespace SDUI.Controls
{
    public class ListBox : UIElementBase
    {
        public BorderStyle BorderStyle;

        private IndexedList<object> _items;
        private int _selectedIndex;
        private int _itemHeight;
        private int _topIndex;
        private int _leftIndex;
        private readonly ScrollBar _verticalScrollBar;
        private readonly ScrollBar _horizontalScrollBar;
        private bool _autoScroll;
        private int _maxItemWidth;

        public event EventHandler SelectedIndexChanged;

        public ListBox()
        {
            _items = [];
            _selectedIndex = -1;
            _itemHeight = 30;
            _topIndex = 0;
            _leftIndex = 0;
            _autoScroll = true;

            _verticalScrollBar = new ScrollBar
            {
                Dock = DockStyle.Right,
                Width = 15,
                Minimum = 0,
                SmallChange = 1,
                LargeChange = 5,
                Visible = false,
                Orientation = Orientation.Vertical
            };

            _horizontalScrollBar = new ScrollBar
            {
                Dock = DockStyle.Bottom,
                Height = 15,
                Minimum = 0,
                SmallChange = 10,
                LargeChange = 50,
                Visible = false,
                Orientation = Orientation.Horizontal
            };

            _verticalScrollBar.ValueChanged += OnVerticalScrollValueChanged;
            _horizontalScrollBar.ValueChanged += OnHorizontalScrollValueChanged;

            Controls.Add(_verticalScrollBar);
            Controls.Add(_horizontalScrollBar);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IndexedList<object> Items
        {
            get => _items;
            set
            {
                _items = value ?? [];
                UpdateMaxItemWidth();
                UpdateScrollbars();
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnSelectedIndexChanged(EventArgs.Empty);
                    EnsureVisible(_selectedIndex);
                    Invalidate();
                }
            }
        }

        [DefaultValue(30)]
        public int ItemHeight
        {
            get => _itemHeight;
            set
            {
                if (_itemHeight != value)
                {
                    _itemHeight = value;
                    UpdateMaxItemWidth();
                    UpdateScrollbars();
                    Invalidate();
                }
            }
        }

        [DefaultValue(true)]
        public bool AutoScroll
        {
            get => _autoScroll;
            set
            {
                if (_autoScroll != value)
                {
                    _autoScroll = value;
                    UpdateScrollbars();
                }
            }
        }

        public object SelectedItem
        {
            get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
            set
            {
                var index = _items.IndexOf(value);
                if (index != -1)
                    SelectedIndex = index;
            }
        }

        private void UpdateMaxItemWidth()
        {
            _maxItemWidth = 0;
            using var font = new SKFont
            {
                Size = ItemHeight - 10,
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
                Edging = SKFontEdging.SubpixelAntialias
            };

            foreach (var item in _items)
            {
                var text = item?.ToString() ?? string.Empty;
                var width = (int)font.MeasureText(text);
                _maxItemWidth = Math.Max(_maxItemWidth, width + 10); // 10 pixel padding
            }
        }

        private bool _isUpdatingScrollbars = false;

        private void UpdateScrollbars()
        {
            if (_isUpdatingScrollbars) return;
            _isUpdatingScrollbars = true;

            try
            {
                if (!_autoScroll)
                {
                    if (_verticalScrollBar.Visible) _verticalScrollBar.Visible = false;
                    if (_horizontalScrollBar.Visible) _horizontalScrollBar.Visible = false;
                    return;
                }

                var totalItems = _items.Count;

                // Two-step visibility evaluation to account for scrollbar interaction
                var heightNoH = Height - Padding.Vertical; // assume no horizontal bar
                var visibleItemsNoH = heightNoH / ItemHeight;
                var needV = totalItems > visibleItemsNoH;

                var clientWidthIfV = Width - (needV ? _verticalScrollBar.Width : 0);
                var needH = _maxItemWidth > clientWidthIfV;

                // Re-evaluate vertical need now that horizontal may be present
                var heightIfH = Height - Padding.Vertical - (needH ? _horizontalScrollBar.Height : 0);
                var visibleItemsIfH = heightIfH / ItemHeight;
                needV = totalItems > visibleItemsIfH;

                if (needV != _verticalScrollBar.Visible) _verticalScrollBar.Visible = needV;
                if (needH != _horizontalScrollBar.Visible) _horizontalScrollBar.Visible = needH;

                // Update vertical scrollbar range using final visibleItemsIfH
                if (_verticalScrollBar.Visible)
                {
                    var max = Math.Max(0, totalItems - visibleItemsIfH);
                    if (_verticalScrollBar.Maximum != max) _verticalScrollBar.Maximum = max;
                    if (_verticalScrollBar.Value > _verticalScrollBar.Maximum) _verticalScrollBar.Value = _verticalScrollBar.Maximum;
                    var newHeight = Height - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Height : 0);
                    if (_verticalScrollBar.Height != newHeight) _verticalScrollBar.Height = newHeight;
                    var newX = Width - _verticalScrollBar.Width;
                    if (_verticalScrollBar.Location.X != newX) _verticalScrollBar.Location = new Point(newX, 0);
                }

                // Update horizontal scrollbar using final vertical visibility
                var clientWidth = Width - (_verticalScrollBar.Visible ? _verticalScrollBar.Width : 0);
                if (_horizontalScrollBar.Visible)
                {
                    var max = Math.Max(0, _maxItemWidth - clientWidth);
                    if (_horizontalScrollBar.Maximum != max) _horizontalScrollBar.Maximum = max;
                    if (_horizontalScrollBar.Value > _horizontalScrollBar.Maximum) _horizontalScrollBar.Value = _horizontalScrollBar.Maximum;

                    var newWidth = Width - (_verticalScrollBar.Visible ? _verticalScrollBar.Width : 0);
                    if (_horizontalScrollBar.Width != newWidth) _horizontalScrollBar.Width = newWidth;
                    var newY = Height - _horizontalScrollBar.Height;
                    if (_horizontalScrollBar.Location.Y != newY) _horizontalScrollBar.Location = new Point(0, newY);
                }
            }
            finally
            {
                _isUpdatingScrollbars = false;
            }
        }

        public void EnsureVisible(int index)
        {
            if (!_autoScroll || index < 0 || index >= _items.Count)
                return;

            var visibleItems = (Height - Padding.Vertical - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Height : 0)) / ItemHeight;

            if (index < _topIndex)
                _verticalScrollBar.Value = index;
            else if (index >= _topIndex + visibleItems)
                _verticalScrollBar.Value = Math.Min(index - visibleItems + 1, _verticalScrollBar.Maximum);
        }

        public void BeginUpdate() { SuspendLayout(); }

        public void EndUpdate()
        {
            ResumeLayout(true);
            UpdateMaxItemWidth();
            UpdateScrollbars();
            Invalidate();
        }

        private void OnVerticalScrollValueChanged(object sender, EventArgs e)
        {
            _topIndex = _verticalScrollBar.Value;
            Invalidate();
        }

        private void OnHorizontalScrollValueChanged(object sender, EventArgs e)
        {
            _leftIndex = _horizontalScrollBar.Value;
            Invalidate();
        }

        public override void OnPaint(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            using var bgPaint = new SKPaint
            {
                Color = ColorScheme.BackColor.ToSKColor()
            };
            canvas.DrawRect(0, 0, Width, Height, bgPaint);

            using var font = new SKFont
            {
                Size = 9f.PtToPx(this),
                Typeface = SDUI.Helpers.FontManager.GetSKTypeface(Font),
                Edging = SKFontEdging.SubpixelAntialias
            };

            using var paint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                IsAntialias = true
            };

            var clientRect = new Rectangle(
                0,
                0,
                Width - (_verticalScrollBar.Visible ? _verticalScrollBar.Width : 0),
                Height - (_horizontalScrollBar.Visible ? _horizontalScrollBar.Height : 0)
            );

            var visibleItems = clientRect.Height / ItemHeight;

            for (int i = _topIndex; i < Math.Min(_items.Count, _topIndex + visibleItems + 1); i++)
            {
                var itemRect = new SKRect(
                    clientRect.Left - _leftIndex,
                    (i - _topIndex) * ItemHeight,
                    clientRect.Right,
                    (i - _topIndex + 1) * ItemHeight
                );

                if (i == _selectedIndex)
                {
                    using var selectionPaint = new SKPaint
                    {
                        Color = ColorScheme.AccentColor.ToSKColor()
                    };
                    canvas.DrawRect(itemRect, selectionPaint);
                    paint.Color = ColorScheme.AccentColor.Determine().ToSKColor();
                }
                else
                {
                    paint.Color = ColorScheme.ForeColor.ToSKColor();
                }

                var text = _items[i]?.ToString() ?? string.Empty;
                var y = itemRect.MidY - (font.Metrics.Ascent + font.Metrics.Descent) / 2;
                TextRenderingHelper.DrawText(canvas, text, itemRect.Left + 5, y, SKTextAlign.Left, font, paint);
            }
        }

        internal override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                var index = _topIndex + (e.Y / ItemHeight);
                if (index >= 0 && index < _items.Count)
                {
                    SelectedIndex = index;
                }
            }
        }

        internal override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int scrollLines = SystemInformation.MouseWheelScrollLines;
            int delta = (e.Delta / 120) * scrollLines;

            // Shift+wheel => horizontal scroll
            if ((ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                var newLeft = _horizontalScrollBar.Value - delta * 10; // multiplier to feel snappier
                newLeft = Math.Max(_horizontalScrollBar.Minimum, Math.Min(_horizontalScrollBar.Maximum, newLeft));
                _horizontalScrollBar.Value = newLeft;
                _leftIndex = _horizontalScrollBar.Value;
            }
            else
            {
                var newTop = _verticalScrollBar.Value - delta;
                newTop = Math.Max(_verticalScrollBar.Minimum, Math.Min(_verticalScrollBar.Maximum, newTop));
                _verticalScrollBar.Value = newTop;
                _topIndex = _verticalScrollBar.Value;
            }

            Invalidate();
        }
        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }

        public void AddItem(object item)
        {
            _items.Add(item);
            UpdateMaxItemWidth();
            UpdateScrollbars();
            Invalidate();
        }

        public void RemoveItem(object item)
        {
            if (_items.Remove(item))
            {
                UpdateMaxItemWidth();
                UpdateScrollbars();
                Invalidate();
            }
        }

        public void ClearItems()
        {
            _items.Clear();
            _selectedIndex = -1;
            _maxItemWidth = 0;
            UpdateScrollbars();
            Invalidate();
        }

        protected override void OnLayout(UILayoutEventArgs e)
        {
            base.OnLayout(e);
            UpdateScrollbars();
        }
    }
}
