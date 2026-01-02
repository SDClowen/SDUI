using SDUI.Controls;
using System;

namespace SDUI;

public class UIElementEventArgs : EventArgs
{
    public IUIElement Element { get; }

    public UIElementEventArgs(IUIElement element)
    {
        Element = element;
    }
}
