using System;
using System.ComponentModel;
using System.Windows.Forms;
using SDUI.Controls;

namespace SDUI.Test
{
    [ToolboxItem(false)]
    public partial class MultiPageControlTestPage : DoubleBufferedControl
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
            item.Text = control.Text;
            item.Controls.Add(control);
        }
    }
}
