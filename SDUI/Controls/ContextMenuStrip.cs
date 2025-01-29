using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class ContextMenuStrip : MenuStrip
{
    private UIElementBase _sourceElement;
    private bool _isOpen;
    private UIWindow _dropDownWindow;
    private bool _autoClose = true;

    public event EventHandler Opening;
    public event EventHandler Closing;

    public ContextMenuStrip()
    {
        Visible = false;
        ShowHoverEffect = true;
        RoundedCorners = true;
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

        Opening?.Invoke(this, EventArgs.Empty);

        _sourceElement = element;
        CreateDropDownWindow();
        PositionDropDown(location);
        _dropDownWindow.Show();
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

        _dropDownWindow?.Close();
        _dropDownWindow = null;
        _sourceElement = null;
        _isOpen = false;
    }

    private void CreateDropDownWindow()
    {
        _dropDownWindow = new UIWindow
        {
            ShowTitle = false,
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = MenuBackColor,
            TopMost = true
        };

        _dropDownWindow.Controls.Add(this);
        Visible = true;
        BringToFront();

        if (AutoClose)
        {
            _dropDownWindow.Deactivate += (s, e) => Hide();
        }

        _dropDownWindow.FormClosed += (s, e) =>
        {
            _isOpen = false;
            Visible = false;
            _dropDownWindow = null;
        };
    }

    private void PositionDropDown(Point location)
    {
        if (_dropDownWindow == null) return;

        var screen = _sourceElement != null
            ? Screen.FromControl(_sourceElement.ParentWindow)
            : Screen.FromPoint(location);

        var workingArea = screen.WorkingArea;
        var size = GetPreferredSize();

        // Menünün ekran dışına taşmasını önle
        if (location.X + size.Width > workingArea.Right)
            location.X = workingArea.Right - size.Width;
        if (location.Y + size.Height > workingArea.Bottom)
            location.Y = workingArea.Bottom - size.Height;
        if (location.X < workingArea.Left)
            location.X = workingArea.Left;
        if (location.Y < workingArea.Top)
            location.Y = workingArea.Top;

        _dropDownWindow.Location = location;
        _dropDownWindow.Size = size;
        Size = size;
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
            _dropDownWindow?.Dispose();
        }
        base.Dispose(disposing);
    }
}