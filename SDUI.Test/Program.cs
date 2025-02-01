using SDUI.Controls;
using System;
using System.Windows.Forms;

namespace SDUI.Demo;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetHighDpiMode(HighDpiMode.PerMonitor | HighDpiMode.PerMonitorV2);
        Application.SetCompatibleTextRenderingDefault(false);

        Application.SetDefaultFont(SDUI.Helpers.FontManager.Inter);

        var newForm = new UIWindow() { Width = 800, Height = 400 };
        newForm.Text = "SDUI Demo";
        var panel = new Controls.Panel() { Width = 500, Height = 500, ShadowDepth = 1, Border = new(1), Dock = DockStyle.Fill, Name = "panel1" };
        var button = new Controls.Button() { Margin = new(10), Padding = new(15), Text = "Hello World Top", ShadowDepth = 4, Dock = DockStyle.Top, Name = "button1", Width = 120, Height = 32 };
        button.Click += (sender, e) => MessageBox.Show("Hello World");

        newForm.Controls.Add(panel);
        newForm.Controls.Add(button);
        newForm.Controls.Add(new Controls.Button() { Padding = new(15), Text = "Hello World Fill", ShadowDepth = 4, Dock = DockStyle.Fill, Name = "button3", Width = 120, Height = 32 });
        newForm.Controls.Add(new Controls.Button() { Padding = new(15), Text = "Hello World Bottom", ShadowDepth = 4, Dock = DockStyle.Bottom, Name = "button2", Width = 120, Height = 32 });
        newForm.Controls.Add(new Controls.Button() { Padding = new(15), Text = "Hello World Right", ShadowDepth = 4, Dock = DockStyle.Right, Name = "button4", Width = 120, Height = 32 });
        newForm.Controls.Add(new Controls.Button() { Padding = new(15), Text = "Hello World Left", ShadowDepth = 4, Dock = DockStyle.Left, Name = "button3", Width = 120, Height = 32 });

        Application.Run(newForm);
        return;
        Application.Run(new MainWindow());
    }
}
