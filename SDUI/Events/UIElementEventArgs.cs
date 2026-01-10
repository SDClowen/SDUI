using System;
using SDUI.Controls;

namespace SDUI;

public class UIElementEventArgs : EventArgs
{
    public UIElementEventArgs(IUIElement element)
    {
        Element = element;
    }

    public IUIElement Element { get; }
}