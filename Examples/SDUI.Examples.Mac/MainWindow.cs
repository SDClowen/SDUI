using SDUI.Controls;

namespace SDUI.Examples.Mac
{
    public partial class MainWindow : UIWindow
    {
        public MainWindow()
        {
            ColorScheme.BackColor = Color.FromArgb(20, 20, 20);
            InitializeComponent();
            AddMousePressMove(this);
            AddMousePressMove(panel1);
        }
    }
}
