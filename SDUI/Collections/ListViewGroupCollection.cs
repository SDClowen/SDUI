using System;
using System.Collections;
using System.Collections.Generic;
using SDUI.Controls;

namespace SDUI.Collections;

public class ListViewGroupCollection : IList
{
    private readonly ListView _listView;

    private List<ListViewGroup>? _list;

    internal ListViewGroupCollection(ListView listView)
    {
        _listView = listView;
    }

    private List<ListViewGroup> List => _list ??= [];

    public ListViewGroup this[int index]
    {
        get => List[index];
        set
        {
            if (List.Contains(value)) return;

            CheckListViewItems(value);
            value.ListView = _listView;
            List[index] = value;
        }
    }

    public ListViewGroup? this[string key]
    {
        get
        {
            if (_list is null) return null;

            for (var i = 0; i < _list.Count; i++)
                if (string.Equals(key, this[i].Name, StringComparison.CurrentCulture))
                    return this[i];

            return null;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (_list is null)
                // nothing to do
                return;

            var index = -1;
            for (var i = 0; i < _list.Count; i++)
                if (string.Equals(key, this[i].Name, StringComparison.CurrentCulture))
                {
                    index = i;
                    break;
                }

            if (index != -1) _list[index] = value;
        }
    }

    public int Count => List.Count;

    object ICollection.SyncRoot => this;

    bool ICollection.IsSynchronized => true;

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    object? IList.this[int index]
    {
        get => this[index];
        set
        {
            if (value is ListViewGroup group) this[index] = group;
        }
    }

    int IList.Add(object? value)
    {
        if (value is not ListViewGroup group) return -1;

        return Add(group);
    }

    public void Clear()
    {
        if (_listView.IsHandleCreated)
            for (var i = 0; i < Count; i++)
                _listView.Groups.Remove(this[i]);

        // Dissociate groups from the ListView
        for (var i = 0; i < Count; i++) this[i].ListView = null;

        List.Clear();

        // we have to tell the listView that there are no more groups
        // so the list view knows to remove items from the default group
        _listView.Invalidate(); //.UpdateGroupView();
    }

    bool IList.Contains(object? value)
    {
        if (value is not ListViewGroup group) return false;

        return Contains(group);
    }

    public void CopyTo(Array array, int index)
    {
        ((ICollection)List).CopyTo(array, index);
    }

    public IEnumerator GetEnumerator()
    {
        return List.GetEnumerator();
    }

    int IList.IndexOf(object? value)
    {
        if (value is not ListViewGroup group) return -1;

        return IndexOf(group);
    }

    void IList.Insert(int index, object? value)
    {
        if (value is ListViewGroup group) Insert(index, group);
    }

    void IList.Remove(object? value)
    {
        if (value is ListViewGroup group) Remove(group);
    }

    public void RemoveAt(int index)
    {
        Remove(this[index]);
    }

    public int Add(ListViewGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (Contains(group)) return -1;

        CheckListViewItems(group);
        group.ListView = _listView;
        var index = ((IList)List).Add(group);
        //if (_listView.IsHandleCreated)
        //{
        //    _listView.InsertGroupInListView(List.Count, group);
        //    MoveGroupItems(group);
        //}

        return index;
    }

    public ListViewGroup Add(string? key, string? headerText)
    {
        var group = new ListViewGroup(key, headerText);
        Add(group);
        return group;
    }

    public void AddRange(ListViewGroup[] groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        for (var i = 0; i < groups.Length; i++) Add(groups[i]);
    }

    public void AddRange(ListViewGroupCollection groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        for (var i = 0; i < groups.Count; i++) Add(groups[i]);
    }

    private void CheckListViewItems(ListViewGroup group)
    {
        for (var i = 0; i < group.Items.Count; i++)
        {
            var item = group.Items[i];
            if (item.ListView is not null && item.ListView != _listView)
            {
            }
        }
    }

    public bool Contains(ListViewGroup value)
    {
        return List.Contains(value);
    }

    public int IndexOf(ListViewGroup value)
    {
        return List.IndexOf(value);
    }

    public void Insert(int index, ListViewGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (Contains(group)) return;

        CheckListViewItems(group);
        group.ListView = _listView;
        List.Insert(index, group);
        if (_listView.IsHandleCreated)
        {
            _listView.InsertGroupInListView(index, group);
            MoveGroupItems(group);
        }
    }

    private void MoveGroupItems(ListViewGroup group)
    {
        foreach (ListViewItem item in group.Items)
            if (item.ListView == _listView)
            {
                //item.UpdateStateToListView(item.Index);
            }
    }

    public void Remove(ListViewGroup group)
    {
        if (!List.Remove(group)) return;

        group.ListView = null;

        if (_listView.IsHandleCreated) _listView.RemoveGroupFromListView(group);
    }
}