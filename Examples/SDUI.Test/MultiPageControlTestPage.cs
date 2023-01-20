using System;
using System.Windows.Forms;

namespace SDUI.Test
{
    public partial class MultiPageControlTestPage : UserControl
    {
        private Control[] _list = { new GeneralPage(), new ListViewPage(), new ConfigPage() };
        public MultiPageControlTestPage()
        {
            InitializeComponent();
            this.Text = "Multi Page Control";
        }

        private void multiPageControl_NewPageButtonClicked(object sender, EventArgs e)
        {
            var item = multiPageControl.Add();

            var control = _list[new Random().Next(0, _list.Length)];
            control.Dock = DockStyle.Fill;
            item.Controls.Add(control);
        }
    }
}
