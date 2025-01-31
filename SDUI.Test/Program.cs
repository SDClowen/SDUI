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

        //Application.SetDefaultFont(SDUI.Helpers.FontManager.Inter);

        var newForm = new UIWindow();
        newForm.Text = "SDUI Demo";
        var panel = new Controls.Panel() { Padding = new(0, 40, 0, 0), Width = 500, Height = 500, ShadowDepth = 32, Border = new(2), Dock = DockStyle.Fill, Name = "panel1" };
        var button = new Controls.Button() { Text = "Hello World", Dock = DockStyle.Fill, Name = "button1", Width = 120, Height = 23 };
        panel.Controls.Add(button);

        newForm.Controls.Add(panel);

        //Application.Run(newForm);

        Application.Run(new MainWindow());
    }
}
