using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SDUI.NativeMethods;

namespace SDUI.Collections;

public partial class ListViewItemCollection : IList
{
    internal class ListViewNativeItemCollection : ListViewItemCollection.IInnerList
    {
        private readonly ListView _owner;

        public ListViewNativeItemCollection(ListView owner)
        {
            _owner = owner;
        }

        public int Count
        {
            get
            {
                return _owner.VirtualMode ? _owner.VirtualListSize : _owner.Items.Count;
            }
        }

        public bool OwnerIsVirtualListView => _owner.VirtualMode;

        public bool OwnerIsDesignMode => false;

        public ListViewItem this[int displayIndex]
        {
            get => GetItemByIndexInternal(displayIndex, throwInVirtualMode: true)!;
            set
            {
               
                if (_owner.VirtualMode)
                {
                    throw new InvalidOperationException();
                }

                ArgumentOutOfRangeException.ThrowIfNegative(displayIndex);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(displayIndex, _owner.Items.Count);


                RemoveAt(displayIndex);
                Insert(displayIndex, value);
            }
        }

        public ListViewItem? GetItemByIndex(int index) =>
            GetItemByIndexInternal(index, throwInVirtualMode: false);

        private ListViewItem? GetItemByIndexInternal(int index, [NotNullWhen(true)] bool throwInVirtualMode)
        {
           

            if (_owner.VirtualMode)
            {
                // If we are showing virtual items, we need to get the item from the user.
                RetrieveVirtualItemEventArgs rVI = new(index);
                if (rVI.Item is null)
                {
                    return !throwInVirtualMode ? null : throw new InvalidOperationException(SR.ListViewVirtualItemRequired);
                }

                return rVI.Item;
            }
            else
            {
                ArgumentOutOfRangeException.ThrowIfNegative(index);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _owner.Items.Count);

                if (_owner.IsHandleCreated && !_owner.ListViewHandleDestroyed)
                {
                    _owner._listItemsTable.TryGetValue(DisplayIndexToID(index), out ListViewItem? item);
                    return item!;
                }
                else
                {
                    Debug.Assert(_owner._listViewItems is not null, "listItemsArray is null, but the handle isn't created");
                    return _owner._listViewItems[index];
                }
            }
        }

        public ListViewItem Add(ListViewItem value)
        {
            if (_owner.VirtualMode)
            {
                throw new InvalidOperationException(SR.ListViewCantAddItemsToAVirtualListView);
            }
            else
            {
                Debug.Assert(!_owner.FlipViewToLargeIconAndSmallIcon || Count == 0, "the FlipView... bit is turned off after adding 1 item.");

                // PERF.
                // Get the Checked bit before adding it to the back end.
                // This saves a call into NativeListView to retrieve the real index.
                bool valueChecked = value.Checked;

                _owner.InsertItems(_owner.Items.Count, [value], true);

                if (_owner.IsHandleCreated && !_owner.CheckBoxes && valueChecked)
                {
                    _owner.UpdateSavedCheckedItems(value, true /*addItem*/);
                }

                if (_owner.ExpectingMouseUp)
                {
                    _owner.ItemCollectionChangedInMouseDown = true;
                }

                return value;
            }
        }

        public void AddRange(params ListViewItem[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (_owner.VirtualMode)
            {
                throw new InvalidOperationException(SR.ListViewCantAddItemsToAVirtualListView);
            }

            IComparer? comparer = _owner._listItemSorter;
            _owner._listItemSorter = null;

            Debug.Assert(!_owner.FlipViewToLargeIconAndSmallIcon || Count == 0, "the FlipView... bit is turned off after adding 1 item.");

            bool[]? checkedValues = null;

            if (_owner.IsHandleCreated && !_owner.CheckBoxes)
            {
                // PERF.
                // Cache the Checked bit before adding the item to the list view.
                // This saves a bunch of calls to native list view when we want to UpdateSavedCheckedItems.
                checkedValues = new bool[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    checkedValues[i] = values[i].Checked;
                }
            }

            try
            {
                _owner.BeginUpdate();
                _owner.InsertItems(_owner.Items.Count, values, true);

                if (_owner.IsHandleCreated && !_owner.CheckBoxes)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (checkedValues![i])
                        {
                            _owner.UpdateSavedCheckedItems(values[i], true /*addItem*/);
                        }
                    }
                }
            }
            finally
            {
                _owner._listItemSorter = comparer;
                _owner.EndUpdate();
            }

            if (_owner.ExpectingMouseUp)
            {
                _owner.ItemCollectionChangedInMouseDown = true;
            }

            if (comparer is not null ||
                ((_owner.Sorting != SortOrder.None) && !_owner.VirtualMode))
            {
                _owner.Sort();
            }
        }

        private int DisplayIndexToID(int displayIndex)
        {
            Debug.Assert(!_owner.VirtualMode, "in virtual mode, this method does not make any sense");
            //if (_owner.IsHandleCreated && !_owner.ListViewHandleDestroyed)
            //{
            //    // Obtain internal index of the item
            //    LVITEMW lvItem = new()
            //    {
            //        mask = LIST_VIEW_ITEM_FLAGS.LVIF_PARAM,
            //        iItem = displayIndex
            //    };

            //    PInvokeCore.SendMessage(_owner, PInvoke.LVM_GETITEMW, (WPARAM)0, ref lvItem);
            //    return PARAM.ToInt(lvItem.lParam);
            //}
            //else
            //{
            //    return this[displayIndex]._id;
            //}
        }

        public void Clear()
        {
            if (_owner.Items.Count <= 0)
            {
                return;
            }

           

            if (_owner.IsHandleCreated && !_owner.ListViewHandleDestroyed)
            {
                // Walk the items to see which ones are selected.
                // We use the LVM_GETNEXTITEM message to see what the next selected item is
                // so we can avoid checking selection for each one.
                int count = _owner.Items.Count;
                int nextSelected = (int)PInvokeCore.SendMessage(
                    _owner,
                    PInvoke.LVM_GETNEXTITEM,
                    (WPARAM)(-1),
                    (LPARAM)PInvoke.LVNI_SELECTED);

                for (int i = 0; i < count; i++)
                {
                    ListViewItem item = _owner.Items[i];
                    Debug.Assert(item is not null, $"Failed to get item at index {i}");
                    if (item is null)
                    {
                        continue;
                    }

                    // If it's the one we're looking for, ask for the next one.
                    if (i == nextSelected)
                    {
                        item.StateSelected = true;
                        nextSelected = (int)PInvokeCore.SendMessage(
                            _owner,
                            PInvoke.LVM_GETNEXTITEM,
                            (WPARAM)nextSelected, (LPARAM)PInvoke.LVNI_SELECTED);
                    }
                    else
                    {
                        // Otherwise it's false.
                        item.StateSelected = false;
                    }

                    item.UnHost(i, false);
                }

                Debug.Assert(_owner._listViewItems is null, "listItemsArray not null, even though handle created");

                PInvokeCore.SendMessage(_owner, PInvoke.LVM_DELETEALLITEMS);

                // There's a problem in the list view that if it's in small icon, it won't pick up the small icon
                // sizes until it changes from large icon, so we flip it twice here...
                if (_owner.View == View.SmallIcon)
                {
                    if (Application.ComCtlSupportsVisualStyles)
                    {
                        _owner.FlipViewToLargeIconAndSmallIcon = true;
                    }
                    else
                    {
                        Debug.Assert(!_owner.FlipViewToLargeIconAndSmallIcon, "we only set this when comctl 6.0 is loaded");
                        _owner.View = View.LargeIcon;
                        _owner.View = View.SmallIcon;
                    }
                }
            }
            else
            {
                int count = _owner.Items.Count;

                for (int i = 0; i < count; i++)
                {
                    ListViewItem item = _owner.Items[i];
                    item?.UnHost(i, true);
                }

                Debug.Assert(_owner._listViewItems is not null, "listItemsArray is null, but the handle isn't created");
                _owner._listViewItems.Clear();
            }

            _owner._listItemsTable.Clear();
            if (_owner.IsHandleCreated && !_owner.CheckBoxes)
            {
                _owner._savedCheckedItems = null;
            }

            _owner.Items.Count = 0;

            if (_owner.ExpectingMouseUp)
            {
                _owner.ItemCollectionChangedInMouseDown = true;
            }
        }

        public bool Contains(ListViewItem item)
        {
           
            if (_owner.IsHandleCreated && !_owner.ListViewHandleDestroyed)
            {
                return _owner._listItemsTable.TryGetValue(item._id, out ListViewItem? itemOut)
                    && itemOut == item;
            }
            else
            {
                Debug.Assert(_owner._listViewItems is not null, "listItemsArray is null, but the handle isn't created");
                return _owner._listViewItems.Contains(item);
            }
        }

        public ListViewItem Insert(int index, ListViewItem item)
        {
            int count;
            if (_owner.VirtualMode)
            {
                count = Count;
            }
            else
            {
                count = _owner.Items.Count;
            }

            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, count);

            if (_owner.VirtualMode)
            {
                throw new InvalidOperationException(SR.ListViewCantAddItemsToAVirtualListView);
            }

            Debug.Assert(!_owner.FlipViewToLargeIconAndSmallIcon || Count == 0, "the FlipView... bit is turned off after adding 1 item.");

            if (index < count)
            {
                // if we're not inserting at the end, force the add.
               
            }

            _owner.InsertItems(index, [item], true);
            if (_owner.IsHandleCreated && !_owner.CheckBoxes && item.Checked)
            {
                _owner.UpdateSavedCheckedItems(item, true /*addItem*/);
            }

            if (_owner.ExpectingMouseUp)
            {
                _owner.ItemCollectionChangedInMouseDown = true;
            }

            return item;
        }

        public int IndexOf(ListViewItem item)
        {
            Debug.Assert(!_owner.VirtualMode, "in virtual mode, this function does not make any sense");
            for (int i = 0; i < Count; i++)
            {
                if (item == this[i])
                {
                    return i;
                }
            }

            return -1;
        }

        public void Remove(ListViewItem item)
        {
            int index = _owner.VirtualMode ? Count - 1 : IndexOf(item);

            Debug.Assert(!_owner.FlipViewToLargeIconAndSmallIcon || Count == 0, "the FlipView... bit is turned off after adding 1 item.");

            if (_owner.VirtualMode)
            {
                throw new InvalidOperationException(SR.ListViewCantRemoveItemsFromAVirtualListView);
            }

            if (index != -1)
            {
                RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            if (_owner.VirtualMode)
            {
                throw new InvalidOperationException(SR.ListViewCantRemoveItemsFromAVirtualListView);
            }

            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _owner.Items.Count);

            Debug.Assert(!_owner.FlipViewToLargeIconAndSmallIcon || Count == 0, "the FlipView... bit is turned off after adding 1 item.");

            if (_owner.IsHandleCreated && !_owner.CheckBoxes && this[index].Checked)
            {
                _owner.UpdateSavedCheckedItems(this[index], addItem: false);
            }

           
            int itemID = DisplayIndexToID(index);

            this[index].Focused = false;
            this[index].UnHost(true);

            if (_owner.IsHandleCreated)
            {
                Debug.Assert(_owner._listViewItems is null, "listItemsArray not null, even though handle created");
                if (PInvokeCore.SendMessage(_owner, PInvoke.LVM_DELETEITEM, (WPARAM)index) == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
                }
            }
            else
            {
                Debug.Assert(_owner._listViewItems is not null, "listItemsArray is null, but the handle isn't created");
                _owner._listViewItems.RemoveAt(index);
            }

            _owner.Items.Count--;
            _owner.Items.Remove(itemID);

            if (_owner.ExpectingMouseUp)
            {
                _owner.ItemCollectionChangedInMouseDown = true;
            }
        }

        public void CopyTo(Array dest, int index)
        {
            if (_owner.Items.Count > 0)
            {
                for (int displayIndex = 0; displayIndex < Count; ++displayIndex)
                {
                    dest.SetValue(this[displayIndex], index++);
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            ListViewItem[] items = new ListViewItem[_owner.Items.Count];
            CopyTo(items, 0);

            return items.GetEnumerator();
        }
    }
    internal interface IInnerList
    {
        int Count { get; }
        bool OwnerIsVirtualListView { get; }
        bool OwnerIsDesignMode { get; }
        ListViewItem this[int index] { get; set; }

        ListViewItem Add(ListViewItem item);
        void AddRange(params ListViewItem[] items);
        void Clear();
        bool Contains(ListViewItem item);
        void CopyTo(Array dest, int index);
        IEnumerator GetEnumerator();
        int IndexOf(ListViewItem item);
        ListViewItem Insert(int index, ListViewItem item);
        void Remove(ListViewItem item);
        void RemoveAt(int index);
        ListViewItem? GetItemByIndex(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

            try
            {
                return this[index];
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
    ///  A caching mechanism for key accessor
    ///  We use an index here rather than control so that we don't have lifetime
    ///  issues by holding on to extra references.
    private int lastAccessedIndex = -1;

    private readonly IInnerList innerList;

    public ListViewItemCollection(ListView owner)
    {
        // Kept for APPCOMPAT reasons.
        // In Whidbey this constructor is a no-op.

        // initialize the inner list w/ a dummy list.
        innerList = new ListViewNativeItemCollection(owner);
    }

    internal ListViewItemCollection(IInnerList innerList)
    {
        Debug.Assert(innerList is not null, "Can't pass in null innerList");
        this.innerList = innerList;
    }

    private IInnerList InnerList
    {
        get
        {
            return innerList;
        }
    }

    /// <summary>
    ///  Returns the total number of items within the list view.
    /// </summary>
    [Browsable(false)]
    public int Count
    {
        get
        {
            return InnerList.Count;
        }
    }

    object ICollection.SyncRoot
    {
        get
        {
            return this;
        }
    }

    bool ICollection.IsSynchronized
    {
        get
        {
            return true;
        }
    }

    bool IList.IsFixedSize
    {
        get
        {
            return false;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    ///  Returns the ListViewItem at the given index.
    /// </summary>
    public virtual ListViewItem this[int index]
    {
        get
        {
            if (index < 0 || index >= InnerList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return InnerList[index];
        }
        set
        {
            if (index < 0 || index >= InnerList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            InnerList[index] = value;
        }
    }

    object? IList.this[int index]
    {
        get
        {
            return this[index];
        }
        set
        {
            this[index] = value is ListViewItem item
                ? item
                : new ListViewItem(value!.ToString(), -1);
        }
    }

    /// <summary>
    ///  Retrieves the child control with the specified key.
    /// </summary>
    public virtual ListViewItem? this[string key]
    {
        get
        {
            // We do not support null and empty string as valid keys.
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            // Search for the key in our collection
            int index = IndexOfKey(key);
            if (IsValidIndex(index))
            {
                return this[index];
            }

            return null;
        }
    }

    /// <summary>
    ///  Add an item to the ListView.  The item will be inserted either in
    ///  the correct sorted position, or, if no sorting is set, at the end
    ///  of the list.
    /// </summary>
    public virtual ListViewItem Add(string? text)
    {
        return Add(text, -1);
    }

    int IList.Add(object? item)
    {
        if (item is ListViewItem listViewItem)
        {
            return IndexOf(Add(listViewItem));
        }

        if (item is { } obj)
        {
            return IndexOf(Add(obj.ToString()));
        }

        return -1;
    }

    /// <summary>
    ///  Add an item to the ListView.  The item will be inserted either in
    ///  the correct sorted position, or, if no sorting is set, at the end
    ///  of the list.
    /// </summary>
    public virtual ListViewItem Add(string? text, int imageIndex)
    {
        ListViewItem item = new(text, imageIndex);
        Add(item);
        return item;
    }

    /// <summary>
    ///  Add an item to the ListView.  The item will be inserted either in
    ///  the correct sorted position, or, if no sorting is set, at the end
    ///  of the list.
    /// </summary>
    public virtual ListViewItem Add(ListViewItem value)
    {
        InnerList.Add(value);
        return value;
    }

    // <-- NEW ADD OVERLOADS IN WHIDBEY

    /// <summary>
    ///  Add an item to the ListView.  The item will be inserted either in
    ///  the correct sorted position, or, if no sorting is set, at the end
    ///  of the list.
    /// </summary>
    public virtual ListViewItem Add(string? text, string? imageKey)
    {
        ListViewItem item = new(text, imageKey);
        Add(item);
        return item;
    }

    /// <summary>
    ///  Add an item to the ListView.  The item will be inserted either in
    ///  the correct sorted position, or, if no sorting is set, at the end
    ///  of the list.
    /// </summary>
    public virtual ListViewItem Add(string? key, string? text, string? imageKey)
    {
        ListViewItem item = new(text, imageKey)
        {
            Name = key
        };
        Add(item);
        return item;
    }

    /// <summary>
    ///  Add an item to the ListView.  The item will be inserted either in
    ///  the correct sorted position, or, if no sorting is set, at the end
    ///  of the list.
    /// </summary>
    public virtual ListViewItem Add(string? key, string? text, int imageIndex)
    {
        ListViewItem item = new(text, imageIndex)
        {
            Name = key
        };
        Add(item);
        return item;
    }

    // END - NEW ADD OVERLOADS IN WHIDBEY  -->

    public void AddRange(ListViewItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items);

        InnerList.AddRange(items);
    }

    public void AddRange(ListViewItemCollection items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemArray = new ListViewItem[items.Count];
        items.CopyTo(itemArray, 0);
        InnerList.AddRange(itemArray);
    }

    /// <summary>
    ///  Removes all items from the list view.
    /// </summary>
    public virtual void Clear()
    {
        InnerList.Clear();
    }

    public bool Contains(ListViewItem item)
    {
        return InnerList.Contains(item);
    }

    bool IList.Contains(object? item)
        => item is ListViewItem listViewItem && Contains(listViewItem);

    /// <summary>
    ///  Returns true if the collection contains an item with the specified key, false otherwise.
    /// </summary>
    public virtual bool ContainsKey(string? key)
    {
        return IsValidIndex(IndexOfKey(key));
    }

    public void CopyTo(Array dest, int index)
    {
        InnerList.CopyTo(dest, index);
    }

    /// <summary>
    ///  Searches for Controls by their Name property, builds up an array
    ///  of all the controls that match.
    /// </summary>
    public ListViewItem[] Find(string key, bool searchAllSubItems)
    {
        List<ListViewItem> foundItems = new();
        FindInternal(key, searchAllSubItems, this, foundItems);

        return foundItems.ToArray();
    }

    /// <summary>
    ///  Searches for Controls by their Name property, builds up a list
    ///  of all the controls that match.
    /// </summary>
    private static void FindInternal(string? key, bool searchAllSubItems, ListViewItemCollection listViewItems, List<ListViewItem> foundItems)
    {
        for (int i = 0; i < listViewItems.Count; i++)
        {
            if (listViewItems[i].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                foundItems.Add(listViewItems[i]);
            }
            else
            {
                if (searchAllSubItems)
                {
                    // start from 1, as we've already compared subitems[0]
                    for (int j = 1; j < listViewItems[i].SubItems.Count; j++)
                    {
                        if (listViewItems[i].SubItems[j].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                        {
                            foundItems.Add(listViewItems[i]);
                            break;
                        }
                    }
                }
            }
        }
    }

    public IEnumerator GetEnumerator()
    {
        if (InnerList.OwnerIsVirtualListView && !InnerList.OwnerIsDesignMode)
        {
            // Throw the exception only at runtime.
            throw new InvalidOperationException();
        }

        return InnerList.GetEnumerator();
    }

    public int IndexOf(ListViewItem item)
    {
        for (int index = 0; index < Count; ++index)
        {
            if (this[index] == item)
            {
                return index;
            }
        }

        return -1;
    }

    int IList.IndexOf(object? item)
        => item is ListViewItem listViewItem ? IndexOf(listViewItem) : -1;

    /// <summary>
    ///  The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.
    /// </summary>
    public virtual int IndexOfKey(string? key)
    {
        // Step 0 - Arg validation
        if (string.IsNullOrEmpty(key))
        {
            return -1; // we don't support empty or null keys.
        }

        // step 1 - check the last cached item
        if (IsValidIndex(lastAccessedIndex))
        {
            if (this[lastAccessedIndex].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                return lastAccessedIndex;
            }
        }

        // step 2 - search for the item
        for (int i = 0; i < Count; i++)
        {
            if (this[i].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                lastAccessedIndex = i;
                return i;
            }
        }

        // step 3 - we didn't find it.  Invalidate the last accessed index and return -1.
        lastAccessedIndex = -1;
        return -1;
    }

    /// <summary>
    ///  Determines if the index is valid for the collection.
    /// </summary>
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < Count;
    }

    public ListViewItem Insert(int index, ListViewItem item)
    {
        if (index < 0 || index > Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        InnerList.Insert(index, item);
        return item;
    }

    public ListViewItem Insert(int index, string? text)
    {
        return Insert(index, new ListViewItem(text));
    }

    public ListViewItem Insert(int index, string? text, int imageIndex)
    {
        return Insert(index, new ListViewItem(text, imageIndex));
    }

    void IList.Insert(int index, object? item)
    {
        if (item is ListViewItem listViewItem)
        {
            Insert(index, listViewItem);
        }
        else if (item is { } obj)
        {
            Insert(index, obj.ToString());
        }
    }

    // <-- NEW INSERT OVERLOADS IN WHIDBEY

    public ListViewItem Insert(int index, string? text, string? imageKey)
        => Insert(index, new ListViewItem(text, imageKey));

    public virtual ListViewItem Insert(int index, string? key, string? text, string? imageKey)
        => Insert(index, new ListViewItem(text, imageKey)
        {
            Name = key
        });

    public virtual ListViewItem Insert(int index, string? key, string? text, int imageIndex)
        => Insert(index, new ListViewItem(text, imageIndex)
        {
            Name = key
        });

    // END - NEW INSERT OVERLOADS IN WHIDBEY -->

    /// <summary>
    ///  Removes an item from the ListView
    /// </summary>
    public virtual void Remove(ListViewItem item)
    {
        InnerList.Remove(item);
    }

    /// <summary>
    ///  Removes an item from the ListView
    /// </summary>
    public virtual void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        InnerList.RemoveAt(index);
    }

    /// <summary>
    ///  Removes the child control with the specified key.
    /// </summary>
    public virtual void RemoveByKey(string key)
    {
        int index = IndexOfKey(key);
        if (IsValidIndex(index))
        {
            RemoveAt(index);
        }
    }

    void IList.Remove(object? item)
    {
        if (item is ListViewItem listViewItem)
        {
            Remove(listViewItem);
        }
    }
}