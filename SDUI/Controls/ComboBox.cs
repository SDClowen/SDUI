using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using SDUI.Animation;

namespace SDUI.Controls;

public class ComboBox : SKControl
{
    private class DropDownMenu : SKControl
    {
        private readonly ComboBox _owner;
        private int _hoverIndex = -1;
        private readonly AnimationEngine _animation;
        private float _currentHeight;
        private float _targetHeight;
        private float _hoverY;
        private float _targetHoverY;
        private readonly AnimationEngine _hoverAnimation;

        private const int ITEM_HEIGHT = 32;
        private const int ITEM_PADDING = 8;
        private const int VERTICAL_PADDING = 8;
        private const float CORNER_RADIUS = 8;
        private const int SHADOW_OFFSET = 6;
        private const int SHADOW_BLUR = 16;

        public DropDownMenu(ComboBox owner)
        {
            _owner = owner;
            BackColor = Color.Transparent;
            
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            
            _animation = new AnimationEngine
            {
                Increment = 0.15,
                AnimationType = AnimationType.EaseInOut
            };
            _animation.OnAnimationProgress += (s) => 
            {
                var progress = (float)_animation.GetProgress();
                _currentHeight = _currentHeight + (_targetHeight - _currentHeight) * progress;
                Height = (int)_currentHeight;
                
                if (Height <= 0 && _animation.GetDirection() == AnimationDirection.Out)
                {
                    Visible = false;
                    _owner.DroppedDown = false;
                    if (Parent != null)
                    {
                        var parentControls = Parent.Controls;
                        parentControls.Remove(this);
                    }
                }
                
                Invalidate();
            };

            _hoverAnimation = new AnimationEngine
            {
                Increment = 0.25,
                AnimationType = AnimationType.EaseInOut
            };
            _hoverAnimation.OnAnimationProgress += (s) => 
            {
                var progress = (float)_hoverAnimation.GetProgress();
                _hoverY = _hoverY + (_targetHoverY - _hoverY) * progress;
                Invalidate();
            };
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            var newHoverIndex = GetItemIndexAtPoint(e.Location);
            if (newHoverIndex >= 0 && newHoverIndex < _owner.Items.Count)
            {
                if (newHoverIndex != _hoverIndex)
                {
                    _hoverIndex = newHoverIndex;
                    _targetHoverY = VERTICAL_PADDING + (_hoverIndex * ITEM_HEIGHT);
                    _hoverAnimation.StartNewAnimation(AnimationDirection.In);
                }
            }
            else 
            {
                _hoverIndex = -1;
                _hoverAnimation.StartNewAnimation(AnimationDirection.Out);
            }
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            _hoverAnimation.StartNewAnimation(AnimationDirection.Out);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                var itemIndex = GetItemIndexAtPoint(e.Location);
                if (itemIndex >= 0 && itemIndex < _owner.Items.Count)
                {
                    _owner.SelectedIndex = itemIndex;
                    _owner.OnSelectionChangeCommitted(EventArgs.Empty);
                    BeginClose();
                }
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            if (Height <= 0) return;

            // Gölge
            using (var shadowPaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha(100),
                ImageFilter = SKImageFilter.CreateDropShadow(0, SHADOW_OFFSET, SHADOW_BLUR, SHADOW_BLUR, SKColors.Black.WithAlpha(180)),
                IsAntialias = true
            })
            {
                var shadowRect = new SKRoundRect(new SKRect(0, 0, Width, Height), CORNER_RADIUS);
                canvas.DrawRoundRect(shadowRect, shadowPaint);
            }

            // Arka plan
            using (var paint = new SKPaint
            {
                Color = ColorScheme.BackColor.ToSKColor(),
                IsAntialias = true
            })
            {
                var rect = new SKRoundRect(new SKRect(0, 0, Width, Height), CORNER_RADIUS);
                canvas.DrawRoundRect(rect, paint);
            }

            // Hover efekti
            if (_hoverIndex >= 0 && _hoverAnimation.GetProgress() > 0)
            {
                using var hoverPaint = new SKPaint
                {
                    Color = ColorScheme.AccentColor.Alpha(30).ToSKColor(),
                    IsAntialias = true
                };
                var hoverRect = new SKRoundRect(
                    new SKRect(ITEM_PADDING, _hoverY, Width - ITEM_PADDING, _hoverY + ITEM_HEIGHT), 6);
                canvas.DrawRoundRect(hoverRect, hoverPaint);
            }

            // Öğeleri çiz
            using var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor.ToSKColor(),
                TextSize = _owner.Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(_owner.Font.FontFamily.Name),
                IsAntialias = true,
                SubpixelText = true
            };

            float y = VERTICAL_PADDING;
            for (int i = 0; i < _owner.Items.Count; i++)
            {
                var itemRect = new SKRect(ITEM_PADDING, y, Width - ITEM_PADDING, y + ITEM_HEIGHT);

                // Seçili öğe için arka plan
                if (i == _owner.SelectedIndex)
                {
                    using var selectedPaint = new SKPaint
                    {
                        Color = ColorScheme.AccentColor.Alpha(50).ToSKColor(),
                        IsAntialias = true
                    };
                    var selectedRect = new SKRoundRect(itemRect, 6);
                    canvas.DrawRoundRect(selectedRect, selectedPaint);
                }

                var text = _owner.GetItemText(_owner.Items[i]);

                // Metin rengi ayarla
                textPaint.Color = (i == _owner.SelectedIndex) 
                    ? ColorScheme.AccentColor.ToSKColor() 
                    : ColorScheme.ForeColor.ToSKColor();

                // Metni dikey olarak ortala
                var textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);
                float textY = y + (ITEM_HEIGHT / 2) + (Math.Abs(textPaint.FontMetrics.Ascent + textPaint.FontMetrics.Descent) / 2);
                canvas.DrawText(text, ITEM_PADDING + 4, textY, textPaint);

                y += ITEM_HEIGHT;
            }
        }

        public void BeginOpen()
        {
            _targetHeight = (_owner.Items.Count * ITEM_HEIGHT) + (VERTICAL_PADDING * 2);
            Size = new Size(_owner.DropDownWidth, 0);
            _currentHeight = 0;
            _hoverIndex = -1;
            _hoverY = 0;
            _targetHoverY = 0;
            
            Visible = true;
            _animation.StartNewAnimation(AnimationDirection.In);
            BringToFront();
            
            // Hover animasyonunu sıfırla
            //_hoverAnimation.Reset();
        }

        public void BeginClose()
        {
            _animation.StartNewAnimation(AnimationDirection.Out);
            _owner.DroppedDown = false;
            _owner.OnDropDownClosed(EventArgs.Empty);
            
            if (Parent != null)
            {
                Parent.Controls.Remove(this);
                Visible = false;
            }
        }

        private int GetItemIndexAtPoint(Point point)
        {
            if (point.Y < VERTICAL_PADDING || point.Y > Height - VERTICAL_PADDING)
                return -1;
            
            return (int)Math.Floor((point.Y - VERTICAL_PADDING) / (float)ITEM_HEIGHT);
        }
    }

    public class ObjectCollection : IList
    {
        private readonly List<object> _items;
        private readonly ComboBox _owner;

        public ObjectCollection(ComboBox owner)
        {
            _owner = owner;
            _items = new List<object>();
        }

        public int Count => _items.Count;
        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public object this[int index]
        {
            get => _items[index];
            set
            {
                _items[index] = value;
                _owner.Invalidate();
            }
        }

        public int Add(object value)
        {
            _items.Add(value);
            _owner.Invalidate();
            return _items.Count - 1;
        }

        public void Clear()
        {
            _items.Clear();
            _owner.Invalidate();
        }

        public bool Contains(object value) => _items.Contains(value);

        public void CopyTo(Array array, int index) => _items.CopyTo((object[])array, index);

        public IEnumerator GetEnumerator() => _items.GetEnumerator();

        public int IndexOf(object value) => _items.IndexOf(value);

        public void Insert(int index, object value)
        {
            _items.Insert(index, value);
            _owner.Invalidate();
        }

        public void Remove(object value)
        {
            _items.Remove(value);
            _owner.Invalidate();
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            _owner.Invalidate();
        }

        public void AddRange(IEnumerable items)
        {
            foreach (var item in items)
            {
                _items.Add(item);
            }
            _owner.Invalidate();
        }
    }

    private int _radius = 5;
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            Invalidate();
        }
    }

    private float _shadowDepth = 4f;
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

    #region ComboBox Properties
    private DrawMode _drawMode = DrawMode.Normal;
    [Browsable(true)]
    public DrawMode DrawMode
    {
        get => _drawMode;
        set
        {
            _drawMode = value;
            Invalidate();
        }
    }

    private bool _formattingEnabled;
    [Browsable(true)]
    public bool FormattingEnabled
    {
        get => _formattingEnabled;
        set => _formattingEnabled = value;
    }

    private bool _integralHeight = true;
    [Browsable(true)]
    public bool IntegralHeight
    {
        get => _integralHeight;
        set => _integralHeight = value;
    }

    private int _itemHeight = 36;
    [Browsable(true)]
    public int ItemHeight
    {
        get => _itemHeight;
        set
        {
            if (_itemHeight == value) return;
            _itemHeight = value;
            if (_dropDownMenu != null && _dropDownMenu.Visible)
            {
                var dropDownHeight = Math.Min(Items.Count * _itemHeight, MaxDropDownItems * _itemHeight);
                _dropDownMenu.Size = new Size(DropDownWidth, dropDownHeight);
            }
            Invalidate();
        }
    }

    private ObjectCollection _items;
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObjectCollection Items
    {
        get
        {
            if (_items == null)
                _items = new ObjectCollection(this);
            return _items;
        }
    }

    private object _selectedItem;
    [Browsable(true)]
    public object SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                _selectedIndex = Items.IndexOf(value);
                OnSelectedItemChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    private int _selectedIndex = -1;
    [Browsable(true)]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                _selectedItem = (_selectedIndex >= 0 && _selectedIndex < Items.Count) ? Items[_selectedIndex] : null;
                OnSelectedIndexChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    private string _displayMember;
    [Browsable(true)]
    public string DisplayMember
    {
        get => _displayMember;
        set
        {
            _displayMember = value;
            Invalidate();
        }
    }

    private string _valueMember;
    [Browsable(true)]
    public string ValueMember
    {
        get => _valueMember;
        set => _valueMember = value;
    }

    private object _dataSource;
    [Browsable(true)]
    public object DataSource
    {
        get => _dataSource;
        set
        {
            _dataSource = value;
            if (value is IList list)
            {
                Items.Clear();
                Items.AddRange(list);
            }
            Invalidate();
        }
    }

    private int _dropDownHeight = 106;
    [Browsable(true)]
    public int DropDownHeight
    {
        get => _dropDownHeight;
        set => _dropDownHeight = value;
    }

    private int _dropDownWidth;
    [Browsable(true)]
    public int DropDownWidth
    {
        get => _dropDownWidth == 0 ? Width : _dropDownWidth;
        set => _dropDownWidth = value;
    }

    private ComboBoxStyle _dropDownStyle = ComboBoxStyle.DropDownList;
    [Browsable(true)]
    public ComboBoxStyle DropDownStyle
    {
        get => _dropDownStyle;
        set
        {
            _dropDownStyle = value;
            Invalidate();
        }
    }

    private int _maxDropDownItems = 8;
    [Browsable(true)]
    public int MaxDropDownItems
    {
        get => _maxDropDownItems;
        set => _maxDropDownItems = value;
    }

    private bool _droppedDown;
    [Browsable(true)]
    public bool DroppedDown
    {
        get => _droppedDown;
        set
        {
            if (_droppedDown != value)
            {
                _droppedDown = value;
                if (value)
                    OnDropDown(EventArgs.Empty);
                else
                    OnDropDownClosed(EventArgs.Empty);
            }
        }
    }
    #endregion

    #region Events
    public event EventHandler SelectedIndexChanged;
    public event EventHandler SelectedItemChanged;
    public event EventHandler DropDown;
    public event EventHandler DropDownClosed;
    public event EventHandler SelectionChangeCommitted;
    public event DrawItemEventHandler DrawItem;
    public event MeasureItemEventHandler MeasureItem;
    #endregion

    private DropDownMenu _dropDownMenu;
    private readonly AnimationEngine _animation;

    public ComboBox()
    {
        SetStyle(
            ControlStyles.Selectable |
            ControlStyles.SupportsTransparentBackColor, true
        );

        MinimumSize = new Size(0, 23);
        Height = 23;
        _itemHeight = 36;

        _animation = new AnimationEngine
        {
            Increment = 0.08,
            AnimationType = AnimationType.Linear
        };
        _animation.OnAnimationProgress += (s) => Invalidate();

        _dropDownMenu = new DropDownMenu(this);
        _dropDownMenu.Visible = false;
    }

    protected virtual void OnSelectedIndexChanged(EventArgs e)
    {
        SelectedIndexChanged?.Invoke(this, e);
    }

    protected virtual void OnSelectedItemChanged(EventArgs e)
    {
        SelectedItemChanged?.Invoke(this, e);
    }

    protected virtual void OnDropDown(EventArgs e)
    {
        DropDown?.Invoke(this, e);
    }

    protected virtual void OnDropDownClosed(EventArgs e)
    {
        DropDownClosed?.Invoke(this, e);
    }

    protected virtual void OnSelectionChangeCommitted(EventArgs e)
    {
        SelectionChangeCommitted?.Invoke(this, e);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var rect = new SKRect(0, 0, Width, Height);
        var animationProgress = (float)_animation.GetProgress();

        // Gölge çizimi
        using (var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(50),
            ImageFilter = SKImageFilter.CreateDropShadow(
                0, _shadowDepth * (1 - animationProgress * 0.5f), // Basılı durumda gölge azalır
                3 * (1 - animationProgress * 0.5f), // Basılı durumda blur azalır
                3 * (1 - animationProgress * 0.5f),
                SKColors.Black.WithAlpha(50)),
            IsAntialias = true
        })
        {
            using var path = new SKPath();
            path.AddRoundRect(rect, _radius, _radius);
            canvas.DrawPath(path, shadowPaint);
        }

        // Arka plan çizimi
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BackColor
                .Alpha(200)
                .ToSKColor()
                .InterpolateColor(ColorScheme.AccentColor.ToSKColor(), animationProgress * 0.2f),
            IsAntialias = true
        })
        {
            using var path = new SKPath();
            path.AddRoundRect(rect, _radius, _radius);
            canvas.DrawPath(path, paint);
        }

        // Kenarlık çizimi
        using (var paint = new SKPaint
        {
            Color = ColorScheme.BorderColor
                .ToSKColor()
                .InterpolateColor(ColorScheme.AccentColor.ToSKColor(), animationProgress * 0.5f), // Hover/basılı durumda renk değişimi
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        })
        {
            using var path = new SKPath();
            path.AddRoundRect(rect, _radius, _radius);
            canvas.DrawPath(path, paint);
        }

        // Metin çizimi
        if (_selectedItem != null)
        {
            using var textPaint = new SKPaint
            {
                Color = ColorScheme.ForeColor
                    .ToSKColor()
                    .InterpolateColor(ColorScheme.AccentColor.ToSKColor(), animationProgress * 0.5f),
                TextSize = Font.Size.PtToPx(this), // Font boyutu orijinal haline döndürüldü
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                IsAntialias = true
            };

            string displayText = GetItemText(_selectedItem);
            var textBounds = new SKRect();
            textPaint.MeasureText(displayText, ref textBounds);

            // Basılı durumda metin hafif aşağı kayar
            var yOffset = animationProgress * 1f;
            canvas.DrawText(displayText, 
                8 * (DeviceDpi / 96), // 5px sağa kaydırıldı
                (Height + textBounds.Height) / 2 + yOffset, 
                textPaint);
        }

        // Ok işareti çizimi
        var arrowRect = new SKRect(
            rect.Width - (24f * (DeviceDpi / 96)),
            0,
            rect.Width - (8f * (DeviceDpi / 96)),
            rect.Height - (4f * (DeviceDpi / 96)) + _shadowDepth);

        using (var paint = new SKPaint
        {
            Color = animationProgress > 0 ? ColorScheme.AccentColor.ToSKColor() : ColorScheme.ForeColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        })
        {
            var centerX = (float)(arrowRect.Left + arrowRect.Width / 2 - (1 * (DeviceDpi / 96)));
            var centerY = (float)(arrowRect.Top + arrowRect.Height / 2);

            // Basılı durumda ok hafif aşağı kayar
            var yOffset = (float)(animationProgress * 1f);

            // Sol çizgi
            canvas.DrawLine(
                centerX - (float)(4 * (DeviceDpi / 96)),
                centerY - (float)(2 * (DeviceDpi / 96)) + yOffset,
                centerX,
                centerY + (float)(3 * (DeviceDpi / 96)) + yOffset,
                paint);

            // Sağ çizgi
            canvas.DrawLine(
                centerX + (float)(4 * (DeviceDpi / 96)),
                centerY - (float)(2 * (DeviceDpi / 96)) + yOffset,
                centerX,
                centerY + (float)(3 * (DeviceDpi / 96)) + yOffset,
                paint);
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _animation.StartNewAnimation(AnimationDirection.In);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _animation.StartNewAnimation(AnimationDirection.Out);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _animation.StartNewAnimation(AnimationDirection.InOutIn);
            
            if (_dropDownMenu != null)
            {
                if (_dropDownMenu.Visible)
                {
                    _dropDownMenu.BeginClose();
                }
                else if (Items.Count > 0)
                {
                    ShowDropDown();
                }
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            _animation.StartNewAnimation(AnimationDirection.Out);
        }
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
    }

    private void ShowDropDown()
    {
        if (Items.Count == 0) return;

        var form = FindForm();
        if (form == null) return;
        
        // Eğer dropdown zaten bir form'a ekliyse, önce kaldır
        if (_dropDownMenu.Parent != null)
        {
            _dropDownMenu.Parent.Controls.Remove(_dropDownMenu);
        }
        
        // Form'a ekle ve pozisyonu ayarla
        form.Controls.Add(_dropDownMenu);
        
        var screenPoint = PointToScreen(new Point(0, Height));
        var formPoint = form.PointToClient(screenPoint);
        
        _dropDownMenu.Location = formPoint;
        _dropDownMenu.Width = DropDownWidth;
        DroppedDown = true;
        _dropDownMenu.BeginOpen();
    }

    private string GetItemText(object item)
    {
        if (item == null) return string.Empty;

        if (!string.IsNullOrEmpty(_displayMember) && item is IList list)
        {
            var prop = item.GetType().GetProperty(_displayMember);
            if (prop != null)
                return prop.GetValue(item)?.ToString() ?? string.Empty;
        }

        return item.ToString();
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var size = base.GetPreferredSize(proposedSize);
        size.Height = 23;
        return size;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        Height = 23;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dropDownMenu?.Dispose();
        }
        base.Dispose(disposing);
    }
}
