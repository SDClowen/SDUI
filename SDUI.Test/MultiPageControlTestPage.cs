using System;
using System.Windows.Forms;

namespace SDUI.Test
{
    public partial class MultiPageControlTestPage : UserControl
    {
        public MultiPageControlTestPage()
        {
            InitializeComponent();
            this.Text = "Multi Page Control";
        }

        private void buttonAddTab_Click(object sender, EventArgs e)
        {
            multiPageControl.Add();
        }

        private void buttonRemoveTab_Click(object sender, EventArgs e)
        {
            multiPageControl.Remove();
        }

        private void buttonPrev_Click(object sender, EventArgs e)
        {
            multiPageControl.SelectedIndex--;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            multiPageControl.SelectedIndex++;
        }
    }
}
