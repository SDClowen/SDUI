using System.Runtime.InteropServices;
using static SDUI.Native.Windows.Methods;

namespace SDUI.Native.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct MONITORINFO
{
    public uint cbSize;
    public Rect rcMonitor;
    public Rect rcWork;
    public uint dwFlags;
}
