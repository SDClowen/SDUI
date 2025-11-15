using SDUI.Controls;
using System;
using System.Windows.Forms;

namespace SDUI.Demo
{
    public partial class MultiPageControlTestPage : SDUI.Controls.Panel
    {
        private Type[] _types = { typeof(GeneralPage), typeof(ListViewPage), typeof(ConfigPage) };
        public MultiPageControlTestPage()
        {
            InitializeComponent();
            this.Text = "Multi Page Control";
        }

        private void multiPageControl_NewPageButtonClicked(object sender, EventArgs e)
        {
            var item = new Page();

            var type = _types[new Random().Next(0, _types.Length)];
            var control = Activator.CreateInstance(type) as UIElementBase;
            control.Dock = DockStyle.Fill;
            control.BackColor = ColorScheme.BackColor;
            item.Text = control.Text;
            item.Controls.Add(control);
            multiPageControl.AddPage(item);
        }
    }
}
