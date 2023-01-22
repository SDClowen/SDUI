using System;
using System.Windows.Forms;

namespace SDUI.Test
{
    public partial class MultiPageControlTestPage : UserControl
    {
        private Type[] _types = { typeof(GeneralPage), typeof(ListViewPage), typeof(ConfigPage) };
        public MultiPageControlTestPage()
        {
            InitializeComponent();
            this.Text = "Multi Page Control";
        }

        private void multiPageControl_NewPageButtonClicked(object sender, EventArgs e)
        {
            var item = multiPageControl.Add();

            var type = _types[new Random().Next(0, _types.Length)];
            var control = Activator.CreateInstance(type) as Control;
            control.Dock = DockStyle.Fill;
            control.BackColor = ColorScheme.BackColor;
            item.Controls.Add(control);
        }
    }
}
