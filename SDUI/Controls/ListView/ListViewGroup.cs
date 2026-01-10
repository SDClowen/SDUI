using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Windows.Forms;
using SDUI.Collections;

namespace SDUI.Controls;

internal class Indexer
{
    // Used by TreeViewImageIndexConverter to show "(none)"
    internal const int NoneIndex = -2;

    // Used by generally across the board to indicate unset image
    internal const string DefaultKey = "";
    internal const int DefaultIndex = -1;
    private int _index = DefaultIndex;

    private string _key = DefaultKey;
    private bool _useIntegerIndex = true;

    public virtual ImageList? ImageList { get; set; }

    [AllowNull]
    public virtual string Key
    {
        get => _key;
        set
        {
            _index = DefaultIndex;
            _key = value ?? DefaultKey;
            _useIntegerIndex = false;
        }
    }

    public virtual int Index
    {
        get => _index;
        set
        {
            _key = DefaultKey;
            _index = value;
            _useIntegerIndex = true;
        }
    }

    public virtual int ActualIndex
    {
        get
        {
            if (_useIntegerIndex) return Index;

            if (ImageList is not null) return ImageList.Images.IndexOfKey(Key);

            return DefaultIndex;
        }
    }
}

internal class ListViewGroupImageIndexer : Indexer
{
    private readonly ListViewGroup _owner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ListViewGroupImageIndexer" /> class.
    /// </summary>
    /// <param name="group">The <see cref="ListViewGroup" /> that this object belongs to.</param>
    public ListViewGroupImageIndexer(ListViewGroup group)
    {
        _owner = group;
    }

    /// <summary>
    ///     Gets the <see cref="ListView.GroupImageList" /> of the <see cref="ListView" />
    ///     associated with the group.
    /// </summary>
    public override ImageList? ImageList
    {
        get => _owner.ListView?.GroupImageList;
        set => Debug.Fail("We should never set the image list");
    }
}

[Serializable] // This type is participating in resx serialization scenarios.
public sealed class ListViewGroup : ISerializable
{
    private static int s_nextID;

    private static int s_nextHeader = 1;
    private AccessibleObject? _accessibilityObject;
    private ListViewGroupCollapsedState _collapsedState = ListViewGroupCollapsedState.Default;
    private string? _footer;
    private HorizontalAlignment _footerAlignment = HorizontalAlignment.Left;
    private string? _header;
    private HorizontalAlignment _headerAlignment = HorizontalAlignment.Left;

    private ListViewGroupImageIndexer? _imageIndexer;

    private ListViewItemCollection? _items;
    private string? _subtitle;
    private string? _taskLink;

    /// <summary>
    ///     Creates a ListViewGroup.
    /// </summary>
    public ListViewGroup()
    {
    }

    /// <summary>
    ///     Creates a ListViewItem object from an Stream.
    /// </summary>
    private ListViewGroup(SerializationInfo info, StreamingContext context) : this()
    {
        Deserialize(info);
    }

    /// <summary>
    ///     Creates a ListViewItem object from a Key and a Name
    /// </summary>
    public ListViewGroup(string? key, string? headerText) : this()
    {
        Name = key;
        _header = headerText;
    }

    /// <summary>
    ///     Creates a ListViewGroup.
    /// </summary>
    public ListViewGroup(string? header)
    {
        _header = header;
        ID = s_nextID++;
    }

    /// <summary>
    ///     Creates a ListViewGroup.
    /// </summary>
    public ListViewGroup(string? header, HorizontalAlignment headerAlignment) : this(header)
    {
        _headerAlignment = headerAlignment;
    }

    /// <summary>
    ///     The text displayed in the group header.
    /// </summary>
    public string Header
    {
        get => _header ?? string.Empty;
        set
        {
            if (_header == value) return;

            _header = value;
            UpdateListView();
        }
    }

    /// <summary>
    ///     The alignment of the group header.
    /// </summary>
    public HorizontalAlignment HeaderAlignment
    {
        get => _headerAlignment;
        set
        {
            if (_headerAlignment == value) return;

            _headerAlignment = value;
            UpdateListView();
        }
    }

    /// <summary>
    ///     The text displayed in the group footer.
    /// </summary>
    public string Footer
    {
        get => _footer ?? string.Empty;
        set
        {
            if (_footer == value) return;

            _footer = value;
            UpdateListView();
        }
    }

    /// <summary>
    ///     The alignment of the group footer.
    /// </summary>
    /// <value>
    ///     One of the <see cref="HorizontalAlignment" /> values that specifies the alignment of the footer text. The default
    ///     is <see cref="HorizontalAlignment.Left" />.
    /// </value>
    /// <exception cref="InvalidEnumArgumentException">
    ///     The specified value when setting this property is not a valid <see cref="HorizontalAlignment" /> value.
    /// </exception>
    public HorizontalAlignment FooterAlignment
    {
        get => _footerAlignment;
        set
        {
            if (_footerAlignment == value) return;

            _footerAlignment = value;
            UpdateListView();
        }
    }

    internal bool Focused { get; set; }

    /// <summary>
    ///     Controls which <see cref="ListViewGroupCollapsedState" /> the group will appear as.
    /// </summary>
    /// <value>
    ///     One of the <see cref="ListViewGroupCollapsedState" /> values that specifies how the group is displayed.
    ///     The default is <see cref="ListViewGroupCollapsedState.Default" />.
    /// </value>
    /// <exception cref="InvalidEnumArgumentException">
    ///     The specified value when setting this property is not a valid <see cref="ListViewGroupCollapsedState" /> value.
    /// </exception>
    public ListViewGroupCollapsedState CollapsedState
    {
        get => _collapsedState;
        set
        {
            if (_collapsedState == value) return;

            _collapsedState = value;
            UpdateListView();
        }
    }

    /// <summary>
    ///     The text displayed in the group subtitle.
    /// </summary>
    public string Subtitle
    {
        get => _subtitle ?? string.Empty;
        set
        {
            if (_subtitle == value) return;

            _subtitle = value;
            UpdateListView();
        }
    }

    /// <summary>
    ///     The name of the task link displayed in the group header.
    /// </summary>
    public string TaskLink
    {
        get => _taskLink ?? string.Empty;
        set
        {
            if (value == _taskLink) return;

            _taskLink = value;
            UpdateListView();
        }
    }

    /// <summary>
    ///     Gets or sets the index of the image that is displayed for the group.
    /// </summary>
    /// <value>
    ///     The zero-based index of the image in the ImageList that is displayed for the group. The default is -1.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The value specified is less than -1.
    /// </exception>
    public int TitleImageIndex
    {
        get
        {
            var imageList = ImageIndexer.ImageList;
            return imageList is null || ImageIndexer.Index < imageList.Images.Count
                ? ImageIndexer.Index
                : imageList.Images.Count - 1;
        }
        set
        {
            if (value < Indexer.DefaultIndex) throw new ArgumentOutOfRangeException(nameof(value));

            if (ImageIndexer.Index == value && value != Indexer.DefaultIndex) return;

            ImageIndexer.Index = value;
            UpdateListView();
        }
    }

    /// <summary>
    ///     Gets or sets the key of the image that is displayed for the group.
    /// </summary>
    /// <value>
    ///     The key for the image that is displayed for the group.
    /// </value>
    public string TitleImageKey
    {
        get => ImageIndexer.Key;
        set
        {
            if (ImageIndexer.Key == value && value != Indexer.DefaultKey) return;

            ImageIndexer.Key = value;
            UpdateListView();
        }
    }

    internal ListViewGroupImageIndexer ImageIndexer => _imageIndexer ??= new ListViewGroupImageIndexer(this);

    internal int ID { get; }

    /// <summary>
    ///     The items that belong to this group.
    /// </summary>
    public ListViewItemCollection Items => _items ??= new ListViewItemCollection(new ListViewGroupItemCollection(this));

    public ListView? ListView { get; internal set; }

    public string? Name { get; set; }

    public object? Tag { get; set; }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info);

        info.AddValue(nameof(Header), Header);
        info.AddValue(nameof(HeaderAlignment), HeaderAlignment);
        info.AddValue(nameof(Footer), Footer);
        info.AddValue(nameof(FooterAlignment), FooterAlignment);
        info.AddValue(nameof(Tag), Tag);
        if (!string.IsNullOrEmpty(Name)) info.AddValue(nameof(Name), Name);

        //if (_items is not null && _items.Count > 0)
        //{
        //    info.AddValue("ItemsCount", Items.Count);
        //    for (int i = 0; i < Items.Count; i++)
        //    {
        //        info.AddValue($"Item{i}", Items[i], typeof(ListViewItem));
        //    }
        //}
    }

    private void Deserialize(SerializationInfo info)
    {
        var count = 0;

        foreach (var entry in info)
            if (entry.Name == "Header")
                Header = (string)entry.Value!;
            else if (entry.Name == "HeaderAlignment")
                HeaderAlignment = (HorizontalAlignment)entry.Value!;
            else if (entry.Name == "Footer")
                Footer = (string)entry.Value!;
            else if (entry.Name == "FooterAlignment")
                FooterAlignment = (HorizontalAlignment)entry.Value!;
            else if (entry.Name == "Tag")
                Tag = entry.Value;
            else if (entry.Name == "ItemsCount")
                count = (int)entry.Value!;
            else if (entry.Name == "Name") Name = (string)entry.Value!;

        if (count > 0)
        {
            var items = new ListViewItem[count];
            for (var i = 0; i < count; i++) items[i] = (ListViewItem)info.GetValue($"Item{i}", typeof(ListViewItem))!;

            Items.AddRange(items);
        }
    }

    internal void ReleaseUiaProvider()
    {
        _accessibilityObject = null;
    }

    // Should be used for the cases when sending the message `PInvoke.LVM_SETGROUPINFO` isn't required
    // (for example, collapsing/expanding groups with keyboard is performed inside the native control already, so this message isn't needed)
    internal void SetCollapsedStateInternal(ListViewGroupCollapsedState state)
    {
        if (_collapsedState == state) return;

        _collapsedState = state;
    }

    public override string ToString()
    {
        return Header;
    }

    private void UpdateListView()
    {
    }
}