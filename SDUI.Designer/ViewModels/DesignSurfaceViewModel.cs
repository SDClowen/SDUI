using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SDUI.Controls;

namespace SDUI.Designer.ViewModels;

/// <summary>
/// ViewModel for the design surface
/// </summary>
public class DesignSurfaceViewModel : ViewModelBase
{
    private DesignControl? _selectedControl;
    private string _statusMessage = "Ready";

    public DesignSurfaceViewModel()
    {
        DesignControls = new ObservableCollection<DesignControl>();
        AddControlCommand = new RelayCommand<string>(ExecuteAddControl, CanAddControl);
        DeleteSelectedCommand = new RelayCommand(ExecuteDeleteSelected, CanDeleteSelected);
        ClearAllCommand = new RelayCommand(ExecuteClearAll, CanClearAll);
    }

    public ObservableCollection<DesignControl> DesignControls { get; }

    public DesignControl? SelectedControl
    {
        get => _selectedControl;
        set
        {
            if (SetProperty(ref _selectedControl, value))
            {
                OnPropertyChanged(nameof(HasSelection));
                DeleteSelectedCommand.RaiseCanExecuteChanged();
                OnControlSelected();
            }
        }
    }

    public bool HasSelection => _selectedControl != null;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand<string> AddControlCommand { get; }
    public RelayCommand DeleteSelectedCommand { get; }
    public RelayCommand ClearAllCommand { get; }

    public event EventHandler<DesignControl>? ControlSelected;
    public event EventHandler? SelectionCleared;
    public event EventHandler? ControlsChanged;

    private bool CanAddControl(string? controlType)
    {
        return !string.IsNullOrEmpty(controlType);
    }

    private void ExecuteAddControl(string? controlType)
    {
        if (string.IsNullOrEmpty(controlType))
            return;

        // Create control based on type
        UIElementBase? control = controlType switch
        {
            "Button" => new SDUI.Controls.Button { Text = "Button", Size = new SkiaSharp.SKSize(100, 35) },
            "Label" => new SDUI.Controls.Label { Text = "Label", Size = new SkiaSharp.SKSize(100, 25) },
            "TextBox" => new SDUI.Controls.TextBox { Size = new SkiaSharp.SKSize(200, 30) },
            "Panel" => new SDUI.Controls.Panel { Size = new SkiaSharp.SKSize(200, 150) },
            "CheckBox" => new SDUI.Controls.CheckBox { Text = "CheckBox", Size = new SkiaSharp.SKSize(120, 25) },
            "ComboBox" => new SDUI.Controls.ComboBox { Size = new SkiaSharp.SKSize(150, 30) },
            "ColorPicker" => new SDUI.Controls.ColorPicker { Size = new SkiaSharp.SKSize(180, 32) },
            "ListBox" => new SDUI.Controls.ListBox { Size = new SkiaSharp.SKSize(150, 100) },
            "ProgressBar" => new SDUI.Controls.ProgressBar { Size = new SkiaSharp.SKSize(200, 25), Value = 50 },
            "GroupBox" => new SDUI.Controls.GroupBox { Text = "GroupBox", Size = new SkiaSharp.SKSize(200, 150) },
            _ => null
        };

        if (control == null)
            return;

        control.Name = $"{controlType.ToLower()}{DesignControls.Count + 1}";
        control.Location = new System.Drawing.SKPoint(20 + DesignControls.Count * 10, 20 + DesignControls.Count * 10);

        var designControl = new DesignControl(control)
        {
            Location = control.Location
        };

        DesignControls.Add(designControl);
        SelectedControl = designControl;
        StatusMessage = $"Added {controlType}";
        OnControlsChanged();
    }

    private bool CanDeleteSelected()
    {
        return _selectedControl != null;
    }

    private void ExecuteDeleteSelected()
    {
        if (_selectedControl != null)
        {
            DesignControls.Remove(_selectedControl);
            SelectedControl = null;
            StatusMessage = "Control deleted";
            OnControlsChanged();
        }
    }

    private bool CanClearAll()
    {
        return DesignControls.Count > 0;
    }

    private void ExecuteClearAll()
    {
        DesignControls.Clear();
        SelectedControl = null;
        StatusMessage = "All controls cleared";
        OnControlsChanged();
    }

    public void ClearSelection()
    {
        SelectedControl = null;
        StatusMessage = "Ready";
        OnSelectionCleared();
    }

    private void OnControlSelected()
    {
        if (_selectedControl != null)
        {
            ControlSelected?.Invoke(this, _selectedControl);
        }
    }

    private void OnSelectionCleared()
    {
        SelectionCleared?.Invoke(this, EventArgs.Empty);
    }

    private void OnControlsChanged()
    {
        ControlsChanged?.Invoke(this, EventArgs.Empty);
    }
}
