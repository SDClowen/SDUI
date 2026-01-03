using SDUI.Controls;
using System;

namespace SDUI.Demo;

public partial class MainWindow : UIWindow
{
    public MainWindow()
    {
        Console.WriteLine("MainWindow(): enter");
        InitializeComponent();
        Console.WriteLine("MainWindow(): after InitializeComponent");
        // Try OpenGL first for better performance, fallback to Software if it fails
        try
        {
            RenderBackend = Rendering.RenderBackend.OpenGL;
            Console.WriteLine($"MainWindow(): RenderBackend={RenderBackend}");
        }
        catch
        {
            RenderBackend = Rendering.RenderBackend.Software;
            Console.WriteLine($"MainWindow(): RenderBackend=Software (fallback)");
        }
        ShowPerfOverlay = true;
        Console.WriteLine("MainWindow(): exit");
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
            new TreeViewPage() {Dock = System.Windows.Forms.DockStyle.Fill },
            new ConfigPage() {Dock = System.Windows.Forms.DockStyle.Fill },
            new TabControlTestPage() {Dock = System.Windows.Forms.DockStyle.Fill },
            new SplitContainerPage() {Dock = System.Windows.Forms.DockStyle.Fill },
            new LayoutCompatibilityPage() { Dock = System.Windows.Forms.DockStyle.Fill }
        ]);
    }

    private void toolbarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        // Toggle toolbar visibility (currently no toolbar, just a demo)
        statusStripLabel1.Text = toolbarToolStripMenuItem.Checked ? "Toolbar: Visible" : "Toolbar: Hidden";
    }

    private void statusBarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        // Toggle status bar visibility
        statusStrip1.Visible = statusBarToolStripMenuItem.Checked;
    }

    private void wordWrapToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        // Demo of indeterminate state toggling
        string state = wordWrapToolStripMenuItem.CheckState switch
        {
            SDUI.Enums.CheckState.Checked => "ON",
            SDUI.Enums.CheckState.Unchecked => "OFF",
            SDUI.Enums.CheckState.Indeterminate => "Auto",
            _ => "Unknown"
        };
        statusStripLabel1.Text = $"Word Wrap: {state}";
    }
}