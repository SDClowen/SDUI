using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SDUI.Controls;

namespace SDUI.Collections;

public class UIElementCollection : IList<ElementBase>
{
    private readonly List<ElementBase> _items = new(32);
    private readonly ElementBase _owner;

    public UIElementCollection(ElementBase owner)
    {
        _owner = owner;
    }

    public ElementBase this[int index]
    {
        get => _items[index];
        set
        {
            var oldItem = _items[index];
            if (oldItem != value)
            {
                if (oldItem != null) _owner.OnControlRemoved(new UIElementEventArgs(oldItem));
                _items[index] = value;
                if (value != null) _owner.OnControlAdded(new UIElementEventArgs(value));
            }
        }
    }

    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public void Add(ElementBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _items.Add(item);
        _owner.OnControlAdded(new UIElementEventArgs(item));
    }

    public void Clear()
    {
        var itemsToRemove = _items.ToList();
        _items.Clear();
        foreach (var item in itemsToRemove) _owner.OnControlRemoved(new UIElementEventArgs(item));
    }

    public bool Contains(ElementBase item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(ElementBase[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<ElementBase> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    public int IndexOf(ElementBase item)
    {
        return _items.IndexOf(item);
    }

    public void Insert(int index, ElementBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _items.Insert(index, item);
        _owner.OnControlAdded(new UIElementEventArgs(item));
    }

    public bool Remove(ElementBase item)
    {
        if (item == null)
            return false;

        var result = _items.Remove(item);
        if (result) _owner.OnControlRemoved(new UIElementEventArgs(item));
        return result;
    }

    public void RemoveAt(int index)
    {
        var item = _items[index];
        _items.RemoveAt(index);
        _owner.OnControlRemoved(new UIElementEventArgs(item));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void AddRange(ElementBase[] items)
    {
        foreach (var item in items)
            Add(item);
    }
}