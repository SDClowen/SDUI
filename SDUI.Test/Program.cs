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
        var panel = new Controls.Panel() { Width = 500, Height = 500, ShadowDepth = 1, Border = new(1), Dock = DockStyle.Fill, Name = "panel1" };
        var button = new Controls.Button() { Padding = new(15), Text = "Hello World", ShadowDepth = 4, Dock = DockStyle.Bottom, Anchor = AnchorStyles.Bottom, Name = "button1", Width = 120, Height = 23 };
        button.Click += (sender, e) => MessageBox.Show("Hello World");

        panel.Controls.Add(button);

        newForm.Controls.Add(button);

        Application.Run(newForm);

        Application.Run(new MainWindow());
    }
}
