using SDUI.Controls;
using System;
using System.Drawing;

namespace SDUI.Test
{
    public partial class FluentForm : UIWindow
    {
        public FluentForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ColorScheme.BackColor = Color.Black;
            BackColor = ColorScheme.BackColor;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ColorScheme.BackColor = Color.White;
            BackColor = ColorScheme.BackColor;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ColorScheme.BackColor = Color.Transparent;
            BackColor = ColorScheme.BackColor;
        }
    }
}
