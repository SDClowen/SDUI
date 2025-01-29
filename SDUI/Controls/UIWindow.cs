using SDUI.Animation;
using SDUI.Collections;
using SDUI.Extensions;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class UIWindow : UIWindowBase
{
    public enum TabDesingMode
    {
        Rectangle,
        Rounded,
        Chromed,
    }

    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnFormMenuClick;

    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnExtendBoxClick;

    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler<int> OnCloseTabBoxClick;

    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnNewTabBoxClick;

    /// <summary>
    /// Is form active <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _isActive;

    /// <summary>
    /// If the mouse down <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _formMoveMouseDown;

    /// <summary>
    /// The position of the form when the left mouse button is pressed
    /// </summary>
    private Point _location;

    /// <summary>
    /// The position of the mouse when the left mouse button is pressed
    /// </summary>
    private Point _mouseOffset;

    /// <summary>
    /// The rectangle of control box
    /// </summary>
    private RectangleF _controlBoxRect;

    /// <summary>
    /// The rectangle of maximize box
    /// </summary>
    private RectangleF _maximizeBoxRect;

    /// <summary>
    /// The rectangle of minimize box
    /// </summary>
    private RectangleF _minimizeBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private RectangleF _extendBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private RectangleF _closeTabBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private RectangleF _newTabBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private RectangleF _formMenuRect;

    /// <summary>
    /// The control box left value
    /// </summary>
    private float _controlBoxLeft;

    /// <summary>
    /// The size of the window before it is maximized
    /// </summary>
    private Size _sizeOfBeforeMaximized;

    /// <summary>
    /// The position of the window before it is maximized
    /// </summary>
    private Point _locationOfBeforeMaximized;

    private float _titleHeightDPI => _titleHeight * DPI;
    private float _iconWidthDPI => _iconWidth * DPI;
    private float _symbolSizeDPI => _symbolSize * DPI;

    private float _iconWidth = 42;
    [DefaultValue(42)]
    [Description("Gets or sets the header bar icon width")]
    public float IconWidth
    {
        get => _iconWidth;
        set
        {
            _iconWidth = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    [DefaultValue(true)]
    [Description("Gets or sets form can movable")]
    public bool Movable { get; set; } = true;

    [DefaultValue(false)]
    public bool AllowAddControlOnTitle { get; set; }

    private bool _extendBox;

    [DefaultValue(false)]
    public bool ExtendBox
    {
        get => _extendBox;
        set
        {
            _extendBox = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    private bool _drawTabIcons;

    [DefaultValue(false)]
    public bool DrawTabIcons
    {
        get => _drawTabIcons;
        set
        {
            _drawTabIcons = value;
            Invalidate();
        }
    }

    private bool _tabCloseButton;

    [DefaultValue(false)]
    public bool TabCloseButton
    {
        get => _tabCloseButton;
        set
        {
            _tabCloseButton = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    private bool _newTabButton;

    [DefaultValue(false)]
    public bool NewTabButton
    {
        get => _newTabButton;
        set
        {
            _newTabButton = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }


    private float _symbolSize = 24;

    [DefaultValue(24)]
    public float ExtendSymbolSize
    {
        get => _symbolSize;
        set
        {
            _symbolSize = Math.Max(value, 16);
            _symbolSize = Math.Min(value, 128);
            Invalidate();
        }
    }

    [DefaultValue(null)]
    public ContextMenuStrip ExtendMenu { get; set; }

    [DefaultValue(null)]
    public ContextMenuStrip FormMenu { get; set; }

    /*
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new FormBorderStyle FormBorderStyle
    {
        get
        {
            return base.FormBorderStyle;
        }
        set
        {
            if (!Enum.IsDefined(typeof(FormBorderStyle), value))
                throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(FormBorderStyle));
            base.FormBorderStyle = FormBorderStyle.Sizable;
        }
    }*/

    /// <summary>
    /// Whether to show the title bar of the form
    /// </summary>
    private bool showTitle = true;

    /// <summary>
    /// Gets or sets whether to show the title bar of the form
    /// </summary>
    public bool ShowTitle
    {
        get => showTitle;
        set
        {
            showTitle = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    /// <summary>
    /// Whether to show the title bar of the form
    /// </summary>
    private bool showMenuInsteadOfIcon = false;

    /// <summary>
    /// Gets or sets whether to show the title bar of the form
    /// </summary>
    public bool ShowMenuInsteadOfIcon
    {
        get => showMenuInsteadOfIcon;
        set
        {
            showMenuInsteadOfIcon = value;
            CalcSystemBoxPos();
            Invalidate();
        }
    }

    /// <summary>
    /// Whether to show the title bar of the form
    /// </summary>
    private bool _drawTitleBorder = true;

    /// <summary>
    /// Gets or sets whether to show the title bar of the form
    /// </summary>
    public bool DrawTitleBorder
    {
        get => _drawTitleBorder;
        set
        {
            _drawTitleBorder = value;
            Invalidate();
        }
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);

        foreach (var element in _elements)
        {
            element.OnDpiChanged(EventArgs.Empty);
        }

        // Pencere boyutunu ve konumunu güncelle
        var oldDpi = e.DeviceDpiOld;
        var newDpi = e.DeviceDpiNew;
        var scaleFactor = (float)newDpi / oldDpi;

        Size = new Size(
            (int)(Width * scaleFactor),
            (int)(Height * scaleFactor)
        );

        Location = new Point(
            (int)(Location.X * scaleFactor),
            (int)(Location.Y * scaleFactor)
        );

        Invalidate();
    }

    /// <summary>
    /// Whether to display the control buttons of the form
    /// </summary>
    private bool controlBox = true;

    /// <summary>
    /// Gets or sets whether to display the control buttons of the form
    /// </summary>
    /*public new bool ControlBox
    {
        get => controlBox;
        set
        {
            controlBox = value;
            if (!controlBox)
            {
                MinimizeBox = MaximizeBox = false;
            }

            CalcSystemBoxPos();
            Invalidate();
        }
    }*/

    /// <summary>
    /// Whether to show the maximize button of the form
    /// </summary>
    private bool _maximizeBox = true;

    /// <summary>
    /// Whether to show the maximize button of the form
    /// </summary>
    public new bool MaximizeBox
    {
        get => _maximizeBox;
        set
        {
            _maximizeBox = value;

            if (value)
                _minimizeBox = true;

            CalcSystemBoxPos();
            Invalidate();
        }
    }

    /// <summary>
    /// Whether to show the minimize button of the form
    /// </summary>
    private bool _minimizeBox = true;

    /// <summary>
    /// Whether to show the minimize button of the form
    /// </summary>
    public new bool MinimizeBox
    {
        get => _minimizeBox;
        set
        {
            _minimizeBox = value;

            if (!value)
                _maximizeBox = false;

            CalcSystemBoxPos();
            Invalidate();
        }
    }

    /// <summary>
    /// The title height
    /// </summary>
    private float _titleHeight = 32;

    /// <summary>
    /// Gets or sets the title height
    /// </summary>
    public float TitleHeight
    {
        get => _titleHeight;
        set
        {
            _titleHeight = Math.Max(value, 31);
            Invalidate();
            CalcSystemBoxPos();
        }
    }

    /// <summary>
    /// Gradient header colors
    /// </summary>
    private Color[] _gradient = new[] { Color.Transparent, Color.Transparent };
    public Color[] Gradient
    {
        get => _gradient;
        set
        {
            _gradient = value; Invalidate();
        }
    }

    /// <summary>
    /// The title color
    /// </summary>
    private Color titleColor;

    /// <summary>
    /// Gets or sets the title color
    /// </summary>
    [Description("Title color"), DefaultValue(typeof(Color), "224, 224, 224")]
    public Color TitleColor
    {
        get => titleColor;
        set
        {
            titleColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// The title color
    /// </summary>
    private Color borderColor = Color.Transparent;

    /// <summary>
    /// Gets or sets the title color
    /// </summary>
    [Description("Border Color"), DefaultValue(typeof(Color), "Transparent")]
    public Color BorderColor
    {
        get => borderColor;
        set
        {
            borderColor = value;

            if (value != Color.Transparent)
                WindowsHelper.ApplyBorderColor(this.Handle, this.borderColor);

            Invalidate();
        }
    }

    /// <summary>
    /// Tab desing mode
    /// </summary>

    private TabDesingMode _tabDesingMode = TabDesingMode.Rectangle;
    public TabDesingMode TitleTabDesingMode
    {
        get => _tabDesingMode;
        set
        {
            if (_tabDesingMode == value)
                return;

            _tabDesingMode = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Draw hatch brush on form
    /// </summary>
    public bool FullDrawHatch { get; set; }

    private HatchStyle _hatch = HatchStyle.Percent80;
    public HatchStyle Hatch
    {
        get => _hatch;
        set
        {
            if (_hatch == value)
                return;

            _hatch = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Whether to trigger the stay event on the edge of the display
    /// </summary>
    private bool IsStayAtTopBorder;

    /// <summary>
    /// The time at which the display edge dwell event was triggered
    /// </summary>
    private long TopBorderStayTicks;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly Animation.AnimationEngine minBoxHoverAnimationManager;

    /// <summary>
    /// Tab Area hover animation manager
    /// </summary>
    private readonly Animation.AnimationEngine pageAreaAnimationManager;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly Animation.AnimationEngine maxBoxHoverAnimationManager;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly Animation.AnimationEngine extendBoxHoverAnimationManager;

    /// <summary>
    /// Close tab hover animation manager
    /// </summary>
    private readonly Animation.AnimationEngine closeBoxHoverAnimationManager;

    /// <summary>
    /// new Tab hover animation manager
    /// </summary>
    private readonly Animation.AnimationEngine newTabHoverAnimationManager;

    /// <summary>
    /// tab area animation manager
    /// </summary>
    private readonly Animation.AnimationEngine tabCloseHoverAnimationManager;

    /// <summary>
    /// tab area animation manager
    /// </summary>
    private readonly Animation.AnimationEngine formMenuHoverAnimationManager;

    private int previousSelectedPageIndex;
    private Point animationSource;
    private List<RectangleF> pageRect;
    private const int TAB_HEADER_PADDING = 9;
    private const int TAB_INDICATOR_HEIGHT = 3;

    private long _stickyBorderTime = 5000000;
    [Description("Set or get the maximum time to stay at the edge of the display(ms)")]
    [DefaultValue(500)]
    public long StickyBorderTime
    {
        get => _stickyBorderTime / 10000;
        set => _stickyBorderTime = value * 10000;
    }

    private const float HOVER_ANIMATION_SPEED = 0.15f;
    private const float HOVER_ANIMATION_OPACITY = 0.4f;
    private int _maxZOrder = 0;
    private Cursor _currentCursor;
    private bool _isLayoutSuspended;

    public new SDUI.Controls.ContextMenuStrip ContextMenuStrip { get; set; }

    /// <summary>
    /// The contructor
    /// </summary>
    public UIWindow()
        : base()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.SupportsTransparentBackColor, true);

        UpdateStyles();
        Controls = new UIWindowElementCollection(this);
        enableFullDraggable = false;

        pageAreaAnimationManager = new()
        {
            AnimationType = AnimationType.EaseOut,
            Increment = 0.07
        };

        minBoxHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut
        };
        maxBoxHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut
        };
        closeBoxHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut
        };

        extendBoxHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut
        };

        tabCloseHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut
        };

        newTabHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut
        };

        formMenuHoverAnimationManager = new()
        {
            Increment = HOVER_ANIMATION_SPEED,
            AnimationType = AnimationType.EaseInOut
        };

        minBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        maxBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        closeBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        extendBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        tabCloseHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        newTabHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        pageAreaAnimationManager.OnAnimationProgress += sender => Invalidate();
        formMenuHoverAnimationManager.OnAnimationProgress += sender => Invalidate();

        //WindowsHelper.ApplyRoundCorner(this.Handle);
    }

    private bool _inCloseBox, _inMaxBox, _inMinBox, _inExtendBox, _inTabCloseBox, _inNewTabBox, _inFormMenuBox;

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);

        if (ShowTitle && !AllowAddControlOnTitle && e.Control.Top < TitleHeight)
        {
            e.Control.Top = Padding.Top;
        }

        PerformLayout();
    }

    protected override void OnControlRemoved(ControlEventArgs e)
    {
        base.OnControlRemoved(e);
        PerformLayout();
    }

    private void CalcSystemBoxPos()
    {
        _controlBoxLeft = Width;

        if (controlBox)
        {
            _controlBoxRect = new(Width - _iconWidthDPI, 0, _iconWidthDPI, _titleHeightDPI);
            _controlBoxLeft = _controlBoxRect.Left - 2;

            if (MaximizeBox)
            {
                _maximizeBoxRect = new(_controlBoxRect.Left - _iconWidthDPI, _controlBoxRect.Top, _iconWidthDPI, _titleHeightDPI);
                _controlBoxLeft = _maximizeBoxRect.Left - 2;
            }
            else
            {
                _maximizeBoxRect = new(Width + 1, Height + 1, 1, 1);
            }

            if (MinimizeBox)
            {
                _minimizeBoxRect = new(MaximizeBox ? _maximizeBoxRect.Left - _iconWidthDPI - 2 : _controlBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top, _iconWidthDPI, _titleHeightDPI);
                _controlBoxLeft = _minimizeBoxRect.Left - 2;
            }
            else
            {
                _minimizeBoxRect = new Rectangle(Width + 1, Height + 1, 1, 1);
            }

            if (ExtendBox)
            {
                if (MinimizeBox)
                {
                    _extendBoxRect = new(_minimizeBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top, _iconWidthDPI, _titleHeightDPI);
                }
                else
                {
                    _extendBoxRect = new(_controlBoxRect.Left - _iconWidthDPI - 2, _controlBoxRect.Top, _iconWidthDPI, _titleHeightDPI);
                }
            }
        }
        else
        {
            _extendBoxRect = _maximizeBoxRect = _minimizeBoxRect = _controlBoxRect = new Rectangle(Width + 1, Height + 1, 1, 1);
        }

        var titleIconSize = 24 * DPI;
        _formMenuRect = new(10, _titleHeightDPI / 2 - (titleIconSize / 2), titleIconSize, titleIconSize);

        Padding = new Padding(Padding.Left, (int)(showTitle ? _titleHeightDPI : 0), Padding.Right, Padding.Bottom);
    }
    private void HandleTabKey(bool isShift)
    {
        var tabbableElements = _elements
            .Where(e => e.Visible && e.Enabled && e.TabStop)
            .OrderBy(e => e.TabIndex)
            .ToList();

        if (tabbableElements.Count == 0) return;

        int currentIndex = _focusedElement != null ? tabbableElements.IndexOf(_focusedElement) : -1;

        if (isShift)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = tabbableElements.Count - 1;
        }
        else
        {
            currentIndex++;
            if (currentIndex >= tabbableElements.Count) currentIndex = 0;
        }

        FocusedElement = tabbableElements[currentIndex];
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt)
        {
            HandleTabKey(e.Shift);
            e.Handled = true;
            return;
        }

        if (_focusedElement != null)
        {
            _focusedElement.OnKeyDown(e);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (_focusedElement != null)
        {
            _focusedElement.OnKeyUp(e);
        }
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);

        if (_focusedElement != null)
        {
            _focusedElement.OnKeyPress(e);
        }
    }


    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        if (!ShowTitle)
            return;

        if (_inCloseBox)
        {
            _inCloseBox = false;
            Close();
        }

        if (_inMinBox)
        {
            _inMinBox = false;
            WindowState = FormWindowState.Minimized;
        }

        if (_inMaxBox)
        {
            _inMaxBox = false;
            ShowMaximize();
        }

        if (_inExtendBox)
        {
            _inExtendBox = false;
            if (ExtendMenu != null)
            {
                //ExtendMenu.Show(this, new Point(Convert.ToInt32(_extendBoxRect.Left), Convert.ToInt32(_titleHeightDPI - 1)));
            }
            else
            {
                OnExtendBoxClick?.Invoke(this, EventArgs.Empty);
            }
        }

        if (_inFormMenuBox)
        {
            _inFormMenuBox = false;
            if (FormMenu != null)
            {
                //FormMenu.Show(this, new Point(Convert.ToInt32(_formMenuRect.Left), Convert.ToInt32(_titleHeightDPI - 1)));
            }
            else
            {
                OnFormMenuClick?.Invoke(this, EventArgs.Empty);
            }
        }

        if (_inTabCloseBox)
        {
            _inTabCloseBox = false;

            OnCloseTabBoxClick?.Invoke(this, _windowPageControl.SelectedIndex);
        }

        if (_inNewTabBox)
        {
            _inNewTabBox = false;

            OnNewTabBoxClick?.Invoke(this, EventArgs.Empty);
        }

        if (pageRect == null)
            UpdateTabRects();

        if (_formMoveMouseDown && !MousePosition.Equals(_mouseOffset))
            return;

        for (int i = 0; i < pageRect.Count; i++)
        {
            if (pageRect[i].Contains(e.Location))
            {
                _windowPageControl.SelectedIndex = i;

                if (_tabCloseButton && e.Button == MouseButtons.Middle)
                    OnCloseTabBoxClick?.Invoke(null, i);

                break;
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (_inCloseBox || _inMaxBox || _inMinBox || _inExtendBox || _inTabCloseBox || _inNewTabBox || _inFormMenuBox)
            return;

        if (!ShowTitle)
            return;

        if (e.Y > Padding.Top)
            return;

        if (e.Button == MouseButtons.Left && Movable)
        {
            _formMoveMouseDown = true;
            _location = Location;
            _mouseOffset = MousePosition;
        }

        bool elementClicked = false;
        // Z-order'a göre tersten kontrol et (üstteki elementten başla)
        foreach (var element in _elements.OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            var elementBounds = new Rectangle(element.Location, element.Size);
            if (elementBounds.Contains(e.Location))
            {
                elementClicked = true;
                FocusedElement = element;
                element.OnMouseDown(e);
                // Tıklanan elementi en üste getir
                BringToFront(element);
                break; // İlk tıklanan elementten sonra diğerlerini kontrol etmeye gerek yok
            }
        }

        if (!elementClicked)
        {
            FocusedElement = null;
        }

        if (enableFullDraggable && e.Button == MouseButtons.Left)
        {
            //right = e.Button == MouseButtons.Right;
            //location = e.Location;
            DragForm(Handle);
        }

        if (e.Button == MouseButtons.Right && ContextMenuStrip != null)
        {
            var point = PointToScreen(e.Location);
            //ContextMenuStrip.Show(this, point);
        }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        if (!MaximizeBox)
            return;

        bool inCloseBox = e.Location.InRect(_controlBoxRect);
        bool inMaxBox = e.Location.InRect(_maximizeBoxRect);
        bool inMinBox = e.Location.InRect(_minimizeBoxRect);
        bool inExtendBox = e.Location.InRect(_extendBoxRect);
        bool inCloseTabBox = _tabCloseButton && e.Location.InRect(_closeTabBoxRect);
        bool inNewTabBox = _newTabButton && e.Location.InRect(_newTabBoxRect);
        bool inFormMenuBox = e.Location.InRect(_formMenuRect);

        if (inCloseBox || inMaxBox || inMinBox || inExtendBox || inCloseTabBox || inNewTabBox || inFormMenuBox)
            return;

        if (!ShowTitle)
            return;

        if (e.Y > Padding.Top)
            return;

        ShowMaximize();

        base.OnMouseDoubleClick(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!IsDisposed && _formMoveMouseDown)
        {
            //int screenIndex = GetMouseInScreen(PointToScreen(e.Location));
            var screen = Screen.FromPoint(MousePosition);
            if (MousePosition.Y == screen.WorkingArea.Top && MaximizeBox)
            {
                ShowMaximize(true);
            }

            if (Top < screen.WorkingArea.Top)
            {
                Top = screen.WorkingArea.Top;
            }

            if (Top > screen.WorkingArea.Bottom - TitleHeight)
            {
                Top = Convert.ToInt32(screen.WorkingArea.Bottom - _titleHeightDPI);
            }
        }

        IsStayAtTopBorder = false;
        Cursor.Clip = new Rectangle();
        _formMoveMouseDown = false;

        animationSource = e.Location;

        // Z-order'a göre tersten kontrol et
        foreach (var element in _elements.OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            var elementBounds = new Rectangle(element.Location, element.Size);
            if (elementBounds.Contains(e.Location))
            {
                element.OnMouseUp(e);
                element.OnMouseClick(e);
                break;
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_formMoveMouseDown && !MousePosition.Equals(_mouseOffset))
        {
            if (WindowState == FormWindowState.Maximized)
            {
                int maximizedWidth = Width;
                int locationX = Left;
                ShowMaximize();

                float offsetXRatio = 1 - (float)Width / maximizedWidth;
                _mouseOffset.X -= (int)((_mouseOffset.X - locationX) * offsetXRatio);
            }

            var offsetX = _mouseOffset.X - MousePosition.X;
            var offsetY = _mouseOffset.Y - MousePosition.Y;
            var _workingArea = Screen.GetWorkingArea(this);

            if (MousePosition.Y - _workingArea.Top == 0)
            {
                if (!IsStayAtTopBorder)
                {
                    Cursor.Clip = _workingArea;
                    TopBorderStayTicks = DateTime.Now.Ticks;
                    IsStayAtTopBorder = true;
                }
                else if (DateTime.Now.Ticks - TopBorderStayTicks > _stickyBorderTime)
                {
                    Cursor.Clip = new Rectangle();
                }
            }

            Location = new Point(_location.X - offsetX, _location.Y - offsetY);
        }
        else
        {
            bool inCloseBox = e.Location.InRect(_controlBoxRect);
            bool inMaxBox = e.Location.InRect(_maximizeBoxRect);
            bool inMinBox = e.Location.InRect(_minimizeBoxRect);
            bool inExtendBox = e.Location.InRect(_extendBoxRect);
            bool inCloseTabBox = _tabCloseButton && e.Location.InRect(_closeTabBoxRect);
            bool inNewTabBox = _newTabButton && e.Location.InRect(_newTabBoxRect);
            bool inFormMenuBox = e.Location.InRect(_formMenuRect);
            bool isChange = false;

            if (inCloseBox != _inCloseBox)
            {
                _inCloseBox = inCloseBox;
                isChange = true;
                if (inCloseBox)
                    closeBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    closeBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inMaxBox != _inMaxBox)
            {
                _inMaxBox = inMaxBox;
                isChange = true;
                if (inMaxBox)
                    maxBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    maxBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inMinBox != _inMinBox)
            {
                _inMinBox = inMinBox;
                isChange = true;
                if (inMinBox)
                    minBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    minBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inExtendBox != _inExtendBox)
            {
                _inExtendBox = inExtendBox;
                isChange = true;
                if (inExtendBox)
                    extendBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    extendBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inCloseTabBox != _inTabCloseBox)
            {
                _inTabCloseBox = inCloseTabBox;
                isChange = true;
                if (inCloseTabBox)
                    tabCloseHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    tabCloseHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inNewTabBox != _inNewTabBox)
            {
                _inNewTabBox = inNewTabBox;
                isChange = true;
                if (inNewTabBox)
                    newTabHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    newTabHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (inFormMenuBox != _inFormMenuBox)
            {
                _inFormMenuBox = inFormMenuBox;
                isChange = true;
                if (inFormMenuBox)
                    formMenuHoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                else
                    formMenuHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
            }

            if (isChange)
                Invalidate();
        }

        UIElementBase hoveredElement = null;

        // Z-order'a göre tersten kontrol et
        foreach (var element in _elements.OrderByDescending(el => el.ZOrder).Where(el => el.Visible && el.Enabled))
        {
            var elementBounds = new Rectangle(element.Location, element.Size);
            if (elementBounds.Contains(e.Location))
            {
                hoveredElement = element;
                element.OnMouseMove(e);
                break; // İlk hover edilen elementten sonra diğerlerini kontrol etmeye gerek yok
            }
        }

        if (hoveredElement != _lastHoveredElement)
        {
            _lastHoveredElement?.OnMouseLeave(EventArgs.Empty);
            hoveredElement?.OnMouseEnter(EventArgs.Empty);
            _lastHoveredElement = hoveredElement;
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _inExtendBox = _inCloseBox = _inMaxBox = _inMinBox = _inTabCloseBox = _inNewTabBox = _inFormMenuBox = false;
        closeBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        minBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        maxBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        extendBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        tabCloseHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        newTabHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        formMenuHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);

        _lastHoveredElement?.OnMouseLeave(e);
        _lastHoveredElement = null;

        Invalidate();
    }

    private void ShowMaximize(bool IsOnMoving = false)
    {
        Screen screen = Screen.FromPoint(MousePosition);
        base.MaximumSize = screen.WorkingArea.Size;
        if (screen.Primary)
            MaximizedBounds = screen.WorkingArea;
        else
            MaximizedBounds = new Rectangle(0, 0, 0, 0);

        if (WindowState == FormWindowState.Normal)
        {
            _sizeOfBeforeMaximized = Size;
            _locationOfBeforeMaximized = IsOnMoving ? _location : Location;
            WindowState = FormWindowState.Maximized;
        }
        else if (WindowState == FormWindowState.Maximized)
        {
            if (_sizeOfBeforeMaximized.Width == 0 || _sizeOfBeforeMaximized.Height == 0)
            {
                int w = 800;
                if (MinimumSize.Width > 0) w = MinimumSize.Width;
                int h = 600;
                if (MinimumSize.Height > 0) h = MinimumSize.Height;
                _sizeOfBeforeMaximized = new Size(w, h);
            }

            Size = _sizeOfBeforeMaximized;
            if (_locationOfBeforeMaximized.X == 0 && _locationOfBeforeMaximized.Y == 0)
            {
                _locationOfBeforeMaximized = new Point(screen.Bounds.Left + screen.Bounds.Width / 2 - _sizeOfBeforeMaximized.Width / 2,
                  screen.Bounds.Top + screen.Bounds.Height / 2 - _sizeOfBeforeMaximized.Height / 2);
            }

            Location = _locationOfBeforeMaximized;
            WindowState = FormWindowState.Normal;
        }

        Invalidate();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _isActive = true;
        Invalidate();
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        _isActive = false;
        Invalidate();
    }


    private readonly List<UIElementBase> _elements = new();
    private UIElementBase _focusedElement;
    private UIElementBase _lastHoveredElement;
    private Bitmap bitmap;
    public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.Width, bitmap.Height);

    public new UIWindowElementCollection Controls { get; }

    public UIElementBase FocusedElement
    {
        get => _focusedElement;
        internal set
        {
            if (_focusedElement == value) return;

            var oldFocus = _focusedElement;
            _focusedElement = value;

            if (oldFocus != null)
            {
                oldFocus.OnLostFocus(EventArgs.Empty);
                oldFocus.OnLeave(EventArgs.Empty);
            }

            if (_focusedElement != null)
            {
                _focusedElement.OnGotFocus(EventArgs.Empty);
                _focusedElement.OnEnter(EventArgs.Empty);
            }
        }
    }

    public UIElementBase LastHoveredElement
    {
        get => _lastHoveredElement;
        internal set
        {
            if (_lastHoveredElement != value)
            {
                _lastHoveredElement = value;
                UpdateCursor(value);
            }
        }
    }
    private SKImageInfo CreateBitmap()
    {
        var info = new SKImageInfo(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul);

        if (bitmap == null || bitmap.Width != info.Width || bitmap.Height != info.Height)
        {
            FreeBitmap();
            if (info.Width != 0 && info.Height != 0)
            {
                bitmap = new Bitmap(info.Width, info.Height, PixelFormat.Format32bppPArgb);
            }
        }

        return info;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var info = CreateBitmap();
        if (info.Width == 0 || info.Height == 0)
            return;

        try
        {
            var data = bitmap.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb
            );

            try
            {
                using var surface = SKSurface.Create(info, data.Scan0, data.Stride);
                if (surface != null)
                {
                    surface.Canvas.Clear(SKColors.Transparent);
                    this.PaintSurface(surface.Canvas, info);
                    surface.Canvas.Flush();
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
        }
    }

    private void FreeBitmap()
    {
        if (bitmap != null)
        {
            bitmap.Dispose();
            bitmap = null;
        }
    }
    private void PaintSurface(SKCanvas canvas, SKImageInfo info)
    {
        if (info.Width <= 0 || info.Height <= 0)
            return;

        if (!ShowTitle)
        {
            canvas.Clear(ColorScheme.BackColor.ToSKColor());
            return;
        }

        var foreColor = ColorScheme.ForeColor.ToSKColor();
        var hoverColor = ColorScheme.BorderColor.ToSKColor();

        // Arka planı temizle
        //canvas.Clear(ColorScheme.BackColor.ToSKColor());

        if (FullDrawHatch)
        {
            using var paint = new SKPaint
            {
                Color = hoverColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                PathEffect = SKPathEffect.Create2DLine(4, SKMatrix.CreateScale(4, 4))
            };
            canvas.DrawRect(0, 0, Width, Height, paint);
        }

        if (titleColor != Color.Empty)
        {
            foreColor = titleColor.Determine().ToSKColor();
            hoverColor = foreColor.WithAlpha((byte)(20));
            using var paint = new SKPaint { Color = titleColor.ToSKColor() };
            canvas.DrawRect(0, 0, Width, _titleHeightDPI, paint);
        }
        else if (_gradient.Length == 2 &&
            !(_gradient[0] == Color.Transparent && _gradient[1] == Color.Transparent))
        {
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(Width, _titleHeightDPI),
                new[] { _gradient[0].ToSKColor(), _gradient[1].ToSKColor() },
                null,
                SKShaderTileMode.Clamp);

            using var paint = new SKPaint { Shader = shader };
            canvas.DrawRect(0, 0, Width, _titleHeightDPI, paint);

            foreColor = _gradient[0].Determine().ToSKColor();
            hoverColor = foreColor.WithAlpha((byte)(20));
        }

        // Kontrol düğmeleri çizimi
        if (controlBox)
        {
            var closeHoverColor = new SKColor(232, 17, 35);

            if (_inCloseBox)
            {
                using var paint = new SKPaint
                {
                    Color = closeHoverColor.WithAlpha((byte)(closeBoxHoverAnimationManager.GetProgress() * 120)),
                    IsAntialias = true
                };
                canvas.DrawRect(_controlBoxRect.ToSKRect(), paint);
            }

            using var closePaint = new SKPaint
            {
                Color = _inCloseBox ? SKColors.White : foreColor,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            // Çarpı işareti
            var centerX = _controlBoxRect.Left + _controlBoxRect.Width / 2;
            var centerY = _controlBoxRect.Top + _controlBoxRect.Height / 2;
            var size = 5 * DPI;

            canvas.DrawLine(
                centerX - size,
                centerY - size,
                centerX + size,
                centerY + size,
                closePaint);

            canvas.DrawLine(
                centerX - size,
                centerY + size,
                centerX + size,
                centerY - size,
                closePaint);
        }

        if (MaximizeBox)
        {
            if (_inMaxBox)
            {
                using var paint = new SKPaint
                {
                    Color = hoverColor.WithAlpha((byte)(maxBoxHoverAnimationManager.GetProgress() * 80)),
                    IsAntialias = true
                };
                canvas.DrawRect(_maximizeBoxRect.ToSKRect(), paint);
            }

            using var maxPaint = new SKPaint
            {
                Color = foreColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            // Maximize simgesi
            var centerX = _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2;
            var centerY = _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2;
            var size = 5 * DPI;

            if (WindowState == FormWindowState.Maximized)
            {
                // Restore simgesi
                var offset = 2 * DPI;
                canvas.DrawRect(
                    centerX - size + offset,
                    centerY - size - offset,
                    size * 2,
                    size * 2,
                    maxPaint);

                canvas.DrawRect(
                    centerX - size - offset,
                    centerY - size + offset,
                    size * 2,
                    size * 2,
                    maxPaint);
            }
            else
            {
                canvas.DrawRect(
                    centerX - size,
                    centerY - size,
                    size * 2,
                    size * 2,
                    maxPaint);
            }
        }

        if (MinimizeBox)
        {
            if (_inMinBox)
            {
                using var paint = new SKPaint
                {
                    Color = hoverColor.WithAlpha((byte)(minBoxHoverAnimationManager.GetProgress() * 80)),
                    IsAntialias = true
                };
                canvas.DrawRect(_minimizeBoxRect.ToSKRect(), paint);
            }

            using var minPaint = new SKPaint
            {
                Color = foreColor,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            var centerX = _minimizeBoxRect.Left + _minimizeBoxRect.Width / 2;
            var centerY = _minimizeBoxRect.Top + _minimizeBoxRect.Height / 2;
            var size = 5 * DPI;

            canvas.DrawLine(
                centerX - size,
                centerY,
                centerX + size,
                centerY,
                minPaint);
        }

        // Extend Box çizimi
        if (ExtendBox)
        {
            var color = foreColor;
            if (_inExtendBox)
            {
                var hoverSize = 24 * DPI;
                using var paint = new SKPaint
                {
                    Color = hoverColor.WithAlpha((byte)(extendBoxHoverAnimationManager.GetProgress() * 60)),
                    IsAntialias = true
                };

                var path = new SKPath();
                path.AddRoundRect(new SKRect(
                    _extendBoxRect.X + 20 * DPI,
                    (_titleHeightDPI / 2) - (hoverSize / 2),
                    _extendBoxRect.X + 20 * DPI + hoverSize,
                    (_titleHeightDPI / 2) + (hoverSize / 2)
                ), 15, 15);

                canvas.DrawPath(path, paint);
            }

            var size = 16 * DPI;
            using var extendPaint = new SKPaint
            {
                Color = foreColor,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            var iconRect = new SKRect(
                _extendBoxRect.X + 24 * DPI,
                (_titleHeightDPI / 2) - (size / 2),
                _extendBoxRect.X + 24 * DPI + size,
                (_titleHeightDPI / 2) + (size / 2));

            canvas.DrawLine(
                iconRect.Left + iconRect.Width / 2 - (5 * DPI) - 1,
                iconRect.Top + iconRect.Height / 2 - (2 * DPI),
                iconRect.Left + iconRect.Width / 2 - (1 * DPI),
                iconRect.Top + iconRect.Height / 2 + (3 * DPI),
                extendPaint);

            canvas.DrawLine(
                iconRect.Left + iconRect.Width / 2 + (5 * DPI) - 1,
                iconRect.Top + iconRect.Height / 2 - (2 * DPI),
                iconRect.Left + iconRect.Width / 2 - (1 * DPI),
                iconRect.Top + iconRect.Height / 2 + (3 * DPI),
                extendPaint);
        }

        // Form Menu veya Icon çizimi
        var faviconSize = 16 * DPI;
        if (showMenuInsteadOfIcon)
        {
            using var paint = new SKPaint
            {
                Color = hoverColor.WithAlpha((byte)(formMenuHoverAnimationManager.GetProgress() * 60)),
                IsAntialias = true
            };

            var path = new SKPath();
            path.AddRoundRect(_formMenuRect.ToSKRect(), 10, 10);
            canvas.DrawPath(path, paint);

            using var menuPaint = new SKPaint
            {
                Color = foreColor,
                StrokeWidth = 1.1f * DPI,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            canvas.DrawLine(
                _formMenuRect.Left + _formMenuRect.Width / 2 - (5 * DPI) - 1,
                _formMenuRect.Top + _formMenuRect.Height / 2 - (2 * DPI),
                _formMenuRect.Left + _formMenuRect.Width / 2 - (1 * DPI),
                _formMenuRect.Top + _formMenuRect.Height / 2 + (3 * DPI),
                menuPaint);

            canvas.DrawLine(
                _formMenuRect.Left + _formMenuRect.Width / 2 + (5 * DPI) - 1,
                _formMenuRect.Top + _formMenuRect.Height / 2 - (2 * DPI),
                _formMenuRect.Left + _formMenuRect.Width / 2 - (1 * DPI),
                _formMenuRect.Top + _formMenuRect.Height / 2 + (3 * DPI),
                menuPaint);
        }
        else
        {
            if (ShowIcon && Icon != null)
            {
                using var bitmap = Icon.ToBitmap();
                using var image = SKImage.FromBitmap(SKBitmap.FromImage(SKImage.FromBitmap(bitmap.ToSKBitmap())));
                var iconRect = SKRect.Create(10, (_titleHeightDPI / 2) - (faviconSize / 2), faviconSize, faviconSize);
                canvas.DrawImage(image, iconRect);
            }
        }

        // Form başlığı çizimi
        if (_windowPageControl == null || _windowPageControl.Count == 0)
        {
            using var textPaint = new SKPaint
            {
                Color = foreColor,
                TextSize = Font.Size * DPI,
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                IsAntialias = true,
                SubpixelText = true,
                TextAlign = SKTextAlign.Left
            };

            var bounds = new SKRect();
            textPaint.MeasureText(Text, ref bounds);
            var textX = showMenuInsteadOfIcon ? _formMenuRect.X + _formMenuRect.Width + (8 * DPI) : faviconSize + (14 * DPI);
            var textY = (_titleHeightDPI / 2) + (Math.Abs(textPaint.FontMetrics.Ascent + textPaint.FontMetrics.Descent) / 2);

            canvas.DrawText(Text, textX, textY, textPaint);
        }

        // Tab kontrollerinin çizimi
        if (_windowPageControl != null && _windowPageControl.Count > 0)
        {
            if (!pageAreaAnimationManager.IsAnimating() || pageRect == null || pageRect.Count != _windowPageControl.Count)
                UpdateTabRects();

            var animationProgress = pageAreaAnimationManager.GetProgress();

            // Click feedback
            if (pageAreaAnimationManager.IsAnimating())
            {
                using var ripplePaint = new SKPaint
                {
                    Color = foreColor.WithAlpha((byte)(31 - (animationProgress * 30))),
                    IsAntialias = true
                };

                var rippleSize = (int)(animationProgress * pageRect[_windowPageControl.SelectedIndex].Width * 1.75);
                var rippleRect = new SKRect(
                    animationSource.X - rippleSize / 2,
                    animationSource.Y - rippleSize / 2,
                    animationSource.X + rippleSize / 2,
                    animationSource.Y + rippleSize / 2);

                canvas.Save();
                canvas.ClipRect(pageRect[_windowPageControl.SelectedIndex].ToSKRect());
                canvas.DrawOval(rippleRect, ripplePaint);
                canvas.Restore();
            }

            // fix desing time error
            if (_windowPageControl.SelectedIndex <= -1 || _windowPageControl.SelectedIndex >= _windowPageControl.Count)
                return;

            // Animate page indicator
            if (previousSelectedPageIndex == pageRect.Count)
                previousSelectedPageIndex = -1;

            var previousSelectedPageIndexIfHasOne = previousSelectedPageIndex == -1 ? _windowPageControl.SelectedIndex : previousSelectedPageIndex;
            var previousActivePageRect = pageRect[previousSelectedPageIndexIfHasOne];
            var activePageRect = pageRect[_windowPageControl.SelectedIndex];

            var y = activePageRect.Bottom - 2;
            var x = previousActivePageRect.X + (activePageRect.X - previousActivePageRect.X) * (float)animationProgress;
            var width = previousActivePageRect.Width + (activePageRect.Width - previousActivePageRect.Width) * (float)animationProgress;

            if (_tabDesingMode == TabDesingMode.Rectangle)
            {
                using var tabPaint = new SKPaint
                {
                    Color = ColorScheme.BackColor.ToSKColor().InterpolateColor(hoverColor, 0.15f),
                    IsAntialias = true
                };

                canvas.DrawRect(activePageRect.X, 0, width, _titleHeightDPI, tabPaint);
                canvas.DrawRect(x, 0, width, _titleHeightDPI, tabPaint);

                using var indicatorPaint = new SKPaint
                {
                    Color = SKColors.DodgerBlue,
                    IsAntialias = true
                };

                canvas.DrawRect(x, _titleHeightDPI - TAB_INDICATOR_HEIGHT, width, TAB_INDICATOR_HEIGHT, indicatorPaint);
            }
            else if (_tabDesingMode == TabDesingMode.Rounded)
            {
                if (titleColor != Color.Empty && !titleColor.IsDark())
                    hoverColor = foreColor.WithAlpha(60);

                using var tabPaint = new SKPaint
                {
                    Color = ColorScheme.BackColor.ToSKColor().InterpolateColor(hoverColor, 0.2f),
                    IsAntialias = true
                };

                var tabRect = new SKRect(x, 6, x + width, _titleHeightDPI);
                var radius = 9 * DPI;

                var path = new SKPath();
                path.AddRoundRect(tabRect, radius, radius);
                canvas.DrawPath(path, tabPaint);
            }
            else // Chromed
            {
                if (titleColor != Color.Empty && !titleColor.IsDark())
                    hoverColor = foreColor.WithAlpha(60);

                using var tabPaint = new SKPaint
                {
                    Color = ColorScheme.BackColor.ToSKColor().InterpolateColor(hoverColor, 0.2f),
                    IsAntialias = true
                };

                var tabRect = new SKRect(x, 5, x + width, _titleHeightDPI - 7);
                var radius = 12;

                var path = new SKPath();
                path.AddRoundRect(tabRect, radius, radius);
                canvas.DrawPath(path, tabPaint);
            }

            // Draw tab headers
            foreach (var page in _windowPageControl.Controls)
            {
                var currentTabIndex = _windowPageControl.Controls.IndexOf(page);
                var rect = pageRect[currentTabIndex];
                var closeIconSize = 24 * DPI;

                if (_drawTabIcons)
                {
                    using var textPaint = new SKPaint
                    {
                        Color = foreColor,
                        TextSize = 12f * DPI,
                        Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                        IsAntialias = true,
                        SubpixelText = true,
                        TextAlign = SKTextAlign.Center
                    };

                    var startingIconBounds = new SKRect();
                    textPaint.MeasureText("", ref startingIconBounds);
                    var iconX = rect.X + (TAB_HEADER_PADDING * DPI);

                    var inlinePaddingX = startingIconBounds.Width + (TAB_HEADER_PADDING * DPI);
                    rect.X += inlinePaddingX;
                    rect.Width -= inlinePaddingX + closeIconSize;

                    var textY = (_titleHeightDPI / 2) + (Math.Abs(textPaint.FontMetrics.Ascent + textPaint.FontMetrics.Descent) / 2);
                    canvas.DrawText("", iconX, textY, textPaint);

                    var bounds = new SKRect();
                    textPaint.MeasureText(page.Text, ref bounds);
                    var textX = rect.X + (rect.Width / 2);
                    canvas.DrawText(page.Text, textX, textY, textPaint);
                }
                else
                {
                    using var textPaint = new SKPaint
                    {
                        Color = foreColor,
                        TextSize = 12f * DPI,
                        Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name),
                        IsAntialias = true,
                        SubpixelText = true,
                        TextAlign = SKTextAlign.Center
                    };

                    var bounds = new SKRect();
                    textPaint.MeasureText(page.Text, ref bounds);
                    var textX = rect.X + (rect.Width / 2);
                    var textY = (_titleHeightDPI / 2) + (Math.Abs(textPaint.FontMetrics.Ascent + textPaint.FontMetrics.Descent) / 2);
                    canvas.DrawText(page.Text, textX, textY, textPaint);
                }
            }

            // Tab close button
            if (_tabCloseButton)
            {
                var size = 20 * DPI;
                var closeHoverColor = hoverColor;

                using var buttonPaint = new SKPaint
                {
                    Color = closeHoverColor.WithAlpha((byte)(tabCloseHoverAnimationManager.GetProgress() * 60)),
                    IsAntialias = true
                };

                _closeTabBoxRect = new RectangleF(x + width - TAB_HEADER_PADDING / 2 - size, _titleHeightDPI / 2 - size / 2, size, size);
                var buttonRect = _closeTabBoxRect.ToSKRect();

                canvas.DrawCircle(buttonRect.MidX, buttonRect.MidY, size / 2, buttonPaint);

                using var linePaint = new SKPaint
                {
                    Color = foreColor,
                    StrokeWidth = 1.1f * DPI,
                    IsAntialias = true,
                    StrokeCap = SKStrokeCap.Round
                };

                size = 4f * DPI;
                canvas.DrawLine(
                    buttonRect.MidX - size,
                    buttonRect.MidY - size,
                    buttonRect.MidX + size,
                    buttonRect.MidY + size,
                    linePaint);

                canvas.DrawLine(
                    buttonRect.MidX - size,
                    buttonRect.MidY + size,
                    buttonRect.MidX + size,
                    buttonRect.MidY - size,
                    linePaint);
            }

            // New tab button
            if (_newTabButton)
            {
                var size = 24 * DPI;
                var newHoverColor = hoverColor.WithAlpha(20);

                using var buttonPaint = new SKPaint
                {
                    Color = newHoverColor.WithAlpha((byte)(newTabHoverAnimationManager.GetProgress() * newHoverColor.Alpha)),
                    IsAntialias = true
                };

                var lastTabRect = pageRect[pageRect.Count - 1];
                _newTabBoxRect = new RectangleF(lastTabRect.X + lastTabRect.Width + size / 2, _titleHeightDPI / 2 - size / 2, size, size);
                var buttonRect = _newTabBoxRect.ToSKRect();

                var path = new SKPath();
                path.AddRoundRect(buttonRect, 4, 4);
                canvas.DrawPath(path, buttonPaint);

                using var linePaint = new SKPaint
                {
                    Color = foreColor,
                    StrokeWidth = 1.1f * DPI,
                    IsAntialias = true,
                    StrokeCap = SKStrokeCap.Round
                };

                size = 6 * DPI;
                canvas.DrawLine(
                    buttonRect.MidX - size,
                    buttonRect.MidY,
                    buttonRect.MidX + size,
                    buttonRect.MidY,
                    linePaint);

                canvas.DrawLine(
                    buttonRect.MidX,
                    buttonRect.MidY - size,
                    buttonRect.MidX,
                    buttonRect.MidY + size,
                    linePaint);
            }
        }

        // Title border
        if (_drawTitleBorder)
        {
            using var borderPaint = new SKPaint
            {
                Color = titleColor != Color.Empty ?
                    titleColor.Determine().ToSKColor().WithAlpha(30) :
                    ColorScheme.BorderColor.ToSKColor(),
                StrokeWidth = 1,
                IsAntialias = true
            };

            canvas.DrawLine(Width, _titleHeightDPI - 1, 0, _titleHeightDPI - 1, borderPaint);
        }

        // Draw others

        // Elementleri Z-order'a göre sırala ve render et
        var elements = Controls.OrderBy(el => el.ZOrder).ToList();
        System.Diagnostics.Debug.WriteLine($"Toplam {elements.Count} element render edilecek");

        foreach (var element in elements)
        {
            if (!element.Visible || element.Size.Width <= 0 || element.Size.Height <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"Element atlandı: {element.GetType().Name}, Visible: {element.Visible}, Size: {element.Size}");
                continue;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Element render ediliyor: {element.GetType().Name}, Location: {element.Location}, Size: {element.Size}");

                // Her element için yeni bir surface oluştur
                using var elementSurface = SKSurface.Create(new SKImageInfo(element.Size.Width, element.Size.Height, info.ColorType, info.AlphaType));
                if (elementSurface == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Element surface oluşturulamadı: {element.GetType().Name}");
                    continue;
                }

                // Element'in kendi render metodunu çağır
                element.OnPaint(new SKPaintSurfaceEventArgs(elementSurface, info));

                // Element'in çizim alanını kaydet
                canvas.Save();

                // Element'in çizim alanını kırp
                var clipRect = SKRect.Create(element.Location.X, element.Location.Y, element.Size.Width, element.Size.Height);
                canvas.ClipRect(clipRect);

                // Element'in konumuna taşı
                canvas.Translate(element.Location.X, element.Location.Y);

                // Element'i ana canvas'a çiz
                using var snapshot = elementSurface.Snapshot();
                canvas.DrawImage(snapshot, 0, 0);

                // Element'in çizim alanını geri yükle
                canvas.Restore();

                System.Diagnostics.Debug.WriteLine($"Element başarıyla render edildi: {element.GetType().Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Element render error for {element.GetType().Name}: {ex.Message}");
            }
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        CalcSystemBoxPos();
        PerformLayout();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        CalcSystemBoxPos();
    }

    protected void AddMousePressMove(params Control[] cs)
    {
        foreach (Control ctrl in cs)
        {
            if (ctrl != null && !ctrl.IsDisposed)
            {
                ctrl.MouseDown += CtrlMouseDown;
            }
        }
    }

    /// <summary>
    /// Handles the MouseDown event of the c control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
    private void CtrlMouseDown(object sender, MouseEventArgs e)
    {
        if (WindowState == FormWindowState.Maximized)
            return;

        if (sender == this)
        {
            if (e.Y <= _titleHeightDPI && e.X < _controlBoxLeft)
            {
                DragForm(Handle);
            }
        }
        else
        {
            DragForm(Handle);
        }
    }

    private WindowPageControl _windowPageControl;

    public WindowPageControl WindowPageControl
    {
        get => _windowPageControl;
        set
        {
            _windowPageControl = value;
            if (_windowPageControl == null)
                return;

            previousSelectedPageIndex = _windowPageControl.SelectedIndex;

            _windowPageControl.SelectedIndexChanged += (sender, previousIndex) =>
            {
                previousSelectedPageIndex = previousIndex;
                pageAreaAnimationManager.SetProgress(0);
                pageAreaAnimationManager.StartNewAnimation(AnimationDirection.In);
            };
            _windowPageControl.ControlAdded += delegate
            {
                Invalidate();
            };
            _windowPageControl.ControlRemoved += delegate
            {
                Invalidate();
            };
        }
    }

    private void UpdateTabRects()
    {
        pageRect = new();

        if (_windowPageControl == null || _windowPageControl.Count == 0)
            return;

        float tabAreaWidth = 44 * DPI;

        if (controlBox)
            tabAreaWidth += _controlBoxRect.Width;

        if (MinimizeBox)
            tabAreaWidth += _minimizeBoxRect.Width;

        if (MaximizeBox)
            tabAreaWidth += _maximizeBoxRect.Width;

        if (ExtendBox)
            tabAreaWidth += _extendBoxRect.Width;

        float maxSize = 250f * DPI;

        tabAreaWidth = (Width - tabAreaWidth - (30 * DPI)) / _windowPageControl.Count;
        if (tabAreaWidth > maxSize)
            tabAreaWidth = maxSize;

        pageRect.Add(new(44 * DPI, 0, tabAreaWidth, _titleHeightDPI));
        for (int i = 1; i < _windowPageControl.Count; i++)
            pageRect.Add(new(pageRect[i - 1].Right, 0, tabAreaWidth, _titleHeightDPI));
    }

    internal void UpdateCursor(UIElementBase element)
    {
        if (element == null || !element.Enabled || !element.Visible)
        {
            _currentCursor = Cursors.Default;
            return;
        }

        var newCursor = element.Cursor ?? Cursors.Default;
        if (_currentCursor != newCursor)
        {
            _currentCursor = newCursor;
            if (Handle != IntPtr.Zero)
            {
                SetCursor(_currentCursor.Handle);
            }
        }
    }

    public new void PerformLayout()
    {
        // Önce Dock'lu elementleri yerleştir
        LayoutDockElements();

        // Sonra Anchor'lu elementleri yerleştir
        LayoutAnchoredElements();

        Invalidate();
    }

    private void LayoutDockElements()
    {
        var clientArea = ClientRectangle;
        var remainingArea = clientArea;

        // Dock.Top elementleri
        foreach (var element in _elements.Where(e => e.Visible && e.Dock == DockStyle.Top))
        {
            element.Size = new Size(remainingArea.Width, element.Size.Height);
            element.Location = new Point(remainingArea.Left, remainingArea.Top);
            remainingArea = new Rectangle(
                remainingArea.Left,
                remainingArea.Top + element.Size.Height,
                remainingArea.Width,
                remainingArea.Height - element.Size.Height
            );
        }

        // Dock.Bottom elementleri
        foreach (var element in _elements.Where(e => e.Visible && e.Dock == DockStyle.Bottom))
        {
            element.Size = new Size(remainingArea.Width, element.Size.Height);
            element.Location = new Point(remainingArea.Left, remainingArea.Bottom - element.Size.Height);
            remainingArea = new Rectangle(
                remainingArea.Left,
                remainingArea.Top,
                remainingArea.Width,
                remainingArea.Height - element.Size.Height
            );
        }

        // Dock.Left elementleri
        foreach (var element in _elements.Where(e => e.Visible && e.Dock == DockStyle.Left))
        {
            element.Size = new Size(element.Size.Width, remainingArea.Height);
            element.Location = new Point(remainingArea.Left, remainingArea.Top);
            remainingArea = new Rectangle(
                remainingArea.Left + element.Size.Width,
                remainingArea.Top,
                remainingArea.Width - element.Size.Width,
                remainingArea.Height
            );
        }

        // Dock.Right elementleri
        foreach (var element in _elements.Where(e => e.Visible && e.Dock == DockStyle.Right))
        {
            element.Size = new Size(element.Size.Width, remainingArea.Height);
            element.Location = new Point(remainingArea.Right - element.Size.Width, remainingArea.Top);
            remainingArea = new Rectangle(
                remainingArea.Left,
                remainingArea.Top,
                remainingArea.Width - element.Size.Width,
                remainingArea.Height
            );
        }

        // Dock.Fill elementleri
        foreach (var element in _elements.Where(e => e.Visible && e.Dock == DockStyle.Fill))
        {
            element.Location = new Point(remainingArea.Left, remainingArea.Top);
            element.Size = new Size(remainingArea.Width, remainingArea.Height);
        }
    }

    private void LayoutAnchoredElements()
    {
        var clientArea = ClientRectangle;

        foreach (var element in _elements.Where(e => e.Visible && e.Dock == DockStyle.None))
        {
            var anchor = element.Anchor;
            var location = element.Location;
            var size = element.Size;

            // Yatay konumlandırma
            if ((anchor & AnchorStyles.Left) != 0 && (anchor & AnchorStyles.Right) != 0)
            {
                // Her iki tarafa da bağlı, genişliği ayarla
                size.Width = clientArea.Width - (location.X + (clientArea.Width - (location.X + size.Width)));
            }
            else if ((anchor & AnchorStyles.Right) != 0)
            {
                // Sadece sağa bağlı, X konumunu ayarla
                location.X = clientArea.Width - (clientArea.Width - location.X);
            }

            // Dikey konumlandırma
            if ((anchor & AnchorStyles.Top) != 0 && (anchor & AnchorStyles.Bottom) != 0)
            {
                // Her iki tarafa da bağlı, yüksekliği ayarla
                size.Height = clientArea.Height - (location.Y + (clientArea.Height - (location.Y + size.Height)));
            }
            else if ((anchor & AnchorStyles.Bottom) != 0)
            {
                // Sadece alta bağlı, Y konumunu ayarla
                location.Y = clientArea.Height - (clientArea.Height - location.Y);
            }

            element.Location = location;
            element.Size = size;
        }
    }

    public void BringToFront(UIElementBase element)
    {
        if (!_elements.Contains(element)) return;

        _maxZOrder++;
        element.ZOrder = _maxZOrder;
        InvalidateElement(element);
    }

    public void SendToBack(UIElementBase element)
    {
        if (!_elements.Contains(element)) return;

        var minZOrder = _elements.Min(e => e.ZOrder);
        element.ZOrder = minZOrder - 1;
        InvalidateElement(element);
    }

    private void InvalidateElement(UIElementBase element)
    {
        var bounds = new Rectangle(element.Location, element.Size);
        Invalidate(bounds);
    }

    protected SKPaint GetPaintFromPool()
    {
        var paint = new SKPaint();
        paint.IsAntialias = true;
        return paint;
    }

    protected void ReturnPaintToPool(SKPaint paint)
    {
        paint?.Dispose();
    }

    public new void ResumeLayout()
    {
        ResumeLayout(true);
    }

    public new void ResumeLayout(bool performLayout)
    {
        _isLayoutSuspended = false;
        if (performLayout)
            PerformLayout();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            FreeBitmap();

        base.Dispose(disposing);
    }
}