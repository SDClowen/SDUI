using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ContextMenuStrip : MenuStrip
{
    private UIElementBase _sourceElement;
    private bool _isOpen;
    private UIWindow _ownerWindow;
    private bool _autoClose = true;
    private MouseEventHandler _ownerMouseDownHandler;
    private EventHandler _ownerDeactivateHandler;
    private KeyEventHandler _ownerKeyDownHandler;
    private bool _ownerPreviousKeyPreview;

    public event EventHandler Opening;
    public event EventHandler Closing;

    public ContextMenuStrip()
    {
        Visible = false;
        ShowHoverEffect = true;
        RoundedCorners = true;
        Dock = DockStyle.None;
        Anchor = AnchorStyles.Top | AnchorStyles.Left;
        AutoSize = false;
        TabStop = false;
    }

    [Category("Behavior")]
    [DefaultValue(true)]
    public bool AutoClose
    {
        get => _autoClose;
        set => _autoClose = value;
    }

    [Browsable(false)]
    public bool IsOpen => _isOpen;

    [Browsable(false)]
    public UIElementBase SourceElement => _sourceElement;

    public void Show(UIElementBase element, Point location)
    {
        if (_isOpen) return;

        var owner = ResolveOwnerWindow(element);
        if (owner == null)
            return;

        Opening?.Invoke(this, EventArgs.Empty);

        _sourceElement = element;
        _ownerWindow = owner;

        if (!_ownerWindow.Controls.Contains(this))
        {
            _ownerWindow.Controls.Add(this);
        }

        Visible = true;
        BringToFront();
        PositionDropDown(location);
        AttachOwnerHandlers();
        _ownerWindow.Invalidate();
        _isOpen = true;
    }

    public void Show(Point location)
    {
        Show(null, location);
    }

    public void Hide()
    {
        if (!_isOpen) return;

        Closing?.Invoke(this, EventArgs.Empty);

        DetachOwnerHandlers();

        Visible = false;
        _ownerWindow?.Invalidate();
        _ownerWindow = null;
        _sourceElement = null;
        _isOpen = false;
    }

    private void PositionDropDown(Point screenLocation)
    {
        if (_ownerWindow == null) return;

        var windowLocation = _ownerWindow.PointToClient(screenLocation);
        var clientArea = _ownerWindow.ClientRectangle;
        var size = GetPreferredSize();

        var maxX = Math.Max(0, clientArea.Width - size.Width);
        var maxY = Math.Max(0, clientArea.Height - size.Height);

        windowLocation.X = Math.Min(Math.Max(windowLocation.X, clientArea.Left), maxX);
        windowLocation.Y = Math.Min(Math.Max(windowLocation.Y, clientArea.Top), maxY);

        Location = windowLocation;
        Size = size;
        BringToFront();
    }

    private UIWindow ResolveOwnerWindow(UIElementBase element)
    {
        if (Parent is UIWindow existingWindow)
            return existingWindow;

        if (element != null)
        {
            if (element.ParentWindow is UIWindow parentWindow)
                return parentWindow;

            if (element.FindForm() is UIWindow elementWindow)
                return elementWindow;
        }

        if (Form.ActiveForm is UIWindow activeWindow)
            return activeWindow;

        return Application.OpenForms.OfType<UIWindow>().FirstOrDefault();
    }

    private void AttachOwnerHandlers()
    {
        if (_ownerWindow == null || !AutoClose)
            return;

        _ownerMouseDownHandler ??= OwnerWindowOnMouseDown;
        _ownerDeactivateHandler ??= OwnerWindowOnDeactivate;
        _ownerKeyDownHandler ??= OwnerWindowOnKeyDown;

        _ownerWindow.MouseDown += _ownerMouseDownHandler;
        _ownerPreviousKeyPreview = _ownerWindow.KeyPreview;
        _ownerWindow.KeyPreview = true;

        _ownerWindow.Deactivate += _ownerDeactivateHandler;
        _ownerWindow.KeyDown += _ownerKeyDownHandler;
    }

    private void DetachOwnerHandlers()
    {
        if (_ownerWindow == null)
            return;

        if (_ownerMouseDownHandler != null)
            _ownerWindow.MouseDown -= _ownerMouseDownHandler;
        if (_ownerDeactivateHandler != null)
            _ownerWindow.Deactivate -= _ownerDeactivateHandler;
        if (_ownerKeyDownHandler != null)
            _ownerWindow.KeyDown -= _ownerKeyDownHandler;

        _ownerWindow.KeyPreview = _ownerPreviousKeyPreview;
    }

    private void OwnerWindowOnMouseDown(object sender, MouseEventArgs e)
    {
        if (!_isOpen || !_autoClose)
            return;

        if (!Bounds.Contains(e.Location))
        {
            Hide();
        }
    }

    private void OwnerWindowOnDeactivate(object sender, EventArgs e)
    {
        if (!_isOpen || !_autoClose)
            return;

        Hide();
    }

    private void OwnerWindowOnKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isOpen || !_autoClose)
            return;

        if (e.KeyCode == Keys.Escape)
        {
            Hide();
            e.Handled = true;
        }
    }

    private Size GetPreferredSize()
    {
        float width = ItemPadding * 2;
        float height = ItemPadding;

        foreach (var item in Items)
        {
            if (item.IsSeparator)
            {
                height += SeparatorMargin * 2 + 1;
            }
            else
            {
                width = Math.Max(width, MeasureItemWidth(item) + ItemPadding * 2);
                height += ItemHeight + ItemPadding;
            }
        }

        return new Size((int)width, (int)height);
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Right && AutoClose)
        {
            Hide();
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (AutoClose && !ClientRectangle.Contains(PointToClient(Cursor.Position)))
        {
            Hide();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Hide();
        }
        base.Dispose(disposing);
    }
}