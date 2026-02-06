
using System.Windows.Forms;

namespace SDUI.Demo
{
    public partial class ListViewPage : SDUI.Controls.UIElementBase
    {
        public ListViewPage()
        {
            InitializeComponent();
            this.Text = "ListView";

            listView1.Items.Clear();

            var group1 = new SDUI.Controls.ListViewGroup("Group 1");
            var group2 = new SDUI.Controls.ListViewGroup("Group 2 (collapsible)");
            listView1.Groups.Add(group1);
            listView1.Groups.Add(group2);
            for (int i = 0; i <= 5; i++)
            {
                var title = "Item " + i.ToString();
                var listItem = new SDUI.Controls.ListViewItem(new[] { i.ToString(), title + " Column 2", title + " Column 3", title + " Column 4" }, group1);
                if (i == 0)
                {
                    listItem.BackColor = ControlPaint.Light(ColorScheme.BackColor, .15f);
                    listItem.Font = new Font(Font.FontFamily, 10.25f, FontStyle.Bold);
                }
                listView1.Items.Add(listItem);
            }

            for (int i = 6; i <= 1000; i++)
            {
                string sItem = "Item " + i.ToString();
                var item = new SDUI.Controls.ListViewItem(new[] { i.ToString(), sItem + " Column 2", sItem + " Column 3", sItem + " Column 4" });
                item.Group = group2;
                listView1.Items.Add(item);
            }

            //listView1.SetGroupInfo(listView1.Handle, 1, NativeMethods.LVGS_COLLAPSIBLE);
        }
    }
}
