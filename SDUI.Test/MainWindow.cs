using SDUI.Controls;
using System;

namespace SDUI.Demo;

public partial class MainWindow : UIWindow
{
    public MainWindow()
    {
        InitializeComponent();
        RenderBackend = Rendering.RenderBackend.OpenGL;
        ShowPerfOverlay = true;
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (Controls.Count == 0)
            return;
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
        windowPageControl.Controls.AddRange([
            new ModernControlsPage() {Dock = System.Windows.Forms.DockStyle.Fill },
            new GeneralPage() {Dock = System.Windows.Forms.DockStyle.Fill },
            new ListViewPage() {Dock = System.Windows.Forms.DockStyle.Fill },
            new ConfigPage() {Dock = System.Windows.Forms.DockStyle.Fill },
            new TabControlTestPage() {Dock = System.Windows.Forms.DockStyle.Fill }
        ]);
    }
}