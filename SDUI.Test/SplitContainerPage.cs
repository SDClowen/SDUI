using SDUI.Controls;
using System;

using System.Windows.Forms;

namespace SDUI.Demo
{
    public class SplitContainerPage : UIElementBase
    {
        public SplitContainerPage()
        {

            this.Text = "SplitContainer";

            // Vertical example
            var vSplit = new SDUI.Controls.SplitContainer { Orientation = Orientation.Vertical, Dock = DockStyle.Top, SplitterDistance = 220, SplitterWidth = 6, Height = 300 };
            vSplit.Panel1.BackColor = ColorScheme.BackColor;
            vSplit.Panel2.BackColor = ColorScheme.BackColor;

            var leftLabel = new SDUI.Controls.Label { Text = "Left Pane", Dock = DockStyle.Top, Height = 28 };
            var leftList = new SDUI.Controls.ListBox { Dock = DockStyle.Fill };
            // Add many items (and some long text) to stress scrollbars and layout
            for (int i = 0; i < 120; i++)
            {
                if (i % 10 == 0)
                    leftList.Items.Add($"Long item {i} - this is a long item text intended to trigger horizontal scrolling and force layout recalculation");
                else
                    leftList.Items.Add($"Item {i}");
            }
            vSplit.Panel1.Controls.Add(leftList);
            vSplit.Panel1.Controls.Add(leftLabel);

            var rightLabel = new SDUI.Controls.Label { Text = "Right Pane", Dock = DockStyle.Top, Height = 28 };
            var rightPanel = new SDUI.Controls.Panel { Dock = DockStyle.Fill };

            // AutoSize demo: FlowLayoutPanel with AutoSize and some AutoSize buttons
            var autoDemoPanel = new SDUI.Controls.FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                FlowDirection = FlowDirection.LeftToRight
            };
            autoDemoPanel.Controls.Add(new SDUI.Controls.Button { Text = "Short", AutoSize = true });
            autoDemoPanel.Controls.Add(new SDUI.Controls.Button { Text = "A much longer button text", AutoSize = true });

            // Docked AutoSize demo: a docked top button that should size to preferred height
            var dockAutoButton = new SDUI.Controls.Button { Text = "Docked AutoSize", Dock = DockStyle.Top, AutoSize = true };

            rightPanel.Controls.Add(autoDemoPanel);
            rightPanel.Controls.Add(dockAutoButton);

            // Stacked AutoSize (Top) controls + Fill example to exercise layout
            var stackDemo = new SDUI.Controls.Panel { Dock = DockStyle.Top, Height = 120 };
            stackDemo.Controls.Add(new SDUI.Controls.Button { Text = "Stack Top 1", Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(2) });
            stackDemo.Controls.Add(new SDUI.Controls.Button { Text = "Stack Top 2", Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(3) });
            stackDemo.Controls.Add(new SDUI.Controls.Panel { Dock = DockStyle.Fill, BackColor = Color.LightGray });
            rightPanel.Controls.Add(stackDemo);

            rightPanel.Controls.Add(new SDUI.Controls.Label { Text = "Content goes here", Dock = DockStyle.Top, Height = 24 });
            vSplit.Panel2.Controls.Add(rightPanel);
            vSplit.Panel2.Controls.Add(rightLabel);

            this.Controls.Add(vSplit);

            // Horizontal example
            var hSplit = new SDUI.Controls.SplitContainer { Orientation = Orientation.Horizontal, Dock = DockStyle.Fill, SplitterDistance = 150, SplitterWidth = 6 };
            hSplit.Panel1.Controls.Add(new SDUI.Controls.Label { Text = "Top Pane", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            hSplit.Panel2.Controls.Add(new SDUI.Controls.Label { Text = "Bottom Pane", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            hSplit.Panel1.BackColor = ColorScheme.BackColor;
            hSplit.Panel2.BackColor = ColorScheme.BackColor;

            this.Controls.Add(hSplit);
        }
    }
}
