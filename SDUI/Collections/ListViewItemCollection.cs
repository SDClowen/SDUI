using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SDUI.Controls;

namespace SDUI.Collections;

public class ListViewItemCollection : IList
{
    /// A caching mechanism for key accessor
    /// We use an index here rather than control so that we don't have lifetime
    /// issues by holding on to extra references.
    private int lastAccessedIndex = -1;

    public ListViewItemCollection(ListView owner)
    {
        // Kept for APPCOMPAT reasons.
        // In Whidbey this constructor is a no-op.

        // initialize the inner list w/ a dummy list.
        InnerList = new ListViewNativeItemCollection(owner);
    }

    internal ListViewItemCollection(IInnerList innerList)
    {
        Debug.Assert(innerList is not null, "Can't pass in null innerList");
        this.InnerList = innerList;
    }

    private IInnerList InnerList { get; }

    /// <summary>
    ///     Returns the ListViewItem at the given index.
    /// </summary>
    public virtual ListViewItem this[int index]
    {
        get
        {
            if (index < 0 || index >= InnerList.Count) throw new ArgumentOutOfRangeException(nameof(index));

            return InnerList[index];
        }
        set
        {
            if (index < 0 || index >= InnerList.Count) throw new ArgumentOutOfRangeException(nameof(index));

            InnerList[index] = value;
        }
    }

    /// <summary>
    ///     Retrieves the child control with the specified key.
    /// </summary>
    public virtual ListViewItem? this[string key]
    {
        get
        {
            // We do not support null and empty string as valid keys.
            if (string.IsNullOrEmpty(key)) return null;

            // Search for the key in our collection
            var index = IndexOfKey(key);
            if (IsValidIndex(index)) return this[index];

            return null;
        }
    }

    /// <summary>
    ///     Returns the total number of items within the list view.
    /// </summary>
    [Browsable(false)]
    public int Count => InnerList.Count;

    object ICollection.SyncRoot => this;

    bool ICollection.IsSynchronized => true;

    bool IList.IsFixedSize => false;

    public bool IsReadOnly => false;

    object? IList.this[int index]
    {
        get => this[index];
        set =>
            this[index] = value is ListViewItem item
                ? item
                : new ListViewItem(value!.ToString(), -1);
    }

    int IList.Add(object? item)
    {
        if (item is ListViewItem listViewItem) return IndexOf(Add(listViewItem));

        if (item is { } obj) return IndexOf(Add(obj.ToString()));

        return -1;
    }

    /// <summary>
    ///     Removes all items from the list view.
    /// </summary>
    public virtual void Clear()
    {
        InnerList.Clear();
    }

    bool IList.Contains(object? item)
    {
        return item is ListViewItem listViewItem && Contains(listViewItem);
    }

    public void CopyTo(Array dest, int index)
    {
        InnerList.CopyTo(dest, index);
    }

    public IEnumerator GetEnumerator()
    {
        if (InnerList.OwnerIsVirtualListView && !InnerList.OwnerIsDesignMode)
            // Throw the exception only at runtime.
            throw new InvalidOperationException();

        return InnerList.GetEnumerator();
    }

    int IList.IndexOf(object? item)
    {
        return item is ListViewItem listViewItem ? IndexOf(listViewItem) : -1;
    }

    void IList.Insert(int index, object? item)
    {
        if (item is ListViewItem listViewItem)
            Insert(index, listViewItem);
        else if (item is { } obj) Insert(index, obj.ToString());
    }

    /// <summary>
    ///     Removes an item from the ListView
    /// </summary>
    public virtual void RemoveAt(int index)
    {
        if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

        InnerList.RemoveAt(index);
    }

    void IList.Remove(object? item)
    {
        if (item is ListViewItem listViewItem) Remove(listViewItem);
    }

    /// <summary>
    ///     Add an item to the ListView.  The item will be inserted either in
    ///     the correct sorted position, or, if no sorting is set, at the end
    ///     of the list.
    /// </summary>
    public virtual ListViewItem Add(string? text)
    {
        return Add(text, -1);
    }

    /// <summary>
    ///     Add an item to the ListView.  The item will be inserted either in
    ///     the correct sorted position, or, if no sorting is set, at the end
    ///     of the list.
    /// </summary>
    public virtual ListViewItem Add(string? text, int imageIndex)
    {
        ListViewItem item = new(text, imageIndex);
        Add(item);
        return item;
    }

    /// <summary>
    ///     Add an item to the ListView.  The item will be inserted either in
    ///     the correct sorted position, or, if no sorting is set, at the end
    ///     of the list.
    /// </summary>
    public virtual ListViewItem Add(ListViewItem value)
    {
        InnerList.Add(value);
        return value;
    }

    // <-- NEW ADD OVERLOADS IN WHIDBEY

    /// <summary>
    ///     Add an item to the ListView.  The item will be inserted either in
    ///     the correct sorted position, or, if no sorting is set, at the end
    ///     of the list.
    /// </summary>
    public virtual ListViewItem Add(string? text, string? imageKey)
    {
        ListViewItem item = new(text, imageKey);
        Add(item);
        return item;
    }

    /// <summary>
    ///     Add an item to the ListView.  The item will be inserted either in
    ///     the correct sorted position, or, if no sorting is set, at the end
    ///     of the list.
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
    ///     Add an item to the ListView.  The item will be inserted either in
    ///     the correct sorted position, or, if no sorting is set, at the end
    ///     of the list.
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

    public bool Contains(ListViewItem item)
    {
        return InnerList.Contains(item);
    }

    /// <summary>
    ///     Returns true if the collection contains an item with the specified key, false otherwise.
    /// </summary>
    public virtual bool ContainsKey(string? key)
    {
        return IsValidIndex(IndexOfKey(key));
    }

    /// <summary>
    ///     Searches for Controls by their Name property, builds up an array
    ///     of all the controls that match.
    /// </summary>
    public ListViewItem[] Find(string key, bool searchAllSubItems)
    {
        List<ListViewItem> foundItems = new();
        FindInternal(key, searchAllSubItems, this, foundItems);

        return foundItems.ToArray();
    }

    /// <summary>
    ///     Searches for Controls by their Name property, builds up a list
    ///     of all the controls that match.
    /// </summary>
    private static void FindInternal(string? key, bool searchAllSubItems, ListViewItemCollection listViewItems,
        List<ListViewItem> foundItems)
    {
        for (var i = 0; i < listViewItems.Count; i++)
            if (listViewItems[i].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                foundItems.Add(listViewItems[i]);
            }
            else
            {
                if (searchAllSubItems)
                    // start from 1, as we've already compared subitems[0]
                    for (var j = 1; j < listViewItems[i].SubItems.Count; j++)
                        if (listViewItems[i].SubItems[j].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                        {
                            foundItems.Add(listViewItems[i]);
                            break;
                        }
            }
    }

    public int IndexOf(ListViewItem item)
    {
        for (var index = 0; index < Count; ++index)
            if (this[index] == item)
                return index;

        return -1;
    }

    /// <summary>
    ///     The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.
    /// </summary>
    public virtual int IndexOfKey(string? key)
    {
        // Step 0 - Arg validation
        if (string.IsNullOrEmpty(key)) return -1; // we don't support empty or null keys.

        // step 1 - check the last cached item
        if (IsValidIndex(lastAccessedIndex))
            if (this[lastAccessedIndex].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                return lastAccessedIndex;

        // step 2 - search for the item
        for (var i = 0; i < Count; i++)
            if (this[i].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                lastAccessedIndex = i;
                return i;
            }

        // step 3 - we didn't find it.  Invalidate the last accessed index and return -1.
        lastAccessedIndex = -1;
        return -1;
    }

    /// <summary>
    ///     Determines if the index is valid for the collection.
    /// </summary>
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < Count;
    }

    public ListViewItem Insert(int index, ListViewItem item)
    {
        if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));

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

    // <-- NEW INSERT OVERLOADS IN WHIDBEY

    public ListViewItem Insert(int index, string? text, string? imageKey)
    {
        return Insert(index, new ListViewItem(text, imageKey));
    }

    public virtual ListViewItem Insert(int index, string? key, string? text, string? imageKey)
    {
        return Insert(index, new ListViewItem(text, imageKey)
        {
            Name = key
        });
    }

    public virtual ListViewItem Insert(int index, string? key, string? text, int imageIndex)
    {
        return Insert(index, new ListViewItem(text, imageIndex)
        {
            Name = key
        });
    }

    // END - NEW INSERT OVERLOADS IN WHIDBEY -->

    /// <summary>
    ///     Removes an item from the ListView
    /// </summary>
    public virtual void Remove(ListViewItem item)
    {
        InnerList.Remove(item);
    }

    /// <summary>
    ///     Removes the child control with the specified key.
    /// </summary>
    public virtual void RemoveByKey(string key)
    {
        var index = IndexOfKey(key);
        if (IsValidIndex(index)) RemoveAt(index);
    }

    internal class ListViewNativeItemCollection : IInnerList
    {
        private readonly ListView _owner;

        public ListViewNativeItemCollection(ListView owner)
        {
            _owner = owner;
        }

        public int Count => _owner.VirtualMode ? _owner.VirtualListSize : _owner._listViewItems?.Count ?? 0;

        public bool OwnerIsVirtualListView => _owner.VirtualMode;

        public bool OwnerIsDesignMode => false;

        public ListViewItem this[int displayIndex]
        {
            get => GetItemByIndexInternal(displayIndex, true)!;
            set
            {
                if (_owner.VirtualMode) throw new InvalidOperationException();

                ArgumentOutOfRangeException.ThrowIfNegative(displayIndex);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(displayIndex, _owner._listViewItems?.Count ?? 0);


                RemoveAt(displayIndex);
                Insert(displayIndex, value);
            }
        }

        public ListViewItem? GetItemByIndex(int index)
        {
            return GetItemByIndexInternal(index, false);
        }

        public ListViewItem Add(ListViewItem value)
        {
            if (_owner.VirtualMode) return value;

            // PERF.
            // Get the Checked bit before adding it to the back end.
            // This saves a call into NativeListView to retrieve the real index.
            var valueChecked = value.Checked;

            _owner.InsertItems(_owner._listViewItems?.Count ?? 0, [value]);

            if (_owner.IsHandleCreated && !_owner.CheckBoxes && valueChecked)
                _owner.UpdateSavedCheckedItems(value, true /*addItem*/);

            return value;
        }

        public void AddRange(params ListViewItem[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (_owner.VirtualMode) return;

            bool[]? checkedValues = null;

            if (_owner.IsHandleCreated && !_owner.CheckBoxes)
            {
                // PERF.
                // Cache the Checked bit before adding the item to the list view.
                // This saves a bunch of calls to native list view when we want to UpdateSavedCheckedItems.
                checkedValues = new bool[values.Length];
                for (var i = 0; i < values.Length; i++) checkedValues[i] = values[i].Checked;
            }

            _owner.InsertItems(_owner._listViewItems?.Count ?? 0, values);

            if (_owner.IsHandleCreated && !_owner.CheckBoxes)
                for (var i = 0; i < values.Length; i++)
                    if (checkedValues![i])
                        _owner.UpdateSavedCheckedItems(values[i], true /*addItem*/);
        }

        public void Clear()
        {
            if ((_owner._listViewItems?.Count ?? 0) <= 0) return;

            _owner._listViewItems!.Clear();
            _owner.SelectedIndex = -1;
            _owner.Invalidate();
        }


        public ListViewItem Insert(int index, ListViewItem item)
        {
            int count;
            if (_owner.VirtualMode)
                count = Count;
            else
                count = _owner._listViewItems?.Count ?? 0;

            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, count);


            _owner.InsertItems(index, [item]);
            if (_owner.IsHandleCreated && !_owner.CheckBoxes && item.Checked)
                _owner.UpdateSavedCheckedItems(item, true /*addItem*/);

            return item;
        }

        public int IndexOf(ListViewItem item)
        {
            Debug.Assert(!_owner.VirtualMode, "in virtual mode, this function does not make any sense");
            if (_owner._listViewItems is null) return -1;
            return _owner._listViewItems.IndexOf(item);
        }

        public void Remove(ListViewItem item)
        {
            var index = _owner.VirtualMode ? Count - 1 : IndexOf(item);


            if (index != -1) RemoveAt(index);
        }

        public void RemoveAt(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _owner._listViewItems?.Count ?? 0);


            this[index].Focused = false;

            if (_owner._listViewItems is not null)
            {
                _owner._listViewItems.RemoveAt(index);
                if (_owner.SelectedIndex >= _owner._listViewItems.Count)
                    _owner.SelectedIndex = _owner._listViewItems.Count - 1;
                _owner.Invalidate();
            }
        }

        public void CopyTo(Array dest, int index)
        {
            if ((_owner._listViewItems?.Count ?? 0) > 0)
                for (var displayIndex = 0; displayIndex < Count; ++displayIndex)
                    dest.SetValue(this[displayIndex], index++);
        }

        public IEnumerator GetEnumerator()
        {
            var items = new ListViewItem[_owner._listViewItems?.Count ?? 0];
            CopyTo(items, 0);

            return items.GetEnumerator();
        }

        public bool Contains(ListViewItem item)
        {
            if (_owner.VirtualMode) return false;
            return _owner._listViewItems?.Contains(item) ?? false;
        }

        private ListViewItem? GetItemByIndexInternal(int index, [NotNullWhen(true)] bool throwInVirtualMode)
        {
            if (_owner.VirtualMode)
                // Virtual mode not implemented for now
                return null;

            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _owner._listViewItems?.Count ?? 0);

            if (_owner.IsHandleCreated && _owner._listViewItems is not null) return _owner._listViewItems[index];

            return _owner._listViewItems?[index];
        }

        private int DisplayIndexToID(int displayIndex)
        {
            return displayIndex;
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
}