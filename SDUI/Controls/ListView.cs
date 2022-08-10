using SDUI.Helpers;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Controls
{
    public class ListView : System.Windows.Forms.ListView
    {
        /// <summary>
        /// The column sorter
        /// </summary>
        private ListViewColumnSorter LvwColumnSorter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AeroListView"/> class.
        /// </summary>
        public ListView()
            : base()
        {
            SetStyle(
                ControlStyles.Opaque |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.EnableNotifyMessage, true);

            LvwColumnSorter = new ListViewColumnSorter();
            ListViewItemSorter = LvwColumnSorter;
            View = View.Details;
            FullRowSelect = true;
            UpdateStyles();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:HandleCreated" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetWindowTheme(Handle, "explorer", null);

                SendMessage(Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
            }
        }

        protected override void OnNotifyMessage(Message m)
        {
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:ColumnClick" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ColumnClickEventArgs"/> instance containing the event data.</param>
        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            base.OnColumnClick(e);

            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == LvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                LvwColumnSorter.Order = (LvwColumnSorter.Order == SortOrder.Ascending)
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                LvwColumnSorter.SortColumn = e.Column;
                LvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            if (!VirtualMode)
                Sort();
        }

        /// <summary>
        /// Select all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be selected</param>
        public void SelectAllItems()
        {
            var s = System.Diagnostics.Stopwatch.StartNew();
            Focus();
            SetItemState(-1, 2, 2);
            //MessageBox.Show($"Selected in: {s.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Deselect all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be deselected</param>
        public void DeselectAllItems()
        {
            SetItemState(-1, 2, 0);
        }

        /// <summary>
        /// Set the item state on the given item
        /// </summary>
        /// <param name="list">The listview whose item's state is to be changed</param>
        /// <param name="itemIndex">The index of the item to be changed</param>
        /// <param name="mask">Which bits of the value are to be set?</param>
        /// <param name="value">The value to be set</param>
        public void SetItemState(int itemIndex, int mask, int value)
        {
            LVITEM lvItem = new LVITEM();
            lvItem.stateMask = mask;
            lvItem.state = value;
            SendMessageLVItem(Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);

            EnsureVisible(itemIndex);
        }

        public int SetGroupInfo(IntPtr hWnd, int nGroupID, uint nSate)
        {
            var lvg = new LVGROUP();
            lvg.cbSize = (uint)Marshal.SizeOf(lvg);
            lvg.mask = LVGF_STATE | LVGF_GROUPID | LVGF_HEADER;
            // for test
            SendMessage(hWnd, LVM_GETGROUPINFO, nGroupID, ref lvg);
            lvg.state = nSate;
            lvg.mask = LVGF_STATE;
            SendMessage(hWnd, LVM_SETGROUPINFO, nGroupID, ref lvg);
            return -1;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_REFLECT + WM_NOFITY)
            {
                var pnmhdr = (NMHDR)m.GetLParam(typeof(NMHDR));
                if (pnmhdr.code == NM_CUSTOMDRAW)
                {
                    var pnmlv = (NMLVCUSTOMDRAW)m.GetLParam(typeof(NMLVCUSTOMDRAW));
                    switch ((CDDS)pnmlv.nmcd.dwDrawStage)
                    {
                        case CDDS.CDDS_PREPAINT:
                            if (pnmlv.dwItemType == LVCDI_GROUP)
                            {
                                var rectHeader = new RECT();
                                rectHeader.top = LVGGR_HEADER;
                                var nItem = (int)pnmlv.nmcd.dwItemSpec;

                                SendMessage(m.HWnd, LVM_GETGROUPRECT, nItem, ref rectHeader);

                                using (var graphics = Graphics.FromHdc(pnmlv.nmcd.hdc))
                                {
                                    var rect = new Rectangle(rectHeader.left, rectHeader.top, rectHeader.right - rectHeader.left, rectHeader.bottom - rectHeader.top);

                                    //var backgroundBrush = new SolidBrush(_groupHeadingBackColor);
                                    //graphics.FillRectangle(backgroundBrush, rect);

                                    var lvg = new LVGROUP();
                                    lvg.cbSize = (uint)Marshal.SizeOf(lvg);
                                    lvg.mask = LVGF_STATE | LVGF_GROUPID | LVGF_HEADER;

                                    SendMessage(m.HWnd, LVM_GETGROUPINFO, nItem, ref lvg);
                                    var sText = Marshal.PtrToStringUni(lvg.pszHeader);
                                    var textSize = graphics.MeasureString(sText, Font);

                                    var rectHeightMiddle = (int)Math.Round((rect.Height - textSize.Height) / 2f);

                                    rect.Offset(10, rectHeightMiddle);

                                    var color = Color.FromArgb(80, 1, 52, 153).Brightness(ColorScheme.BackColor.Determine().GetBrightness());
                                    using (var drawBrush = new SolidBrush(color))
                                    {
                                        TextRenderer.DrawText(graphics, sText, Font, rect, color, TextFormatFlags.Left);

                                        rect.Offset(0, -rectHeightMiddle);

                                        using (var lineBrush = new SolidBrush(color))
                                        {
                                            graphics.DrawLine(new Pen(lineBrush), rect.X + graphics.MeasureString(sText, Font).Width + 10, rect.Y + (int)Math.Round(rect.Height / 2d), rect.X + (int)Math.Round(rect.Width * 95 / 100d), rect.Y + (int)Math.Round(rect.Height / 2d));
                                        }
                                    }
                                }

                                m.Result = new IntPtr((int)CDRF.CDRF_SKIPDEFAULT); return;
                            }
                            /*else
                            {
                                m.Result = new IntPtr((int)CDRF.CDRF_NOTIFYITEMDRAW);
                            }*/

                            break;
                            /*
                        case CDDS.CDDS_ITEMPREPAINT:
                            m.Result = new IntPtr((int)(CDRF.CDRF_NOTIFYSUBITEMDRAW | CDRF.CDRF_NOTIFYPOSTPAINT));

                            ListView lv = this;
                            IntPtr headerControl = GetHeaderControl(lv);
                            IntPtr hdc = GetDC(headerControl);

                            using (var graphics = Graphics.FromHdc(hdc))
                            {
                                graphics.FillRectangle(new SolidBrush(ColorScheme.BackColor), graphics.ClipBounds);

                                var width = 0;
                                foreach (ColumnHeader column in Columns)
                                {
                                    var size = TextRenderer.MeasureText(column.Text, Font);
                                    var bounds = new Rectangle(new Point(width, 0), new Size(column.Width + 5, 24));

                                    if(column.TextAlign == HorizontalAlignment.Left)
                                        TextRenderer.DrawText(graphics, column.Text, Font, bounds, ColorScheme.ForeColor, TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.PathEllipsis);
                                    else if (column.TextAlign == HorizontalAlignment.Right)
                                        TextRenderer.DrawText(graphics, column.Text, Font, bounds, ColorScheme.ForeColor, TextFormatFlags.Right);
                                    else
                                        TextRenderer.DrawText(graphics, column.Text, Font, bounds, ColorScheme.ForeColor, TextFormatFlags.HorizontalCenter);

                                    var x = bounds.X - 2;
                                    graphics.DrawLine(new Pen(ColorScheme.BorderColor), x, 0, x, Height);

                                    width += column.Width;
                                }
                            }

                            ReleaseDC(headerControl, hdc);

                                break;*/
                    }
                    }
                }
                else if (m.Msg != WM_KILLFOCUS &&
                    (m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL))
                    Invalidate();

                base.WndProc(ref m);
            }
        }
    }
