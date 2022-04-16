using SDUI.Controls;

namespace SDUI.Test
{
    public partial class MainWindow : CleanForm
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

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

            ColorScheme.BackColor = Color.FromArgb(r, g, b);
            ChangeTheme();
        }
    }
}