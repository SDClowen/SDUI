using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using System.Runtime.Serialization;
using System.Windows.Forms;

namespace SDUI.Controls;

public partial class ListViewItem : ICloneable
{
    private const int MaxSubItems = 4096;

    private static readonly BitVector32.Section s_stateSelectedSection = BitVector32.CreateSection(1);

    private static readonly BitVector32.Section s_stateImageMaskSet =
        BitVector32.CreateSection(1, s_stateSelectedSection);

    private static readonly BitVector32.Section s_stateWholeRowOneStyleSection =
        BitVector32.CreateSection(1, s_stateImageMaskSet);

    private static readonly BitVector32.Section s_savedStateImageIndexSection =
        BitVector32.CreateSection(15, s_stateWholeRowOneStyleSection);

    private static readonly BitVector32.Section s_subItemCountSection =
        BitVector32.CreateSection(MaxSubItems, s_savedStateImageIndexSection);

    private readonly SKPoint _position = new(-1, -1);

    private AccessibleObject? _accessibilityObject;
    private View _accessibilityObjectView;

    internal ListViewGroup? _group;
    private string? _groupName;
    private ListViewItemImageIndexer? _imageIndexer;

    private int _indentCount;

    // we stash the last index we got as a seed to GetDisplayIndex.
    private int _lastIndex = -1;

    internal ListView? _listView;

    private ListViewSubItemCollection? _listViewSubItemCollection;

    private BitVector32 _state;
    private List<ListViewSubItem> _subItems = new();
    private string _toolTipText = string.Empty;

    // An ID unique relative to a given list view that comctl uses to identify items.
    internal int ID = -1;

    public ListViewItem()
    {
        StateSelected = false;
        UseItemStyleForSubItems = true;
        SavedStateImageIndex = -1;
    }

    /// <summary>
    ///     Creates a ListViewItem object from an Stream.
    /// </summary>
    protected ListViewItem(SerializationInfo info, StreamingContext context)
        : this()
    {
    }

    public ListViewItem(string? text)
        : this(text, -1)
    {
    }

    public ListViewItem(string? text, int imageIndex)
        : this()
    {
        ImageIndexer.Index = imageIndex;
        Text = text;
    }

    public ListViewItem(string[]? items)
        : this(items, -1)
    {
    }

    public ListViewItem(string[]? items, int imageIndex)
        : this()
    {
        ImageIndexer.Index = imageIndex;
        if (items is not null && items.Length > 0)
        {
            _subItems.EnsureCapacity(items.Length);
            for (var i = 0; i < items.Length; i++) _subItems.Add(new ListViewSubItem(this, items[i]));

            SubItemCount = items.Length;
        }
    }

    public ListViewItem(string[]? items, int imageIndex, SKColor foreColor, SKColor backColor, Font? font)
        : this(items, imageIndex)
    {
        ForeColor = foreColor;
        BackColor = backColor;
        Font = font;
    }

    public ListViewItem(ListViewSubItem[] subItems, int imageIndex)
        : this()
    {
        ArgumentNullException.ThrowIfNull(subItems);

        ImageIndexer.Index = imageIndex;
        SubItemCount = subItems.Length;

        // Update the owner of these subitems
        for (var i = 0; i < subItems.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(subItems[i], nameof(subItems));

            subItems[i]._owner = this;
            _subItems.Add(subItems[i]);
        }
    }

    public ListViewItem(ListViewGroup? group)
        : this()
    {
        Group = group;
    }

    public ListViewItem(string? text, ListViewGroup? group)
        : this(text)
    {
        Group = group;
    }

    public ListViewItem(string? text, int imageIndex, ListViewGroup? group)
        : this(text, imageIndex)
    {
        Group = group;
    }

    public ListViewItem(string[]? items, ListViewGroup? group)
        : this(items)
    {
        Group = group;
    }

    public ListViewItem(string[]? items, int imageIndex, ListViewGroup? group)
        : this(items, imageIndex)
    {
        Group = group;
    }

    public ListViewItem(string[]? items, int imageIndex, SKColor foreColor, SKColor backColor, Font? font,
        ListViewGroup? group)
        : this(items, imageIndex, foreColor, backColor, font)
    {
        Group = group;
    }

    public ListViewItem(ListViewSubItem[] subItems, int imageIndex, ListViewGroup? group)
        : this(subItems, imageIndex)
    {
        Group = group;
    }

    public ListViewItem(string? text, string? imageKey)
        : this()
    {
        ImageIndexer.Key = imageKey;
        Text = text;
    }

    public ListViewItem(string[]? items, string? imageKey)
        : this()
    {
        ImageIndexer.Key = imageKey;
        if (items is not null && items.Length > 0)
        {
            _subItems = new List<ListViewSubItem>(items.Length);
            for (var i = 0; i < items.Length; i++) _subItems.Add(new ListViewSubItem(this, items[i]));

            SubItemCount = items.Length;
        }
    }

    public ListViewItem(string[]? items, string? imageKey, SKColor foreColor, SKColor backColor, Font? font)
        : this(items, imageKey)
    {
        ForeColor = foreColor;
        BackColor = backColor;
        Font = font;
    }

    public ListViewItem(ListViewSubItem[] subItems, string? imageKey)
        : this()
    {
        ArgumentNullException.ThrowIfNull(subItems);

        ImageIndexer.Key = imageKey;
        SubItemCount = subItems.Length;

        // Update the owner of these subitems
        for (var i = 0; i < subItems.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(subItems[i], nameof(subItems));

            subItems[i]._owner = this;
            _subItems.Add(subItems[i]);
        }
    }

    public ListViewItem(string? text, string? imageKey, ListViewGroup? group)
        : this(text, imageKey)
    {
        Group = group;
    }

    public ListViewItem(string[]? items, string? imageKey, ListViewGroup? group)
        : this(items, imageKey)
    {
        Group = group;
    }

    public ListViewItem(string[]? items, string? imageKey, SKColor foreColor, SKColor backColor, Font? font,
        ListViewGroup? group)
        : this(items, imageKey, foreColor, backColor, font)
    {
        Group = group;
    }

    public ListViewItem(ListViewSubItem[] subItems, string? imageKey, ListViewGroup? group)
        : this(subItems, imageKey)
    {
        Group = group;
    }

    internal virtual AccessibleObject AccessibilityObject
    {
        get
        {
            var owningListView = _listView ?? Group?.ListView;
            if (_accessibilityObject is null || owningListView.View != _accessibilityObjectView)
                _accessibilityObjectView = owningListView.View;
            //_accessibilityObject = _accessibilityObjectView switch
            //{
            //    System.Windows.Forms.View.Details => new ListViewItemDetailsAccessibleObject(this),
            //    View.LargeIcon => new ListViewItemLargeIconAccessibleObject(this),
            //    View.List => new ListViewItemListAccessibleObject(this),
            //    View.SmallIcon => new ListViewItemSmallIconAccessibleObject(this),
            //    View.Tile => new ListViewItemTileAccessibleObject(this),
            //    _ => throw new Exception()
            //};
            return _accessibilityObject;
        }
    }

    private bool IsAccessibilityObjectCreated => _accessibilityObject is not null;

    /// <summary>
    ///     The font that this item will be displayed in. If its value is null, it will be displayed
    ///     using the global font for the ListView control that hosts it.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SKColor BackColor
    {
        get
        {
            if (SubItemCount == 0)
            {
                if (_listView is not null) return _listView.BackColor;

                return SystemColors.Window;
            }

            return _subItems[0].BackColor;
        }
        set => SubItems[0].BackColor = value;
    }

    /// <summary>
    ///     Returns the ListViewItem's bounding rectangle, including subitems. The bounding rectangle is empty if
    ///     the ListViewItem has not been added to a ListView control.
    /// </summary>
    [Browsable(false)]
    public SKRect Bounds
    {
        get
        {
            if (_listView is not null) return _listView.GetItemRect(Index);

            return default;
        }
    }

    [DefaultValue(false)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public bool Checked
    {
        get => StateImageIndex > 0;
        set
        {
            if (Checked != value)
            {
                if (_listView is not null && _listView.IsHandleCreated)
                {
                    StateImageIndex = value ? 1 : 0;

                    // the setter for StateImageIndex calls ItemChecked handler
                    // thus need to verify validity of the listView again
                    if (_listView is not null && !_listView.UseCompatibleStateImageBehavior)
                        if (!_listView.CheckBoxes)
                            _listView.UpdateSavedCheckedItems(this, value);
                }
                else
                {
                    SavedStateImageIndex = value ? 1 : 0;
                }
            }
        }
    }

    /// <summary>
    ///     Returns the focus state of the ListViewItem.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public bool Focused
    {
        get
        {
            if (_listView is not null && _listView.IsHandleCreated)
            {
            }

            return false;
        }

        set
        {
            if (_listView is not null && _listView.IsHandleCreated)
            {
            }
        }
    }

    [AllowNull]
    public Font Font
    {
        get
        {
            if (SubItemCount == 0)
            {
                if (_listView is not null) return _listView.Font;

                return Control.DefaultFont;
            }

            return _subItems[0].Font;
        }
        set => SubItems[0].Font = value;
    }

    public SKColor ForeColor
    {
        get
        {
            if (SubItemCount == 0)
            {
                if (_listView is not null) return _listView.ForeColor;

                return SystemColors.WindowText;
            }

            return _subItems[0].ForeColor;
        }
        set => SubItems[0].ForeColor = value;
    }

    [DefaultValue(null)]
    [Localizable(true)]
    public ListViewGroup? Group
    {
        get => _group;
        set
        {
            if (_group != value)
            {
                if (value is not null)
                    value.Items.Add(this);
                else
                    _group!.Items.Remove(this);
            }

            Debug.Assert(_group == value, "BUG: group member variable wasn't updated!");

            // If the user specifically sets the group then don't use the groupName again.
            _groupName = null;
        }
    }

    /// <summary>
    ///     Returns the ListViewItem's currently set image index
    /// </summary>
    [Localizable(true)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public int ImageIndex
    {
        get =>
            ImageList is null || ImageIndexer.Index < ImageList.Images.Count
                ? ImageIndexer.Index
                : ImageList.Images.Count - 1;
        set
        {
            if (ImageIndexer.Index == value)
                return;

            ImageIndexer.Index = value;

            if (_listView is not null && _listView.IsHandleCreated)
                _listView.SetItemImage(Index, ImageIndexer.ActualIndex);
        }
    }

    internal ListViewItemImageIndexer ImageIndexer => _imageIndexer ??= new ListViewItemImageIndexer(this);

    /// <summary>
    ///     Returns the ListViewItem's currently set image index
    /// </summary>
    [Localizable(true)]
    public string ImageKey
    {
        get => ImageIndexer.Key;
        set
        {
            if (ImageIndexer.Key == value)
                return;

            ImageIndexer.Key = value;

            if (_listView is not null && _listView.IsHandleCreated)
                _listView.SetItemImage(Index, ImageIndexer.ActualIndex);
        }
    }

    [Browsable(false)]
    public ImageList? ImageList
    {
        get
        {
            if (_listView is not null)
                switch (_listView.View)
                {
                    case View.LargeIcon:
                    case View.Tile:
                        return _listView.LargeImageList;
                    case View.SmallIcon:
                    case View.Details:
                    case View.List:
                        return _listView.SmallImageList;
                }

            return null;
        }
    }

    [DefaultValue(0)]
    public int IndentCount
    {
        get => _indentCount;
        set
        {
            if (value < 0)
                return;

            if (value != _indentCount)
            {
                _indentCount = value;
                if (_listView is not null && _listView.IsHandleCreated)
                    _listView.SetItemIndentCount(Index, _indentCount);
            }
        }
    }

    /// <summary>
    ///     Returns ListViewItem's current index in the listview, or -1 if it has not been added to a ListView control.
    /// </summary>
    [Browsable(false)]
    public int Index
    {
        get
        {
            if (_listView is not null)
            {
                // if the list is virtual, the ComCtrl control does not keep any information
                // about any list view items, so we use our cache instead.
                if (!_listView.VirtualMode) _lastIndex = _listView.GetDisplayIndex(this, _lastIndex);

                return _lastIndex;
            }

            return -1;
        }
    }

    /// <summary>
    ///     Returns the ListView control that holds this ListViewItem. May be null if no
    ///     control has been assigned yet.
    /// </summary>
    [Browsable(false)]
    public ListView? ListView => _listView;

    /// <summary>
    ///     Name associated with this ListViewItem
    /// </summary>
    [Localizable(true)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [AllowNull]
    public string Name
    {
        get
        {
            if (SubItemCount == 0) return string.Empty;

            return _subItems[0].Name;
        }
        set => SubItems[0].Name = value;
    }

    /// <summary>
    ///     Accessor for our state bit vector.
    /// </summary>
    private int SavedStateImageIndex
    {
        get =>
            // State goes from zero to 15, but we need a negative
            // number, so we store + 1.
            _state[s_savedStateImageIndexSection] - 1;
        set
        {
            // flag whether we've set a value.
            _state[s_stateImageMaskSet] = value == -1 ? 0 : 1;

            // push in the actual value
            _state[s_savedStateImageIndexSection] = value + 1;
        }
    }

    /// <summary>
    ///     Treats the ListViewItem as a row of strings, and returns an array of those strings
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Selected
    {
        get => StateSelected;
        set
        {
            if (_listView is not null && _listView.IsHandleCreated)
            {
            }
            else
            {
                StateSelected = value;
            }
        }
    }

    [RelatedImageList("ListView.StateImageList")]
    public int StateImageIndex
    {
        get
        {
            if (_listView is not null && _listView.IsHandleCreated)
            {
                // return (((int)state >> 12) - 1);   // index is 1-based
            }

            return SavedStateImageIndex;
        }
        set
        {
            if (value < 0 || value > 14)
            {
            }

            if (_listView is not null && _listView.IsHandleCreated) _state[s_stateImageMaskSet] = value == -1 ? 0 : 1;

            SavedStateImageIndex = value;
        }
    }

    internal bool StateImageSet => _state[s_stateImageMaskSet] != 0;

    /// <summary>
    ///     Accessor for our state bit vector.
    /// </summary>
    internal bool StateSelected
    {
        get => _state[s_stateSelectedSection] == 1;
        set => _state[s_stateSelectedSection] = value ? 1 : 0;
    }

    /// <summary>
    ///     Accessor for our state bit vector.
    /// </summary>
    private int SubItemCount // Do NOT rename (binary serialization).
    {
        get => _state[s_subItemCountSection];
        set => _state[s_subItemCountSection] = value;
    }

    public ListViewSubItemCollection SubItems
    {
        get
        {
            // Use direct state access to avoid property recursion
            if (_state[s_subItemCountSection] == 0)
            {
                _subItems = new List<ListViewSubItem>(1);
                _subItems.Add(new ListViewSubItem(this, string.Empty));
                _state[s_subItemCountSection] = 1;
            }

            return _listViewSubItemCollection ??= new ListViewSubItemCollection(this);
        }
    }

    [DefaultValue(null)]
    [TypeConverter(typeof(StringConverter))]
    public object? Tag { get; set; }

    /// <summary>
    ///     Text associated with this ListViewItem
    /// </summary>
    [Localizable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [AllowNull]
    public string Text
    {
        get
        {
            if (SubItemCount == 0) return string.Empty;

            return _subItems[0].Text;
        }
        set => SubItems[0].Text = value;
    }

    /// <summary>
    ///     Tool tip text associated with this ListViewItem
    /// </summary>
    [DefaultValue("")]
    [AllowNull]
    public string ToolTipText
    {
        get => _toolTipText;
        set
        {
            value ??= string.Empty;

            if (!_toolTipText.Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                _toolTipText = value;

                // tell the list view about this change
                if (_listView is not null && _listView.IsHandleCreated)
                {
                    //_listView.ListViewItemToolTipChanged(this);
                }
            }
        }
    }

    /// <summary>
    ///     Whether or not the font and coloring for the ListViewItem will be used for all of its subitems.
    ///     If true, the ListViewItem style will be used when drawing the subitems.
    ///     If false, the ListViewItem and its subitems will be drawn in their own individual styles
    ///     if any have been set.
    /// </summary>
    [DefaultValue(true)]
    public bool UseItemStyleForSubItems
    {
        get => _state[s_stateWholeRowOneStyleSection] == 1;
        set => _state[s_stateWholeRowOneStyleSection] = value ? 1 : 0;
    }

    public virtual object Clone()
    {
        var clonedSubItems = new ListViewSubItem[SubItems.Count];
        for (var index = 0; index < SubItems.Count; ++index)
        {
            var subItem = SubItems[index];
            clonedSubItems[index] = new ListViewSubItem(
                null,
                subItem.Text,
                subItem.ForeColor,
                subItem.BackColor,
                subItem.Font)
            {
                Tag = subItem.Tag
            };
        }

        var clonedType = GetType();

        ListViewItem newItem;
        if (clonedType == typeof(ListViewItem))
            newItem = new ListViewItem(clonedSubItems, ImageIndexer.Index);
        else
            newItem = (ListViewItem)Activator.CreateInstance(clonedType)!;

        foreach (var subItem in clonedSubItems) newItem._subItems.Add(subItem);

        newItem.ImageIndexer.Index = ImageIndexer.Index;
        newItem.SubItemCount = SubItemCount;
        newItem.Checked = Checked;
        newItem.UseItemStyleForSubItems = UseItemStyleForSubItems;
        newItem.Tag = Tag;

        // Only copy over the ImageKey if we're using it.
        if (!string.IsNullOrEmpty(ImageIndexer.Key)) newItem.ImageIndexer.Key = ImageIndexer.Key;

        newItem._indentCount = _indentCount;
        newItem.StateImageIndex = StateImageIndex;
        newItem._toolTipText = _toolTipText;
        newItem.BackColor = BackColor;
        newItem.ForeColor = ForeColor;
        newItem.Font = Font;
        newItem.Text = Text;
        newItem.Group = Group;

        return newItem;
    }

    /// <summary>
    ///     Initiate editing of the item's label. Only effective if LabelEdit property is true.
    /// </summary>
    public void BeginEdit()
    {
        if (Index >= 0)
        {
            var lv = ListView!;
            if (!lv.LabelEdit)
            {
            }

            if (!lv.Focused) lv.Focus();
        }
    }

    /// <summary>
    ///     Ensure that the item is visible, scrolling the view as necessary.
    /// </summary>
    public virtual void EnsureVisible()
    {
        if (_listView is not null && _listView.IsHandleCreated) _listView.EnsureVisible(Index);
    }

    public ListViewItem? FindNearestItem(SearchDirectionHint searchDirection)
    {
        var r = Bounds;
        var xCenter = r.Left + (r.Right - r.Left) / 2;
        var yCenter = r.Top + (r.Bottom - r.Top) / 2;

        return ListView?.FindNearestItem(searchDirection, xCenter, yCenter);
    }

    /// <summary>
    ///     Returns a specific portion of the ListViewItem's bounding rectangle.
    ///     The rectangle returned is empty if the ListViewItem has not been added to a ListView control.
    /// </summary>
    public SKRect GetBounds(ItemBoundsPortion portion)
    {
        if (_listView is not null && _listView.IsHandleCreated) return _listView.GetItemRect(Index, portion);

        return default;
    }

    public ListViewSubItem? GetSubItemAt(int x, int y)
    {
        if (_listView is not null && _listView.IsHandleCreated && _listView.View == View.Details)
        {
            _listView.GetSubItemAt(x, y, out var iItem, out var iSubItem);
            if (iItem == Index && iSubItem != -1 && iSubItem < SubItems.Count) return SubItems[iSubItem];

            return null;
        }

        return null;
    }


    // we need this function to set the index when the list view is in virtual mode.
    // the index of the list view item is used in ListView::set_TopItem property
    internal void SetItemIndex(ListView listView, int index)
    {
        Debug.Assert(listView is not null && listView.VirtualMode,
            "ListViewItem::SetItemIndex should be used only when the list is virtual");
        Debug.Assert(index > -1, "can't set the index on a virtual list view item to -1");
        _listView = listView;
        _lastIndex = index;
    }

    internal static bool ShouldSerializeText()
    {
        return false;
    }

    private bool ShouldSerializePosition()
    {
        return !_position.Equals(new SKPoint(-1, -1));
    }

    public override string ToString()
    {
        return $"ListViewItem: {{{Text}}}";
    }

    internal void InvalidateListView()
    {
        // The ListItem's state (or a SubItem's state) has changed, so invalidate the ListView control
        if (_listView is not null && _listView.IsHandleCreated) _listView.Invalidate();
    }

    internal void UpdateSubItems(int index)
    {
        UpdateSubItems(index, SubItemCount);
    }

    internal void UpdateSubItems(int index, int oldCount)
    {
        if (_listView is not null && _listView.IsHandleCreated)
        {
            var subItemCount = SubItemCount;
            var itemIndex = Index;
            if (index != -1)
                _listView.SetItemText(itemIndex, index, _subItems[index].Text);
            else
                for (var i = 0; i < subItemCount; i++)
                    _listView.SetItemText(itemIndex, i, _subItems[i].Text);

            for (var i = subItemCount; i < oldCount; i++) _listView.SetItemText(itemIndex, i, string.Empty);
        }
    }
}