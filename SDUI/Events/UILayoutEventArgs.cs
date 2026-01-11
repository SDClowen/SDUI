using SDUI.Layout;
using System;

namespace SDUI;

public class UILayoutEventArgs : EventArgs
{
    public UILayoutEventArgs(IArrangedElement affectedElement)
    {
        AffectedElement = affectedElement;
    }
    public UILayoutEventArgs(IArrangedElement affectedElement, string property)
    {
        AffectedElement = affectedElement;
        Property = property;
    }

    public IArrangedElement AffectedElement { get; }
    public string Property { get; }
}