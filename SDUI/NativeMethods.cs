using System;
using System.Runtime.InteropServices;

namespace SDUI
{
    public class NativeMethods
    {
        private const string user32 = "user32.dll";
        private const string uxtheme = "uxtheme.dll";
        private const string dwmapi = "dwmapi.dll";

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int CS_DROPSHADOW = 0x00020000;
        public const int WM_NCPAINT = 0x0085;
        public const int WM_NCHITTEST = 0x84;
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

        public struct MARGINS                           // struct for box shadow
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        [DllImport(user32)]
        public static extern bool ReleaseCapture();

        [DllImport(user32, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

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
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref RECT lParam);

        [DllImport(user32, EntryPoint = "PostMessageW", SetLastError = true)]
        public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);

        [DllImport(uxtheme, CharSet = CharSet.Unicode, SetLastError = true)]
        public extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

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
        public partial struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public partial struct NMCUSTOMDRAW
        {
            public NMHDR hdr;
            public int dwDrawStage;
            public IntPtr hdc;
            public RECT rc;
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
            public RECT rcText;
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
            CDRF_DODEFAULTField = 0x0,
            CDRF_NEWFONTField = 0x2,
            CDRF_SKIPDEFAULTField = 0x4,
            CDRF_DOERASEField = 0x8,
            CDRF_SKIPPOSTPAINTField = 0x100,
            CDRF_NOTIFYPOSTPAINTField = 0x10,
            CDRF_NOTIFYITEMDRAWField = 0x20,
            CDRF_NOTIFYSUBITEMDRAWField = 0x20,
            CDRF_NOTIFYPOSTERASEField = 0x40
        }

        [Flags]
        public enum CDDS
        {
            CDDS_PREPAINTField = 0x1,
            CDDS_POSTPAINTField = 0x2,
            CDDS_PREERASEField = 0x3,
            CDDS_POSTERASEField = 0x4,
            CDDS_ITEMField = 0x10000,
            CDDS_ITEMPREPAINTField = CDDS_ITEMField | CDDS_PREPAINTField,
            CDDS_ITEMPOSTPAINTField = CDDS_ITEMField | CDDS_POSTPAINTField,
            CDDS_ITEMPREERASEField = CDDS_ITEMField | CDDS_PREERASEField,
            CDDS_ITEMPOSTERASEField = CDDS_ITEMField | CDDS_POSTERASEField,
            CDDS_SUBITEMField = 0x20000
        }
    }
}
