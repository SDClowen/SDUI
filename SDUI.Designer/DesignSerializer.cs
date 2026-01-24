using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SDUI.Designer;

/// <summary>
/// Serializes and deserializes design surface data
/// </summary>
internal static class DesignSerializer
{
    public static string Serialize(IEnumerable<DesignControl> controls)
    {
        var data = controls.Select(dc => new DesignData
        {
            ControlType = dc.ControlType,
            Name = dc.Control.Name ?? "",
            Location = dc.Control.Location,
            Size = dc.Control.Size,
            BackColor = ColorToHex(dc.Control.BackColor),
            ForeColor = ColorToHex(dc.Control.ForeColor),
            Enabled = dc.Control.Enabled,
            Visible = dc.Control.Visible,
            Properties = ExtractProperties(dc)
        }).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(data, options);
    }

    public static List<DesignData> Deserialize(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<DesignData>>(json, options) ?? new List<DesignData>();
    }

    public static void SaveToFile(string filePath, IEnumerable<DesignControl> controls)
    {
        var json = Serialize(controls);
        File.WriteAllText(filePath, json);
    }

    public static List<DesignData> LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return new List<DesignData>();

        var json = File.ReadAllText(filePath);
        return Deserialize(json);
    }

    private static Dictionary<string, object> ExtractProperties(DesignControl dc)
    {
        var props = new Dictionary<string, object>();

        if (dc.Control is SDUI.Controls.Button btn)
            props["Text"] = btn.Text;
        else if (dc.Control is SDUI.Controls.Label lbl)
            props["Text"] = lbl.Text;
        else if (dc.Control is SDUI.Controls.TextBox txt)
            props["Text"] = txt.Text;
        else if (dc.Control is SDUI.Controls.CheckBox chk)
        {
            props["Text"] = chk.Text;
            props["Checked"] = chk.Checked;
        }
        else if (dc.Control is SDUI.Controls.Radio rad)
        {
            props["Text"] = rad.Text;
            props["Checked"] = rad.Checked;
        }

        return props;
    }

    private static string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return Color.White;
        
        hex = hex.Replace("#", "");
        if (hex.Length == 6)
        {
            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return Color.FromArgb(r, g, b);
        }
        
        return Color.White;
    }
}

/// <summary>
/// Serializable design data
/// </summary>
public class DesignData
{
    public string ControlType { get; set; } = "";
    public string Name { get; set; } = "";
    public Point Location { get; set; }
    public Size Size { get; set; }
    public string BackColor { get; set; } = "";
    public string ForeColor { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public bool Visible { get; set; } = true;
    public Dictionary<string, object> Properties { get; set; } = new();
}
