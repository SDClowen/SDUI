using SDUI.Controls;
using System;

namespace SDUI;

public class UILayoutEventArgs : EventArgs
{
    public UIElementBase AffectedElement { get; }

    public UILayoutEventArgs(UIElementBase affectedElement)
    {
        AffectedElement = affectedElement;
    }
}
