using SDUI.Controls;
using System;
using System.Windows.Forms;

namespace SDUI.Test;

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

        Application.Run(new MainWindow());
    }
}
