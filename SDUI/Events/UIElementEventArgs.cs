using SDUI.Controls;
using System;

namespace SDUI;

public class UIElementEventArgs : EventArgs
{
    public UIElementBase Element { get; }

    public UIElementEventArgs(UIElementBase element)
    {
        Element = element;
    }
}
