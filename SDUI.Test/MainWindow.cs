using SDUI.Controls;
using System;

namespace SDUI.Test;

public partial class MainWindow : UIWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (Controls.Count == 0)
            return;

    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
        windowPageControl.Controls.AddRange(new System.Windows.Forms.Control[] {
            new GeneralPage(),
            new ListViewPage(),
            new ConfigPage(),
            new MultiPageControlTestPage()
        });
    }
}