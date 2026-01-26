using System;
using System.Drawing;
using SDUI.Controls;

namespace SDUI.Designer;

/// <summary>
/// Properties panel using the new PropertyGrid control
/// </summary>
internal class PropertiesPanel : SDUI.Controls.GroupBox
{
    private readonly PropertyGrid _propertyGrid;
    private DesignControl? _selectedControl;
    private int _expandedWidth = 300;

    public PropertiesPanel()
    {
        Width = _expandedWidth;
        BackColor = ColorScheme.BackColor.Brightness(0.02f);
        Text = "Properties";
        ShadowDepth = 0;
        Radius = 0;
        Collapsible = true;
        CollapseDirection = CollapseDirection.Horizontal;

        _propertyGrid = new PropertyGrid
        {
            Location = new Point(10, 45),
            Width = Width - 20,
            Height = 600,
            PropertySort = PropertySort.Categorized,
            BackColor = Color.Transparent,
            Dock = System.Windows.Forms.DockStyle.Fill
        };
        _propertyGrid.PropertyValueChanged += OnPropertyValueChanged;

        Controls.Add(_propertyGrid);
    }

    public void ShowProperties(DesignControl control)
    {
        _selectedControl = control;
        _propertyGrid.SelectedObject = control.Control;
    }

    public void Clear()
    {
        _selectedControl = null;
        _propertyGrid.SelectedObject = null;
    }

    private void OnPropertyValueChanged(object? sender, PropertyValueChangedEventArgs e)
    {
        if (_selectedControl == null) return;

        // Update DesignControl wrapper if needed
        if (e.PropertyName == "Location" && e.NewValue is Point newLocation)
        {
            _selectedControl.Location = newLocation;
        }
        else if (e.PropertyName == "Size" && e.NewValue is Size newSize)
        {
            _selectedControl.Size = newSize;
        }
        else if (e.PropertyName == "Text" && e.NewValue is string newText)
        {
            _selectedControl.Text = newText;
        }

        // Trigger refresh
        _selectedControl.Control.Invalidate();
    }
}
