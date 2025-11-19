using SDUI.Controls;
using System;
using System.Windows.Forms;

namespace SDUI.Demo
{
    public partial class TabControlTestPage : SDUI.Controls.UIElementBase
    {
        private Type[] _types = { typeof(GeneralPage), typeof(ListViewPage), typeof(ConfigPage) };
        public TabControlTestPage()
        {
            InitializeComponent();
            this.Text = "Multi Page Control";
            
            // Add initial pages
            AddInitialPages();
        }

        private void AddInitialPages()
        {
            // Page 1 - General
            var generalPage = new Controls.TabPage { Text = "General" };
            var generalContent = new GeneralPage { Dock = DockStyle.Fill };
            generalPage.Controls.Add(generalContent);
            multiPageControl.AddPage(generalPage);

            // Page 2 - ListView  
            var listViewPage = new Controls.TabPage { Text = "ListView" };
            var listViewContent = new ListViewPage { Dock = DockStyle.Fill };
            listViewPage.Controls.Add(listViewContent);
            multiPageControl.AddPage(listViewPage);

            // Page 3 - Config
            var configPage = new Controls.TabPage { Text = "Config" };
            var configContent = new ConfigPage { Dock = DockStyle.Fill };
            configPage.Controls.Add(configContent);
            multiPageControl.AddPage(configPage);
        }

        private void multiPageControl_NewPageButtonClicked(object sender, EventArgs e)
        {
            var item = new Controls.TabPage();

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
