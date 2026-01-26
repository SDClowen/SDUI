using System;
using System.Collections.Generic;
using System.Drawing;
using SDUI.Controls;
using SDUI.Extensions;
using SDUI.Designer.ViewModels;
using System.Linq;

namespace SDUI.Designer;

/// <summary>
/// Main designer window using MVVM pattern
/// </summary>
public partial class DesignerMainWindow : UIWindow
{
    private readonly DesignerViewModel _viewModel;
    
    private SDUI.Controls.SplitContainer _mainSplitContainer = null!;
    private DesignSurface _designSurface = null!;
    private SDUI.Controls.TextBox _codeOutput = null!;
    private SDUI.Controls.MenuStrip _mainMenu = null!;
    private SDUI.Controls.StatusStrip _statusStrip = null!;
    private Toolbox _toolbox = null!;
    private PropertiesPanel _propertiesPanel = null!;
    private MenuItem? _statusLabel;

    public DesignerMainWindow()
    {
        Console.WriteLine("DesignerMainWindow Constructor");
        _viewModel = new DesignerViewModel();
        Console.WriteLine("ViewModel created");
        InitializeComponent();
        Console.WriteLine("InitializeComponent done");
        SetupUI();
        Console.WriteLine("SetupUI done");
        BindViewModel();
        Console.WriteLine("BindViewModel done");
        
        // Handle Load event to ensure controls are properly laid out
        this.Load += DesignerMainWindow_Load;
        this.KeyDown += DesignerMainWindow_KeyDown;
    }

    private void DesignerMainWindow_KeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
    {
        // Undo/Redo
        if (e.Control && e.KeyCode == System.Windows.Forms.Keys.Z)
        {
            _designSurface.UndoRedoManager.Undo();
            UpdateCodeOutput();
            e.Handled = true;
        }
        else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.Y)
        {
            _designSurface.UndoRedoManager.Redo();
            UpdateCodeOutput();
            e.Handled = true;
        }
            else if (e.Control && (e.KeyCode == System.Windows.Forms.Keys.Add || e.KeyCode == System.Windows.Forms.Keys.Oemplus))
            {
                _designSurface.ZoomIn();
                e.Handled = true;
            }
            else if (e.Control && (e.KeyCode == System.Windows.Forms.Keys.Subtract || e.KeyCode == System.Windows.Forms.Keys.OemMinus))
            {
                _designSurface.ZoomOut();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.D0)
            {
                _designSurface.ResetZoom();
                e.Handled = true;
            }
        // Copy/Paste/Cut
        else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.C)
        {
            if (_designSurface.CopySelected())
                e.Handled = true;
        }
        else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.V)
        {
            if (_designSurface.PasteClipboard())
            {
                UpdateCodeOutput();
                e.Handled = true;
            }
        }
        else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.X)
        {
            if (_designSurface.CopySelected())
            {
                DeleteSelectedControl();
                e.Handled = true;
            }
        }
        // Delete
        else if (e.KeyCode == System.Windows.Forms.Keys.Delete)
        {
            DeleteSelectedControl();
            e.Handled = true;
        }
        // Escape
        else if (e.KeyCode == System.Windows.Forms.Keys.Escape)
        {
            _designSurface.ClearSelection();
            _designSurface.PendingControlType = null;
            _viewModel.PendingControlType = null;
            e.Handled = true;
        }
    }
    
    private void DesignerMainWindow_Load(object? sender, EventArgs e)
    {
        Console.WriteLine("=== Load Event ===");
        Console.WriteLine($"Window Size: {Size}");
        Console.WriteLine($"ClientSize: {ClientSize}");
        Console.WriteLine($"Menu Bounds: {_mainMenu.Bounds}");
        Console.WriteLine($"Toolbox Bounds: {_toolbox.Bounds}");
        Console.WriteLine($"Properties Bounds: {_propertiesPanel.Bounds}");
        Console.WriteLine($"SplitContainer Bounds: {_mainSplitContainer.Bounds}");
        Console.WriteLine($"StatusStrip Bounds: {_statusStrip.Bounds}");
        
        // Force all controls to be visible
        _mainMenu.Visible = true;
        _mainMenu.BringToFront();
        _toolbox.Visible = true;
        _toolbox.BringToFront();
        _propertiesPanel.Visible = true;
        _propertiesPanel.BringToFront();
        _statusStrip.Visible = true;
        _statusStrip.BringToFront();
        
        PerformLayout();
        Refresh();
    }

    public DesignerViewModel ViewModel => _viewModel;

    private void InitializeComponent()
    {
        Text = "SDUI Designer";
        Size = new Size(1600, 900);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        MinimumSize = new Size(1200, 700);
        BackColor = ColorScheme.BackColor;
    }

    private void SetupUI()
    {
        // Create all controls first
        CreateMenuStrip();
        CreateToolbox();
        CreatePropertiesPanel();
        CreateMainContent();
        CreateStatusStrip();

        // Add controls in correct order (Dock.Fill must be added last)
        // Z-order: Last added = on top
        Controls.Add(_mainSplitContainer); // Dock.Fill - Add FIRST (bottom layer)
        Controls.Add(_toolbox);         // Dock.Left
        Controls.Add(_propertiesPanel); // Dock.Right
        Controls.Add(_mainMenu);        // Dock.Top
        Controls.Add(_statusStrip);     // Dock.Bottom

        // Force layout update
        PerformLayout();

        // Debug output
        Console.WriteLine($"Menu: Visible={_mainMenu.Visible}, Height={_mainMenu.Height}, Dock={_mainMenu.Dock}");
        Console.WriteLine($"Toolbox: Visible={_toolbox.Visible}, Width={_toolbox.Width}, Dock={_toolbox.Dock}");
        Console.WriteLine($"Properties: Visible={_propertiesPanel.Visible}, Width={_propertiesPanel.Width}, Dock={_propertiesPanel.Dock}");
        Console.WriteLine($"StatusStrip: Visible={_statusStrip.Visible}, Height={_statusStrip.Height}, Dock={_statusStrip.Dock}");
        Console.WriteLine($"SplitContainer: Visible={_mainSplitContainer.Visible}, Dock={_mainSplitContainer.Dock}");
        Console.WriteLine($"Total Controls: {Controls.Count}");
    }

    private void CreateMenuStrip()
    {
        _mainMenu = new SDUI.Controls.MenuStrip
        {
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 40,
            Visible = true
        };

        var fileMenu = new MenuItem("File");
        
        var newItem = new MenuItem("New");
        newItem.Click += (s, e) => ClearDesigner();
        fileMenu.AddDropDownItem(newItem);
        
        var saveItem = new MenuItem("Save");
        saveItem.Click += (s, e) => SaveDesign();
        fileMenu.AddDropDownItem(saveItem);
        
        var loadItem = new MenuItem("Load");
        loadItem.Click += (s, e) => LoadDesign();
        fileMenu.AddDropDownItem(loadItem);
        
        fileMenu.AddDropDownItem(new MenuItemSeparator());
        
        var exitItem = new MenuItem("Exit");
        exitItem.Click += (s, e) => Close();
        fileMenu.AddDropDownItem(exitItem);

        var editMenu = new MenuItem("Edit");
        
        var undoItem = new MenuItem("Undo");
        undoItem.Click += (s, e) =>
        {
            _designSurface.UndoRedoManager.Undo();
            UpdateCodeOutput();
        };
        editMenu.AddDropDownItem(undoItem);
        
        var redoItem = new MenuItem("Redo");
        redoItem.Click += (s, e) =>
        {
            _designSurface.UndoRedoManager.Redo();
            UpdateCodeOutput();
        };
        editMenu.AddDropDownItem(redoItem);
        
        editMenu.AddDropDownItem(new MenuItemSeparator());
        
        var deleteItem = new MenuItem("Delete");
        deleteItem.Click += (s, e) => DeleteSelectedControl();
        editMenu.AddDropDownItem(deleteItem);

        var viewMenu = new MenuItem("View");
        
        var gridSnapItem = new MenuItem("Grid Snapping (On)")
        {
            Tag = true // Store current state
        };
        gridSnapItem.Click += (s, e) =>
        {
            var isEnabled = (bool)(gridSnapItem.Tag ?? true);
            isEnabled = !isEnabled;
            gridSnapItem.Tag = isEnabled;
            gridSnapItem.Text = isEnabled ? "Grid Snapping (On)" : "Grid Snapping (Off)";
            _designSurface.GridSnappingEnabled = isEnabled;
        };
        viewMenu.AddDropDownItem(gridSnapItem);
        
        var gridLinesItem = new MenuItem("Show Grid Lines (On)")
        {
            Tag = true // Store current state
        };
        gridLinesItem.Click += (s, e) =>
        {
            var isEnabled = (bool)(gridLinesItem.Tag ?? true);
            isEnabled = !isEnabled;
            gridLinesItem.Tag = isEnabled;
            gridLinesItem.Text = isEnabled ? "Show Grid Lines (On)" : "Show Grid Lines (Off)";
            _designSurface.ShowGridLines = isEnabled;
        };
        viewMenu.AddDropDownItem(gridLinesItem);
        
        var snapLinesItem = new MenuItem("Show Snap Lines (On)")
        {
            Tag = true // Store current state
        };
        snapLinesItem.Click += (s, e) =>
        {
            var isEnabled = (bool)(snapLinesItem.Tag ?? true);
            isEnabled = !isEnabled;
            snapLinesItem.Tag = isEnabled;
            snapLinesItem.Text = isEnabled ? "Show Snap Lines (On)" : "Show Snap Lines (Off)";
            _designSurface.ShowSnapLines = isEnabled;
        };
        viewMenu.AddDropDownItem(snapLinesItem);
        
        viewMenu.AddDropDownItem(new MenuItemSeparator());
        
        // Grid size submenu
        var gridSizeMenu = new MenuItem("Grid Size");
        var gridSizes = new[] { 5, 10, 15, 20, 25 };
        foreach (var size in gridSizes)
        {
            var sizeItem = new MenuItem($"{size}px");
            sizeItem.Click += (s, e) =>
            {
                _designSurface.GridSize = size;
            };
            gridSizeMenu.AddDropDownItem(sizeItem);
        }
        viewMenu.AddDropDownItem(gridSizeMenu);
        
        viewMenu.AddDropDownItem(new MenuItemSeparator());
        viewMenu.AddDropDownItem(new MenuItem("Zoom In"));
        viewMenu.AddDropDownItem(new MenuItem("Zoom Out"));
        viewMenu.AddDropDownItem(new MenuItem("Reset Zoom"));

        var toolsMenu = new MenuItem("Tools");
        
        var generateItem = new MenuItem("Generate Code");
        generateItem.Click += (s, e) => UpdateCodeOutput();
        toolsMenu.AddDropDownItem(generateItem);

        _mainMenu.AddItem(fileMenu);
        _mainMenu.AddItem(editMenu);
        _mainMenu.AddItem(viewMenu);
        _mainMenu.AddItem(toolsMenu);
    }

    private void ClearDesigner()
    {
        _designSurface.ClearAll();
        UpdateCodeOutput();
    }

    private void DeleteSelectedControl()
    {
        _designSurface.DeleteSelected();
        UpdateCodeOutput();
    }

    private void SaveDesign()
    {
        using var dialog = new System.Windows.Forms.SaveFileDialog
        {
            Filter = "SDUI Design Files (*.sdui)|*.sdui|All Files (*.*)|*.*",
            DefaultExt = "sdui",
            Title = "Save Design"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            try
            {
                DesignSerializer.SaveToFile(dialog.FileName, _designSurface.DesignControls);
                System.Windows.Forms.MessageBox.Show($"Design saved to {dialog.FileName}", "Success",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error saving design: {ex.Message}", "Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    private void LoadDesign()
    {
        using var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Filter = "SDUI Design Files (*.sdui)|*.sdui|All Files (*.*)|*.*",
            DefaultExt = "sdui",
            Title = "Load Design"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            try
            {
                var designData = DesignSerializer.LoadFromFile(dialog.FileName);
                var controls = designData.Select(d => new DesignControl 
                { 
                    ControlType = d.ControlType, 
                    Name = d.Name, 
                    Location = d.Location, 
                    Size = d.Size, 
                    Text = d.Name 
                }).ToList();
                _designSurface.LoadDesign(controls);
                UpdateCodeOutput();
                System.Windows.Forms.MessageBox.Show($"Design loaded from {dialog.FileName}", "Success",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading design: {ex.Message}", "Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    private void CreateToolbox()
    {
        _toolbox = new Toolbox
        {
            Visible = true
        };
        _toolbox.ControlRequested += OnControlRequested;
    }

    private void CreatePropertiesPanel()
    {
        _propertiesPanel = new PropertiesPanel
        {
            Dock = System.Windows.Forms.DockStyle.Right,
            Width = 300,
            Visible = true,
            BackColor = ColorScheme.OnSurfaceVariant,
            ShadowDepth = 0,
            Radius = 0,
            AutoScroll = true
        };
    }

    private void CreateMainContent()
    {
        // Main Split Container
        _mainSplitContainer = new SDUI.Controls.SplitContainer
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 800,
            SplitterWidth = 6
        };

        // Left Panel - Design Surface
        _designSurface = new DesignSurface
        {
            Dock = System.Windows.Forms.DockStyle.Fill
        };

        _designSurface.ControlSelected += OnDesignControlSelected;
        _designSurface.SelectionCleared += OnDesignSelectionCleared;
        _designSurface.ControlsChanged += (s, e) => UpdateCodeOutput();
        _designSurface.MouseClick += DesignSurface_MouseClick;
        
        // Undo/Redo status updates
        _designSurface.UndoRedoManager.StateChanged += (s, e) =>
        {
            if (_statusLabel != null)
            {
                var undoDesc = _designSurface.UndoRedoManager.UndoDescription;
                var redoDesc = _designSurface.UndoRedoManager.RedoDescription;
                var status = "Ready";
                if (undoDesc != null || redoDesc != null)
                {
                    var parts = new List<string>();
                    if (undoDesc != null) parts.Add($"Undo: {undoDesc}");
                    if (redoDesc != null) parts.Add($"Redo: {redoDesc}");
                    status = string.Join(" | ", parts);
                }
                _statusLabel.Text = status;
            }
        };

        var designLabel = new SDUI.Controls.Label
        {
            Text = "Design Surface\n\nClick a control in the Toolbox,\nthen click here to add it",
            Dock = System.Windows.Forms.DockStyle.Fill,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = ColorScheme.ForeColor.Alpha(26),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _designSurface.Controls.Add(designLabel);
        _mainSplitContainer.Panel1.Controls.Add(_designSurface);

        // Right Panel - Code Output
        var codePanel = new SDUI.Controls.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Padding = new System.Windows.Forms.Padding(10, 10, 10, 10),
            Radius = 0,
            ShadowDepth = 0,
            Border = new System.Windows.Forms.Padding(0)
        };

        var codeLabel = new SDUI.Controls.Label
        {
            Text = "Generated Code",
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _codeOutput = new SDUI.Controls.TextBox
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            MultiLine = true,
            Font = new Font("Consolas", 10),
            BackColor = ColorScheme.Surface,
            ForeColor = ColorScheme.ForeColor,
            ReadOnly = true
        };

        codePanel.Controls.Add(_codeOutput);
        codePanel.Controls.Add(codeLabel);
        _mainSplitContainer.Panel2.Controls.Add(codePanel);
    }

    private void CreateStatusStrip()
    {
        _statusStrip = new SDUI.Controls.StatusStrip
        {
            Dock = System.Windows.Forms.DockStyle.Bottom,
            Height = 30,
            Visible = true
        };

        _statusLabel = new MenuItem("Ready | Click a control in the Toolbox to add it to the design surface");
        _statusStrip.AddItem(_statusLabel);
    }

    private void BindViewModel()
    {
        // Bind ViewModel to UI
        _viewModel.DesignSurface.ControlSelected += (s, control) =>
        {
            _propertiesPanel.ShowProperties(control);
        };

        _viewModel.DesignSurface.SelectionCleared += (s, e) =>
        {
            _propertiesPanel.Clear();
        };

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(DesignerViewModel.GeneratedCode))
            {
                _codeOutput.Text = _viewModel.GeneratedCode;
            }
        };

        _viewModel.DesignSurface.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(DesignSurfaceViewModel.StatusMessage))
            {
                if (_statusLabel != null)
                    _statusLabel.Text = _viewModel.DesignSurface.StatusMessage;
            }
        };

        // Set initial code
        _codeOutput.Text = _viewModel.GeneratedCode;
    }

    private void OnControlRequested(object? sender, string controlType)
    {
        _viewModel.PendingControlType = controlType;
        _designSurface.PendingControlType = controlType;
        Console.WriteLine($"Control requested: {controlType}");
    }

    private void OnDesignControlSelected(object? sender, DesignControl control)
    {
        _viewModel.DesignSurface.SelectedControl = control;
        UpdateCodeOutput();
    }

    private void OnDesignSelectionCleared(object? sender, EventArgs e)
    {
        _viewModel.DesignSurface.ClearSelection();
    }

    private void UpdateCodeOutput()
    {
        var code = CodeGenerator.GenerateCode(_designSurface.DesignControls);
        _codeOutput.Text = code;
    }

    private void DesignSurface_MouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (e.Button == System.Windows.Forms.MouseButtons.Right && _designSurface.SelectedControl != null)
        {
            ShowLayoutContextMenu(e.Location);
        }
    }

    private void ShowLayoutContextMenu(Point location)
    {
        var contextMenu = new SDUI.Controls.ContextMenuStrip();

        var layoutMenu = new MenuItem("Layout");
        
        var bringToFrontItem = new MenuItem("Bring to Front");
        bringToFrontItem.Click += (s, e) =>
        {
            if (_designSurface.SelectedControl?.Control != null)
            {
                _designSurface.SelectedControl.Control.BringToFront();
                _designSurface.Invalidate();
            }
        };
        layoutMenu.AddDropDownItem(bringToFrontItem);

        var sendToBackItem = new MenuItem("Send to Back");
        sendToBackItem.Click += (s, e) =>
        {
            if (_designSurface.SelectedControl?.Control != null)
            {
                _designSurface.SelectedControl.Control.SendToBack();
                _designSurface.Invalidate();
            }
        };
        layoutMenu.AddDropDownItem(sendToBackItem);

        layoutMenu.AddDropDownItem(new MenuItemSeparator());

        var alignLeftItem = new MenuItem("Align Left");
        alignLeftItem.Click += (s, e) =>
        {
            if (_designSurface.SelectedControl?.Control != null)
            {
                var control = _designSurface.SelectedControl.Control;
                control.Location = new Point(10, control.Location.Y);
                _designSurface.Invalidate();
            }
        };
        layoutMenu.AddDropDownItem(alignLeftItem);

        var alignTopItem = new MenuItem("Align Top");
        alignTopItem.Click += (s, e) =>
        {
            if (_designSurface.SelectedControl?.Control != null)
            {
                var control = _designSurface.SelectedControl.Control;
                control.Location = new Point(control.Location.X, 10);
                _designSurface.Invalidate();
            }
        };
        layoutMenu.AddDropDownItem(alignTopItem);

        layoutMenu.AddDropDownItem(new MenuItemSeparator());

        var centerHorizontalItem = new MenuItem("Center Horizontally");
        centerHorizontalItem.Click += (s, e) =>
        {
            if (_designSurface.SelectedControl?.Control != null)
            {
                var control = _designSurface.SelectedControl.Control;
                var centerX = (_designSurface.Width - control.Width) / 2;
                control.Location = new Point(centerX, control.Location.Y);
                _designSurface.Invalidate();
            }
        };
        layoutMenu.AddDropDownItem(centerHorizontalItem);

        var centerVerticalItem = new MenuItem("Center Vertically");
        centerVerticalItem.Click += (s, e) =>
        {
            if (_designSurface.SelectedControl?.Control != null)
            {
                var control = _designSurface.SelectedControl.Control;
                var centerY = (_designSurface.Height - control.Height) / 2;
                control.Location = new Point(control.Location.X, centerY);
                _designSurface.Invalidate();
            }
        };
        layoutMenu.AddDropDownItem(centerVerticalItem);

        contextMenu.AddItem(layoutMenu);

        contextMenu.AddItem(new MenuItemSeparator());

        var deleteItem = new MenuItem("Delete");
        deleteItem.Click += (s, e) => DeleteSelectedControl();
        contextMenu.AddItem(deleteItem);

        // Show context menu at cursor position
        contextMenu.Show(_designSurface, location);
    }
}
