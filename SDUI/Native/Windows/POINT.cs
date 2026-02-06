using System.Runtime.InteropServices;

namespace SDUI.Native.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}
