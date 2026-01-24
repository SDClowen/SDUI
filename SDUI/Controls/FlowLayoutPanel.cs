using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SDUI.Animation;
using SkiaSharp;

namespace SDUI.Controls;

public class FlowLayoutPanel : UIElementBase
{
    #region Enums

    public enum FlowAlignment
    {
        Near,
        Center,
        Far
    }

    #endregion

    public FlowLayoutPanel()
    {
        BackColor = Color.Transparent;

        _vScrollBar = new ScrollBar
        {
            Dock = DockStyle.Right,
            Visible = false,
            Orientation = Orientation.Vertical
        };
        _hScrollBar = new ScrollBar
        {
            Dock = DockStyle.Bottom,
            Visible = false,
            Orientation = Orientation.Horizontal
        };

        _vScrollBar.ValueChanged += ScrollBar_ValueChanged;
        _hScrollBar.ValueChanged += ScrollBar_ValueChanged;

        Controls.Add(_vScrollBar);
        Controls.Add(_hScrollBar);
    }

    private void ScrollBar_ValueChanged(object sender, EventArgs e)
    {
        if (_isLayouting) return;
        PerformLayout();
        Invalidate();
    }

    internal override void OnControlAdded(UIElementEventArgs e)
    {
        base.OnControlAdded(e);
        if (e.Element is UIElementBase el && (el == _vScrollBar || el == _hScrollBar)) return;

        // Yeni kontrol için animasyon motoru oluştur
        _animations[e.Element] = new AnimationManager()
        {
            Increment = 0.25f,
            AnimationType = AnimationType.Linear,
            InterruptAnimation = true
        };

        PerformLayout();
    }

    internal override void OnControlRemoved(UIElementEventArgs e)
    {
        base.OnControlRemoved(e);
        if (e.Element is UIElementBase el && (el == _vScrollBar || el == _hScrollBar)) return;

        // Kontrol kaldırıldığında animasyonu temizle
        if (_animations.ContainsKey(e.Element))
        {
            _animations[e.Element]?.Dispose();
            _animations.Remove(e.Element);
        }

        if (_targetLocations.ContainsKey(e.Element)) _targetLocations.Remove(e.Element);

        PerformLayout();
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_autoScroll)
        {
            var clientArea = GetClientArea();
            var hover = clientArea.Contains(e.Location);
            if (hover != _hoveringContentArea)
            {
                _hoveringContentArea = hover;
                _vScrollBar?.SetHostHover(hover);
                _hScrollBar?.SetHostHover(hover);
            }
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_autoScroll && _hoveringContentArea)
        {
            _hoveringContentArea = false;
            _vScrollBar?.SetHostHover(false);
            _hScrollBar?.SetHostHover(false);
        }
    }

    protected override void OnLayout(UILayoutEventArgs e)
    {
        if (_isLayouting) return;

        _isLayouting = true;

        var controls = Controls.OfType<UIElementBase>()
            .Where(c => c != _vScrollBar && c != _hScrollBar && c.Visible)
            .ToList();

        if (controls.Count == 0)
        {
            _isLayouting = false;
            return;
        }

        var clientArea = GetClientArea();
        var hOffset = _autoScroll && _hScrollBar.Visible ? _hScrollBar.Value : 0;
        var vOffset = _autoScroll && _vScrollBar.Visible ? _vScrollBar.Value : 0;
        var contentOriginX = clientArea.Left - hOffset;
        var contentOriginY = clientArea.Top - vOffset;
        var currentX = contentOriginX;
        var currentY = contentOriginY;
        var rowHeight = 0;
        var columnWidth = 0;
        var contentMaxWidth = 0;
        var contentMaxHeight = 0;

        // Ensure AutoSize children are sized to their preferred sizes before layout calculations
        foreach (var control in controls)
            if (control.AutoSize)
            {
                // Provide the available client area as proposed size so children can measure accordingly
                var proposed = control.GetPreferredSize(new Size(clientArea.Width, clientArea.Height));
                if (control.AutoSizeMode == AutoSizeMode.GrowOnly)
                {
                    proposed.Width = Math.Max(control.Size.Width, proposed.Width);
                    proposed.Height = Math.Max(control.Size.Height, proposed.Height);
                }

                // Clamp to min/max
                if (control.MinimumSize.Width > 0) proposed.Width = Math.Max(proposed.Width, control.MinimumSize.Width);
                if (control.MinimumSize.Height > 0)
                    proposed.Height = Math.Max(proposed.Height, control.MinimumSize.Height);
                if (control.MaximumSize.Width > 0) proposed.Width = Math.Min(proposed.Width, control.MaximumSize.Width);
                if (control.MaximumSize.Height > 0)
                    proposed.Height = Math.Min(proposed.Height, control.MaximumSize.Height);

                if (control.Size != proposed)
                    control.Size = proposed;
            }

        // Yatay düzenleme
        if (_flowDirection == FlowDirection.LeftToRight || _flowDirection == FlowDirection.RightToLeft)
        {
            var row = new List<UIElementBase>();

            foreach (var control in controls)
            {
                // Satır sonuna gelindi mi kontrol et (kullanılabilir genişlik clientArea içinde hesaplanır)
                if (_wrapContents && currentX + control.Width + _itemPadding.Left > clientArea.Right - hOffset)
                {
                    // Mevcut satırı hizala
                    AlignRow(row, currentY, rowHeight, clientArea);
                    row.Clear();

                    currentX = contentOriginX;
                    currentY += rowHeight + _itemPadding.Top;
                    rowHeight = 0;
                }

                var targetPoint = new Point(currentX, currentY);
                _targetLocations[control] = targetPoint;

                // Animasyon başlat
                if (!control.Location.Equals(targetPoint)) StartAnimation(control, targetPoint);

                // Hesaplama: içeriğin sağ sınırını güncelle
                var endX = currentX + control.Width;
                currentX += control.Width + _itemPadding.Left;
                rowHeight = Math.Max(rowHeight, control.Height);
                contentMaxWidth = Math.Max(contentMaxWidth, endX - contentOriginX);
                row.Add(control);
            }

            // Son satırı hizala
            if (row.Count > 0) AlignRow(row, currentY, rowHeight, clientArea);

            contentMaxHeight = Math.Max(contentMaxHeight, currentY + rowHeight - contentOriginY);
        }
        // Dikey düzenleme
        else
        {
            var column = new List<UIElementBase>();

            foreach (var control in controls)
            {
                // Sütun sonuna gelindi mi kontrol et
                if (_wrapContents && currentY + control.Height + _itemPadding.Top > clientArea.Bottom - vOffset)
                {
                    // Mevcut sütunu hizala
                    AlignColumn(column, currentX, columnWidth, clientArea);
                    column.Clear();

                    currentY = contentOriginY;
                    currentX += columnWidth + _itemPadding.Left;
                    columnWidth = 0;
                }

                var targetPoint = new Point(currentX, currentY);
                _targetLocations[control] = targetPoint;

                // Animasyon başlat
                if (!control.Location.Equals(targetPoint)) StartAnimation(control, targetPoint);

                var endY = currentY + control.Height;
                currentY += control.Height + _itemPadding.Top;
                columnWidth = Math.Max(columnWidth, control.Width);
                contentMaxHeight = Math.Max(contentMaxHeight, endY - contentOriginY);
                column.Add(control);
            }

            // Son sütunu hizala
            if (column.Count > 0) AlignColumn(column, currentX, columnWidth, clientArea);

            contentMaxWidth = Math.Max(contentMaxWidth, currentX + columnWidth - contentOriginX);
        }

        // ScrollBar'ları güncelle
        if (_autoScroll) UpdateScrollBars(contentMaxWidth, contentMaxHeight);

        _isLayouting = false;
    }

    private void StartAnimation(UIElementBase control, Point targetPoint)
    {
        // Animasyonu devre dışı bırak - direkt pozisyon değiştir
        control.Location = targetPoint;
    }

    private void AlignRow(List<UIElementBase> row, int y, int height, Rectangle clientArea)
    {
        if (row.Count == 0) return;

        var totalWidth = row.Sum(c => c.Width) + (row.Count - 1) * _itemPadding.Left;
        var startXBase = clientArea.Left - (_hScrollBar?.Value ?? 0);
        var startX = startXBase;

        switch (_horizontalAlignment)
        {
            case FlowAlignment.Center:
                startX = startXBase + (clientArea.Width - totalWidth) / 2;
                break;
            case FlowAlignment.Far:
                startX = startXBase + (clientArea.Width - totalWidth);
                break;
        }

        foreach (var control in row)
        {
            var targetY = y;
            switch (_verticalAlignment)
            {
                case FlowAlignment.Center:
                    targetY += (height - control.Height) / 2;
                    break;
                case FlowAlignment.Far:
                    targetY += height - control.Height;
                    break;
            }

            _targetLocations[control] = new Point(startX, targetY);
            startX += control.Width + _itemPadding.Left;
        }
    }

    private void AlignColumn(List<UIElementBase> column, int x, int width, Rectangle clientArea)
    {
        if (column.Count == 0) return;

        var totalHeight = column.Sum(c => c.Height) + (column.Count - 1) * _itemPadding.Top;
        var startYBase = clientArea.Top - (_vScrollBar?.Value ?? 0);
        var startY = startYBase;

        switch (_verticalAlignment)
        {
            case FlowAlignment.Center:
                startY = startYBase + (clientArea.Height - totalHeight) / 2;
                break;
            case FlowAlignment.Far:
                startY = startYBase + (clientArea.Height - totalHeight);
                break;
        }

        foreach (var control in column)
        {
            var targetX = x;
            switch (_horizontalAlignment)
            {
                case FlowAlignment.Center:
                    targetX += (width - control.Width) / 2;
                    break;
                case FlowAlignment.Far:
                    targetX += width - control.Width;
                    break;
            }

            _targetLocations[control] = new Point(targetX, startY);
            startY += control.Height + _itemPadding.Top;
        }
    }

    private Rectangle GetClientArea()
    {
        var area = ClientRectangle;
        // Respect own Padding as content inset
        area.X += Padding.Left;
        area.Y += Padding.Top;
        area.Width -= Padding.Horizontal;
        area.Height -= Padding.Vertical;

        if (_autoScroll)
        {
            if (_vScrollBar.Visible)
                area.Width -= _vScrollBar.Width;
            if (_hScrollBar.Visible)
                area.Height -= _hScrollBar.Height;
        }

        return area;
    }

    private void UpdateScrollBars(int contentWidth, int contentHeight)
    {
        // Determine thresholds combining client area and AutoScrollMinSize
        var clientArea = GetClientArea();
        var clientW = clientArea.Width;
        var clientH = clientArea.Height;

        var hThreshold = Math.Max(clientW, AutoScrollMinSize.Width);
        var vThreshold = Math.Max(clientH, AutoScrollMinSize.Height);

        var needHScroll = contentWidth > hThreshold;
        var needVScroll = contentHeight > vThreshold;

        // Re-evaluate if one scrollbar appearing would reduce client area
        if (needHScroll && !_hScrollBar.Visible)
            needVScroll = contentHeight > Math.Max(clientH - _hScrollBar.Height, AutoScrollMinSize.Height);
        if (needVScroll && !_vScrollBar.Visible)
            needHScroll = contentWidth > Math.Max(clientW - _vScrollBar.Width, AutoScrollMinSize.Width);

        _hScrollBar.Visible = needHScroll;
        _vScrollBar.Visible = needVScroll;

        if (needHScroll)
        {
            _hScrollBar.Minimum = 0;
            _hScrollBar.Maximum = contentWidth;
            _hScrollBar.LargeChange = clientW;
            _hScrollBar.SmallChange = _itemPadding.Left;
        }

        if (needVScroll)
        {
            _vScrollBar.Minimum = 0;
            _vScrollBar.Maximum = contentHeight;
            _vScrollBar.LargeChange = clientH;
            _vScrollBar.SmallChange = _itemPadding.Top;
        }
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        // Basit arka plan çizimi - performans için gölge ve rounded corner'ları kaldır
        if (BackColor != Color.Transparent)
        {
            using var paint = new SKPaint
            {
                Color = BackColor.ToSKColor(),
                IsAntialias = false
            };
            canvas.DrawRect(0, 0, Width, Height, paint);
        }
        
        // Kenarlık varsa basit çiz
        if (_border.All > 0 || _border.Left > 0 || _border.Top > 0 || _border.Right > 0 || _border.Bottom > 0)
        {
            var borderColor = _borderColor == Color.Transparent ? ColorScheme.BorderColor : _borderColor;
            using var paint = new SKPaint
            {
                Color = borderColor.ToSKColor(),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = false
            };

            if (_border.Left > 0)
                canvas.DrawLine(0, 0, 0, Height, paint);
            if (_border.Top > 0)
                canvas.DrawLine(0, 0, Width, 0, paint);
            if (_border.Right > 0)
                canvas.DrawLine(Width - 1, 0, Width - 1, Height, paint);
            if (_border.Bottom > 0)
                canvas.DrawLine(0, Height - 1, Width, Height - 1, paint);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var animation in _animations.Values)
                animation?.Dispose();

            _animations.Clear();
            _targetLocations.Clear();

            if (_vScrollBar != null)
            {
                _vScrollBar.ValueChanged -= ScrollBar_ValueChanged;
                _vScrollBar.Dispose();
            }

            if (_hScrollBar != null)
            {
                _hScrollBar.ValueChanged -= ScrollBar_ValueChanged;
                _hScrollBar.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    #region Variables

    private int _radius = 10;
    private Padding _border;
    private Color _borderColor = Color.Transparent;
    private float _shadowDepth = 4;
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
    private bool _wrapContents = true;
    private bool _autoScroll;
    private FlowAlignment _verticalAlignment = FlowAlignment.Near;
    private FlowAlignment _horizontalAlignment = FlowAlignment.Near;
    private Padding _itemPadding = new(3);
    private readonly Dictionary<IUIElement, Point> _targetLocations = new();
    private readonly Dictionary<IUIElement, AnimationManager> _animations = new();
    private readonly ScrollBar _vScrollBar;
    private readonly ScrollBar _hScrollBar;
    private bool _isLayouting;
    private bool _hoveringContentArea;

    #endregion

    #region Properties

    [Category("Appearance")]
    public int Radius
    {
        get => _radius;
        set
        {
            if (_radius == value)
                return;

            _radius = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Padding Border
    {
        get => _border;
        set
        {
            if (_border == value)
                return;

            _border = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor == value)
                return;

            _borderColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    public float ShadowDepth
    {
        get => _shadowDepth;
        set
        {
            if (_shadowDepth == value)
                return;

            _shadowDepth = value;
            Invalidate();
        }
    }

    [Category("Layout")]
    public FlowDirection FlowDirection
    {
        get => _flowDirection;
        set
        {
            if (_flowDirection == value)
                return;

            _flowDirection = value;
            PerformLayout();
        }
    }

    [Category("Layout")]
    public bool WrapContents
    {
        get => _wrapContents;
        set
        {
            if (_wrapContents == value)
                return;

            _wrapContents = value;
            PerformLayout();
        }
    }

    [Category("Layout")]
    public bool AutoScroll
    {
        get => _autoScroll;
        set
        {
            if (_autoScroll == value)
                return;

            _autoScroll = value;
            _vScrollBar.Visible = _hScrollBar.Visible = value;
            PerformLayout();
        }
    }

    [Category("Layout")]
    public FlowAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set
        {
            if (_verticalAlignment == value)
                return;

            _verticalAlignment = value;
            PerformLayout();
        }
    }

    [Category("Layout")]
    public FlowAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set
        {
            if (_horizontalAlignment == value)
                return;

            _horizontalAlignment = value;
            PerformLayout();
        }
    }

    [Category("Layout")]
    public Padding ItemPadding
    {
        get => _itemPadding;
        set
        {
            if (_itemPadding == value)
                return;

            _itemPadding = value;
            PerformLayout();
        }
    }

    #endregion
}