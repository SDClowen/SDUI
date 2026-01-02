using SDUI.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Demo
{
    public partial class GeneralPage : SDUI.Controls.UIElementBase
    {
        private static readonly Random _rng = new();

        private static async Task ApplyThemeAndSyncWindowAsync(UIWindow parent, Action startTransition, int durationMs = 260)
        {
            void SyncWindowBackground(object? _, EventArgs __)
            {
                if (parent.IsDisposed) return;
                parent.BackColor = ColorScheme.Surface;
                parent.Invalidate();
            }

            ColorScheme.ThemeChanged += SyncWindowBackground;
            try
            {
                startTransition();
                SyncWindowBackground(null, EventArgs.Empty);
                await Task.Delay(durationMs);
            }
            finally
            {
                ColorScheme.ThemeChanged -= SyncWindowBackground;
            }
        }

        public GeneralPage()
        {
            InitializeComponent();
            this.Text = "General";
            comboBox6.Items.AddRange(Enum.GetNames<HatchStyle>());

            // Demo: programmatically select text in the RichTextBox and set SelectionBackColor
            richTextBox1.Text = "This is a demo of RichTextBox selection and SelectionBackColor.";
            richTextBox1.Select(5, 10);
            richTextBox1.SelectionBackColor = Color.LightBlue;
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

        private async void buttonRandomColor_Click(object sender, EventArgs e)
        {
            var r = _rng.Next(0, 256);
            var g = _rng.Next(0, 256);
            var b = _rng.Next(0, 256);

            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;
            if (parent != null)
            {
                var targetBackground = Color.FromArgb(r, g, b);

                // Random background-driven theme (animated + auto text/derived colors)
                await ApplyThemeAndSyncWindowAsync(parent, () => ColorScheme.StartThemeTransition(targetBackground));
            }
        }

        private async void buttonDark_Click(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            if (form is UIWindow parent)
            {
                // Smooth switch to dark mode from any current background
                await ApplyThemeAndSyncWindowAsync(parent, () => ColorScheme.IsDarkMode = true);
            }
        }

        private async void buttonLight_Click(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            if (form is UIWindow parent)
            {
                // Smooth switch to light mode from any current background
                await ApplyThemeAndSyncWindowAsync(parent, () => ColorScheme.IsDarkMode = false);
            }
        }
    }
}
