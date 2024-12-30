using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using static SDUI.NativeMethods;

namespace SDUI.Controls.Subclasses
{
    class ListViewHeaderSubclassedWindow : MarshalByRefObject, System.Windows.Forms.IWin32Window
    {
        // prevents collection of ListViewHeaderSubclassedWindow that is still in use
        static private HashSet<ListViewHeaderSubclassedWindow> _instancesInUse = new();

        // The number of uses we still have for this instances:
        // - some window attached, or
        // - inside a window procedure
        private int _uses = 0;

        // Our window procedure delegate
        private SUBCLASSPROC _windowProc;

        // The native handle for our delegate
        private IntPtr _windowProcHandle;

        static ListViewHeaderSubclassedWindow()
        {
            AppDomain.CurrentDomain.ProcessExit += OnShutdown;
        }

        public ListViewHeaderSubclassedWindow()
        {
            _windowProc = new SUBCLASSPROC(Callback);
            _windowProcHandle = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(_windowProc);
        }

        /// <summary>
        ///  Gets the handle for this window.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        ///  Assigns a handle to this <see cref="NativeWindow"/> instance.
        /// </summary>
        public void AssignHandle(IntPtr handle)
        {
            CheckReleased();
            Debug.Assert(handle != IntPtr.Zero, "handle is 0");

            if (0 == _uses)
            {
                lock (_instancesInUse)
                {
                    _instancesInUse.Add(this);
                }
            } // else may happen if handle gets reassigned inside WndProc.
            // This is legal after any call to DefWndProc.

            ++_uses;
            Handle = handle;


            IntPtr hHeader = SendMessage(handle, LVM_GETHEADER, 0, 0);

            var isDark = ColorScheme.BackColor.IsDark();
            SetWindowTheme(hHeader, isDark ? "DarkMode_ItemsView" : "ItemsView", null); // DarkMode
            SetWindowTheme(handle, isDark ? "DarkMode_ItemsView" : "ItemsView", null); // DarkMode
            SetWindowSubclass(handle, _windowProcHandle, UIntPtr.Zero, UIntPtr.Zero);

            OnHandleChange();
        }

        /// <summary>
        ///  Window message callback method. Control arrives here when a window
        ///  message is sent to this Window. This method packages the window message
        ///  in a Message object and invokes the WndProc() method. A WM_NCDESTROY
        ///  message automatically causes the ReleaseHandle() method to be called.
        /// </summary>
        private IntPtr Callback(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr uIdSubclass,
            UIntPtr dwRefData
        )
        {
            Debug.Assert(0 < _uses);
            ++_uses;

            try
            {
                var m = System.Windows.Forms.Message.Create(hWnd, msg, wParam, lParam);
                switch (m.Msg)
                {
                    case WM_NOTIFY:

                        var pnmhdr = (NMHDR)m.GetLParam(typeof(NMHDR));

                        if (pnmhdr.code == NM_CUSTOMDRAW)
                        {
                            var nmcd = (NMCUSTOMDRAW)m.GetLParam(typeof(NMCUSTOMDRAW));

                            switch (nmcd.dwDrawStage)
                            {
                                case (int)CDDS.CDDS_PREPAINT:
                                    m.Result = new IntPtr((int)CDRF.CDRF_NOTIFYITEMDRAW);
                                    break;
                                case (int)CDDS.CDDS_ITEMPREPAINT:

                                    var info = (SubclassInfo)Marshal.PtrToStructure(unchecked((IntPtr)(long)(ulong)dwRefData), typeof(SubclassInfo));
                                    SetTextColor(nmcd.hdc, info.headerTextColor);

                                    m.Result = new IntPtr((int)CDRF.CDRF_DODEFAULT);

                                    break;
                                default:
                                    m.Result = new IntPtr((int)CDRF.CDRF_NOTIFYPOSTPAINT);
                                    break;
                            }
                        }
                        break;
                    case WM_THEMECHANGED:

                        IntPtr hHeader = SendMessage(Handle, LVM_GETHEADER, 0, 0);
                        var isDark = ColorScheme.BackColor.IsDark();

                        //SetWindowTheme(hHeader, isDark ? "DarkMode_ItemsView" : "ItemsView", null); // DarkMode
                        //SetWindowTheme(Handle, isDark ? "DarkMode_ItemsView" : "ItemsView", null); // DarkMode


                        //AllowDarkModeForWindow(Handle, ColorScheme.BackColor.IsDark());
                        //AllowDarkModeForWindow(hHeader, ColorScheme.BackColor.IsDark());

                        var hTheme = OpenThemeData(IntPtr.Zero, isDark ? "DarkMode_ItemsView" : "ItemsView");

                        const int HP_HEADERITEM = 1;
                        const int TMT_FILLCOLOR = 3802;
                        const int TMT_TEXTCOLOR = 3803;

                        if (hTheme != IntPtr.Zero)
                        {
                            COLORREF color;
                            if (isDark)
                                color.R = color.G = color.B = 255;
                            else
                                color.R = color.G = color.B = 0;

                            //if (GetThemeColor(hTheme, 0, 0, TMT_TEXTCOLOR, out color) > 0)
                            {
                                SendMessage(Handle, LVM_FIRST + 36, IntPtr.Zero, ref color);
                            }

                            //if (GetThemeColor(hTheme, 0, 0, TMT_FILLCOLOR, out color) > 0)
                            {
                                SendMessage(Handle, LVM_FIRST + 38, IntPtr.Zero, ref color);
                                SendMessage(Handle, LVM_FIRST + 1, IntPtr.Zero, ref color);
                            }
                            CloseThemeData(hTheme);
                        }

                        hTheme = OpenThemeData(hHeader, "Header");
                        if (hTheme != IntPtr.Zero)
                        {
                            SubclassInfo info;
                            //var info = (SubclassInfo)Marshal.PtrToStructure(unchecked((IntPtr)(long)(ulong)dwRefData), typeof(SubclassInfo));

                            GetThemeColor(hTheme, HP_HEADERITEM, 0, TMT_TEXTCOLOR, out info.headerTextColor);
                            CloseThemeData(hTheme);
                        }

                        SendMessage(hHeader, WM_THEMECHANGED, m.WParam, m.LParam);

                        RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, 0x0400 | 0x0001);

                        break;
                    default:
                        break;
                }

                m.Result = DefSubclassProc(Handle, m.Msg, m.WParam, m.LParam);
                return m.Result;
            }
            catch (Exception e)
            {
                OnThreadException(e);
                return IntPtr.Zero;
            }
            finally
            {
                if (msg == 0x82/*WM_NCDESTROY*/ && Handle != IntPtr.Zero)
                {
                    InternalReleaseHandle();
                }
                if (0 == --_uses)
                {
                    lock (_instancesInUse)
                    {
                        _instancesInUse.Remove(this);
                    }
                }
            }
        }

        /// <summary>
        ///  Raises an exception if the window handle is not zero.
        /// </summary>
        private void CheckReleased()
        {
            if (Handle != IntPtr.Zero)
            {
                throw new InvalidOperationException("Window handle already exists.");
            }
        }

        /// <summary>
        ///  Invokes the default window procedure associated with this Window. It is
        ///  an error to call this method when the Handle property is zero.
        /// </summary>
        public void DefWndProc(ref System.Windows.Forms.Message m)
        {
            Debug.Assert(m.HWnd == Handle, "ListViewHeaderSubclassedWindow is not attached to the window m is addressed to.");


        }

        /// <summary>
        ///  Specifies a notification method that is called when the handle for a
        ///  window is changed.
        /// </summary>
        protected virtual void OnHandleChange()
        {
        }

        /// <summary>
        ///  On class load, we connect an event to Application to let us know when
        ///  the process or domain terminates.  When this happens, we attempt to
        ///  clear our window class cache.  We cannot destroy windows (because we don't
        ///  have access to their thread), and we cannot unregister window classes
        ///  (because the classes are in use by the windows we can't destroy).  Instead,
        ///  we move the class and window procs to DefWndProc
        /// </summary>
        [System.Runtime.ConstrainedExecution.PrePrepareMethod]
        private static void OnShutdown(object sender, EventArgs e)
        {
            // No lock because access here should be race-free, no concurrent ListViewHeaderSubclassedWindow.AttachHandle/ReleaseHandle
            // should happen while shutting down.
            //Debug.Assert(0 == _instancesInUse.Count);
        }

        /// <summary>
        ///  When overridden in a derived class, manages an unhandled thread exception.
        /// </summary>
        protected virtual void OnThreadException(Exception e)
        {
        }

        private void InternalReleaseHandle()
        {
            Debug.Assert(Handle != IntPtr.Zero);
            RemoveWindowSubclass(Handle, _windowProcHandle, UIntPtr.Zero);
            Handle = IntPtr.Zero;
            OnHandleChange();
            --_uses;
        }

        /// <summary>
        ///  Releases the handle associated with this window.
        /// </summary>
        public void ReleaseHandle()
        {
            if (Handle != IntPtr.Zero)
            {
                InternalReleaseHandle();
                if (0 == _uses)
                {
                    lock (_instancesInUse)
                    {
                        _instancesInUse.Remove(this);
                    }
                }
            }
        }

        /// <summary>
        ///  Invokes the default window procedure associated with this window.
        /// </summary>
        protected virtual void WndProc(ref System.Windows.Forms.Message m)
        {
            DefWndProc(ref m);
        }
    }
}
