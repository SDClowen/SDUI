using System;
using System.Drawing;
using SDUI.Controls;
using SDUI.Extensions;

namespace SDUI.Designer;

/// <summary>
/// Toolbox panel containing available controls
/// </summary>
public class Toolbox : SDUI.Controls.Panel
{
    private readonly SDUI.Controls.FlowLayoutPanel _flowPanel;
    private readonly SDUI.Controls.Button _toggleButton;
    private bool _isCollapsed = false;
    private int _expandedWidth = 220;

    public Toolbox()
    {
        Dock = System.Windows.Forms.DockStyle.Left;
        Width = _expandedWidth;
        BackColor = ColorScheme.SurfaceContainer;
        ShadowDepth = 0;
        Radius = 0;
        Border = new System.Windows.Forms.Padding(0, 0, 1, 0);
        AutoScroll = true;
        
        var titleLabel = new SDUI.Controls.Label
        {
            Text = "Toolbox",
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 45,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new System.Windows.Forms.Padding(15, 0, 0, 0)
        };

        _toggleButton = new SDUI.Controls.Button
        {
            Text = "◀",
            Size = new Size(30, 30),
            Location = new Point(185, 8),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _toggleButton.Click += ToggleButton_Click;
        titleLabel.Controls.Add(_toggleButton);

        _flowPanel = new SDUI.Controls.FlowLayoutPanel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
            AutoScroll = true,
            WrapContents = false,
            Padding = new System.Windows.Forms.Padding(10)
        };

        CreateToolboxItems();

        Controls.Add(_flowPanel);
        Controls.Add(titleLabel);
    }

    public event EventHandler<string>? ControlRequested;

    private void ToggleButton_Click(object? sender, EventArgs e)
    {
        _isCollapsed = !_isCollapsed;
        
        if (_isCollapsed)
        {
            Width = 35;
            _toggleButton.Text = "▶";
            _flowPanel.Visible = false;
        }
        else
        {
            Width = _expandedWidth;
            _toggleButton.Text = "◀";
            _flowPanel.Visible = true;
        }
        
        _toggleButton.Location = new Point(Width - 45, 8);
        
        // Trigger parent layout to adjust surrounding controls
        if (Parent is UIWindow window)
        {
            window.PerformLayout();
            window.Invalidate();
        }
        else if (Parent is UIElementBase element)
        {
            element.PerformLayout();
            element.Invalidate();
        }
    }

    private void CreateToolboxItems()
    {
        string[] controls = new[]
        {
            // Basic Controls
            "Button", "Label", "TextBox", "Panel",
            "CheckBox", "Radio", "ComboBox",
            
            // Containers
            "GroupBox", "SplitContainer", "TabControl",
            "FlowLayoutPanel",
            
            // Lists & Trees
            "ListView", "TreeView", "ListBox",
            
            // Input Controls
            "NumUpDown", "TrackBar", "ToggleButton",
            
            // Progress & Visual
            "ProgressBar", "ShapeProgressBar", "ScrollBar",
            
            // Special
            "PictureBox", "ChatBubble", "PropertyGrid",
            
            // Menus & Strips
            "MenuStrip", "StatusStrip", "ContextMenuStrip"
        };

        _flowPanel.SuspendLayout();
        
        foreach (var controlName in controls)
        {
            var btn = new SDUI.Controls.Button
            {
                Text = controlName,
                Width = 190,
                Height = 35,
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 5),
                Tag = controlName,
                Font = new Font("Segoe UI", 8.5f)
            };

            btn.Click += (s, e) =>
            {
                if (s is SDUI.Controls.Button button && button.Tag is string type)
                {
                    ControlRequested?.Invoke(this, type);
                }
            };

            _flowPanel.Controls.Add(btn);
        }
        
        _flowPanel.ResumeLayout(true);
    }

    private void OnControlRequested(string controlType)
    {
        ControlRequested?.Invoke(this, controlType);
    }
}
