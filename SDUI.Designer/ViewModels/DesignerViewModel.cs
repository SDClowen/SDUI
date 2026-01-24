using System;

namespace SDUI.Designer.ViewModels;

/// <summary>
/// ViewModel for the main designer window
/// </summary>
public class DesignerViewModel : ViewModelBase
{
    private string _generatedCode = string.Empty;
    private string? _pendingControlType;

    public DesignerViewModel()
    {
        DesignSurface = new DesignSurfaceViewModel();
        
        // Subscribe to design surface events
        DesignSurface.ControlSelected += OnDesignControlSelected;
        DesignSurface.SelectionCleared += OnDesignSelectionCleared;
        DesignSurface.ControlsChanged += OnDesignControlsChanged;

        GenerateInitialCode();
    }

    public DesignSurfaceViewModel DesignSurface { get; }

    public string GeneratedCode
    {
        get => _generatedCode;
        set => SetProperty(ref _generatedCode, value);
    }

    public string? PendingControlType
    {
        get => _pendingControlType;
        set
        {
            if (SetProperty(ref _pendingControlType, value) && value != null)
            {
                DesignSurface.StatusMessage = $"Click on the design surface to add {value}";
            }
        }
    }

    public void AddControlToSurface(string controlType)
    {
        DesignSurface.AddControlCommand.Execute(controlType);
        PendingControlType = null;
    }

    private void OnDesignControlSelected(object? sender, DesignControl control)
    {
        DesignSurface.StatusMessage = $"Selected: {control.ControlType}";
        UpdateCode();
    }

    private void OnDesignSelectionCleared(object? sender, EventArgs e)
    {
        DesignSurface.StatusMessage = "Ready";
    }

    private void OnDesignControlsChanged(object? sender, EventArgs e)
    {
        UpdateCode();
    }

    private void UpdateCode()
    {
        GeneratedCode = CodeGenerator.GenerateCode(DesignSurface.DesignControls);
    }

    private void GenerateInitialCode()
    {
        GeneratedCode = @"// Generated code will appear here

using System;
using System.Drawing;
using SDUI.Controls;

public class MyForm : UIWindow
{
    public MyForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Window settings
        Text = ""My Window"";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        // Add your controls here
    }
}";
    }
}
