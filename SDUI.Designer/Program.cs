using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SDUI.Designer;

static class Program
{
    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();

    [STAThread]
    static void Main()
    {
        AllocConsole(); // Create console for debugging
        Console.WriteLine("SDUI Designer Starting...");
        
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        
        Console.WriteLine("Running application...");
        Application.Run(new DesignerMainWindow());
    }
}
