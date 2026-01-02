using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Demo
{
    public partial class TreeViewPage : SDUI.Controls.UIElementBase
    {
        private SDUI.Controls.Label hintLabel;

        public TreeViewPage()
        {
            InitializeComponent();
            this.Text = "TreeView";

            // demo data
            var rootA = new SDUI.Controls.TreeNode("Documents");
            var a1 = new SDUI.Controls.TreeNode("Work");
            a1.Nodes.Add(new SDUI.Controls.TreeNode("Report.docx"));
            a1.Nodes.Add(new SDUI.Controls.TreeNode("Budget.xlsx"));
            rootA.Nodes.Add(a1);
            var a2 = new SDUI.Controls.TreeNode("Personal");
            a2.Nodes.Add(new SDUI.Controls.TreeNode("Vacation.jpg"));
            a2.Nodes.Add(new SDUI.Controls.TreeNode("Letter.docx"));
            rootA.Nodes.Add(a2);

            var rootB = new SDUI.Controls.TreeNode("Projects");
            var p1 = new SDUI.Controls.TreeNode("SDUI");
            p1.Nodes.Add(new SDUI.Controls.TreeNode("Controls"));
            p1.Nodes.Add(new SDUI.Controls.TreeNode("Rendering"));
            p1.Nodes.Add(new SDUI.Controls.TreeNode("Tests"));
            rootB.Nodes.Add(p1);

            var images = new SDUI.Controls.TreeNode("Images");
            for (int i = 1; i <= 8; i++) images.Nodes.Add(new SDUI.Controls.TreeNode($"img_{i}.png"));

            treeView1.Nodes.Add(rootA);
            treeView1.Nodes.Add(rootB);
            treeView1.Nodes.Add(images);

            rootA.Expanded = true;
            treeView1.SetSelectedNode(rootA);

            // small polish: expand first child
            if (rootA.Nodes.Count > 0) rootA.Nodes[0].Expanded = true;

            // keyboard hint
            hintLabel = new SDUI.Controls.Label
            {
                Text = "Use arrow keys to navigate, click triangles to expand/collapse.",
                Dock = DockStyle.Bottom,
                AutoSize = true,
                ForeColor = SDUI.ColorScheme.ForeColor
            };
            hintLabel.BackColor = Color.Transparent;
            hintLabel.Margin = new Padding(6);
            this.Controls.Add(hintLabel);

            // demo icons
            rootA.IconColor = Color.FromArgb(0xFF, 0x66, 0x99, 0xCC);
            rootB.IconColor = Color.FromArgb(0xFF, 0x88, 0xCC, 0x66);
            images.IconColor = Color.FromArgb(0xFF, 0xFF, 0xBB, 0x66);

            treeView1.NodeMouseClick += (s, ea) => hintLabel.Text = $"Clicked: {ea.Node.Text} ({ea.Button})";
            treeView1.NodeMouseDoubleClick += (s, ea) => hintLabel.Text = $"Double-clicked: {ea.Node.Text}";
            treeView1.NodeMouseRightClick += (s, ea) =>
            {
                var menu = new SDUI.Controls.ContextMenuStrip();
                var menuItem = new SDUI.Controls.MenuItem("Open", null);
                menuItem.Click += (ss, ee) => hintLabel.Text = $"Open {ea.Node.Text}";

                var menuItem2 = new SDUI.Controls.MenuItem("Delete", null);
                menuItem2.Click += (ss, ee) => hintLabel.Text = $"Delete {ea.Node.Text}";

                menu.Items.Add(menuItem);
                menu.Items.Add(menuItem2);
                // TreeView is not a WinForms Control; show menu at cursor position instead
                menu.Show(Cursor.Position);
            };
        }
    }
}