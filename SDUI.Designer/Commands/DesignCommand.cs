using System;
using System.Drawing;
using SDUI.Controls;

namespace SDUI.Designer.Commands;

/// <summary>
/// Base class for undo/redo commands
/// </summary>
public abstract class DesignCommand
{
    public abstract void Execute();
    public abstract void Undo();
    public abstract string Description { get; }
}

/// <summary>
/// Command for adding a control
/// </summary>
public class AddControlCommand : DesignCommand
{
    private readonly DesignSurface _surface;
    private readonly string _controlType;
    private readonly Point _location;
    private DesignControl? _addedControl;

    public AddControlCommand(DesignSurface surface, string controlType, Point location)
    {
        _surface = surface;
        _controlType = controlType;
        _location = location;
    }

    public override string Description => $"Add {_controlType}";

    public override void Execute()
    {
        _surface.AddDesignControlInternal(_controlType, _location);
        _addedControl = _surface.SelectedControl;
    }

    public override void Undo()
    {
        if (_addedControl != null)
        {
            _surface.RemoveDesignControl(_addedControl);
        }
    }
}

/// <summary>
/// Command for deleting a control
/// </summary>
public class DeleteControlCommand : DesignCommand
{
    private readonly DesignSurface _surface;
    private readonly DesignControl _control;
    private readonly int _index;

    public DeleteControlCommand(DesignSurface surface, DesignControl control, int index)
    {
        _surface = surface;
        _control = control;
        _index = index;
    }

    public override string Description => $"Delete {_control.ControlType}";

    public override void Execute()
    {
        _surface.RemoveDesignControl(_control);
    }

    public override void Undo()
    {
        _surface.InsertDesignControl(_control, _index);
    }
}

/// <summary>
/// Command for moving a control
/// </summary>
public class MoveControlCommand : DesignCommand
{
    private readonly DesignControl _control;
    private readonly Point _oldLocation;
    private readonly Point _newLocation;

    public MoveControlCommand(DesignControl control, Point oldLocation, Point newLocation)
    {
        _control = control;
        _oldLocation = oldLocation;
        _newLocation = newLocation;
    }

    public override string Description => $"Move {_control.ControlType}";

    public override void Execute()
    {
        _control.Control.Location = _newLocation;
        _control.Location = _newLocation;
    }

    public override void Undo()
    {
        _control.Control.Location = _oldLocation;
        _control.Location = _oldLocation;
    }
}

/// <summary>
/// Command for resizing a control
/// </summary>
public class ResizeControlCommand : DesignCommand
{
    private readonly DesignControl _control;
    private readonly Rectangle _oldBounds;
    private readonly Rectangle _newBounds;

    public ResizeControlCommand(DesignControl control, Rectangle oldBounds, Rectangle newBounds)
    {
        _control = control;
        _oldBounds = oldBounds;
        _newBounds = newBounds;
    }

    public override string Description => $"Resize {_control.ControlType}";

    public override void Execute()
    {
        _control.Control.Bounds = _newBounds;
        _control.Bounds = _newBounds;
    }

    public override void Undo()
    {
        _control.Control.Bounds = _oldBounds;
        _control.Bounds = _oldBounds;
    }
}

/// <summary>
/// Command for changing a property value
/// </summary>
public class PropertyChangeCommand : DesignCommand
{
    private readonly object _target;
    private readonly string _propertyName;
    private readonly object? _oldValue;
    private readonly object? _newValue;

    public PropertyChangeCommand(object target, string propertyName, object? oldValue, object? newValue)
    {
        _target = target;
        _propertyName = propertyName;
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public override string Description => $"Change {_propertyName}";

    public override void Execute()
    {
        var property = _target.GetType().GetProperty(_propertyName);
        property?.SetValue(_target, _newValue);
    }

    public override void Undo()
    {
        var property = _target.GetType().GetProperty(_propertyName);
        property?.SetValue(_target, _oldValue);
    }
}

/// <summary>
/// Command for pasting a control from clipboard snapshot
/// </summary>
public class PasteControlCommand : DesignCommand
{
    private readonly DesignSurface _surface;
    private readonly DesignSurface.ControlSnapshot _snapshot;
    private readonly Point _location;
    private DesignControl? _addedControl;

    public PasteControlCommand(DesignSurface surface, DesignSurface.ControlSnapshot snapshot, Point location)
    {
        _surface = surface;
        _snapshot = snapshot;
        _location = location;
    }

    public override string Description => $"Paste {_snapshot.ControlType}";

    public override void Execute()
    {
        _addedControl = _surface.AddDesignControlFromSnapshot(_snapshot, _location);
    }

    public override void Undo()
    {
        if (_addedControl != null)
            _surface.RemoveDesignControl(_addedControl);
    }
}
