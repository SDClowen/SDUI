using System.Windows.Forms;

namespace SDUI.Controls;

public class ToolStripItem : MenuItem
{
    public ToolStripItemAlignment Alignment { get; set; }
}

public class ToolStripMenuItem : ToolStripItem
{
}

public class ToolStripSeparator : ToolStripMenuItem
{
}