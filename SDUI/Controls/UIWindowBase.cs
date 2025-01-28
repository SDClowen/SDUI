using SDUI.Helpers;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using static SDUI.NativeMethods;

namespace SDUI.Controls;

public class UIWindowBase : Form
{
    protected bool enableFullDraggable;
    private int dwmMargin = 1;
    private bool right = false;
    private Point location;
    private readonly List<UIElementBase> _elements = new();
    private UIElementBase _focusedElement;
    private UIElementBase _lastHoveredElement;
    private bool _mouseInClient;
    private Cursor _currentCursor;

    // Z-order için yeni özellikler
    private int _maxZOrder = 0;

    public int DwmMargin
    {
        get => dwmMargin;
        set => dwmMargin = value;
    }

    /// <summary>
    /// Get DPI
    /// </summary>
    public float DPI => DeviceDpi / 96.0f;

    /// <summary>
    /// Has aero enabled by windows <c>true</c>; otherwise <c>false</c>
    /// </summary>
    private bool _aeroEnabled
    {
        get
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return enabled == 1;
            }

            return false;
        }
    }

    private Bitmap bitmap;
    private readonly bool designMode;
    public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.Width, bitmap.Height);

    [Category("Appearance")]
    public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

    public UIElementBase FocusedElement
    {
        get => _focusedElement;
        private set
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

    internal UIElementBase LastHoveredElement
    {
        get => _lastHoveredElement;
        set
        {
            if (_lastHoveredElement != value)
            {
                _lastHoveredElement = value;
                UpdateCursor(value);
            }
        }
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

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr hCursor);

    public UIWindowBase()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw, true
        );

        designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        UpdateStyles();
    }

    public new List<UIElementBase> Controls => _elements;

    public void AddElement(UIElementBase element)
    {
        if (!_elements.Contains(element))
        {
            element.Parent = this;
            _maxZOrder++;
            element.ZOrder = _maxZOrder;
            _elements.Add(element);
            
            if (_focusedElement == null && element.TabStop)
            {
                FocusedElement = element;
            }
            
            PerformLayout();
        }
    }

    public void RemoveElement(UIElementBase element)
    {
        if (_elements.Remove(element))
        {
            element.Parent = null;
            if (_focusedElement == element)
            {
                FocusedElement = null;
            }
            PerformLayout();
        }
    }

    protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        PaintSurface?.Invoke(this, e);

        // Elementleri Z-order'a göre sırala ve render et
        foreach (var element in _elements.OrderBy(el => el.ZOrder))
        {
            if (element.Visible)
            {
                using (var elementSurface = SKSurface.Create(e.Info))
                {
                    element.OnPaint(new SKPaintSurfaceEventArgs(elementSurface, new SKImageInfo(element.Size.Width, element.Size.Height)));
                    
                    // Element'in surface'ini ana surface'e kopyala
                    var elementImage = elementSurface.Snapshot();
                    e.Surface.Canvas.DrawImage(elementImage, element.Location.X, element.Location.Y);
                }
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (designMode)
            return;

        base.OnPaint(e);

        var info = CreateBitmap();
        if (info.Width == 0 || info.Height == 0)
            return;

        var data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

        using (var surface = SKSurface.Create(info, data.Scan0, data.Stride))
        {
            OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
            surface.Canvas.Flush();
        }

        bitmap.UnlockBits(data);
        e.Graphics.DrawImage(bitmap, 0, 0);
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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        
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
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

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
            right = e.Button == MouseButtons.Right;
            location = e.Location;
            DragForm(Handle);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

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

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _lastHoveredElement?.OnMouseLeave(e);
        _lastHoveredElement = null;
    }

    private SKImageInfo CreateBitmap()
    {
        var info = new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

        if (bitmap == null || bitmap.Width != info.Width || bitmap.Height != info.Height)
        {
            FreeBitmap();
            if (info.Width != 0 && info.Height != 0)
                bitmap = new Bitmap(info.Width, info.Height, PixelFormat.Format32bppPArgb);
        }

        return info;
    }

    private void FreeBitmap()
    {
        if (bitmap != null)
        {
            bitmap.Dispose();
            bitmap = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            FreeBitmap();

        base.Dispose(disposing);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (msg.Msg == 256 | msg.Msg == 260)
            if (keyData == Keys.Escape)
                Close();

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected void DragForm(IntPtr handle)
    {
        ReleaseCapture();
        SendMessage(handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            if (!_aeroEnabled)
                cp.ClassStyle |= CS_DROPSHADOW;

            cp.ClassStyle |= CS_DBLCLKS;

            var style = (uint)cp.Style;

            if (DesignMode)
                style &= ~(uint)SetWindowLongFlags.WS_CAPTION;

            style &= ~(uint)SetWindowLongFlags.WS_SYSMENU;
            style &= ~(uint)SetWindowLongFlags.WS_THICKFRAME;
            style &= ~(uint)SetWindowLongFlags.WS_MINIMIZE;
            style &= ~(uint)SetWindowLongFlags.WS_MINIMIZEBOX;
            style &= ~(uint)SetWindowLongFlags.WS_MAXIMIZE;
            style &= ~(uint)SetWindowLongFlags.WS_MAXIMIZEBOX;
            //style &= ~(uint)SetWindowLongFlags.WS_BORDER;
            style |= (uint)SetWindowLongFlags.WS_TILED;
            cp.Style = (int)style;

            return cp;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (DesignMode)
            return;

        // Otherwise, it will not be applied.
        if (StartPosition == FormStartPosition.CenterScreen)
            CenterToScreen();

        if (BackColor != ColorScheme.BackColor)
            BackColor = ColorScheme.BackColor;
    }

    private const int htLeft = 10;
    private const int htRight = 11;
    private const int htTop = 12;
    private const int htTopLeft = 13;
    private const int htTopRight = 14;
    private const int htBottom = 15;
    private const int htBottomLeft = 16;
    private const int htBottomRight = 17;
    protected override void WndProc(ref Message m)
    {
        if (DesignMode)
        {
            base.WndProc(ref m);
            return;
        }

        switch (m.Msg)
        {
            case WM_NCHITTEST:
                {
                    if (WindowState != FormWindowState.Maximized)
                    {
                        int gripDist = 10;

                        var pt = PointToClient(Cursor.Position);

                        Size clientSize = ClientSize;
                        ///allow resize on the lower right corner
                        if (pt.X >= clientSize.Width - gripDist && pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htBottomLeft : htBottomRight);
                            return;
                        }
                        ///allow resize on the lower left corner
                        if (pt.X <= gripDist && pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htBottomRight : htBottomLeft);
                            return;
                        }
                        ///allow resize on the upper right corner
                        if (pt.X <= gripDist && pt.Y <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htTopRight : htTopLeft);
                            return;
                        }
                        ///allow resize on the upper left corner
                        if (pt.X >= clientSize.Width - gripDist && pt.Y <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)(IsMirrored ? htTopLeft : htTopRight);
                            return;
                        }
                        ///allow resize on the top border
                        if (pt.Y <= 2 && clientSize.Height >= 2)
                        {
                            m.Result = (IntPtr)htTop;
                            return;
                        }
                        ///allow resize on the bottom border
                        if (pt.Y >= clientSize.Height - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htBottom;
                            return;
                        }
                        ///allow resize on the left border
                        if (pt.X <= gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htLeft;
                            return;
                        }
                        ///allow resize on the right border
                        if (pt.X >= clientSize.Width - gripDist && clientSize.Height >= gripDist)
                        {
                            m.Result = (IntPtr)htRight;
                            return;
                        }
                    }

                    if ((int)m.Result == HTCLIENT)     // drag the form
                        m.Result = (IntPtr)HTCAPTION;

                    break;
                }
            case WM_NCCALCSIZE:

                var handle = Handle;

                var lpwp = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
                if (lpwp.HWND == IntPtr.Zero)
                    return;

                if ((lpwp.flags & (SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW)) != 0)
                {
                    return;
                }
                // TEMPORARY CODE
                // if (OS.IsAppThemed ()) {
                // OS.InvalidateRect (handle, null, true);
                // return result;
                // }
                int bits = GetWindowLong(handle, WindowLongIndexFlags.GWL_STYLE).ToInt32();
                if ((bits & TCS_MULTILINE) != 0)
                {
                    InvalidateRect(handle, new Rect(), true);
                    return;
                }

                var rect = new Rect();
                SetRect(rect, 0, 0, lpwp.cx, lpwp.cy);

                SendMessage(handle, WM_NCCALCSIZE, 0, ref rect);
                int newWidth = rect.Right - rect.Left;
                int newHeight = rect.Bottom - rect.Top;
                GetClientRect(handle, ref rect);
                int oldWidth = rect.Right - rect.Left;
                int oldHeight = rect.Bottom - rect.Top;
                if (newWidth == oldWidth && newHeight == oldHeight)
                {
                    return;
                }
                var inset = new Rect();
                SendMessage(handle, TCM_ADJUSTRECT, 0, ref inset);
                int marginX = -inset.Right, marginY = -inset.Bottom;
                if (newWidth != oldWidth)
                {
                    int left = oldWidth;
                    if (newWidth < oldWidth)
                        left = newWidth;
                    SetRect(rect, left - marginX, 0, newWidth, newHeight);
                    InvalidateRect(handle, rect, true);
                }
                if (newHeight != oldHeight)
                {
                    int bottom = oldHeight;
                    if (newHeight < oldHeight)
                        bottom = newHeight;
                    if (newWidth < oldWidth)
                        oldWidth -= marginX;
                    SetRect(rect, 0, bottom - marginY, oldWidth, newHeight);
                    InvalidateRect(handle, rect, true);

                }
                return;
        }

        base.WndProc(ref m);
    }

    public void ChangeControlsTheme(Control control)
    {
        control.Enabled = false;

        var isDark = ColorScheme.BackColor.IsDark();
        if (control is ListView)
        {
            if (WindowsHelper.IsModern)
            {
                SendMessage(control.Handle, 0x0127, (1 << 16) | (1 & 0xffff), 0);

                AllowDarkModeForWindow(control.Handle, true);
            }
        }

        if (control is RichTextBox || control is ListBox)
        {
            try
            {
                control.BackColor = ColorScheme.BackColor;
                control.ForeColor = ColorScheme.ForeColor;
            }
            catch (Exception)
            {
            }
        }

        WindowsHelper.UseImmersiveDarkMode(control.Handle, isDark);

        foreach (Control subControl in control.Controls)
        {
            ChangeControlsTheme(subControl);
        }
        control.Enabled = true;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (DesignMode)
            return;

        WindowsHelper.ApplyRoundCorner(this.Handle);

        if (_aeroEnabled)
        {
            var v = 2;

            DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, ref v, 4);
            var margins = new MARGINS()
            {
                Bottom = dwmMargin,
                Left = dwmMargin,
                Right = dwmMargin,
                Top = dwmMargin
            };

            DwmExtendFrameIntoClientArea(this.Handle, ref margins);
        }

        SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (DesignMode)
            return;

        ChangeControlsTheme(this);

        if (!WindowsHelper.IsModern)
            return;

        WindowsHelper.UseImmersiveDarkMode(Handle, ColorScheme.BackColor.IsDark());

        if (ColorScheme.BackColor.IsDark())
        {
            var flag = DWMSBT_TABBEDWINDOW;
            DwmSetWindowAttribute(
                Handle,
                DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                ref flag,
                 Marshal.SizeOf<int>());
        }

    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        PerformLayout();
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        PerformLayout();
    }

    protected override void OnControlRemoved(ControlEventArgs e)
    {
        base.OnControlRemoved(e);
        PerformLayout();
    }

    public void PerformLayout()
    {
        if (designMode) return;

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

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);

        // Alt kontrollere DPI değişikliğini bildir
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

        // Yeniden çizim
        Invalidate();
    }
}