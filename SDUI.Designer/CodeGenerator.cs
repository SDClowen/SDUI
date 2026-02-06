using System;
using System.Collections.Generic;
using System.Text;

namespace SDUI.Designer;

/// <summary>
/// Generates C# code from design surface controls
/// </summary>
public static class CodeGenerator
{
    public static string GenerateCode(IEnumerable<DesignControl> controls, string className = "MyForm")
    {
        var sb = new StringBuilder();

        // Using statements
        sb.AppendLine("using System;");
        sb.AppendLine("");
        sb.AppendLine("using SDUI.Controls;");
        sb.AppendLine();

        // Class declaration
        sb.AppendLine($"public class {className} : UIWindow");
        sb.AppendLine("{");

        // Field declarations
        foreach (var control in controls)
        {
            var fieldName = GetFieldName(control);
            sb.AppendLine($"    private {control.ControlType} {fieldName};");
        }

        sb.AppendLine();

        // Constructor
        sb.AppendLine($"    public {className}()");
        sb.AppendLine("    {");
        sb.AppendLine("        InitializeComponent();");
        sb.AppendLine("    }");
        sb.AppendLine();

        // InitializeComponent method
        sb.AppendLine("    private void InitializeComponent()");
        sb.AppendLine("    {");
        sb.AppendLine("        // Window settings");
        sb.AppendLine("        Text = \"My Window\";");
        sb.AppendLine("        Size = new Size(800, 600);");
        sb.AppendLine("        StartPosition = FormStartPosition.CenterScreen;");
        sb.AppendLine();

        // Initialize controls
        foreach (var control in controls)
        {
            sb.Append(GenerateControlCode(control));
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateControlCode(DesignControl designControl)
    {
        var sb = new StringBuilder();
        var fieldName = GetFieldName(designControl);

        sb.AppendLine($"        // {fieldName}");
        sb.AppendLine($"        {fieldName} = new {designControl.ControlType}");
        sb.AppendLine("        {");

        // Common properties
        sb.AppendLine($"            Location = new SKPoint({designControl.Control.Location.X}, {designControl.Control.Location.Y}),");
        sb.AppendLine($"            Size = new Size({designControl.Control.Size.Width}, {designControl.Control.Size.Height}),");

        // Text property
        if (!string.IsNullOrEmpty(designControl.Control.Text))
            sb.AppendLine($"            Text = \"{designControl.Control.Text}\",");

        sb.AppendLine("        };");
        sb.AppendLine($"        Controls.Add({fieldName});");
        sb.AppendLine();

        return sb.ToString();
    }

    private static string GetFieldName(DesignControl control)
    {
        var baseName = control.Control.Name;
        if (string.IsNullOrEmpty(baseName))
        {
            baseName = char.ToLowerInvariant(control.ControlType[0]) + control.ControlType.Substring(1) + "1";
        }
        return baseName;
    }
}
