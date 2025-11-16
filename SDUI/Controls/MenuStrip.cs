using SDUI.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SDUI.Animation;

namespace SDUI.Controls;

public class MenuStrip : UIElementBase
{
    // backing fields
    private readonly List<MenuItem> _items = new();
    private MenuItem _hoveredItem;
    private MenuItem _openedItem;
    private ContextMenuStrip _activeDropDown;
    private MenuItem _activeDropDownOwner;
    private Color _menuBackColor = Color.FromArgb(45, 45, 45);
    private Color _menuForeColor = Color.White;
    private Color _hoverBackColor = Color.FromArgb(62, 62, 62);
    private Color _hoverForeColor = Color.FromArgb(0, 122, 204);
    private Color _submenuBackColor = Color.FromArgb(37, 37, 37);
    private Color _submenuBorderColor = Color.FromArgb(80, 80, 80);
    private Color _separatorColor = Color.FromArgb(80, 80, 80);
    private float _itemHeight = 28f;
    private float _itemPadding = 6f; // 12 -> 6 (daha az margin)
    private float _submenuArrowSize = 8f;
    private float _iconSize = 16f;
    private readonly Dictionary<MenuItem, Animation.AnimationEngine> _itemHoverAnims = new();
    private bool _stretch = true;
    private Size _imageScalingSize = new(20, 20);
    private bool _showSubmenuArrow = true;
    private bool _showIcons = true;
    private bool _showHoverEffect = true;
    private bool _roundedCorners = true;
    private float _cornerRadius = 6f;
    private float _submenuCornerRadius = 8f;
    private float _submenuOffset = 2f;
    private int _submenuAnimationDuration = 150;
    private bool _isAnimating;
    private float _animationProgress;
    private Timer _animationTimer;
    private Color _separatorBackColor = Color.FromArgb(50, 50, 50);
    private Color _separatorForeColor = Color.FromArgb(100, 100, 100);
    private float _separatorMargin = 4f;
    private SDUI.Orientation _orientation = SDUI.Orientation.Horizontal;
    private float _separatorHeight = 1f;

    public MenuStrip()
    {
        Height = (int)_itemHeight;
        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;
        InitializeAnimationTimer();
    }

    [Browsable(false)] public List<MenuItem> Items => _items;
    [Category("Behavior")][DefaultValue(true)] public bool Stretch { get=>_stretch; set{ if(_stretch==value) return; _stretch=value; Invalidate(); } }
    [Category("Appearance")][DefaultValue(SDUI.Orientation.Horizontal)] public SDUI.Orientation Orientation { get=>_orientation; set{ if(_orientation==value) return; _orientation=value; Invalidate(); } }
    [Category("Appearance")][DefaultValue(typeof(Size),"20, 20")] public Size ImageScalingSize { get=>_imageScalingSize; set{ if(_imageScalingSize==value) return; _imageScalingSize=value; _iconSize=Math.Min(value.Width,value.Height); Invalidate(); } }
    [Category("Behavior")][DefaultValue(true)] public bool ShowSubmenuArrow { get=>_showSubmenuArrow; set{ if(_showSubmenuArrow==value) return; _showSubmenuArrow=value; Invalidate(); } }

    private void InitializeAnimationTimer(){ _animationTimer=new Timer{ Interval=16 }; _animationTimer.Tick+=(s,e)=>{ if(!_isAnimating) return; _animationProgress=Math.Min(1f,_animationProgress+(16f/_submenuAnimationDuration)); if(_animationProgress>=1f){ _isAnimating=false; _animationTimer.Stop(); } Invalidate(); }; }

    [Category("Appearance")] public Color MenuBackColor { get=>_menuBackColor; set{ if(_menuBackColor==value) return; _menuBackColor=value; Invalidate(); } }
    [Category("Appearance")] public Color MenuForeColor { get=>_menuForeColor; set{ if(_menuForeColor==value) return; _menuForeColor=value; Invalidate(); } }
    [Category("Appearance")] public Color HoverBackColor { get=>_hoverBackColor; set{ if(_hoverBackColor==value) return; _hoverBackColor=value; Invalidate(); } }
    [Category("Appearance")] public Color HoverForeColor { get=>_hoverForeColor; set{ if(_hoverForeColor==value) return; _hoverForeColor=value; Invalidate(); } }
    [Category("Appearance")] public Color SubmenuBackColor { get=>_submenuBackColor; set{ if(_submenuBackColor==value) return; _submenuBackColor=value; Invalidate(); } }
    [Category("Appearance")] public Color SubmenuBorderColor { get=>_submenuBorderColor; set{ if(_submenuBorderColor==value) return; _submenuBorderColor=value; Invalidate(); } }
    [Category("Appearance")] public Color SeparatorColor { get=>_separatorColor; set{ if(_separatorColor==value) return; _separatorColor=value; Invalidate(); } }
    [Category("Layout")] public float ItemHeight { get=>_itemHeight; set{ if(_itemHeight==value) return; _itemHeight=value; Height=(int)value; Invalidate(); } }
    [Category("Layout")] public float ItemPadding { get=>_itemPadding; set{ if(_itemPadding==value) return; _itemPadding=value; Invalidate(); } }
    [Category("Appearance")] public bool ShowIcons { get=>_showIcons; set{ if(_showIcons==value) return; _showIcons=value; Invalidate(); } }
    [Category("Behavior")] public bool ShowHoverEffect { get=>_showHoverEffect; set{ if(_showHoverEffect==value) return; _showHoverEffect=value; Invalidate(); } }
    [Category("Appearance")] public bool RoundedCorners { get=>_roundedCorners; set{ if(_roundedCorners==value) return; _roundedCorners=value; Invalidate(); } }
    [Category("Appearance")] public Color SeparatorBackColor { get=>_separatorBackColor; set{ if(_separatorBackColor==value) return; _separatorBackColor=value; Invalidate(); } }
    [Category("Appearance")] public Color SeparatorForeColor { get=>_separatorForeColor; set{ if(_separatorForeColor==value) return; _separatorForeColor=value; Invalidate(); } }
    [Category("Layout")] public float SeparatorMargin { get=>_separatorMargin; set{ if(_separatorMargin==value) return; _separatorMargin=value; Invalidate(); } }

    public void AddItem(MenuItem item){ if(item==null) throw new ArgumentNullException(nameof(item)); _items.Add(item); item.Parent=this; Invalidate(); }
    public void RemoveItem(MenuItem item){ if(item==null) throw new ArgumentNullException(nameof(item)); if(_items.Remove(item)){ item.Parent=null; Invalidate(); } }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        var bounds = ClientRectangle;

        // Flat modern background
        using (var bg = new SKPaint { Color = MenuBackColor.ToSKColor(), IsAntialias = true })
        {
            canvas.DrawRect(new SKRect(0, 0, bounds.Width, bounds.Height), bg);
        }

        // Subtle bottom border
        using (var border = new SKPaint { Color = SeparatorColor.ToSKColor().WithAlpha(100), IsAntialias = true, StrokeWidth = 1 })
        {
            canvas.DrawLine(0, bounds.Height - 1, bounds.Width, bounds.Height - 1, border);
        }

        if (Orientation == SDUI.Orientation.Horizontal)
        {
            float x = ItemPadding;
            float available = bounds.Width - (ItemPadding * 2);
            float total = 0;
            float[] widths = new float[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                widths[i] = MeasureItemWidth(_items[i]);
                total += widths[i];
                if (i < _items.Count - 1) total += ItemPadding;
            }
            float extra = 0;
            if (Stretch && _items.Count > 0 && total < available) extra = (_items.Count > 1) ? (available - total) / (_items.Count - 1) : 0;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var w = widths[i];
                var r = new SKRect(x, 0, x + w, ItemHeight);
                DrawMenuItem(canvas, item, r);
                x += w + ItemPadding + (i < _items.Count - 1 ? extra : 0);
            }
        }
        else
        {
            float y = ItemPadding;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var w = bounds.Width - (ItemPadding * 2);
                var r = new SKRect(ItemPadding, y, ItemPadding + w, y + ItemHeight);
                DrawMenuItem(canvas, item, r);
                y += ItemHeight + ItemPadding;
            }
        }

        if (_activeDropDown == null || !_activeDropDown.IsOpen) _activeDropDownOwner = null;
    }

    private void DrawMenuItem(SKCanvas c, MenuItem item, SKRect bounds)
    {
        bool hover = item == _hoveredItem || item == _openedItem;
        var anim = EnsureHoverAnim(item);

        if (hover)
            anim.StartNewAnimation(AnimationDirection.In);
        else
            anim.StartNewAnimation(AnimationDirection.Out);

        float prog = (float)anim.GetProgress();

        // High-quality hover background with proper anti-aliasing
        if (ShowHoverEffect && hover)
        {
            var blend = _hoverBackColor.ToSKColor();
            byte alpha = (byte)(180 + 70 * prog);
            using var bg = new SKPaint
            {
                Color = blend.WithAlpha(alpha),
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High
            };
            var rr = new SKRoundRect(bounds, 5);
            c.DrawRoundRect(rr, bg);
        }

        float tx = bounds.Left + 10;

        // Icon
        if (ShowIcons && item.Icon != null)
        {
            var iy = bounds.Top + (_itemHeight - _iconSize) / 2;
            using var img = SKImage.FromBitmap(item.Icon.ToSKBitmap());
            using var imgPaint = new SKPaint
            {
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High
            };
            c.DrawImage(img, new SKRect(bounds.Left + 8, iy, bounds.Left + 8 + _iconSize, iy + _iconSize), imgPaint);
            tx += _iconSize + 6;
        }

        // Text with high quality
        var textColor = hover ? MenuForeColor.BlendWith(_hoverForeColor, 0.6f) : MenuForeColor;

        using (var paint = new SKPaint
        {
            Color = textColor.ToSKColor(),
            TextSize = Font.Size.PtToPx(this),
            Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
            IsAntialias = true,
            SubpixelText = true,
            LcdRenderText = true
        })
        {
            var fm = paint.FontMetrics;
            float textHeight = fm.Descent - fm.Ascent;
            float ty = bounds.Top + (bounds.Height - textHeight) / 2f - fm.Ascent;

            if (item.Text.Contains("&"))
                c.DrawTextWithMnemonic(item.Text, paint, tx, ty);
            else
                c.DrawText(item.Text, tx, ty, paint);

            // Measure text width for arrow positioning
            var textBounds = new SKRect();
            paint.MeasureText(item.Text.Replace("&", ""), ref textBounds);
            
            // WinUI3 style chevron arrow with high quality
            if (ShowSubmenuArrow && item.HasDropDown)
            {
                using var arrowPaint = new SKPaint
                {
                    Color = textColor.ToSKColor(),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.2f,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round
                };

                float chevronSize = 5f;
                float arrowX;
                float arrowY = bounds.MidY;

                // Check if this is a ContextMenuStrip instance (not MenuStrip with vertical orientation)
                bool isContextMenu = GetType() == typeof(ContextMenuStrip);
                
                if (Orientation == SDUI.Orientation.Vertical || isContextMenu)
                {
                    // Vertical: chevron at the right edge
                    arrowX = bounds.Right - 14;
                    
                    using var chevronPath = new SKPath();
                    chevronPath.MoveTo(arrowX - chevronSize, arrowY - chevronSize);
                    chevronPath.LineTo(arrowX - chevronSize / 2, arrowY);
                    chevronPath.LineTo(arrowX - chevronSize, arrowY + chevronSize);
                    c.DrawPath(chevronPath, arrowPaint);
                }
                else
                {
                    // Horizontal: chevron after text
                    arrowX = tx + textBounds.Width + 10;
                    
                    using var chevronPath = new SKPath();
                    chevronPath.MoveTo(arrowX - chevronSize, arrowY - chevronSize / 2);
                    chevronPath.LineTo(arrowX, arrowY + chevronSize / 2);
                    chevronPath.LineTo(arrowX + chevronSize, arrowY - chevronSize / 2);
                    c.DrawPath(chevronPath, arrowPaint);
                }
            }
        }
    }

    private List<RectangleF> ComputeItemRects(){ var rects=new List<RectangleF>(_items.Count); var b=ClientRectangle; if(Orientation==SDUI.Orientation.Horizontal){ float x=ItemPadding; float available=b.Width-(ItemPadding*2); float total=0; float[] widths=new float[_items.Count]; for(int i=0;i<_items.Count;i++){ widths[i]=MeasureItemWidth(_items[i]); total+=widths[i]; if(i<_items.Count-1) total+=ItemPadding; } float extra=0; if(Stretch && _items.Count>1 && total<available) extra=(available-total)/(_items.Count-1); for(int i=0;i<_items.Count;i++){ float w=widths[i]; rects.Add(new RectangleF(x,0,w,ItemHeight)); x+=w+ItemPadding+(i<_items.Count-1?extra:0); } } else { float y=ItemPadding; float w=b.Width-(ItemPadding*2); for(int i=0;i<_items.Count;i++){ rects.Add(new RectangleF(ItemPadding,y,w,ItemHeight)); y+=ItemHeight+ItemPadding; } } return rects; }

    private SKRect GetItemBounds(MenuItem item){ var rects=ComputeItemRects(); for(int i=0;i<_items.Count;i++){ if(_items[i]==item){ var r=rects[i]; return new SKRect(r.Left,r.Top,r.Right,r.Bottom); } } return SKRect.Empty; }

    internal override void OnMouseMove(MouseEventArgs e){ base.OnMouseMove(e); var rects=ComputeItemRects(); MenuItem hovered=null; for(int i=0;i<_items.Count;i++){ if(rects[i].Contains(e.Location)){ hovered=_items[i]; break; } } if(_hoveredItem!=hovered){ _hoveredItem=hovered; if(_hoveredItem?.HasDropDown==true && _openedItem!=_hoveredItem) OpenSubmenu(_hoveredItem); Invalidate(); } }

    internal override void OnMouseDown(MouseEventArgs e){ base.OnMouseDown(e); if(e.Button!=MouseButtons.Left) return; var rects=ComputeItemRects(); for(int i=0;i<_items.Count;i++){ if(rects[i].Contains(e.Location)){ var item=_items[i]; if(item.HasDropDown){ if(_openedItem==item){/*keep*/} else OpenSubmenu(item);} else { item.OnClick(); CloseSubmenu(); } return; } } CloseSubmenu(); }

    internal override void OnMouseLeave(EventArgs e){ base.OnMouseLeave(e); _hoveredItem=null; Invalidate(); }

    private void OpenSubmenu(MenuItem item)
    {
        if (!item.HasDropDown) { CloseSubmenu(); return; }
        if (_activeDropDownOwner == item && _activeDropDown != null && _activeDropDown.IsOpen) return;

        CloseSubmenu();
        EnsureDropDownHost();
        _activeDropDown.Items.Clear();

        foreach (var child in item.DropDownItems)
            _activeDropDown.AddItem(CloneMenuItem(child));

        SyncDropDownAppearance();

        var itemBounds = GetItemBounds(item);
        Point screenPoint;
        bool vertical = Orientation == SDUI.Orientation.Vertical || this is ContextMenuStrip;

        if (vertical)
        {
            // Right side, top aligned
            int targetX = (int)(itemBounds.Right + 4);
            int targetY = (int)(itemBounds.Top);
            screenPoint = PointToScreen(new Point(targetX, targetY));
        }
        else
        {
            // Below, left aligned - check if docked at bottom
            int targetX = (int)itemBounds.Left;
            int targetY;
            
            // If MenuStrip is docked at bottom, open upwards
            if (Dock == DockStyle.Bottom)
            {
                var popupSize = _activeDropDown.MeasurePreferredSize();
                targetY = (int)(itemBounds.Top - popupSize.Height - 4);
            }
            else
            {
                targetY = (int)(itemBounds.Bottom + 4);
            }
            
            screenPoint = PointToScreen(new Point(targetX, targetY));
        }

        _activeDropDownOwner = item;
        _openedItem = item;
        _activeDropDown.Show(this, screenPoint);
        Invalidate();
    }

    private void CloseSubmenu(){ if(_activeDropDown!=null && _activeDropDown.IsOpen) _activeDropDown.Hide(); _openedItem=null; _activeDropDownOwner=null; Invalidate(); }

    private void EnsureDropDownHost(){ if(_activeDropDown!=null) return; _activeDropDown=new ContextMenuStrip{ AutoClose=true, Dock=DockStyle.None }; _activeDropDown.Opening+=(_, _)=>SyncDropDownAppearance(); _activeDropDown.Closing+=(_, _)=>{ _openedItem=null; _activeDropDownOwner=null; Invalidate(); }; }

    private void SyncDropDownAppearance(){ if(_activeDropDown==null) return; _activeDropDown.MenuBackColor=SubmenuBackColor; _activeDropDown.MenuForeColor=MenuForeColor; _activeDropDown.HoverBackColor=HoverBackColor; _activeDropDown.HoverForeColor=HoverForeColor; _activeDropDown.SubmenuBackColor=SubmenuBackColor; _activeDropDown.SeparatorColor=SeparatorColor; _activeDropDown.RoundedCorners=RoundedCorners; _activeDropDown.ItemPadding=ItemPadding; _activeDropDown.Orientation=SDUI.Orientation.Vertical; _activeDropDown.ImageScalingSize=ImageScalingSize; _activeDropDown.ShowSubmenuArrow=ShowSubmenuArrow; _activeDropDown.ShowIcons=ShowIcons; }

    private MenuItem CloneMenuItem(MenuItem source){ if(source is MenuItemSeparator separator){ var cloneSeparator=new MenuItemSeparator{ Height=separator.Height, Margin=separator.Margin, LineColor=separator.LineColor, ShadowColor=separator.ShadowColor }; return cloneSeparator; } var clone=new MenuItem{ Text=source.Text, Icon=source.Icon, Image=source.Image, ShortcutKeys=source.ShortcutKeys, ShowSubmenuArrow=source.ShowSubmenuArrow, ForeColor=source.ForeColor, BackColor=source.BackColor, Enabled=source.Enabled, Visible=source.Visible, Font=source.Font, AutoSize=source.AutoSize, Padding=source.Padding, Tag=source.Tag, Checked=source.Checked }; foreach(var child in source.DropDownItems) clone.AddDropDownItem(CloneMenuItem(child)); clone.Click+=(_, _)=>{ source.OnClick(); _activeDropDown?.Hide(); }; return clone; }

    protected float MeasureItemWidth(MenuItem item)
    {
        if (item is MenuItemSeparator) return 20f;
        
        using var p = new SKPaint
        {
            TextSize = Font.Size.PtToPx(this),
            Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name)
        };
        
        var tb = new SKRect();
        p.MeasureText(item.Text, ref tb);
        float w = tb.Width;
        
        if (ShowIcons && item.Icon != null)
            w += _iconSize + 4;
            
        if (ShowSubmenuArrow && item.HasDropDown)
            w += _submenuArrowSize + 12; // More space for arrow
            
        return w + 20;
    }

    protected override void Dispose(bool disposing){ if(disposing){ _animationTimer?.Dispose(); _activeDropDown?.Dispose(); } base.Dispose(disposing); }

    private Animation.AnimationEngine EnsureHoverAnim(MenuItem item){ if(!_itemHoverAnims.TryGetValue(item,out var engine)){ engine=new Animation.AnimationEngine(singular:true){ Increment=0.28, AnimationType= AnimationType.EaseOut, InterruptAnimation=true }; engine.OnAnimationProgress+=_=>Invalidate(); _itemHoverAnims[item]=engine; } return engine; }
}