using System.Drawing;
using System.Windows.Forms;
using SDUI.Controls;
using Button = SDUI.Controls.Button;
using CheckBox = SDUI.Controls.CheckBox;
using GroupBox = SDUI.Controls.GroupBox;
using Panel = SDUI.Controls.Panel;
using ProgressBar = SDUI.Controls.ProgressBar;

namespace SDUI.Demo
{
    public class ModernControlsPage : UIElementBase
    {
        public ModernControlsPage()
        {
            Text = "Modern Controls";
            Padding = new Padding(20);
            
            // Container for layout
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20)
            };
            
            // 1. Buttons Section
            var btnGroup = new GroupBox
            {
                Text = "Modern Buttons",
                Size = new Size(300, 150),
                Location = new Point(20, 20)
            };
            
            var btnPrimary = new Button
            {
                Text = "Primary Button",
                Location = new Point(20, 40),
                Size = new Size(120, 36),
                Color = Color.Transparent // Will use Primary color
            };

            var btnSecondary = new Button
            {
                Text = "Secondary",
                Location = new Point(150, 40),
                Size = new Size(120, 36),
                Color = ColorScheme.Secondary // Explicit secondary color
            };

            var btnDisabled = new Button
            {
                Text = "Disabled",
                Location = new Point(20, 90),
                Size = new Size(120, 36),
                Enabled = false
            };

            btnGroup.Controls.Add(btnPrimary);
            btnGroup.Controls.Add(btnSecondary);
            btnGroup.Controls.Add(btnDisabled);

            // 2. Toggles Section
            var toggleGroup = new GroupBox
            {
                Text = "Toggles & Inputs",
                Size = new Size(300, 150),
                Location = new Point(340, 20)
            };

            var cb1 = new CheckBox
            {
                Text = "Checkbox 1",
                Location = new Point(20, 40),
                Checked = true
            };

            var cb2 = new CheckBox
            {
                Text = "Checkbox 2",
                Location = new Point(20, 70)
            };

            var rb1 = new Radio
            {
                Text = "Radio Option 1",
                Location = new Point(150, 40),
                Checked = true
            };

            var rb2 = new Radio
            {
                Text = "Radio Option 2",
                Location = new Point(150, 70)
            };

            toggleGroup.Controls.Add(cb1);
            toggleGroup.Controls.Add(cb2);
            toggleGroup.Controls.Add(rb1);
            toggleGroup.Controls.Add(rb2);

            // 3. Progress Section
            var progressGroup = new GroupBox
            {
                Text = "Progress Indicators",
                Size = new Size(620, 100),
                Location = new Point(20, 190)
            };

            var pb1 = new ProgressBar
            {
                Value = 60,
                Location = new Point(20, 40),
                Size = new Size(580, 10),
                ShowValue = true
            };

            var pb2 = new ProgressBar
            {
                Value = 30,
                Location = new Point(20, 70),
                Size = new Size(580, 6),
                Gradient = new[] { ColorScheme.Secondary, ColorScheme.SecondaryContainer }
            };

            progressGroup.Controls.Add(pb1);
            progressGroup.Controls.Add(pb2);

            // Add all to page
            this.Controls.Add(btnGroup);
            this.Controls.Add(toggleGroup);
            this.Controls.Add(progressGroup);
        }
    }
}
