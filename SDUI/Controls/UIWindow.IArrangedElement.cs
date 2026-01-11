using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SDUI.Layout;

namespace SDUI.Controls;

public partial class UIWindow
{
    #region IArrangedElement Explicit Implementation

    void IArrangedElement.SetBounds(Rectangle bounds, BoundsSpecified specified)
    {
        // Update specified bounds tracking (used by layout engines)
        CommonProperties.UpdateSpecifiedBounds(this, bounds.X, bounds.Y, bounds.Width, bounds.Height, specified);

        // Apply the bounds
        if (Bounds != bounds)
            Bounds = bounds;
    }

    bool IArrangedElement.ParticipatesInLayout => Visible;

    IArrangedElement? IArrangedElement.Container => null; // Form has no container

    ArrangedElementCollection IArrangedElement.Children
    {
        get
        {
            // Create a list of IArrangedElement from our Controls
            var arrangedElements = new List<IArrangedElement>();
            foreach (var control in Controls.OfType<UIElementBase>())
            {
                arrangedElements.Add(control);
            }
            return new ArrangedElementCollection(arrangedElements);
        }
    }

    void IArrangedElement.PerformLayout(IArrangedElement affectedElement, string? propertyName)
    {
        if (affectedElement is UIElementBase element)
            PerformLayout();
        else
            PerformLayout();
    }

    PropertyStore IArrangedElement.Properties => _properties ?? (_properties = new PropertyStore());
    private PropertyStore? _properties;

    #endregion
}
