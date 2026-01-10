using System;
using SDUI.Controls;

namespace SDUI;

public class UILayoutEventArgs : EventArgs
{
    public UILayoutEventArgs(UIElementBase affectedElement)
    {
        AffectedElement = affectedElement;
    }

    public UIElementBase AffectedElement { get; }
}