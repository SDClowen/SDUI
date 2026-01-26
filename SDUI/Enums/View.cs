namespace SDUI;

public enum View
{
    //
    // Summary:
    //     Each item appears as a full-sized icon with a label below it.
    LargeIcon = 0,
    //
    // Summary:
    //     Each item appears on a separate line with further information about each item
    //     arranged in columns. The left-most column contains a small icon and label, and
    //     subsequent columns contain sub items as specified by the application. A column
    //     displays a header which can display a caption for the column. The user can resize
    //     each column at run time.
    Details = 1,
    //
    // Summary:
    //     Each item appears as a small icon with a label to its right.
    SmallIcon = 2,
    //
    // Summary:
    //     Each item appears as a small icon with a label to its right. Items are arranged
    //     in columns with no column headers.
    List = 3,
    //
    // Summary:
    //     Each item appears as a full-sized icon with the item label and subitem information
    //     to the right of it. The subitem information that appears is specified by the
    //     application. This view is available only on Windows XP and the Windows Server
    //     2003 family. On earlier operating systems, this value is ignored and the System.Windows.Forms.ListView
    //     control displays in the System.Windows.Forms.View.LargeIcon view.
    Tile = 4
}