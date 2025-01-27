using SDUI.Controls;

namespace SDUI.Skia;

public partial class MainWindow : SKForm
{
    public MainWindow()
    {
        InitializeComponent();

        var windowPageController = new WindowPageControl();
        windowPageController.Controls.Add(new SDUI.Controls.Label() { Text = "Deneme 1", Dock = DockStyle.Fill });
        windowPageController.Controls.Add(new SDUI.Controls.Label() { Text = "Deneme 2", Dock = DockStyle.Fill });
        windowPageController.Controls.Add(new SDUI.Controls.Label() { Text = "Deneme 3", Dock = DockStyle.Fill });

        WindowPageControl = windowPageController;
    }
}
