using SDUI.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Test
{
    public partial class GeneralPage : UserControl
    {
        public GeneralPage()
        {
            InitializeComponent();
            this.Text = "General";
            comboBox6.Items.AddRange(Enum.GetNames<HatchStyle>());

            toolTip1.SetToolTip(progressBar8, "Test tooltip");
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            panel2.ShadowDepth = trackBar1.Value;
            progressBar6.Maximum = trackBar1.Maximum;
            progressBar6.Value = trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            panel2.Radius = trackBar2.Value;
            progressBar7.Maximum = trackBar2.Maximum;
            progressBar7.Value = trackBar2.Value;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            ColorScheme.DrawDebugBorders = checkBox6.Checked;
            var backup = BackColor;
            BackColor = BackColor.Determine();
            BackColor = backup;
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Enum.TryParse<HatchStyle>(comboBox6.SelectedItem.ToString(), out var value))
                return;

            progressBar1.HatchType = value;
            progressBar1.Invalidate();
            progressBar2.HatchType = value;
            progressBar2.Invalidate();
            progressBar3.HatchType = value;
            progressBar3.Invalidate();
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            await Task.Delay(5000);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Testing";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label4.Text = textBox1.Text;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            MessageBox.Show(textBox1.Text);
        }


        private void buttonOpenInputDialog_Click(object sender, EventArgs e)
        {
            var dialog = new InputDialog("The input dialog", "This is a input dialog", "Please set the value!");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"The value: {dialog.Value}");
            }
        }

        private void buttonRandomColor_Click(object sender, EventArgs e)
        {
            var random = new Random();
            var r = random.Next(0, 256);
            var g = random.Next(0, 256);
            var b = random.Next(0, 256);

            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;
            ColorScheme.BackColor = Color.FromArgb(r, g, b);
            parent.BackColor = ColorScheme.BackColor;
        }

        private void buttonDark_Click(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;
            ColorScheme.BackColor = Color.Black;
            parent.BackColor = ColorScheme.BackColor;
        }

        private void buttonLight_Click(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;
            ColorScheme.BackColor = Color.White;
            parent.BackColor = ColorScheme.BackColor;
        }
    }
}
