using System.Diagnostics;
using System.Windows.Forms;

namespace SDUI.Controls;

internal class ListViewItemImageIndexer : Indexer
{
    private readonly ListViewItem _owner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ListViewItemImageIndexer" /> class.
    /// </summary>
    /// <param name="item">The <see cref="ListViewItem" /> that this object belongs to.</param>
    public ListViewItemImageIndexer(ListViewItem item)
    {
        _owner = item;
    }

    /// <summary>
    ///     Gets the <see cref="ListViewItem.ImageList" /> associated with the item.
    /// </summary>
    public override ImageList? ImageList
    {
        get => _owner.ImageList;
        set => Debug.Fail("We should never set the image list");
    }
}