
using System.Text.Json.Serialization;
using System.Reflection;
using SDUI.Controls;
using System.Linq;

namespace SDUI.Designer;

/// <summary>
/// Wraps a control for design-time operations
/// </summary>
public class DesignControl
{
    public DesignControl(UIElementBase control)
    {
        Control = control;
        Name = control.Name;
        ControlType = control.GetType().Name;
        Location = control.Location;
        Size = control.Size;
        Text = control.Text;
    }

    [JsonConstructor]
    public DesignControl()
    {
        Control = null!;
        Name = string.Empty;
        ControlType = string.Empty;
        Text = string.Empty;
    }

    [JsonIgnore]
    public UIElementBase Control { get; set; }

    public string Name { get; set; }
    public string ControlType { get; set; }
    public SKPoint Location { get; set; }
    public Size Size { get; set; }
    public string Text { get; set; }

    public Rectangle Bounds
    {
        get => new Rectangle(Location, Size);
        set
        {
            Location = value.Location;
            Size = value.Size;
        }
    }

    public PropertyInfo[] GetProperties()
    {
        return Control.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.CanRead)
            .ToArray();
    }
}
