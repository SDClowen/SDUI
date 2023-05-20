using SDUI.Animation;
using SDUI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class UIWindow : UIWindowBase
{
    /// <summary>
    /// If extend box clicked invoke the event
    /// </summary>
    public event EventHandler OnExtendBoxClick;

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
    private Rectangle _controlBoxRect;

    /// <summary>
    /// The rectangle of maximize box
    /// </summary>
    private Rectangle _maximizeBoxRect;

    /// <summary>
    /// The rectangle of minimize box
    /// </summary>
    private Rectangle _minimizeBoxRect;

    /// <summary>
    /// The rectangle of extend box
    /// </summary>
    private Rectangle _extendBoxRect;

    /// <summary>
    /// The control box left value
    /// </summary>
    private int _controlBoxLeft;

    /// <summary>
    /// The size of the window before it is maximized
    /// </summary>
    private Size _sizeOfBeforeMaximized;

    /// <summary>
    /// The position of the window before it is maximized
    /// </summary>
    private Point _locationOfBeforeMaximized;

    private int _iconWidth = 42;
    [DefaultValue(42)]
    [Description("Gets or sets the header bar icon width")]
    public int IconWidth
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

    [DefaultValue(false)]
    public bool AllowShowTitle
    {
        get => ShowTitle;
        set => ShowTitle = value;
    }

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

    private int _symbolSize = 24;

    [DefaultValue(24)]
    public int ExtendSymbolSize
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
            Padding = new Padding(Padding.Left, value ? _titleHeight : 0, Padding.Right, Padding.Bottom);
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
    private int _titleHeight = 32;

    /// <summary>
    /// Gets or sets the title height
    /// </summary>
    public int TitleHeight
    {
        get => _titleHeight;
        set
        {
            _titleHeight = Math.Max(value, 31);
            Padding = new Padding(0, showTitle ? _titleHeight : 0, 0, 0);
            Invalidate();
            CalcSystemBoxPos();
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
    /// The title font
    /// </summary>
    private Font _titleFont = new Font("Segoe UI", 9.25f);

    /// <summary>
    /// Gets or sets the title font
    /// </summary>
    [Description("The title font")]
    public Font TitleFont
    {
        get => _titleFont;
        set
        {
            _titleFont = value;
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
    private readonly AnimationManager minBoxHoverAnimationManager;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager maxBoxHoverAnimationManager;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager closeBoxHoverAnimationManager;

    /// <summary>
    /// Min Box hover animation manager
    /// </summary>
    private readonly AnimationManager extendBoxHoverAnimationManager;

    /// <summary>
    /// tab area animation manager
    /// </summary>
    private readonly AnimationManager pageAreaAnimationManager;

    private int previousSelectedPageIndex;
    private Point animationSource;
    private List<Rectangle> pageRect;
    private const int TAB_HEADER_PADDING = 9;
    private const int TAB_INDICATOR_HEIGHT = 2;

    private long _stickyBorderTime = 5000000;
    [Description("Set or get the maximum time to stay at the edge of the display(ms)")]
    [DefaultValue(500)]
    public long StickyBorderTime
    {
        get => _stickyBorderTime / 10000;
        set => _stickyBorderTime = value * 10000;
    }

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

        pageAreaAnimationManager = new AnimationManager
        {
            AnimationType = AnimationType.EaseOut,
            Increment = 0.06
        };

        minBoxHoverAnimationManager = new AnimationManager
        {
            Increment = 0.1,
            AnimationType = AnimationType.Linear
        };
        maxBoxHoverAnimationManager = new AnimationManager
        {
            Increment = 0.1,
            AnimationType = AnimationType.Linear
        };
        closeBoxHoverAnimationManager = new AnimationManager
        {
            Increment = 0.1,
            AnimationType = AnimationType.Linear
        };
        extendBoxHoverAnimationManager = new AnimationManager
        {
            Increment = 0.1,
            AnimationType = AnimationType.Linear
        };

        minBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        maxBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        closeBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        extendBoxHoverAnimationManager.OnAnimationProgress += sender => Invalidate();
        pageAreaAnimationManager.OnAnimationProgress += sender => Invalidate();

        WindowsHelper.ApplyRoundCorner(this.Handle);
    }

    private bool _inCloseBox, _inMaxBox, _inMinBox, _inExtendBox;

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
            _controlBoxRect = new Rectangle(Width - _iconWidth, 0, _iconWidth, _titleHeight);
            _controlBoxLeft = _controlBoxRect.Left - 2;

            if (MaximizeBox)
            {
                _maximizeBoxRect = new Rectangle(_controlBoxRect.Left - _iconWidth, _controlBoxRect.Top, _iconWidth, _titleHeight);
                _controlBoxLeft = _maximizeBoxRect.Left - 2;
            }
            else
            {
                _maximizeBoxRect = new Rectangle(Width + 1, Height + 1, 1, 1);
            }

            if (MinimizeBox)
            {
                _minimizeBoxRect = new Rectangle(MaximizeBox ? _maximizeBoxRect.Left - _iconWidth - 2 : _controlBoxRect.Left - _iconWidth - 2, _controlBoxRect.Top, _iconWidth, _titleHeight);
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
                    _extendBoxRect = new Rectangle(_minimizeBoxRect.Left - _iconWidth - 2, _controlBoxRect.Top, _iconWidth, _titleHeight);
                }
                else
                {
                    _extendBoxRect = new Rectangle(_controlBoxRect.Left - _iconWidth - 2, _controlBoxRect.Top, _iconWidth, _titleHeight);
                }
            }
        }
        else
        {
            _extendBoxRect = _maximizeBoxRect = _minimizeBoxRect = _controlBoxRect = new Rectangle(Width + 1, Height + 1, 1, 1);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (ShowTitle)
        {
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
                    ExtendMenu.Show(this, _extendBoxRect.Left, TitleHeight - 1);
                }
                else
                {
                    OnExtendBoxClick?.Invoke(this, EventArgs.Empty);
                }
            }
        }
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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (_inCloseBox || _inMaxBox || _inMinBox || _inExtendBox)
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
        base.OnMouseDoubleClick(e);

        if (!MaximizeBox) return;
        if (_inCloseBox || _inMaxBox || _inMinBox || _inExtendBox) return;
        if (!ShowTitle) return;
        if (e.Y > Padding.Top) return;

        ShowMaximize();
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
                Top = screen.WorkingArea.Bottom - TitleHeight;
            }
        }

        IsStayAtTopBorder = false;
        Cursor.Clip = new Rectangle();
        _formMoveMouseDown = false;

        if (pageRect == null)
            UpdateTabRects();

        for (int i = 0; i < pageRect.Count; i++)
        {
            if (pageRect[i].Contains(e.Location))
            {
                _windowPageControl.SelectedIndex = i;
            }
        }

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

            if (isChange)
                Invalidate();
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _inExtendBox = _inCloseBox = _inMaxBox = _inMinBox = false;
        closeBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        minBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        maxBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
        extendBoxHoverAnimationManager.StartNewAnimation(AnimationDirection.Out);

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
        var foreColor = ColorScheme.ForeColor;
        if (titleColor != Color.Empty)
            foreColor = titleColor.Determine();

        var hoverColor = ColorScheme.BorderColor;
        if (titleColor != Color.Empty)
            hoverColor = foreColor.Alpha(20);

        var graphics = e.Graphics;

        //base.OnPaint(e);
        //graphics.Clear(BackColor);

        if (FullDrawHatch)
        {
            using var hatchBrush = new HatchBrush(_hatch, hoverColor, titleColor);
            graphics.FillRectangle(hatchBrush, 0, 0, Width, Height);
        }
        else
            graphics.FillRectangle(ColorScheme.BackColor, 0, 0, Width, Height);

        if (Width <= 0 || Height <= 0)
            return;

        if (!ShowTitle)
            return;

        graphics.SetHighQuality();

        if (titleColor != Color.Empty)
            graphics.FillRectangle(titleColor, 0, 0, Width, _titleHeight);

        if (controlBox)
        {
            var closeHoverColor = Color.FromArgb(222, 179, 30, 30);

            // if (_inCloseBox)
            graphics.FillRectangle(Color.FromArgb((int)(closeBoxHoverAnimationManager.GetProgress() * closeHoverColor.A), closeHoverColor.RemoveAlpha()), _controlBoxRect);

            for (int i = 0; i < 3; i++)
            {
                graphics.DrawLine(_inCloseBox ? Color.White : foreColor,
                _controlBoxRect.Left + _controlBoxRect.Width / 2 - 5,
                _controlBoxRect.Top + _controlBoxRect.Height / 2 - 5,
                _controlBoxRect.Left + _controlBoxRect.Width / 2 + 5,
                _controlBoxRect.Top + _controlBoxRect.Height / 2 + 5);

                graphics.DrawLine(_inCloseBox ? Color.White : foreColor,
                    _controlBoxRect.Left + _controlBoxRect.Width / 2 - 5,
                    _controlBoxRect.Top + _controlBoxRect.Height / 2 + 5,
                    _controlBoxRect.Left + _controlBoxRect.Width / 2 + 5,
                    _controlBoxRect.Top + _controlBoxRect.Height / 2 - 5);
            }
        }

        if (MaximizeBox)
        {
            //if (_inMaxBox)
            graphics.FillRectangle(Color.FromArgb((int)(maxBoxHoverAnimationManager.GetProgress() * hoverColor.A), hoverColor.RemoveAlpha()), _maximizeBoxRect);

            if (WindowState == FormWindowState.Maximized)
            {
                graphics.DrawRectangle(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - 5,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - 1,
                    7, 7);

                graphics.DrawLine(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - 2,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - 1,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - 2,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - 4);

                graphics.DrawLine(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - 2,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - 4,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + 5,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - 4);

                graphics.DrawLine(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + 5,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - 4,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + 5,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 + 3);

                graphics.DrawLine(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + 5,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 + 3,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 + 3,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 + 3);
            }

            if (WindowState == FormWindowState.Normal)
            {
                graphics.DrawRectangle(foreColor,
                    _maximizeBoxRect.Left + _maximizeBoxRect.Width / 2 - 5,
                    _maximizeBoxRect.Top + _maximizeBoxRect.Height / 2 - 4,
                    10, 9);
            }
        }

        if (MinimizeBox)
        {
            //if (_inMinBox)
            graphics.FillRectangle(Color.FromArgb((int)(minBoxHoverAnimationManager.GetProgress() * hoverColor.A), hoverColor.RemoveAlpha()), _minimizeBoxRect);

            graphics.DrawLine(foreColor,
                _minimizeBoxRect.Left + _minimizeBoxRect.Width / 2 - 6,
                _minimizeBoxRect.Top + _minimizeBoxRect.Height / 2,
                _minimizeBoxRect.Left + _minimizeBoxRect.Width / 2 + 5,
                _minimizeBoxRect.Top + _minimizeBoxRect.Height / 2);
        }

        if (ExtendBox)
        {
            //if (_inExtendBox)
            graphics.FillRectangle(Color.FromArgb((int)(extendBoxHoverAnimationManager.GetProgress() * hoverColor.A), hoverColor.RemoveAlpha()), _extendBoxRect);

            graphics.DrawLine(foreColor,
                    _extendBoxRect.Left + _extendBoxRect.Width / 2 - 5 - 1,
                    _extendBoxRect.Top + _extendBoxRect.Height / 2 - 2,
                    _extendBoxRect.Left + _extendBoxRect.Width / 2 - 1,
                    _extendBoxRect.Top + _extendBoxRect.Height / 2 + 3);

            graphics.DrawLine(foreColor,
                _extendBoxRect.Left + _extendBoxRect.Width / 2 + 5 - 1,
                _extendBoxRect.Top + _extendBoxRect.Height / 2 - 2,
                _extendBoxRect.Left + _extendBoxRect.Width / 2 - 1,
                _extendBoxRect.Top + _extendBoxRect.Height / 2 + 3);
        }

        graphics.SetDefaultQuality();

        var faviconSize = 16;
        if (Icon != null)
            graphics.DrawIcon(Icon, new Rectangle(10, (_titleHeight / 2) - (faviconSize / 2), faviconSize, faviconSize));

        if (_windowPageControl == null)
        {
            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
            var stringSize = TextRenderer.MeasureText(Text, TitleFont);
            var textPoint = new Point(14 + (Icon != null ? faviconSize : 0), TitleHeight / 2);
            TextRenderer.DrawText(e.Graphics, Text, TitleFont, textPoint, foreColor, flags);
        }

        if (_windowPageControl == null)
            return;

        if (!pageAreaAnimationManager.IsAnimating() || pageRect == null || pageRect.Count != _windowPageControl.Count)
            UpdateTabRects();

        var animationProgress = pageAreaAnimationManager.GetProgress();

        //Click feedback
        if (pageAreaAnimationManager.IsAnimating())
        {
            var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationProgress * 50)), foreColor));
            var rippleSize = (int)(animationProgress * pageRect[_windowPageControl.SelectedIndex].Width * 1.75);

            graphics.SetClip(pageRect[_windowPageControl.SelectedIndex]);
            graphics.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
            graphics.ResetClip();
            rippleBrush.Dispose();
        }

        // fix desing time error
        if (_windowPageControl.SelectedIndex <= -1 || _windowPageControl.SelectedIndex >= _windowPageControl.Count)
            return;

        //Animate page indicator
        var previousSelectedPageIndexIfHasOne = previousSelectedPageIndex == -1 ? _windowPageControl.SelectedIndex : previousSelectedPageIndex;
        var previousActivePageRect = pageRect[previousSelectedPageIndexIfHasOne];
        var activePageRect = pageRect[_windowPageControl.SelectedIndex];

        var y = activePageRect.Bottom - 2;
        var x = previousActivePageRect.X + (int)((activePageRect.X - previousActivePageRect.X) * animationProgress);
        var width = previousActivePageRect.Width + (int)((activePageRect.Width - previousActivePageRect.Width) * animationProgress);

        graphics.DrawRectangle(hoverColor, activePageRect.X, 0, width, _titleHeight);
        graphics.FillRectangle(hoverColor, x, 0, width, _titleHeight);
        graphics.FillRectangle(Color.DeepSkyBlue, x, y, width, TAB_INDICATOR_HEIGHT);

        //Draw tab headers
        foreach (UserControl page in _windowPageControl.Controls)
        {
            var currentTabIndex = _windowPageControl.Controls.IndexOf(page);
            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
            TextRenderer.DrawText(graphics, page.Text, _titleFont, pageRect[currentTabIndex], foreColor, flags);
        }

        if (_drawTitleBorder)
        {
            if (titleColor != Color.Empty)
            {
                using var pen = TitleColor.Determine().Alpha(25).Pen();
                graphics.DrawLine(pen, Width, _titleHeight - 1, 0, _titleHeight - 1);
            }
            else
            {
                using var pen = ColorScheme.BorderColor.Pen();
                graphics.DrawLine(pen, Width, _titleHeight - 1, 0, _titleHeight - 1);
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
            if (e.Y <= _titleHeight && e.X < _controlBoxLeft)
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
        pageRect = new List<Rectangle>();

        //If there isn't a base tab control, the rects shouldn't be calculated
        //If there aren't tab pages in the base tab control, the list should just be empty which has been set already; exit the void
        if (_windowPageControl == null || _windowPageControl.Count == 0)
            return;

        //Calculate the bounds of each tab header specified in the base tab control
        pageRect.Add(new Rectangle(44, 0, TAB_HEADER_PADDING * 2 + TextRenderer.MeasureText(_windowPageControl.Controls[0].Text, _titleFont).Width, _titleHeight));
        for (int i = 1; i < _windowPageControl.Count; i++)
            pageRect.Add(new Rectangle(pageRect[i - 1].Right, 0, TAB_HEADER_PADDING * 2 + TextRenderer.MeasureText(_windowPageControl.Controls[i].Text, _titleFont).Width, _titleHeight));
    }
}