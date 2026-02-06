using SDUI.Controls;
using System;
using System.ComponentModel;

namespace SDUI;

public sealed class LayoutEventArgs : EventArgs
{
    private readonly WeakReference<IComponent>? _affectedComponent;

    public LayoutEventArgs(IComponent? affectedComponent, string? affectedProperty)
    {
        _affectedComponent = affectedComponent is not null ? new(affectedComponent) : null;
        AffectedProperty = affectedProperty;
    }

    public LayoutEventArgs(IElement? affectedControl, string? affectedProperty)
        : this((IComponent?)affectedControl, affectedProperty)
    {
    }

    public IComponent? AffectedComponent
    {
        get
        {
            IComponent? target = null;
            _affectedComponent?.TryGetTarget(out target);
            return target;
        }
    }

    public IElement? AffectedControl => AffectedComponent as IElement;

    public string? AffectedProperty { get; }
}
