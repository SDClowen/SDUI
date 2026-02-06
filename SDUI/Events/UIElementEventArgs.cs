using System;
using SDUI.Controls;

namespace SDUI;

public class UIElementEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the UIElementEventArgs class with the specified UI element.
    /// </summary>
    /// <param name="element">The UI element associated with the event. Cannot be null.</param>
    public UIElementEventArgs(IElement element)
    {
        Element = element;
    }

    /// <summary>
    /// Gets the underlying UI element associated with this instance.
    /// </summary>
    public IElement Element { get; }
}