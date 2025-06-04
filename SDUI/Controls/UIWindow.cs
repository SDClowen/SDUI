using SDUI.Animation;
using SDUI.Helpers;
using Svg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
        _cachedMetrics.IsMetricsValid = false;
        base.OnDpiChanged(e);
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

    private struct CachedMetrics
    {
        public float TitleHeightDPI;
        public float IconWidthDPI;
        public float SymbolSizeDPI;
        public bool IsMetricsValid;
    }

    private CachedMetrics _cachedMetrics;
    private bool _needsLayoutUpdate;

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

        enableFullDraggable = false;

        pageAreaAnimationManager = new()
        {
            AnimationType = AnimationType.EaseOut,
            Increment = 0.07
        };

        minBoxHoverAnimationManager = new()
        {
            Increment = 0.15,
            AnimationType = AnimationType.Linear
        };
        maxBoxHoverAnimationManager = new()
        {
            Increment = 0.15,
            AnimationType = AnimationType.Linear
        };
        closeBoxHoverAnimationManager = new()
        {
            Increment = 0.15,
            AnimationType = AnimationType.Linear
        };

        extendBoxHoverAnimationManager = new()
        {
            Increment = 0.15,
            AnimationType = AnimationType.Linear
        };

        tabCloseHoverAnimationManager = new()
        {
            Increment = 0.15,
            AnimationType = AnimationType.Linear
        };

        newTabHoverAnimationManager = new()
        {
            Increment = 0.15,
            AnimationType = AnimationType.Linear
        };

        formMenuHoverAnimationManager = new()
        {
            Increment = 0.15,
            AnimationType = AnimationType.Linear
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
                ExtendMenu.Show(this, Convert.ToInt32(_extendBoxRect.Left), Convert.ToInt32(_titleHeightDPI - 1));
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
                FormMenu.Show(this, Convert.ToInt32(_formMenuRect.Left), Convert.ToInt32(_titleHeightDPI - 1));
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

            // If the current mouse stays on the upper edge of the container, it will trigger an edge wait for MaximumBorderInterval(ms),
            // If the movement ends at this time, the window will be automatically maximized, this function is provided for multiple monitors arranged up and down
            // The advantage of setting the judgment to a specific value here is that it is difficult to trigger the stay event if the form is quickly moved across the monitor
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

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.HighSpeed;
        graphics.CompositingQuality = CompositingQuality.HighSpeed;
        graphics.InterpolationMode = InterpolationMode.Low;

        if (Width <= 0 || Height <= 0)
            return;

        var foreColor = ColorScheme.ForeColor;
        var hoverColor = ColorScheme.BorderColor;

        if (FullDrawHatch)
        {
            using var hatchBrush = new HatchBrush(_hatch, ColorScheme.BackColor, hoverColor);
            graphics.FillRectangle(hatchBrush, 0, 0, Width, Height);
        }
        else
            graphics.FillRectangle(ColorScheme.BackColor, ClientRectangle);

        if (!ShowTitle)
            return;

        graphics.SetHighQuality();

        if (titleColor != Color.Empty)
        {
            foreColor = titleColor.Determine();
            hoverColor = foreColor.Alpha(20);
            graphics.FillRectangle(titleColor, 0, 0, Width, _titleHeightDPI);
        }
        else if (_gradient.Length == 2 && !(_gradient[0] == Color.Transparent && _gradient[1] == Color.Transparent))
        {
            using var brush = new LinearGradientBrush(new RectangleF(0, 0, Width, _titleHeightDPI), _gradient[0], _gradient[1], 45);
            graphics.FillRectangle(brush, 0, 0, Width, _titleHeightDPI);

            foreColor = _gradient[0].Determine();
            hoverColor = foreColor.Alpha(20);
        }

        if (controlBox)
        {
            var closeHoverColor = Color.FromArgb(222, 179, 30, 30);

            if (_inCloseBox)
                graphics.FillRectangle(Color.FromArgb((int)(closeBoxHoverAnimationManager.GetProgress() * closeHoverColor.A), closeHoverColor.RemoveAlpha()), _controlBoxRect);

            using var closePen = new Pen(_inCloseBox ? Color.White : foreColor);
            graphics.DrawLine(closePen,
                _controlBoxRect.Left + _controlBoxRect.Width / 2 - (6 * DPI),
                _controlBoxRect.Top + _controlBoxRect.Height / 2 - (6 * DPI),
                _controlBoxRect.Left + _controlBoxRect.Width / 2 + (6 * DPI),
                _controlBoxRect.Top + _controlBoxRect.Height / 2 + (6 * DPI));

            graphics.DrawLine(closePen,
                _controlBoxRect.Left + _controlBoxRect.Width / 2 - (6 * DPI),
                _controlBoxRect.Top + _controlBoxRect.Height / 2 + (6 * DPI),
                _controlBoxRect.Left + _controlBoxRect.Width / 2 + (6 * DPI),
                _controlBoxRect.Top + _controlBoxRect.Height / 2 - (6 * DPI));
        }

        // Maximize Box
        if (MaximizeBox)
        {
            if (_inMaxBox)
                graphics.FillRectangle(Color.FromArgb((int)(maxBoxHoverAnimationManager.GetProgress() * hoverColor.A), hoverColor.RemoveAlpha()), _maximizeBoxRect);

            graphics.DrawRectangle(foreColor,
                _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - (12 * DPI / 2),
                _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - (11 * DPI / 2),
                12 * DPI, 11 * DPI);

            if (WindowState == FormWindowState.Maximized)
            {
                graphics.DrawLine(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - (3 * DPI),
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - (5 * DPI),
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - (3 * DPI),
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - (7 * DPI));

                graphics.DrawLine(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - (3 * DPI),
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - (7 * DPI),
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + (9 * DPI),
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - (7 * DPI));

                graphics.DrawLine(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + (9 * DPI),
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - (6 * DPI),
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + (9 * DPI),
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 + (4 * DPI));

                graphics.DrawLine(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + (8 * DPI),
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 + (5 * DPI),
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + (5 * DPI),
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 + (5 * DPI));
            }
        }

        // Minimize Box
        if (MinimizeBox)
        {
            if (_inMinBox)
                graphics.FillRectangle(Color.FromArgb((int)(minBoxHoverAnimationManager.GetProgress() * hoverColor.A), hoverColor.RemoveAlpha()), _minimizeBoxRect);

            graphics.DrawLine(foreColor,
                _minimizeBoxRect.Left + _minimizeBoxRect.Width / 2 - (7 * DPI),
                _minimizeBoxRect.Top + _minimizeBoxRect.Height / 2,
                _minimizeBoxRect.Left + _minimizeBoxRect.Width / 2 + (6 * DPI),
                _minimizeBoxRect.Top + _minimizeBoxRect.Height / 2);
        }

        // Extend Box
        if (ExtendBox)
        {
            var color = foreColor;
            if (_inExtendBox)
            {
                var hoverSize = 24 * DPI;
                var brush = new SolidBrush(Color.FromArgb((int)(extendBoxHoverAnimationManager.GetProgress() * hoverColor.A), hoverColor.RemoveAlpha()));
                graphics.FillPath(brush, new RectangleF(_extendBoxRect.X + 20 * DPI, (_titleHeightDPI / 2) - (hoverSize / 2), hoverSize, hoverSize).Radius(15));
            }

            var size = 16 * DPI;
            graphics.DrawSvg(SvgIcons.Settings, color, new(_extendBoxRect.X + 24 * DPI, (_titleHeightDPI / 2) - (size / 2), size, size));
        }

        // Form Menu/Icon
        var faviconSize = 16 * DPI;
        if (showMenuInsteadOfIcon)
        {
            using var brush = new SolidBrush(Color.FromArgb((int)(formMenuHoverAnimationManager.GetProgress() * hoverColor.A), hoverColor.RemoveAlpha()));
            graphics.FillPath(brush, _formMenuRect.Radius(10));

            graphics.DrawLine(foreColor,
                _formMenuRect.Left + _formMenuRect.Width / 2 - (5 * DPI) - 1,
                _formMenuRect.Top + _formMenuRect.Height / 2 - (2 * DPI),
                _formMenuRect.Left + _formMenuRect.Width / 2 - (1 * DPI),
                _formMenuRect.Top + _formMenuRect.Height / 2 + (3 * DPI));

            graphics.DrawLine(foreColor,
                _formMenuRect.Left + _formMenuRect.Width / 2 + (5 * DPI) - 1,
                _formMenuRect.Top + _formMenuRect.Height / 2 - (2 * DPI),
                _formMenuRect.Left + _formMenuRect.Width / 2 - (1 * DPI),
                _formMenuRect.Top + _formMenuRect.Height / 2 + (3 * DPI));
        }
        else
        {
            if (ShowIcon && Icon != null)
                graphics.DrawImage(Icon.ToBitmap(), 10, (_titleHeightDPI / 2) - (faviconSize / 2), faviconSize, faviconSize);
        }

        // Window Title or Tabs
        if (_windowPageControl == null || _windowPageControl.Count == 0)
        {
            var stringSize = graphics.MeasureString(Text, Font);
            var textPoint = new PointF((showMenuInsteadOfIcon ? _formMenuRect.X + _formMenuRect.Width : faviconSize + 14), (_titleHeightDPI / 2 - stringSize.Height / 2));

            using var textBrush = new SolidBrush(foreColor);
            graphics.DrawString(Text, Font, textBrush, textPoint, StringFormat.GenericDefault);
        }
        else
        {
            if (!pageAreaAnimationManager.IsAnimating() || pageRect == null || pageRect.Count != _windowPageControl.Count)
                UpdateTabRects();

            var animationProgress = pageAreaAnimationManager.GetProgress();

            //Click feedback
            if (pageAreaAnimationManager.IsAnimating())
            {
                using var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationProgress * 50)), foreColor));
                var rippleSize = (int)(animationProgress * pageRect[_windowPageControl.SelectedIndex].Width * 1.75);

                graphics.SetClip(pageRect[_windowPageControl.SelectedIndex]);
                graphics.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                graphics.ResetClip();
            }

            // fix desing time error
            if (_windowPageControl.SelectedIndex <= -1 || _windowPageControl.SelectedIndex >= _windowPageControl.Count)
                return;

            //Animate page indicator
            if (previousSelectedPageIndex == pageRect.Count)
                previousSelectedPageIndex = -1;

            var previousSelectedPageIndexIfHasOne = previousSelectedPageIndex == -1 ? _windowPageControl.SelectedIndex : previousSelectedPageIndex;
            var previousActivePageRect = pageRect[previousSelectedPageIndexIfHasOne];
            var activePageRect = pageRect[_windowPageControl.SelectedIndex];

            var y = activePageRect.Bottom - 2;
            var x = previousActivePageRect.X + (int)((activePageRect.X - previousActivePageRect.X) * animationProgress);
            var width = previousActivePageRect.Width + (int)((activePageRect.Width - previousActivePageRect.Width) * animationProgress);

            if (_tabDesingMode == TabDesingMode.Rectangle)
            {
                graphics.DrawRectangle(hoverColor, activePageRect.X, 0, width, _titleHeightDPI);
                graphics.FillRectangle(hoverColor, x, 0, width, _titleHeightDPI);
                graphics.FillRectangle(Color.DodgerBlue, x, _titleHeightDPI - TAB_INDICATOR_HEIGHT, width, TAB_INDICATOR_HEIGHT);
            }
            else if (_tabDesingMode == TabDesingMode.Rounded)
            {
                if (titleColor != Color.Empty && !titleColor.IsDark())
                    hoverColor = ForeColor.Alpha(60);

                using var hoverBrush = hoverColor.Brush();
                var tabRect = new RectangleF(x, 6, width, _titleHeightDPI);

                var radius = 9 * DPI;
                graphics.FillPath(hoverBrush, tabRect.Radius(radius, radius, 0, 0));
            }
            else
            {
                if (titleColor != Color.Empty && !titleColor.IsDark())
                    hoverColor = ForeColor.Alpha(60);

                using var hoverBrush = hoverColor.Brush();
                var tabRect = new RectangleF(x, 5, width, _titleHeightDPI - 7);

                var radius = 12;
                graphics.FillPath(hoverBrush, tabRect.ChromePath(radius));
            }

            //Draw tab headers
            foreach (Control page in _windowPageControl.Controls)
            {
                var currentTabIndex = _windowPageControl.Controls.IndexOf(page);
                var rect = pageRect[currentTabIndex];
                var closeIconSize = 24 * DPI;

                if (_drawTabIcons)
                {
                    var startingIconMeasure = graphics.MeasureString("", Font);
                    var iconX = rect.X + (TAB_HEADER_PADDING * DPI);

                    var inlinePaddingX = startingIconMeasure.Width + (TAB_HEADER_PADDING * DPI);
                    rect.X += inlinePaddingX;
                    rect.Width -= inlinePaddingX + closeIconSize;

                    graphics.DrawString("", Font, foreColor.Brush(), new RectangleF(iconX, _titleHeightDPI / 2 - startingIconMeasure.Height / 2, startingIconMeasure.Width, startingIconMeasure.Height));

                    using var format = new StringFormat()
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap
                    };

                    graphics.DrawString(page.Text, Font, foreColor.Brush(), rect, format);
                }
                else
                {
                    page.DrawString(graphics, foreColor, rect);
                }
            }

            // Tab Close Button
            if (_tabCloseButton)
            {
                var size = 20 * DPI;
                using var brush = new SolidBrush(Color.FromArgb((int)(tabCloseHoverAnimationManager.GetProgress() * hoverColor.A), hoverColor.RemoveAlpha()));

                _closeTabBoxRect = new(x + width - TAB_HEADER_PADDING / 2 - size, _titleHeightDPI / 2 - size / 2, size, size);
                graphics.FillPie(brush, _closeTabBoxRect.X, _closeTabBoxRect.Y, _closeTabBoxRect.Width, _closeTabBoxRect.Height, 0, 360);

                using var linePen = new Pen(foreColor) { Width = 1.6f };
                size = 4f * DPI;

                graphics.DrawLine(linePen,
                    _closeTabBoxRect.Left + _closeTabBoxRect.Width / 2 - size,
                    _closeTabBoxRect.Top + _closeTabBoxRect.Height / 2 - size,
                    _closeTabBoxRect.Left + _closeTabBoxRect.Width / 2 + size,
                    _closeTabBoxRect.Top + _closeTabBoxRect.Height / 2 + size);

                graphics.DrawLine(linePen,
                    _closeTabBoxRect.Left + _closeTabBoxRect.Width / 2 - size,
                    _closeTabBoxRect.Top + _closeTabBoxRect.Height / 2 + size,
                    _closeTabBoxRect.Left + _closeTabBoxRect.Width / 2 + size,
                    _closeTabBoxRect.Top + _closeTabBoxRect.Height / 2 - size);
            }

            // New Tab Button
            if (_newTabButton)
            {
                var size = 24 * DPI;
                var newHoverColor = hoverColor.Alpha(30);
                var color = Color.FromArgb((int)(newTabHoverAnimationManager.GetProgress() * newHoverColor.A), newHoverColor.RemoveAlpha());

                using var brush = new SolidBrush(color);
                color = foreColor.Alpha(220);

                using var linePen = new Pen(color) { Width = 1.6f };

                graphics.FillPath(brush, _newTabBoxRect.Radius(4));
                var lastTabRect = pageRect[pageRect.Count - 1];
                _newTabBoxRect = new(lastTabRect.X + lastTabRect.Width + size / 2, _titleHeightDPI / 2 - size / 2, size, size);

                size = 6 * DPI;

                graphics.DrawLine(linePen,
                    _newTabBoxRect.Left + _newTabBoxRect.Width / 2 - size,
                    _newTabBoxRect.Top + _newTabBoxRect.Height / 2,
                    _newTabBoxRect.Left + _newTabBoxRect.Width / 2 + size,
                    _newTabBoxRect.Top + _newTabBoxRect.Height / 2);

                graphics.DrawLine(linePen,
                    _newTabBoxRect.Left + _newTabBoxRect.Width / 2,
                    _newTabBoxRect.Top + _newTabBoxRect.Height / 2 - size,
                    _newTabBoxRect.Left + _newTabBoxRect.Width / 2,
                    _newTabBoxRect.Top + _newTabBoxRect.Height / 2 + size);
            }
        }

        // Title Border
        if (_drawTitleBorder)
        {
            if (titleColor != Color.Empty)
            {
                using var pen = TitleColor.Determine().Alpha(30).Pen();
                graphics.DrawLine(pen, Width, _titleHeightDPI - 1, 0, _titleHeightDPI - 1);
            }
            else
            {
                using var pen = ColorScheme.BorderColor.Pen();
                graphics.DrawLine(pen, Width, _titleHeightDPI - 1, 0, _titleHeightDPI - 1);
            }
        }
    }

    private void UpdateCachedMetrics()
    {
        _cachedMetrics = new CachedMetrics
        {
            TitleHeightDPI = _titleHeight * DPI,
            IconWidthDPI = _iconWidth * DPI,
            SymbolSizeDPI = _symbolSize * DPI,
            IsMetricsValid = true
        };
    }

    private void DrawControlBoxButtons(Graphics g, Color foreColor)
    {
        // Cache hover colors
        var closeHoverColor = Color.FromArgb(222, 179, 30, 30);
        
        if (_inCloseBox)
        {
            var alpha = (int)(closeBoxHoverAnimationManager.GetProgress() * closeHoverColor.A);
            g.FillRectangle(Color.FromArgb(alpha, closeHoverColor.RemoveAlpha()), _controlBoxRect);
        }

        // Draw close button
        using var closePen = new Pen(_inCloseBox ? Color.White : foreColor);
        var centerX = _controlBoxRect.Left + _controlBoxRect.Width / 2;
        var centerY = _controlBoxRect.Top + _controlBoxRect.Height / 2;
        var offset = 6 * DPI;

        g.DrawLine(closePen, centerX - offset, centerY - offset, centerX + offset, centerY + offset);
        g.DrawLine(closePen, centerX - offset, centerY + offset, centerX + offset, centerY - offset);

        // Draw other control buttons similarly...
    }

    private void DrawWindowTitle(Graphics g, Color foreColor)
    {
        var stringSize = g.MeasureString(Text, Font);
        var textPoint = new PointF(
            (showMenuInsteadOfIcon ? _formMenuRect.X + _formMenuRect.Width : 16 * DPI + 14),
            (_cachedMetrics.TitleHeightDPI / 2 - stringSize.Height / 2));

        using var textBrush = new SolidBrush(foreColor);
        g.DrawString(Text, Font, textBrush, textPoint);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        _needsLayoutUpdate = true;
        _cachedMetrics.IsMetricsValid = false;
        base.OnSizeChanged(e);
        CalcSystemBoxPos();
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

        //If there isn't a base tab control, the rects shouldn't be calculated
        //If there aren't tab pages in the base tab control, the list should just be empty which has been set already; exit the void
        if (_windowPageControl == null || _windowPageControl.Count == 0)
            return;

        //Calculate the bounds of each tab header specified in the base tab control

        float tabAreaWidth = 44;

        if (controlBox)
            tabAreaWidth += _controlBoxRect.Width;

        if (MinimizeBox)
            tabAreaWidth += _minimizeBoxRect.Width;

        if (MaximizeBox)
            tabAreaWidth += _maximizeBoxRect.Width;

        if (ExtendBox)
            tabAreaWidth += _extendBoxRect.Width;

        float maxSize = 200f * DPI;

        tabAreaWidth = (Width - tabAreaWidth - 30) / _windowPageControl.Count;
        if (tabAreaWidth > maxSize)
            tabAreaWidth = maxSize;

        pageRect.Add(new(44, 0, tabAreaWidth, _titleHeightDPI));
        for (int i = 1; i < _windowPageControl.Count; i++)
            pageRect.Add(new(pageRect[i - 1].Right, 0, tabAreaWidth, _titleHeightDPI));
    }
}