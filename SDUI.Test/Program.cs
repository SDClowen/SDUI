using SDUI.Controls;
using System;
using System.Windows.Forms;

namespace SDUI.Demo;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Console.WriteLine("Program.Main: start");
        // Temporarily disable WinForms convenience helpers to avoid potential native init crashes in CI environments.
        // Application.EnableVisualStyles();
        // Application.SetHighDpiMode(HighDpiMode.PerMonitor | HighDpiMode.PerMonitorV2);
        // Application.SetCompatibleTextRenderingDefault(false);
        Console.WriteLine("Program.Main: about to run MainWindow");
        Application.Run(new MainWindow());
        Console.WriteLine("Program.Main: exit");
    }
}
