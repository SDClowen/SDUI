using System;
using System.Runtime.InteropServices;

namespace SDUI.Helpers
{
    public static class SystemAnimations
    {
        private const uint SPI_GETANIMATION = 0x0048;

        [StructLayout(LayoutKind.Sequential)]
        public struct ANIMATIONINFO
        {
            public uint cbSize;
            public int iMinAnimate;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            ref ANIMATIONINFO pvParam,
            uint fWinIni
        );

        private static readonly bool _areAnimationsEnabled = GetAreAnimationsEnabled();

        public static bool AreAnimationsEnabled => _areAnimationsEnabled;

        private static bool GetAreAnimationsEnabled()
        {
            try
            {
                ANIMATIONINFO ai = new ANIMATIONINFO();
                ai.cbSize = (uint)Marshal.SizeOf(ai);

                if (SystemParametersInfo(SPI_GETANIMATION, ai.cbSize, ref ai, 0))
                {
                    return ai.iMinAnimate != 0;
                }

                return true; // Default to true if the call fails
            }
            catch
            {
                return true; // Default to true in case of an exception
            }
        }
    }
}
