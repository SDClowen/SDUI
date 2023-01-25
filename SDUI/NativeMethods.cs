using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace SDUI;

public class NativeMethods
{
    public const int DWMSBT_MAINWINDOW = 2; // Mica
    public const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic
    public const int DWMSBT_TABBEDWINDOW = 4; // Tabbed

    private const string user32 = "user32.dll";
    private const string uxtheme = "uxtheme.dll";
    private const string dwmapi = "dwmapi.dll";

    public const int WM_NCLBUTTONDOWN = 0xA1;
    public const int HT_CAPTION = 0x2;
    public const int CS_DROPSHADOW = 0x00020000;
    public const int WS_MINIMIZEBOX = 0x20000;
    public const int CS_DBLCLKS = 0x8;
    public const int WM_NCPAINT = 0x0085;
    public const int WM_NCHITTEST = 0x84;
    public const int WM_NCCALCSIZE = 0x0083;
    public const int WM_SYSCOMMAND = 0x0112;
    public const int SC_MOVE = 0xF010;
    public const int HTCAPTION = 0x2;
    public const int HTCLIENT = 0x1;
    public const int LVCDI_ITEM = 0x0;
    public const int LVCDI_GROUP = 0x1;
    public const int LVCDI_ITEMSLIST = 0x2;
    public const int LVM_FIRST = 0x1000;
    public const int LVM_SETITEMSTATE = LVM_FIRST + 43;
    public const int LVM_GETGROUPRECT = LVM_FIRST + 98;
    public const int LVM_ENABLEGROUPVIEW = LVM_FIRST + 157;
    public const int LVM_SETGROUPINFO = LVM_FIRST + 147;
    public const int LVM_GETGROUPINFO = LVM_FIRST + 149;
    public const int LVM_REMOVEGROUP = LVM_FIRST + 150;
    public const int LVM_MOVEGROUP = LVM_FIRST + 151;
    public const int LVM_GETGROUPCOUNT = LVM_FIRST + 152;
    public const int LVM_GETGROUPINFOBYINDEX = LVM_FIRST + 153;
    public const int LVM_MOVEITEMTOGROUP = LVM_FIRST + 154;
    public const int WM_LBUTTONUP = 0x202;

    public const int LVGF_NONE = 0x0;
    public const int LVGF_HEADER = 0x1;
    public const int LVGF_FOOTER = 0x2;
    public const int LVGF_STATE = 0x4;
    public const int LVGF_ALIGN = 0x8;
    public const int LVGF_GROUPID = 0x10;
    public const int LVGF_SUBTITLE = 0x100; // pszSubtitle is valid
    public const int LVGF_TASK = 0x200; // pszTask is valid
    public const int LVGF_DESCRIPTIONTOP = 0x400; // pszDescriptionTop is valid
    public const int LVGF_DESCRIPTIONBOTTOM = 0x800; // pszDescriptionBottom is valid
    public const int LVGF_TITLEIMAGE = 0x1000; // iTitleImage is valid
    public const int LVGF_EXTENDEDIMAGE = 0x2000; // iExtendedImage is valid
    public const int LVGF_ITEMS = 0x4000; // iFirstItem and cItems are valid
    public const int LVGF_SUBSET = 0x8000; // pszSubsetTitle is valid
    public const int LVGF_SUBSETITEMS = 0x10000; // readonly, cItems holds count of items in visible subset, iFirstItem is valid
    public const int LVGS_NORMAL = 0x0;
    public const int LVGS_COLLAPSED = 0x1;
    public const int LVGS_HIDDEN = 0x2;
    public const int LVGS_NOHEADER = 0x4;
    public const int LVGS_COLLAPSIBLE = 0x8;
    public const int LVGS_FOCUSED = 0x10;
    public const int LVGS_SELECTED = 0x20;
    public const int LVGS_SUBSETED = 0x40;
    public const int LVGS_SUBSETLINKFOCUSED = 0x80;
    public const int LVGA_HEADER_LEFT = 0x1;
    public const int LVGA_HEADER_CENTER = 0x2;
    public const int LVGA_HEADER_RIGHT = 0x4; // Don't forget to validate exclusivity
    public const int LVGA_FOOTER_LEFT = 0x8;
    public const int LVGA_FOOTER_CENTER = 0x10;
    public const int LVGA_FOOTER_RIGHT = 0x20; // Don't forget to validate exclusivity
    public const int LVGGR_GROUP = 0; // Entire expanded group
    public const int LVGGR_HEADER = 1;  // Header only (collapsed group)
    public const int LVGGR_LABEL = 2;  // Label only
    public const int LVGGR_SUBSETLINK = 3;  // subset link only
    public const int LVS_EX_DOUBLEBUFFER = 0x10000;
    public const int LVM_SETEXTENDEDLISTVIEWSTYLE = 4150;

    public const int NM_FIRST = 0;
    public const int NM_CLICK = NM_FIRST - 2;
    public const int NM_CUSTOMDRAW = NM_FIRST - 12;
    public const int WM_REFLECT = 0x2000;
    public const int WM_NOFITY = 0x4E;
    public const int WM_KILLFOCUS = 0x8;
    public const int WM_VSCROLL = 0x115;
    public const int WM_HSCROLL = 0x114;
    public const int WM_THEMECHANGED = 0x031A;

    public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    public const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

    public struct MARGINS                           // struct for box shadow
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [DllImport(user32)]
    public static extern bool ReleaseCapture();

    [DllImport(user32, SetLastError = true)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam); 

    [DllImport(user32, SetLastError = true)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam); 
    
    [DllImport("user32.dll")]
    public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);

    [DllImport(dwmapi)]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport(dwmapi)]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport(dwmapi)]
    public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

    [DllImport(uxtheme, ExactSpelling = true)]
    public extern static int DrawThemeParentBackground(IntPtr hWnd, IntPtr hdc, ref System.Drawing.Rectangle pRect);

    [DllImport(user32, EntryPoint = "SendMessageW", SetLastError = true)]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref LVGROUP lParam);

    [DllImport(user32, EntryPoint = "SendMessageW", SetLastError = true)]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref Rect lParam);

    [DllImport(user32, EntryPoint = "PostMessageW", SetLastError = true)]
    public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);

    [DllImport(uxtheme, CharSet = CharSet.Unicode, SetLastError = true)]
    public extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

    [DllImport(uxtheme, EntryPoint = "#133", SetLastError = true)]
    internal static extern bool AllowDarkModeForWindow(IntPtr window, bool isDarkModeAllowed);

    [DllImport(user32, EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);

    [DllImport(user32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, string lParam);

    [DllImport(user32, SetLastError = true)]
    public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [DllImport(user32, SetLastError = true)]
    public static extern IntPtr SetCursor(IntPtr hCursor);

    [DllImport(user32, SetLastError = true)]
    public static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport(user32, SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

    [DllImport(user32, EntryPoint = "SetWindowLong", SetLastError = true)]
    public static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport(user32, EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("Gdi32.dll", EntryPoint = "DeleteObject")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport(user32)]
    public static extern bool SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    [SecurityCritical]
    [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern NTSTATUS RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

    [DllImport("user32")]
    public static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32")]
    public static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

    private delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);
    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

    private static bool EnumWindow(IntPtr handle, IntPtr pointer)
    {
        GCHandle gch = GCHandle.FromIntPtr(pointer);
        List<IntPtr> list = gch.Target as List<IntPtr>;
        if (list == null)
            throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
        list.Add(handle);
        return true;
    }

    public static void DisableVisualStylesForFirstChild(IntPtr parent)
    {
        List<IntPtr> children = new List<IntPtr>();
        GCHandle listHandle = GCHandle.Alloc(children);
        try
        {
            EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
            EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            if (children.Count > 0)
                SetWindowTheme(children[0], "", "");
        }
        finally
        {
            if (listHandle.IsAllocated)
                listHandle.Free();
        }
    }

    public static void EnableAcrylic(IWin32Window window, Color blurColor)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));

        var accentPolicy = new AccentPolicy
        {
            AccentState = ACCENT.ENABLE_ACRYLICBLURBEHIND,
            GradientColor = blurColor.ToAbgr()
        };

        unsafe
        {
            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                Data = &accentPolicy,
                SizeOfData = Marshal.SizeOf<AccentPolicy>()
            };

            SetWindowCompositionAttribute(
                window.Handle,
                ref data);
        }
    }

    public enum ACCENT
    {
        DISABLED = 0,
        ENABLE_GRADIENT = 1,
        ENABLE_TRANSPARENTGRADIENT = 2,
        ENABLE_BLURBEHIND = 3,
        ENABLE_ACRYLICBLURBEHIND = 4,
        INVALID_STATE = 5
    }


    public struct AccentPolicy
    {
        public ACCENT AccentState;
        public uint AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    public static IntPtr GetHeaderControl(ListView list)
    {
        const int LVM_GETHEADER = 0x1000 + 31;
        return SendMessage(list.Handle, LVM_GETHEADER, 0, 0);
    }

    public enum NTSTATUS : uint
    {
        /// <summary>
        /// The operation completed successfully. 
        /// </summary>
        STATUS_SUCCESS = 0x00000000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct OSVERSIONINFOEX
    {
        // The OSVersionInfoSize field must be set to Marshal.SizeOf(typeof(OSVERSIONINFOEX))
        public int OSVersionInfoSize;
        public int MajorVersion;
        public int MinorVersion;
        public int BuildNumber;
        public int PlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string CSDVersion;
        public ushort ServicePackMajor;
        public ushort ServicePackMinor;
        public ushort SuiteMask;
        public byte ProductType;
        public byte Reserved;
    }

    public unsafe struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public void* Data;
        public int SizeOfData;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct LVITEM
    {
        public int mask;
        public int iItem;
        public int iSubItem;
        public int state;
        public int stateMask;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pszText;
        public int cchTextMax;
        public int iImage;
        public IntPtr lParam;
        public int iIndent;
        public int iGroupId;
        public int cColumns;
        public IntPtr puColumns;
    };

    [StructLayout(LayoutKind.Sequential)]
    public partial struct NMHDR
    {
        public IntPtr hwndFrom;
        public IntPtr idFrom;
        public int code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int X
        {
            get => Left;
            set
            {
                Right -= Left - value;
                Left = value;
            }
        }

        public int Y
        {
            get => Top;
            set
            {
                Bottom -= Top - value;
                Top = value;
            }
        }

        public int Height
        {
            get => Bottom - Top;
            set => Bottom = value + Top;
        }

        public int Width
        {
            get => Right - Left;
            set => Right = value + Left;
        }

    }

    public enum DWMWINDOWATTRIBUTE : int
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
    // what value of the enum to set.
    // Copied from dwmapi.h
    public enum DWM_WINDOW_CORNER_PREFERENCE : int
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct NMCUSTOMDRAW
    {
        public NMHDR hdr;
        public int dwDrawStage;
        public IntPtr hdc;
        public Rect rc;
        public IntPtr dwItemSpec;
        public uint uItemState;
        public IntPtr lItemlParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct NMLVCUSTOMDRAW
    {
        public NMCUSTOMDRAW nmcd;
        public int clrText;
        public int clrTextBk;
        public int iSubItem;
        public int dwItemType;
        public int clrFace;
        public int iIconEffect;
        public int iIconPhase;
        public int iPartId;
        public int iStateId;
        public Rect rcText;
        public uint uAlign;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public partial struct LVGROUP
    {
        public uint cbSize;
        public uint mask;
        public IntPtr pszHeader;
        public int cchHeader;
        public IntPtr pszFooter;
        public int cchFooter;
        public int iGroupId;
        public uint stateMask;
        public uint state;
        public uint uAlign;
        public IntPtr pszSubtitle;
        public uint cchSubtitle;
        public IntPtr pszTask;
        public uint cchTask;
        public IntPtr pszDescriptionTop;
        public uint cchDescriptionTop;
        public IntPtr pszDescriptionBottom;
        public uint cchDescriptionBottom;
        public int iTitleImage;
        public int iExtendedImage;
        public int iFirstItem;
        public uint cItems;
        public IntPtr pszSubsetTitle;
        public uint cchSubsetTitle;
    }

    [Flags]
    public enum CDRF
    {
        CDRF_DODEFAULT = 0x0,
        CDRF_NEWFONT = 0x2,
        CDRF_SKIPDEFAULT = 0x4,
        CDRF_DOERASE = 0x8,
        CDRF_SKIPPOSTPAINT = 0x100,
        CDRF_NOTIFYPOSTPAINT = 0x10,
        CDRF_NOTIFYITEMDRAW = 0x20,
        CDRF_NOTIFYSUBITEMDRAW = 0x20,
        CDRF_NOTIFYPOSTERASE = 0x40
    }

    [Flags]
    public enum CDDS
    {
        CDDS_PREPAINT = 0x1,
        CDDS_POSTPAINT = 0x2,
        CDDS_PREERASE = 0x3,
        CDDS_POSTERASE = 0x4,
        CDDS_ITEM = 0x10000,
        CDDS_ITEMPREPAINT = CDDS_ITEM | CDDS_PREPAINT,
        CDDS_ITEMPOSTPAINT = CDDS_ITEM | CDDS_POSTPAINT,
        CDDS_ITEMPREERASE = CDDS_ITEM | CDDS_PREERASE,
        CDDS_ITEMPOSTERASE = CDDS_ITEM | CDDS_POSTERASE,
        CDDS_SUBITEMField = 0x20000
    }

    public enum WindowCompositionAttribute
    {
        WCA_UNDEFINED = 0,
        WCA_NCRENDERING_ENABLED = 1,
        WCA_NCRENDERING_POLICY = 2,
        WCA_TRANSITIONS_FORCEDISABLED = 3,
        WCA_ALLOW_NCPAINT = 4,
        WCA_CAPTION_BUTTON_BOUNDS = 5,
        WCA_NONCLIENT_RTL_LAYOUT = 6,
        WCA_FORCE_ICONIC_REPRESENTATION = 7,
        WCA_EXTENDED_FRAME_BOUNDS = 8,
        WCA_HAS_ICONIC_BITMAP = 9,
        WCA_THEME_ATTRIBUTES = 10,
        WCA_NCRENDERING_EXILED = 11,
        WCA_NCADORNMENTINFO = 12,
        WCA_EXCLUDED_FROM_LIVEPREVIEW = 13,
        WCA_VIDEO_OVERLAY_ACTIVE = 14,
        WCA_FORCE_ACTIVEWINDOW_APPEARANCE = 15,
        WCA_DISALLOW_PEEK = 16,
        WCA_CLOAK = 17,
        WCA_CLOAKED = 18,
        WCA_ACCENT_POLICY = 19,
        WCA_FREEZE_REPRESENTATION = 20,
        WCA_EVER_UNCLOAKED = 21,
        WCA_VISUAL_OWNER = 22,
        WCA_HOLOGRAPHIC = 23,
        WCA_EXCLUDED_FROM_DDA = 24,
        WCA_PASSIVEUPDATEMODE = 25,
        WCA_USEDARKMODECOLORS = 26,
        WCA_LAST = 27
    };
}