using SDUI.Controls;
using System.Drawing;
using System.Windows.Forms;

using SPanel = SDUI.Controls.Panel;
using SButton = SDUI.Controls.Button;
using SLabel = SDUI.Controls.Label;

namespace SDUI.Demo
{
    public class LayoutCompatibilityPage : UIElementBase
    {
        public LayoutCompatibilityPage()
        {
            Text = "Layout Compatibility";

            // Section 1: Min/Max enforcement with AutoSize
            var minMaxPanel = new SPanel { Dock = DockStyle.Top, Height = 150 };
            minMaxPanel.Controls.Add(new SButton { Text = "AutoSize (MaxHeight=30)", AutoSize = true, Dock = DockStyle.Top, MaximumSize = new Size(0, 30), Margin = new Padding(4) });
            minMaxPanel.Controls.Add(new SButton { Text = "AutoSize (MinHeight=40)", AutoSize = true, Dock = DockStyle.Top, MinimumSize = new Size(0, 40), Margin = new Padding(4) });
            minMaxPanel.Controls.Add(new SLabel { Text = "Verify: Controls should not exceed Max or be smaller than Min when AutoSize'd.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });

            // Section 2: Left/Right stacked AutoSize controls
            var lrPanel = new SPanel { Dock = DockStyle.Top, Height = 160 };
            lrPanel.Controls.Add(new SLabel { Text = "Left/Right stacking (AutoSize) - verify widths and stacking order", Dock = DockStyle.Top, Height = 24 });
            var lrInner = new SPanel { Dock = DockStyle.Fill };
            var left1 = new SButton { Text = "L1", Dock = DockStyle.Left, AutoSize = true, Margin = new Padding(3) };
            var left2 = new SButton { Text = "Left Longer", Dock = DockStyle.Left, AutoSize = true, Margin = new Padding(3) };
            var right1 = new SButton { Text = "R1", Dock = DockStyle.Right, AutoSize = true, Margin = new Padding(3) };
            var right2 = new SButton { Text = "Right Much Longer", Dock = DockStyle.Right, AutoSize = true, Margin = new Padding(3) };
            lrInner.Controls.Add(left1); lrInner.Controls.Add(left2); lrInner.Controls.Add(right1); lrInner.Controls.Add(right2);
            lrPanel.Controls.Add(lrInner);

            // Section 3: Nested dock + anchor combos
            var nested = new SPanel { Dock = DockStyle.Fill, Padding = new Padding(6) };
            var outer = new SPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(230, 230, 255) };
            var topAuto = new SButton { Text = "Top AutoSize", Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(2) };
            var innerContainer = new SPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 240, 255) };
            var anchored = new SButton { Text = "Anchored L+R", Size = new Size(100, 30), Anchor = AnchorStyles.Left | AnchorStyles.Right, Location = new Point(10, 10) };
            innerContainer.Controls.Add(anchored);
            outer.Controls.Add(innerContainer);
            outer.Controls.Add(topAuto);
            nested.Controls.Add(outer);

            Controls.Add(nested);
            Controls.Add(lrPanel);
            Controls.Add(minMaxPanel);

            // Quick instructions for manual verification
            var note = new SLabel { Dock = DockStyle.Bottom, Height = 40, Text = "Instructions: Resize the window, observe AutoSize/min/max behavior, stack order, and anchored resizing. Use the SplitContainer demo to try more combinations.", TextAlign = ContentAlignment.MiddleLeft };
            Controls.Add(note);
        }
    }
}