using System;
using System.Collections.Generic;
using System.Drawing;
using SDUI;
using SDUI.Controls;
using SDUI.Extensions;
using SDUI.Designer.Commands;

namespace SDUI.Designer;

/// <summary>
/// Design surface where user can drag and drop controls
/// </summary>
public class DesignSurface : DoubleBufferedControl
{
    private readonly DoubleBufferedControl _canvas;
    private readonly List<DesignControl> _designControls = new();
    private readonly UndoRedoManager _undoRedoManager = new();
    private DesignControl? _selectedControl;
    private string? _pendingControlType;
    private Point _dragStart;
    private bool _isDragging;
    private bool _isResizing;
    private ResizeHandle _activeHandle = ResizeHandle.None;
    private Rectangle _resizeStartBounds;
    private Point _moveStartLocation;
    private UIElementBase? _highlightedContainer;
    private SelectionOverlay? _selectionOverlay;
    private DesignGuides? _designGuides;
    private int _gridSize = 10;
    private const int HandleSize = 8;
    private bool _gridSnappingEnabled = true;
    private bool _showGridLines = true;
    private bool _showSnapLines = true;

    private enum ResizeHandle
    {
        None,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left
    }

    public DesignSurface()
    {
        BackColor = ColorScheme.BackColor.Brightness(0.05f);
        AutoScroll = true;
        AutoScrollMargin = new Size(120, 120);

        _canvas = new DoubleBufferedControl
        {
            BackColor = Color.Transparent,
            Location = Point.Empty,
            Size = Size
        };
        Controls.Add(_canvas);
        
        // Create design guides overlay
        _designGuides = new DesignGuides();
        Controls.Add(_designGuides);
        
        // Create selection overlay (SDUI Panel with border)
        _selectionOverlay = new SelectionOverlay();
        Controls.Add(_selectionOverlay);
        
        // Enable mouse events
        this.MouseDown += DesignSurface_MouseDown;
        this.MouseMove += DesignSurface_MouseMove;
        this.MouseUp += DesignSurface_MouseUp;
        this.MouseWheel += (s, e) => HandleMouseWheel(e.Delta);
        this.SizeChanged += (s, e) => UpdateCanvasSize();

        if (_vScrollBar != null)
            _vScrollBar.ValueChanged += (s, e) => UpdateCanvasScroll();
        if (_hScrollBar != null)
            _hScrollBar.ValueChanged += (s, e) => UpdateCanvasScroll();

        _vScrollBar?.BringToFront();
        _hScrollBar?.BringToFront();
    }

    public event EventHandler<DesignControl>? ControlSelected;
    public event EventHandler? SelectionCleared;
    public event EventHandler? ControlsChanged;

    public UndoRedoManager UndoRedoManager => _undoRedoManager;

    public string? PendingControlType
    {
        get => _pendingControlType;
        set => _pendingControlType = value;
    }

    private void DesignSurface_MouseDown(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (e.Button == System.Windows.Forms.MouseButtons.Left)
        {
            // If there's a pending control type, add it
            if (!string.IsNullOrEmpty(_pendingControlType))
            {
                var location = _gridSnappingEnabled ? SnapToGrid(e.Location) : e.Location;
                AddDesignControl(_pendingControlType, location);
                _pendingControlType = null;
                return;
            }

            // Check if we're clicking a resize handle first
            if (_selectedControl != null)
            {
                _activeHandle = GetHandleAtPoint(e.Location, GetAbsoluteBounds(_selectedControl.Control));
                if (_activeHandle != ResizeHandle.None)
                {
                    _isResizing = true;
                    _resizeStartBounds = _selectedControl.Control.Bounds;
                    _dragStart = e.Location;
                    this.Cursor = GetCursorForHandle(_activeHandle);
                    return;
                }
            }

            // Check if clicked on a control
            var clickedControl = FindControlAtPoint(e.Location);
            if (clickedControl != null)
            {
                SelectControl(clickedControl);
                _dragStart = e.Location;
                _moveStartLocation = clickedControl.Control.Location;
            }
            else
            {
                ClearSelection();
            }
        }
    }

    private void DesignSurface_MouseMove(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        // Update cursor based on handle hover
        if (_selectedControl != null && !_isResizing && !_isDragging)
        {
            var handle = GetHandleAtPoint(e.Location, GetAbsoluteBounds(_selectedControl.Control));
            this.Cursor = GetCursorForHandle(handle);
        }

        if (_selectedControl != null && e.Button == System.Windows.Forms.MouseButtons.Left)
        {
            // Handle resizing
            if (_isResizing)
            {
                PerformResize(e.Location);
                return;
            }

            // Handle dragging
            if (!_isDragging && Math.Abs(e.X - _dragStart.X) + Math.Abs(e.Y - _dragStart.Y) > 3)
            {
                _isDragging = true;
            }

            if (_isDragging)
            {
                var deltaX = e.X - _dragStart.X;
                var deltaY = e.Y - _dragStart.Y;
                
                var control = _selectedControl.Control;
                
                // Yeni absolute pozisyonu hesapla (henüz uygulamadan)
                var currentAbsoluteLocation = GetAbsoluteLocation(control);
                var newAbsoluteX = currentAbsoluteLocation.X + deltaX;
                var newAbsoluteY = currentAbsoluteLocation.Y + deltaY;
                
                // Mouse noktasını kullanarak potansiyel parent'ı bul
                var potentialParent = FindContainerAtPoint(e.Location, control);
                
                // Yeni location'u hesapla (parent'a göre relative)
                Point newLocation;
                if (potentialParent == _canvas)
                {
                    // Ana surface'e göre absolute
                    newLocation = new Point(newAbsoluteX, newAbsoluteY);
                }
                else
                {
                    // Parent control'e göre relative
                    var parentAbsoluteLocation = GetAbsoluteLocation(potentialParent);
                    newLocation = new Point(
                        newAbsoluteX - parentAbsoluteLocation.X,
                        newAbsoluteY - parentAbsoluteLocation.Y
                    );
                }
                
                // Calculate snap lines and get snapped position
                if (_showSnapLines && _designGuides != null)
                {
                    var otherBounds = GetOtherControlBounds(control);
                    var draggedBounds = new Rectangle(newAbsoluteX, newAbsoluteY, control.Width, control.Height);
                    
                    _designGuides.ShowSnapLines(draggedBounds, otherBounds);
                    
                    // Apply snap to absolute location
                    var snappedAbsoluteLocation = _designGuides.SnapToGuides(draggedBounds.Location, control.Size);
                    
                    // Convert back to relative location
                    if (potentialParent == _canvas)
                    {
                        newLocation = snappedAbsoluteLocation;
                    }
                    else
                    {
                        var parentAbsoluteLocation = GetAbsoluteLocation(potentialParent);
                        newLocation = new Point(
                            snappedAbsoluteLocation.X - parentAbsoluteLocation.X,
                            snappedAbsoluteLocation.Y - parentAbsoluteLocation.Y
                        );
                    }
                }
                
                // Eğer parent değiştiyse, kontrolü yeni parent'a taşı
                var currentParent = control.Parent as UIElementBase;
                if (potentialParent != currentParent)
                {
                    // Eski parent'tan çıkar
                    if (currentParent != null)
                    {
                        currentParent.Controls.Remove(control);
                    }
                    else
                    {
                        Controls.Remove(control);
                    }
                    
                    // Yeni parent'a ekle
                    if (potentialParent == _canvas)
                    {
                        _canvas.Controls.Add(control);
                    }
                    else
                    {
                        potentialParent.Controls.Add(control);
                    }
                }
                
                // Location'ı güncelle
                _selectedControl.Control.Location = newLocation;
                _selectedControl.Location = newLocation;
                UpdateCanvasSize();
                
                // Container highlight güncelle
                if (potentialParent != _highlightedContainer)
                {
                    _highlightedContainer = potentialParent;
                    
                    if (_designGuides != null)
                    {
                        if (_highlightedContainer != null && _highlightedContainer != this)
                        {
                            var containerAbsoluteLocation = GetAbsoluteLocation(_highlightedContainer);
                            var containerBounds = new Rectangle(containerAbsoluteLocation, _highlightedContainer.Size);
                            _designGuides.ShowContainerHighlight(containerBounds);
                        }
                        else
                        {
                            _designGuides.ClearContainerHighlight();
                        }
                    }
                }
                
                UpdateSelectionOverlay();
                
                _dragStart = e.Location;
                _selectedControl.Control.Invalidate();
            }
        }
    }

    private void DesignSurface_MouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (_isResizing && _selectedControl != null)
        {
            // Apply grid snap on resize complete
            if (_gridSnappingEnabled)
            {
                var control = _selectedControl.Control;
                var snappedLocation = SnapToGrid(control.Location);
                var snappedSize = SnapToGrid(control.Size);
                control.Location = snappedLocation;
                control.Size = snappedSize;
                UpdateSelectionOverlay();
            }
            _isResizing = false;
            _activeHandle = ResizeHandle.None;
            this.Cursor = System.Windows.Forms.Cursors.Default;
            UpdateCanvasSize();
        }
        else if (_isDragging && _selectedControl != null)
        {
            var control = _selectedControl.Control;
            
            // Apply grid snap on mouse up for final position
            if (_gridSnappingEnabled)
            {
                var snappedLocation = SnapToGrid(control.Location);
                if (snappedLocation != control.Location)
                {
                    control.Location = snappedLocation;
                    UpdateSelectionOverlay();
                }
            }
            
            // Create undo command if position changed
            if (_moveStartLocation != control.Location)
            {
                var moveCommand = new MoveControlCommand(_selectedControl, _moveStartLocation, control.Location);
                _undoRedoManager.ExecuteCommand(moveCommand);
            }
            
            _isDragging = false;
            
            // Guides ve highlight'ı temizle
            _designGuides?.ClearSnapLines();
            _designGuides?.ClearContainerHighlight();
            _highlightedContainer = null;
            UpdateCanvasSize();
        }
        
        this.Cursor = System.Windows.Forms.Cursors.Default;
    }

    private ResizeHandle GetHandleAtPoint(Point point, Rectangle bounds)
    {
        int half = HandleSize / 2;
        
        // Check corners first (higher priority)
        if (IsPointNear(point, new Point(bounds.Left, bounds.Top), half))
            return ResizeHandle.TopLeft;
        if (IsPointNear(point, new Point(bounds.Right, bounds.Top), half))
            return ResizeHandle.TopRight;
        if (IsPointNear(point, new Point(bounds.Right, bounds.Bottom), half))
            return ResizeHandle.BottomRight;
        if (IsPointNear(point, new Point(bounds.Left, bounds.Bottom), half))
            return ResizeHandle.BottomLeft;
        
        // Check edges
        if (IsPointNear(point, new Point(bounds.Left + bounds.Width / 2, bounds.Top), half))
            return ResizeHandle.Top;
        if (IsPointNear(point, new Point(bounds.Right, bounds.Top + bounds.Height / 2), half))
            return ResizeHandle.Right;
        if (IsPointNear(point, new Point(bounds.Left + bounds.Width / 2, bounds.Bottom), half))
            return ResizeHandle.Bottom;
        if (IsPointNear(point, new Point(bounds.Left, bounds.Top + bounds.Height / 2), half))
            return ResizeHandle.Left;
        
        return ResizeHandle.None;
    }

    private bool IsPointNear(Point p1, Point p2, int tolerance)
    {
        return Math.Abs(p1.X - p2.X) <= tolerance && Math.Abs(p1.Y - p2.Y) <= tolerance;
    }

    private System.Windows.Forms.Cursor GetCursorForHandle(ResizeHandle handle)
    {
        return handle switch
        {
            ResizeHandle.TopLeft or ResizeHandle.BottomRight => System.Windows.Forms.Cursors.SizeNWSE,
            ResizeHandle.TopRight or ResizeHandle.BottomLeft => System.Windows.Forms.Cursors.SizeNESW,
            ResizeHandle.Top or ResizeHandle.Bottom => System.Windows.Forms.Cursors.SizeNS,
            ResizeHandle.Left or ResizeHandle.Right => System.Windows.Forms.Cursors.SizeWE,
            _ => System.Windows.Forms.Cursors.Default
        };
    }

    private void PerformResize(Point currentPoint)
    {
        if (_selectedControl == null) return;

        var control = _selectedControl.Control;
        int deltaX = currentPoint.X - _dragStart.X;
        int deltaY = currentPoint.Y - _dragStart.Y;

        var newBounds = _resizeStartBounds;

        switch (_activeHandle)
        {
            case ResizeHandle.TopLeft:
                newBounds.X += deltaX;
                newBounds.Y += deltaY;
                newBounds.Width -= deltaX;
                newBounds.Height -= deltaY;
                break;
            case ResizeHandle.Top:
                newBounds.Y += deltaY;
                newBounds.Height -= deltaY;
                break;
            case ResizeHandle.TopRight:
                newBounds.Y += deltaY;
                newBounds.Width += deltaX;
                newBounds.Height -= deltaY;
                break;
            case ResizeHandle.Right:
                newBounds.Width += deltaX;
                break;
            case ResizeHandle.BottomRight:
                newBounds.Width += deltaX;
                newBounds.Height += deltaY;
                break;
            case ResizeHandle.Bottom:
                newBounds.Height += deltaY;
                break;
            case ResizeHandle.BottomLeft:
                newBounds.X += deltaX;
                newBounds.Width -= deltaX;
                newBounds.Height += deltaY;
                break;
            case ResizeHandle.Left:
                newBounds.X += deltaX;
                newBounds.Width -= deltaX;
                break;
        }

        // Enforce minimum size
        const int minSize = 20;
        if (newBounds.Width < minSize) newBounds.Width = minSize;
        if (newBounds.Height < minSize) newBounds.Height = minSize;

        control.Bounds = newBounds;
        _selectedControl.Bounds = newBounds;
        
        UpdateSelectionOverlay();
        control.Invalidate();
    }

    private Point SnapToGrid(Point location)
    {
        return new Point(
            (location.X / _gridSize) * _gridSize,
            (location.Y / _gridSize) * _gridSize
        );
    }

    private Size SnapToGrid(Size size)
    {
        return new Size(
            Math.Max(_gridSize, (size.Width / _gridSize) * _gridSize),
            Math.Max(_gridSize, (size.Height / _gridSize) * _gridSize)
        );
    }

    private DesignControl? FindControlAtPoint(Point point)
    {
        // En üstteki kontrolden başla (Z-order)
        for (int i = _designControls.Count - 1; i >= 0; i--)
        {
            var designControl = _designControls[i];
            var absoluteLocation = GetAbsoluteLocation(designControl.Control);
            var absoluteBounds = new Rectangle(
                absoluteLocation,
                designControl.Control.Size
            );
            
            if (absoluteBounds.Contains(point))
            {
                return designControl;
            }
        }
        return null;
    }

    private UIElementBase? FindContainerAtPoint(Point point, UIElementBase excludeControl)
    {
        UIElementBase? deepestContainer = null;
        int deepestLevel = -1;
        
        // Tüm kontrolleri kontrol et, en derin (nested) container'ı bul
        for (int i = _designControls.Count - 1; i >= 0; i--)
        {
            var designControl = _designControls[i];
            var control = designControl.Control;
            
            // Kendisini, overlay kontrolleri ve parent'ını atla
            if (control == excludeControl || control == _canvas || control is SelectionOverlay || control is DesignGuides)
                continue;
            
            // Exclude control'ün child'ıysa atla (sonsuz döngü önlemek için)
            if (IsParentOf(control, excludeControl))
                continue;
            
            // Kontrolün absolute koordinatlarını al
            var absoluteLocation = GetAbsoluteLocation(control);
            var absoluteBounds = new Rectangle(absoluteLocation, control.Size);
            
            if (absoluteBounds.Contains(point))
            {
                // Bu kontrolün ne kadar derin olduğunu hesapla
                var level = GetNestingLevel(control);
                if (level > deepestLevel)
                {
                    deepestLevel = level;
                    deepestContainer = control;
                }
            }
        }
        
        // Hiçbir container bulunamadı, ana surface döndür
        return deepestContainer ?? _canvas;
    }

    private bool IsParentOf(UIElementBase child, UIElementBase potentialParent)
    {
        var parent = child.Parent as UIElementBase;
        while (parent != null && parent != this)
        {
            if (parent == potentialParent)
                return true;
            parent = parent.Parent as UIElementBase;
        }
        return false;
    }

    private int GetNestingLevel(UIElementBase control)
    {
        int level = 0;
        var parent = control.Parent as UIElementBase;
        while (parent != null && parent != this)
        {
            level++;
            parent = parent.Parent as UIElementBase;
        }
        return level;
    }

    private Point GetAbsoluteLocation(UIElementBase control)
    {
        var location = control.Location;
        var parent = control.Parent as UIElementBase;
        
        while (parent != null && parent != this)
        {
            location.X += parent.Location.X;
            location.Y += parent.Location.Y;
            parent = parent.Parent as UIElementBase;
        }
        
        return location;
    }

    private List<Rectangle> GetOtherControlBounds(UIElementBase excludeControl)
    {
        var bounds = new List<Rectangle>();
        
        foreach (var designControl in _designControls)
        {
            if (designControl.Control != excludeControl && designControl.Control.Visible)
            {
                var absoluteLocation = GetAbsoluteLocation(designControl.Control);
                bounds.Add(new Rectangle(absoluteLocation, designControl.Control.Size));
            }
        }
        
        return bounds;
    }

    private void SelectControl(DesignControl control)
    {
        _selectedControl = control;
        UpdateSelectionOverlay();
        ControlSelected?.Invoke(this, control);
    }

    public void ClearSelection()
    {
        _selectedControl = null;
        _selectionOverlay?.Clear();
        SelectionCleared?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateSelectionOverlay()
    {
        if (_selectedControl != null && _selectionOverlay != null)
        {
            var absoluteLocation = GetAbsoluteLocation(_selectedControl.Control);
            _selectionOverlay.ShowSelection(new Rectangle(absoluteLocation, _selectedControl.Control.Size));
            Invalidate();
        }
    }

    public void AddDesignControl(string controlType, Point location)
    {
        var command = new AddControlCommand(this, controlType, location);
        _undoRedoManager.ExecuteCommand(command);
    }
    
    internal void AddDesignControlInternal(string controlType, Point location)
    {
        var control = CreateControl(controlType);
        if (control != null)
        {
            control.Location = location;
            control.Name = $"{controlType.Replace("SDUI.", "").Replace("Controls.", "").Replace("Layouts.", "")}_{_designControls.Count + 1}";
            _canvas.Controls.Add(control);
            AttachControlHandlers(control);
            
            var designControl = new DesignControl(control);
            _designControls.Add(designControl);
            
            SelectControl(designControl);
            ControlsChanged?.Invoke(this, EventArgs.Empty);
            UpdateCanvasSize();
        }
    }

    private new UIElementBase? CreateControl(string controlType)
    {
        return controlType switch
        {
            "Button" => new SDUI.Controls.Button { Text = "Button", Size = new Size(100, 30) },
            "Label" => new SDUI.Controls.Label { Text = "Label", Size = new Size(100, 20) },
            "TextBox" => new SDUI.Controls.TextBox { Text = "", Size = new Size(150, 25) },
            "Panel" => new SDUI.Controls.Panel { Size = new Size(200, 150), BackColor = ColorScheme.BackColor.Brightness(0.1f) },
            "CheckBox" => new SDUI.Controls.CheckBox { Text = "CheckBox", Size = new Size(100, 20) },
            "Radio" => new SDUI.Controls.Radio { Text = "Radio", Size = new Size(100, 20) },
            "ComboBox" => new SDUI.Controls.ComboBox { Size = new Size(150, 25) },
            "GroupBox" => new SDUI.Controls.GroupBox { Text = "GroupBox", Size = new Size(200, 150) },
            "TabControl" => CreateDesignTimeTabControl(),
            "ListView" => new SDUI.Controls.ListView { Size = new Size(300, 200) },
            "TreeView" => new SDUI.Controls.TreeView { Size = new Size(200, 300) },
            "ListBox" => new SDUI.Controls.ListBox { Size = new Size(150, 200) },
            "NumUpDown" => new SDUI.Controls.NumUpDown { Size = new Size(120, 25) },
            "TrackBar" => new SDUI.Controls.TrackBar { Size = new Size(200, 45) },
            "ToggleButton" => new SDUI.Controls.ToggleButton { Text = "Toggle", Size = new Size(100, 30) },
            "ProgressBar" => new SDUI.Controls.ProgressBar { Size = new Size(200, 25) },
            "ShapeProgressBar" => new SDUI.Controls.ShapeProgressBar { Size = new Size(100, 100) },
            "ScrollBar" => new SDUI.Controls.ScrollBar { Size = new Size(20, 200) },
            "PictureBox" => new SDUI.Controls.PictureBox { Size = new Size(150, 150) },
            "ChatBubble" => new SDUI.Controls.ChatBubble { Text = "Message", Size = new Size(200, 60) },
            "FlowLayoutPanel" => new SDUI.Controls.FlowLayoutPanel { Size = new Size(250, 200), BackColor = ColorScheme.BackColor.Brightness(0.1f) },
            "SplitContainer" => new SDUI.Controls.SplitContainer { Size = new Size(400, 300) },
            "PropertyGrid" => new SDUI.Controls.PropertyGrid { Size = new Size(300, 400) },
            "MenuStrip" => new SDUI.Controls.MenuStrip { Height = 40, Dock = System.Windows.Forms.DockStyle.Top },
            "StatusStrip" => new SDUI.Controls.StatusStrip { Height = 30, Dock = System.Windows.Forms.DockStyle.Bottom },
            "ContextMenuStrip" => new SDUI.Controls.ContextMenuStrip(),
            _ => null
        };
    }

    private SDUI.Controls.TabControl CreateDesignTimeTabControl()
    {
        var tabControl = new SDUI.Controls.TabControl
        {
            Size = new Size(300, 200),
            RenderNewPageButton = true,
            RenderPageClose = true,
            RenderPageIcon = false
        };

        tabControl.NewPageButtonClicked += (_, __) => AddDesignTimeTabPage(tabControl);

        if (GetTabPageCount(tabControl) == 0)
        {
            AddDesignTimeTabPage(tabControl);
            AddDesignTimeTabPage(tabControl);
        }

        return tabControl;
    }

    private void AddDesignTimeTabPage(SDUI.Controls.TabControl tabControl)
    {
        var index = GetTabPageCount(tabControl) + 1;
        var page = new SDUI.Controls.TabPage
        {
            Text = $"Tab {index}",
            Name = $"TabPage{index}"
        };
        tabControl.AddPage(page);
    }

    private int GetTabPageCount(SDUI.Controls.TabControl tabControl)
    {
        var count = 0;
        for (int i = 0; i < tabControl.Controls.Count; i++)
        {
            if (tabControl.Controls[i] is SDUI.Controls.TabPage)
                count++;
        }
        return count;
    }

    public void DeleteSelectedControl()
    {
        if (_selectedControl != null)
        {
            var index = _designControls.IndexOf(_selectedControl);
            var command = new DeleteControlCommand(this, _selectedControl, index);
            _undoRedoManager.ExecuteCommand(command);
        }
    }
    
    internal void RemoveDesignControl(DesignControl control)
    {
        if (control.Control.Parent is UIElementBase parent)
            parent.Controls.Remove(control.Control);
        DetachControlHandlers(control.Control);
        _designControls.Remove(control);
        ClearSelection();
        ControlsChanged?.Invoke(this, EventArgs.Empty);
        UpdateCanvasSize();
    }
    
    internal void InsertDesignControl(DesignControl control, int index)
    {
        _canvas.Controls.Add(control.Control);
        AttachControlHandlers(control.Control);
        if (index >= 0 && index < _designControls.Count)
            _designControls.Insert(index, control);
        else
            _designControls.Add(control);
        SelectControl(control);
        ControlsChanged?.Invoke(this, EventArgs.Empty);
        UpdateCanvasSize();
    }

    public void ClearAll()
    {
        foreach (var designControl in _designControls)
            DetachControlHandlers(designControl.Control);
        _canvas.Controls.Clear();
        _designControls.Clear();
        ClearSelection();
        ControlsChanged?.Invoke(this, EventArgs.Empty);
        UpdateCanvasSize();
    }

    public void DeleteSelected()
    {
        DeleteSelectedControl();
    }

    public DesignControl? SelectedControl => _selectedControl;

    public IReadOnlyList<DesignControl> DesignControls => _designControls;

    public void ToggleGridSnapping()
    {
        _gridSnappingEnabled = !_gridSnappingEnabled;
    }

    public bool GridSnappingEnabled
    {
        get => _gridSnappingEnabled;
        set => _gridSnappingEnabled = value;
    }

    public bool ShowGridLines
    {
        get => _showGridLines;
        set
        {
            if (_showGridLines != value)
            {
                _showGridLines = value;
                Invalidate();
            }
        }
    }
    
    public int GridSize
    {
        get => _gridSize;
        set
        {
            if (_gridSize != value && value > 0)
            {
                _gridSize = Math.Max(5, Math.Min(50, value));
                Invalidate();
            }
        }
    }

    public bool ShowSnapLines
    {
        get => _showSnapLines;
        set => _showSnapLines = value;
    }

    public override void OnPaint(SkiaSharp.SKCanvas canvas)
    {
        base.OnPaint(canvas);

        // Draw grid lines if enabled
        if (_showGridLines)
        {
            DrawGridLines(canvas);
        }
    }

    private void DrawGridLines(SkiaSharp.SKCanvas canvas)
    {
        using var paint = new SkiaSharp.SKPaint
        {
            Color = ColorScheme.ForeColor.Alpha(20).ToSKColor(),
            StrokeWidth = 1,
            Style = SkiaSharp.SKPaintStyle.Stroke,
            IsAntialias = true
        };

        // Draw vertical lines
        for (int x = _gridSize; x < Width; x += _gridSize)
        {
            canvas.DrawLine(x, 0, x, Height, paint);
        }

        // Draw horizontal lines
        for (int y = _gridSize; y < Height; y += _gridSize)
        {
            canvas.DrawLine(0, y, Width, y, paint);
        }
    }

    public void LoadDesign(List<DesignControl> controls)
    {
        SuspendLayout();
        try
        {
            _canvas.Controls.Clear();
            _designControls.Clear();
            
            foreach (var designControl in controls)
            {
                var control = CreateControl(designControl.ControlType);
                if (control != null)
                {
                    control.Name = designControl.Name;
                    control.Bounds = new Rectangle(designControl.Location, designControl.Size);
                    control.Text = designControl.Text;

                    _canvas.Controls.Add(control);
                    AttachControlHandlers(control);
                    _designControls.Add(new DesignControl(control));
                }
            }
            
            ControlsChanged?.Invoke(this, EventArgs.Empty);
            UpdateCanvasSize();
        }
        finally
        {
            ResumeLayout();
        }
    }

    private void UpdateCanvasSize()
    {
        if (_canvas == null)
            return;

        var maxRight = Width;
        var maxBottom = Height;

        foreach (var designControl in _designControls)
        {
            var control = designControl.Control;
            if (!control.Visible)
                continue;

            var bounds = GetCanvasRelativeBounds(control);
            maxRight = Math.Max(maxRight, bounds.Right);
            maxBottom = Math.Max(maxBottom, bounds.Bottom);
        }

        if (maxRight > Width)
            maxRight += AutoScrollMargin.Width;
        if (maxBottom > Height)
            maxBottom += AutoScrollMargin.Height;

        var newSize = new Size(Math.Max(Width, maxRight), Math.Max(Height, maxBottom));
        if (_canvas.Size != newSize)
            _canvas.Size = newSize;

        UpdateSurfaceScrollBars();
        UpdateCanvasScroll();
    }

    private void UpdateCanvasScroll()
    {
        var x = (_hScrollBar != null && _hScrollBar.Visible) ? _hScrollBar.Value : 0;
        var y = (_vScrollBar != null && _vScrollBar.Visible) ? _vScrollBar.Value : 0;
        var newLocation = new Point(-x, -y);
        if (_canvas.Location != newLocation)
            _canvas.Location = newLocation;

        if (_selectedControl != null)
            UpdateSelectionOverlay();

        Invalidate();
    }

    private void UpdateSurfaceScrollBars()
    {
        if (!AutoScroll || _vScrollBar == null || _hScrollBar == null)
            return;

        var contentWidth = _canvas.Width;
        var contentHeight = _canvas.Height;
        var clientWidth = Width;
        var clientHeight = Height;

        var needsHScroll = contentWidth > clientWidth;
        var needsVScroll = contentHeight > clientHeight;

        _hScrollBar.Visible = needsHScroll;
        _vScrollBar.Visible = needsVScroll;

        if (needsHScroll)
        {
            _hScrollBar.Maximum = Math.Max(0, contentWidth - clientWidth);
            _hScrollBar.LargeChange = Math.Max(1, clientWidth / 2);
            if (_hScrollBar.Value > _hScrollBar.Maximum)
                _hScrollBar.Value = _hScrollBar.Maximum;
        }

        if (needsVScroll)
        {
            _vScrollBar.Maximum = Math.Max(0, contentHeight - clientHeight);
            _vScrollBar.LargeChange = Math.Max(1, clientHeight / 2);
            if (_vScrollBar.Value > _vScrollBar.Maximum)
                _vScrollBar.Value = _vScrollBar.Maximum;
        }
    }

    private Rectangle GetAbsoluteBounds(UIElementBase control)
    {
        var absoluteLocation = GetAbsoluteLocation(control);
        return new Rectangle(absoluteLocation, control.Size);
    }

    private Rectangle GetCanvasRelativeBounds(UIElementBase control)
    {
        var location = GetCanvasRelativeLocation(control);
        return new Rectangle(location, control.Size);
    }

    private Point GetCanvasRelativeLocation(UIElementBase control)
    {
        var location = control.Location;
        var parent = control.Parent as UIElementBase;

        while (parent != null && parent != _canvas && parent != this)
        {
            location.X += parent.Location.X;
            location.Y += parent.Location.Y;
            parent = parent.Parent as UIElementBase;
        }

        return location;
    }

    private void HandleMouseWheel(int delta)
    {
        if (!AutoScroll || _vScrollBar == null)
            return;

        if (_vScrollBar.Visible)
        {
            var scrollLines = System.Windows.Forms.SystemInformation.MouseWheelScrollLines;
            var step = Math.Max(1, _vScrollBar.SmallChange);
            var deltaValue = (delta / 120) * scrollLines * step;
            var newValue = Math.Clamp(_vScrollBar.Value - deltaValue, _vScrollBar.Minimum, _vScrollBar.Maximum);
            _vScrollBar.Value = newValue;
        }
        else if (_hScrollBar != null && _hScrollBar.Visible)
        {
            var step = Math.Max(1, _hScrollBar.SmallChange);
            var deltaValue = (delta / 120) * step;
            var newValue = Math.Clamp(_hScrollBar.Value - deltaValue, _hScrollBar.Minimum, _hScrollBar.Maximum);
            _hScrollBar.Value = newValue;
        }
    }

    private void Control_MouseWheel(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        HandleMouseWheel(e.Delta);
    }

    private void Control_ControlAdded(object? sender, UIElementEventArgs e)
    {
        if (e.Element is UIElementBase child)
            AttachControlHandlers(child);
        UpdateCanvasSize();
    }

    private void Control_ControlRemoved(object? sender, UIElementEventArgs e)
    {
        if (e.Element is UIElementBase child)
            DetachControlHandlers(child);
        UpdateCanvasSize();
    }

    private void AttachControlHandlers(UIElementBase control)
    {
        control.MouseWheel += Control_MouseWheel;
        control.ControlAdded += Control_ControlAdded;
        control.ControlRemoved += Control_ControlRemoved;

        for (int i = 0; i < control.Controls.Count; i++)
        {
            if (control.Controls[i] is UIElementBase child)
                AttachControlHandlers(child);
        }
    }

    private void DetachControlHandlers(UIElementBase control)
    {
        control.MouseWheel -= Control_MouseWheel;
        control.ControlAdded -= Control_ControlAdded;
        control.ControlRemoved -= Control_ControlRemoved;

        for (int i = 0; i < control.Controls.Count; i++)
        {
            if (control.Controls[i] is UIElementBase child)
                DetachControlHandlers(child);
        }
    }
}
