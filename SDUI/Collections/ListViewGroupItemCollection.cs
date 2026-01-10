using System;
using System.Collections;
using System.Collections.Generic;
using SDUI.Collections;
using SDUI.Controls;

internal class ListViewGroupItemCollection : ListViewItemCollection.IInnerList
{
    private readonly ListViewGroup _group;
    private List<ListViewItem>? _items;

    public ListViewGroupItemCollection(ListViewGroup group)
    {
        _group = group;
    }

    private List<ListViewItem> Items => _items ??= new List<ListViewItem>();

    public int Count => Items.Count;

    public bool OwnerIsVirtualListView => _group.ListView is not null && _group.ListView.VirtualMode;

    public bool OwnerIsDesignMode => false;

    public ListViewItem this[int index]
    {
        get => Items[index];
        set
        {
            if (value != Items[index])
            {
                MoveToGroup(Items[index], null);
                Items[index] = value;
                MoveToGroup(Items[index], _group);
            }
        }
    }

    public ListViewItem Add(ListViewItem value)
    {
        CheckListViewItem(value);

        MoveToGroup(value, _group);
        Items.Add(value);
        return value;
    }

    public void AddRange(ListViewItem[] items)
    {
        for (var i = 0; i < items.Length; i++) CheckListViewItem(items[i]);

        Items.AddRange(items);

        for (var i = 0; i < items.Length; i++) MoveToGroup(items[i], _group);
    }

    public void Clear()
    {
        for (var i = 0; i < Count; i++) MoveToGroup(this[i], null);

        Items.Clear();
    }

    public bool Contains(ListViewItem item)
    {
        return Items.Contains(item);
    }

    public void CopyTo(Array dest, int index)
    {
        ((ICollection)Items).CopyTo(dest, index);
    }

    public IEnumerator GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    public int IndexOf(ListViewItem item)
    {
        return Items.IndexOf(item);
    }

    public ListViewItem Insert(int index, ListViewItem item)
    {
        CheckListViewItem(item);

        MoveToGroup(item, _group);
        Items.Insert(index, item);
        return item;
    }

    public void Remove(ListViewItem item)
    {
        Items.Remove(item);

        if (item._group == _group)
        {
            item._group = null;
            UpdateNativeListViewItem(item);
        }
    }

    public void RemoveAt(int index)
    {
        Remove(this[index]);
    }

    private void CheckListViewItem(ListViewItem item)
    {
        if (item.ListView is not null && item.ListView != _group.ListView) throw new ArgumentException(nameof(item));
    }

    private static void MoveToGroup(ListViewItem item, ListViewGroup? newGroup)
    {
        var oldGroup = item.Group;
        if (oldGroup != newGroup)
        {
            item._group = newGroup;
            oldGroup?.Items.Remove(item);
            UpdateNativeListViewItem(item);
        }
    }

    private static void UpdateNativeListViewItem(ListViewItem item)
    {
    }
}